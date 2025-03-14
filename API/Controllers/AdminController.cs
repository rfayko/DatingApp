using API.Controllers;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AdminController(UserManager<AppUser> userManager) : BaseApiController
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

  [Authorize(Policy = "ModeratePhotoRole")]
  [HttpGet("photos-to-moderate")]
  public ActionResult GetPhotosForModeration()
  {
    return Ok("Only Admins or Moderators can see this.");
  }
}