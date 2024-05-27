using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(); // Add CORS services
builder.Services.AddEndpointsApiExplorer();

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors(policy => policy.WithOrigins("http://localhost:5255")
    .AllowAnyMethod()
    .AllowAnyHeader()
);

app.UseAuthorization();

app.MapControllers();

await app.UseOcelot();

app.Run();
