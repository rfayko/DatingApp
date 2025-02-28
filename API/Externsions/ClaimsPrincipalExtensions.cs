using System;
using System.Security.Claims;

namespace API.Externsions;

public static class ClaimsPrincipalExtensions
{
  public static string GetUserName(this ClaimsPrincipal user)
  {
      var username = user.FindFirstValue(ClaimTypes.Name) ?? throw new Exception("Cannot get username from token");
      return username;
  }

  public static int GetUserId(this ClaimsPrincipal user)
  {
      var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("Cannot get User Id from token");
      return int.Parse(userId);
  }
}
