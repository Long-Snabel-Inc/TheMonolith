using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TheMonolith.Data;
using TheMonolith.Database.Repositories;
using TheMonolith.Models;

namespace TheMonolith.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ServiceOptions _serviceOptions;
    private readonly UserRepository _userRepository;

    public UserController(ServiceOptions serviceOptions, UserRepository userRepository)
    {
        _serviceOptions = serviceOptions;
        _userRepository = userRepository;
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

        return Ok(new
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            UserName = user.UserName,
            token = tokenString(user),
        });
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
}
