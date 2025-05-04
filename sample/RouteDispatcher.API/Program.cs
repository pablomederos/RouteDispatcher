using RouteDispatcher.Extensions;
using RouteDispatcher.API;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RouteDispatcher API",
        Version = "v1",
        Description = "API para la gestión de Route Dispatcher",
        Contact = new OpenApiContact
        {
            Name = "Soporte",
            Email = "soporte@routedispatcher.com"
        }
    });
});

builder.Services.AddRouteDispatcher();

builder.Services.AddTransient<MessageService>();

var app = builder.Build();

// Enable Swagger and Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RouteDispatcher API v1");
    c.RoutePrefix = string.Empty; // Para que Swagger UI se muestre en la raíz
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Message}/{action=Get}");

app.MapControllers();

app.Run();

app.Run();