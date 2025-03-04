using API.DTOs;
using API.Entities;
using API.Externsions;
using API.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController(ILikesRepository likesRepo) : BaseApiController
{
    [HttpPost("{targetUserId:int}")]
    public async Task<ActionResult> ToggleResult(int targetUserId)
    {
        var sourceUserId = User.GetUserId();

        if (sourceUserId == targetUserId) return BadRequest("You can't like yourself. At least in this app.");

        var existingLike = await likesRepo.GetUserLike(sourceUserId, targetUserId);

        if (existingLike == null)
        {
            var like = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = targetUserId
            };

            likesRepo.AddLike(like);
        }
        else
        {
            likesRepo.DeleteLike(existingLike);
        }

        if (await likesRepo.SaveChanges()) return Ok();

        return BadRequest("Failed to update Like ");
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
    {
        return Ok(await likesRepo.GetCurrentUserLikeIds(User.GetUserId()));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery]string predicate)
    {
        return Ok(await likesRepo.GetUserLikes(predicate, User.GetUserId()));
    }
}
