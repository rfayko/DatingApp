using System;

namespace API.Helpers;

public class LikesParams : PaginationParams
{
  public required int UserId { get; set; }
  public required string Predicate { get; set; } = "liked";
}
