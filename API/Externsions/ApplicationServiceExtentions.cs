using System;
using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Externsions;

public static class ApplicationServiceExtentions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection servcices, IConfiguration config)
    {
        servcices.AddControllers();
        servcices.AddDbContext<DataContext>(opt =>
        {
            opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
        });
        servcices.AddCors();
        servcices.AddScoped<ITokenService, TokenService>();

        return servcices;
    }
}
