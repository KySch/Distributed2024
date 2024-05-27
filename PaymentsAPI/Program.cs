using PaymentsAPI.Models;
using PaymentsAPI.Services;
using Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("Database"));

builder.Services.Configure<GCPSettings>(
    builder.Configuration.GetSection("GCP"));

builder.Services.AddJwt(builder.Configuration);

builder.Services.AddTransient<IEncryptor, Encryptor>();

builder.Services.AddSingleton<PaymentsService>();

builder.Services.AddSingleton<PublisherService>();


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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