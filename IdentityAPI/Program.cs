using IdentityAPI.Models;
using IdentityAPI.Services;
using Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseSettings>(
	builder.Configuration.GetSection("Database"));

builder.Services.Configure<GCPSettings>(
	builder.Configuration.GetSection("GCP"));

builder.Services.AddJwt(builder.Configuration);

builder.Services.AddTransient<IEncryptor, Encryptor>();

builder.Services.AddSingleton<UserService>();

builder.Services.AddSingleton<NotificationsService>();

builder.Services.AddSingleton<PublisherService>();

builder.Services.AddHostedService<SubscriberService>();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors(policy => policy.WithOrigins("http://localhost:5255")
	.AllowAnyMethod()
	.AllowAnyHeader()
);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
