using System;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepo, IMapper mapper) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        var users = await userRepo.GetMembersAsync();
        return Ok(users);
    }

    [HttpGet("{id:int}")] // /api/users/[id]
    public async Task<ActionResult<AppUser>> GetUsersById(int id)
    {
        var user = await userRepo.GetUserByIdAsync(id);

        if(user == null) return NotFound();

        return user;
    }

    
    [HttpGet("{username}")] // /api/users/[id]
    public async Task<ActionResult<MemberDto>> GetUsersByUsername(string username)
    {
        var user = await userRepo.GetMemberAsync(username);

        if(user == null) return NotFound();

        return user;
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (username == null) return BadRequest("No username found in token.");

        var user = await userRepo.GetUserByUsernameAsync(username);
        if (user == null) return BadRequest("Could not find user to update.");

        mapper.Map(memberUpdateDto, user);

        if (await userRepo.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update user.");
    }
}
