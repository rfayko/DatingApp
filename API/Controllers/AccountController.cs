using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(
    UserManager<AppUser> userManager, 
    ITokenService tokenService, 
    IMapper mapper) : BaseApiController
{
    [HttpPost("register")]  //account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if( await UserExists(registerDto.Username)) return BadRequest("Username is already taken.");

        var user = mapper.Map<AppUser>(registerDto);
        user.UserName = registerDto.Username.ToLower();

        var result = await userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return new UserDto
        {
            Username = user.UserName,
            KnownAs = user.KnownAs,
            Gender = user.Gender,
            Token = tokenService.CreateToken(user)
        };        
    }
    
    [HttpPost("login")]  //account/register
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await userManager.Users
                .Include(u => u.Photos)
                .FirstOrDefaultAsync(u => u.NormalizedUserName == loginDto.Username.ToUpper());
        
        if (user == null || user.UserName == null) return Unauthorized("Invalid Username");

        if(await userManager.CheckPasswordAsync(user, loginDto.Password)) return Unauthorized();

        return new UserDto
        {
            Username = user.UserName,
            KnownAs = user.KnownAs,
            Gender = user.Gender,
            Token = tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await userManager.Users.AnyAsync(u => u.NormalizedUserName == username.ToUpper());
    }
}
