using System;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class PhotoRepository(DataContext context, IMapper mapper) : IPhotoRepository
{
    public async Task<Photo?> GetPhotoById(int id)
    {
         return await context.Photos
                        .IgnoreQueryFilters()
                        .SingleOrDefaultAsync(x => x.Id == id);      
    }

    public async Task<IEnumerable<PhotoForApprovalDto>?> GetUnapprovedPhotos()
    {
        return await context.Photos
            .Where(p => !p.IsApproved)
            .ProjectTo<PhotoForApprovalDto>(mapper.ConfigurationProvider)
            .IgnoreQueryFilters()
            .ToListAsync();
    }

    public void RemovePhoto(Photo photo)
    {
        context.Photos.Remove(photo);
    }
}
