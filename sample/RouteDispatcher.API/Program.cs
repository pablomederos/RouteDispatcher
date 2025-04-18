using RouteDispatcher.Extensions;
using RouteDispatcher.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddRouteDispatcher();
builder.Services.AddTransient<MessageService>();

var app = builder.Build();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Message}/{action=Get}");

app.MapControllers();

app.Run();