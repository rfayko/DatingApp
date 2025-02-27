using System.Collections.Frozen;
using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Externsions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;

namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepo, IMapper mapper, IPhotoService photoService) : BaseApiController
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
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await userRepo.GetMemberAsync(username);

        if(user == null) return NotFound();

        return user;
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await userRepo.GetUserByUsernameAsync(User.GetUserName());
      
        mapper.Map(memberUpdateDto, user);

        if (await userRepo.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update user.");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await userRepo.GetUserByUsernameAsync(User.GetUserName());

        if (user == null) return BadRequest("Cannot update user");

        var result = await photoService.AddPhotoAsync(file);

        if (result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo{
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
            IsMain = user.Photos.Count == 0
        };

        user.Photos.Add(photo);

        if (await userRepo.SaveAllAsync()) 
            return CreatedAtAction(nameof(GetUser), new {username = user.UserName}, mapper.Map<PhotoDto>(photo));

        return BadRequest("Problem adding Photo");
 
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await userRepo.GetUserByUsernameAsync(User.GetUserName());
      
        if (user == null) return BadRequest("Could not find user");

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

        if (photo == null || photo.IsMain) return BadRequest("Could not find photo or is already Main.");

        var mainPhoto = user.Photos.FirstOrDefault(p => p.IsMain);
        if (mainPhoto != null)
            mainPhoto.IsMain = false;

        photo.IsMain = true;

        if (await userRepo.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update main photo.");
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await userRepo.GetUserByUsernameAsync(User.GetUserName());
      
        if (user == null) return BadRequest("Could not find user");

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

        if (photo == null || photo.IsMain) return BadRequest("Cannot delete Main Photo.");

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest("Failed to delete photo from Cloudinary: " + result.Error.Message);
        }        

        var mainPhoto = user.Photos.Remove(photo);

        if (await userRepo.SaveAllAsync()) return Ok();

        return BadRequest("Failed to delete photo.");
    }
}
