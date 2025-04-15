using System;
using API.DTOs;
using API.Entities;
using API.Externsions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
  public AutoMapperProfiles()
  {
    CreateMap<AppUser, MemberDto>()
      .ForMember(d => d.Age, o => o.MapFrom(s => s.DateOfBirth.CalculateAge()))
      .ForMember(d => d.PhotoUrl, o => o.MapFrom(s => s.Photos.FirstOrDefault(p => p.IsMain)!.Url));

    CreateMap<Photo, PhotoDto>();
    CreateMap<MemberUpdateDto, AppUser>();
    CreateMap<RegisterDto, AppUser>();
    CreateMap<string, DateOnly>().ConstructUsing(s => DateOnly.Parse(s));
    CreateMap<Message, MessageDto>()
      .ForMember(d => d.SenderPhotoUrl, o => o.MapFrom(s => s.Sender.Photos.FirstOrDefault(p => p.IsMain)!.Url))
      .ForMember(d => d.RecipientPhotoUrl, o => o.MapFrom(s => s.Recipient.Photos.FirstOrDefault(p => p.IsMain)!.Url));

    // Below handles situations where dates are not formatted as Utc such as when returning from SQLite DB.
    CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
    CreateMap<DateTime?, DateTime?>().ConvertUsing(d => d.HasValue ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null); //To handle optional date fields.

    CreateMap<Photo, PhotoForApprovalDto>()
      .ForMember(d => d.Username, o => o.MapFrom(u => u.AppUser.UserName));
  }
}
