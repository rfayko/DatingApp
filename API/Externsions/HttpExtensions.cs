using System;
using System.Text.Json;
using API.Helpers;

namespace API.Externsions;

public static class HttpExtensions
{
  public static void AddPaginationHeader<T>(this HttpResponse response, PagedList<T> data)
  {
    var paginationHeader = new PaginationHeader(data.CurrentPage, data.PageSize, data.TotalCount, data.TotalPages);

    var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    var jsonString = JsonSerializer.Serialize(paginationHeader, jsonOptions);

    response.Headers.Append("Pagination", jsonString);
    response.Headers.Append("Access-Control-Expose-Headers", "Pagination");
  }
}
