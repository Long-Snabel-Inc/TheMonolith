using IronPython.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Scripting.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TheMonolith.Data;
using TheMonolith.Database.Repositories;
using TheMonolith.Models;
using TheMonolith.Services;

namespace TheMonolith.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ServiceOptions _serviceOptions;
    private readonly UserRepository _userRepository;
    private readonly ScoreRepository _scoreRepository;
    private readonly UserScoreRepository _userScoreRepository;
    private readonly ScoreService _scoreService;
    private readonly ILogger<UserController> _logger;

    public UserController(ServiceOptions serviceOptions, UserRepository userRepository, ScoreRepository scoreRepository, UserScoreRepository userScoreRepository, ILogger<UserController> logger, ScoreService scoreService)
    {
        _serviceOptions = serviceOptions;
        _userRepository = userRepository;
        _scoreRepository = scoreRepository;
        _userScoreRepository = userScoreRepository;
        _logger = logger;
        _scoreService = scoreService;
    }

    [HttpPost("Authenticate")]
    [AllowAnonymous]
    public async Task<ActionResult> Authenticate([FromBody] LoginModel loginModel)
    {
        var user = await _userRepository.Get(loginModel.UserName);
        if (user is null || !await _userRepository.Login(user, loginModel.Password))
            return BadRequest("Invadlid username/password");

        return Ok(new
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            UserName = user.UserName,
            token = tokenString(user),
        });
    }

    [HttpPost("Register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register([FromBody] RegisterModel registerModel)
    {
        if (await _userRepository.ExistsUsername(registerModel.UserName) || await _userRepository.ExistsEmail(registerModel.Email))
            return BadRequest("Email or username already excists");

        var user = await _userRepository.Create(registerModel.UserName, registerModel.Email, registerModel.FullName, registerModel.Password);

        await JudgeFromGoogle(user);

        return Ok(new
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            UserName = user.UserName,
            token = tokenString(user),
        });
    }

    [HttpGet("{userId:int}/Score")]
    public async Task<ActionResult<double>> GetScore([FromRoute] int userId)
    {
        var user = await _userRepository.Get(userId);
        if (user is null)
        {
            return NotFound();
        }

        return await _scoreService.GetWeightedScore(user!);
    }

    [HttpGet("AllUsers")]
    public async Task<ActionResult<List<User>>> AllUsers()
    {
        var allUsers = _userRepository.GetAll();

        return Ok(allUsers);
    }

    [HttpGet("LastRating")]
    public async Task<ActionResult<DateTime>> LastRating()
    {
        if (int.TryParse(User.Identity?.Name, out var id))
        {
            return Ok(await _userScoreRepository.LatestUpdate(id));
        }
        return BadRequest("Not Logged In");
    }

    [HttpGet("Rate/{userId:int}/{i:int}")]
    public async Task<ActionResult<DateTime>> Rate([FromRoute] int userId, [FromRoute] int i)
    {
        if (int.TryParse(User.Identity?.Name, out var id))
        {
            await _userScoreRepository.Upsert(id, userId, i);
            return Ok(i);
        }
        return BadRequest("Not Logged In");
    }

    private string tokenString(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_serviceOptions.PrivateKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                        new Claim(ClaimTypes.Name, user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task JudgeFromGoogle(User user)
    {
        var start = new ProcessStartInfo
        {
            FileName = @"python",
            Arguments = $@"C:\repos\LongSnabel\Maratus\main.py ""{user.FullName}""",
            WorkingDirectory = @"C:\repos\LongSnabel\Maratus",
            UseShellExecute = false, // Do not use OS shell
            CreateNoWindow = true, // We don't need new window
            RedirectStandardOutput = true, // Any output, generated by application will be redirected back
            RedirectStandardError = true // Any error in standard output will be redirected back (for example exceptions)
        };
        using var process = Process.Start(start);
        using var reader = process!.StandardOutput;
        var stderr = await process.StandardError.ReadToEndAsync(); // Here are the exceptions from our Python script
        var strRes = await reader.ReadToEndAsync();
        var result = float.Parse(strRes, CultureInfo.InvariantCulture); // Here is the result of StdOut(for example: print "test")
        await _scoreRepository.Create(new Score(user, Score.GoogleType, result));
    }
}
