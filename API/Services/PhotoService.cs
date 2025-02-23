using System;
using API.Interfaces;
using CloudinaryDotNet.Actions;

namespace API.Services;

public class PhotoService : IPhotoService
{
    public Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
    {
        throw new NotImplementedException();
    }

    public Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        throw new NotImplementedException();
    }
}
