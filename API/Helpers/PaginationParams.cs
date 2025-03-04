using System;

namespace API.Helpers;

public class PaginationParams
{
  private const int maxPageSize = 50;
  private int pageSize = 10;


  public int PageNumber { get; set; } = 1;
  public int PageSize 
  { 
    get => pageSize;
    set => pageSize = value > maxPageSize ? maxPageSize : value; 
  }
}
