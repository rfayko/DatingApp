using System.Net.Mail;
using System.Threading.Tasks;
using API.Controllers;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Externsions;
using API.Interfaces;
using API.Services;
using AutoMapper;
using CloudinaryDotNet.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AdminController(UserManager<AppUser> userManager, IUnitOfWork uow, IPhotoService photoService) : BaseApiController
{
  [Authorize(Policy = "RequireAdminRole")]
  [HttpGet("users-with-roles")]
  public async Task<ActionResult> GetUsersWithRoles()
  {
    var usersWithRoles = await userManager.Users
                  .OrderBy(u => u.UserName)
                  .Select(u => new 
                  {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                  }).ToListAsync();

    return Ok(usersWithRoles);
  }

  [Authorize(Policy = "RequireAdminRole")]
  [HttpPost("edit-roles/{username}")]
  public async Task<ActionResult> EditRoles(string username, string roles)
  {
    if(string.IsNullOrEmpty(roles)) return BadRequest("You must provide at least one role."); 
    
    var selectedRoles = roles.Split(",").ToArray();

    var user = await userManager.FindByNameAsync(username);
    if (user == null) return BadRequest("User not found");

    var userRoles = await userManager.GetRolesAsync(user);

    var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
    if(!result.Succeeded) return BadRequest("Failed to add User Roles");

    result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
    if(!result.Succeeded) return BadRequest("Failed to remove User Roles");

    return Ok(await userManager.GetRolesAsync(user));
  }

  /***********************************************
  * Photo Moderation IF
  ***********************************************/

  [Authorize(Policy = "ModeratePhotoRole")]
  [HttpGet("photos-to-moderate")]
  public async Task<ActionResult<IEnumerable<PhotoForApprovalDto>>> GetPhotosForApproval()
  {
    return Ok(await uow.PhotoRepository.GetUnapprovedPhotos());
  }

  [Authorize(Policy = "ModeratePhotoRole")]
  [HttpPost("approve-photo/{id:int}")]
  public async Task<ActionResult> ApprovePhoto(int id)
  {
    var photoForApproval = await uow.PhotoRepository.GetPhotoById(id);
    
    if (photoForApproval == null) 
      return NotFound("Photo not found.");
    
    if (photoForApproval.IsApproved) 
      return BadRequest("Photo already approved");

    photoForApproval.IsApproved = true;

    // Get the User and if User does not have Main photo, update this photo to Main
    var user = await GetAppUserByPhotoId(id);
    if (user == null) return BadRequest("Can't get user from DB");
    
    if(!user.Photos.Any(p => p.IsMain)) photoForApproval.IsMain = true;

    if (await uow.Complete())
        return Ok();
    
    return BadRequest("Failed to approve Photo");
  }

  [Authorize(Policy = "ModeratePhotoRole")]
  [HttpPost("reject-photo/{id:int}")]
  public async Task<ActionResult> RejectPhoto(int id)
  {
    var photo = await uow.PhotoRepository.GetPhotoById(id);
    if (photo == null) return NotFound("Photo not found");

    if(photo.PublicId != null)
    {
      var result = await photoService.DeletePhotoAsync(photo.PublicId);

      if (result.Result == "ok")
      {
        uow.PhotoRepository.RemovePhoto(photo);
      }
    }
    else
    {
      uow.PhotoRepository.RemovePhoto(photo);
    }

    if(await uow.Complete()) return Ok();

    return BadRequest("Failed to remove Photo from repository.");

  }

  private async Task<AppUser?> GetAppUserByPhotoId(int photoId)
  {
    var users = await uow.UserRepository.GetUsersAsync();
    if (users == null) return null;
   
    return users.FirstOrDefault(u => u.Photos.Any(p => p.Id == photoId));
  }
}