using System;
using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Externsions;

public static class ApplicationServiceExtentions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers();
        var connectionString = "Sqlite";
        var useSqlServer = config.GetValue<bool>("UseSqlServer");
        var useRemoteSqlServer = config.GetValue<bool>("UseRemoteSqlServer");
        
        if(useSqlServer)
        {
            if (useRemoteSqlServer)
                connectionString = "AzureSqlServer";
            else
                connectionString = "LocalSqlServer";
        }
        
        services.AddDbContext<DataContext>(opt =>
        {
            opt.UseSqlServer(config.GetConnectionString(connectionString));
        });
        
        services.AddCors();
        
        services.AddScoped<ITokenService, TokenService>();
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILikesRepository, LikesRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IPhotoRepository, PhotoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<LogUserActivity>();

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
        services.AddSignalR();
        services.AddSingleton<PresenceTracker>();  // Need singleton to track presence throughout app. This is not a robust imple.

        return services;
    }
}
