using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly DataContext _context;

    public AccountController(DataContext context)
    {
        _context = context;
    }

    // API Controller is taking care of DTO validation before we even get to the Register()
    [HttpPost("register")] // POST: api/account/register
    public async Task<ActionResult<AppUser>> Register(RegisterDto registerDto)
    {

        if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");
        
        using var hmac = new HMACSHA512(); // using keyword triggers the class's dispose() method; disposes of the class after it's done being used

        var user = new AppUser
        {
            UserName = registerDto.Username.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    private async Task<bool> UserExists(string username)
    {
        return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
}