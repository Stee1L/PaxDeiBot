using PaxDeiBot.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options => options.AddPolicy("Any", policyBuilder =>
{
    policyBuilder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

builder.Services.AddDbContext<ItemDbContext>(options =>
{
    var dbType = builder.Configuration["CurrentDatabaseConnectionString"];
    switch (dbType)
    {
        case "PostgreSQL":
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString(dbType));
            break;
        }
        case "SQLServer":
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            break;
        }
    }
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Pax Dei",
        Version = "v1",
        Description = "Web API Pax Dei Items"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("Any");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "Items WebApi v1");
});

app.UseStaticFiles();

app.UseRouting();
//
// app.UseAuthentication();
// app.UseAuthorization();
//
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();