using System;

namespace API.Helpers;

public class UserParams
{
  private const int maxPageSize = 50;
  private int pageSize = 10;


  public int PageNumber { get; set; } = 1;
  public int PageSize 
  { 
    get => pageSize;
    set => pageSize = value > maxPageSize ? maxPageSize : value; 
  }

  public string? Gender { get; set; }
  public string? CurrentUserName { get; set; }
  public int MinAge { get; set; } = 18;
  public int MaxAge { get; set; } = 100;
  public string OrderBy { get; set; } = "LastActive";

}
