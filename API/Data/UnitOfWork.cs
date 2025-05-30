using System;
using API.Interfaces;

namespace API.Data;

public class UnitOfWork(
    DataContext context, 
    IUserRepository userRepository, 
    ILikesRepository likesRepository, 
    IPhotoRepository photoRepository,
    IMessageRepository messageRepository) : IUnitOfWork
{
    public IUserRepository UserRepository => userRepository;

    public ILikesRepository LikesRepository => likesRepository;

    public IMessageRepository MessageRepository => messageRepository;
    
    public IPhotoRepository PhotoRepository => photoRepository;

    public async Task<bool> Complete()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public bool HasChanges()
    {
        return context.ChangeTracker.HasChanges();
    }
}
