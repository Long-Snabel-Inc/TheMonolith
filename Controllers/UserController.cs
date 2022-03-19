using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TheMonolith.Data;

namespace TheMonolith.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ServiceOptions _serviceOptions;

    public UserController(ServiceOptions serviceOptions)
    {
        _serviceOptions = serviceOptions;
    }

    [HttpPost("Authenticate")]
    public IActionResult Authenticate(User user)
    {
        // Dummy authencation
        if (user.UserName != "Strube")
            return BadRequest("You are not Strube");

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
        var tokenString = tokenHandler.WriteToken(token);

        return Ok( tokenString );
    }
}
