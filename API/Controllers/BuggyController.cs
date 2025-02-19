using System;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BuggyController(DataContext context) : BaseApiController
{
  [Authorize]
  [HttpGet("auth")]
  public ActionResult<string> GetAuth()
  {
    return "secret text";
  }


  [HttpGet("not-found")]
  public ActionResult<AppUser> GetNotFound()
  {
    var user = context.Users.Find(-1);
    if(user == null) return NotFound();
    return user;
  }


  [HttpGet("server-error")]
  public ActionResult<AppUser> GetServerError()
  {
    var user = context.Users.Find(-1) ?? throw new Exception("bad thing has happened");
    return user;
  }


  [HttpGet("bad-request")]
  public ActionResult<AppUser> GetBadRequest()
  {
    return BadRequest("This was not a good request");
  }


}
