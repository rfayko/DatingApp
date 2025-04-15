using System;

namespace API.Interfaces;

public interface IUnitOfWork
{
    IUserRepository UserRepository{get;}
    ILikesRepository LikesRepository{get;}
    IMessageRepository MessageRepository{get;}
    IPhotoRepository PhotoRepository{get;}

    Task<bool> Complete();

    bool HasChanges();

}
