using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PaxDeiBot.Data;
using PaxDeiBot.Models;
using PaxDeiBot.Services.TreeItemComponents;

namespace PaxDeiBot;

public static class ProgramExtensions
{
    public static WebApplicationBuilder SetupBuilder(this WebApplicationBuilder builder)
    {
        builder.Services.SetupServices(builder.Configuration);
        return builder;
    }

    public static WebApplication InitializeMigrations(this WebApplication app)
    {
        app.Services.GetService<ItemDbContext>().Database.Migrate();
        return app;
    }

    public static WebApplication SetupApplication(this WebApplication app)
    {
        app.UseCors("Any");

        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("v1/swagger.json", "Items WebApi v1"); });

        app.UseStaticFiles();

        app.UseRouting();
        //
        // app.UseAuthentication();
        // app.UseAuthorization();
        //
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        return app;
    }

    private static IServiceCollection SetupServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options => options.AddPolicy("Any", policyBuilder =>
        {
            policyBuilder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }));

        services.AddDbContext<ItemDbContext>(options =>
        {
            var dbType = configuration["CurrentDatabaseConnectionString"];
            switch (dbType)
            {
                case "PostgreSQL":
                {
                    options.UseNpgsql(configuration.GetConnectionString(dbType));
                    break;
                }
                case "SQLServer":
                {
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                    break;
                }
            }
        });

        services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();

        services.AddTransient<ITreeItemComponents, TreeItemComponents>();
        
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API Pax Dei",
                Version = "v1",
                Description = "Web API Pax Dei Items"
            });
        });
        return services;
    }
}