using WatchListsAPI.Models;
using WatchListsAPI.Services;
using Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("Database"));
builder.Services.Configure<GCPSettings>(
    builder.Configuration.GetSection("GCP"));

builder.Services.AddJwt(builder.Configuration);

builder.Services.AddTransient<IEncryptor, Encryptor>();

builder.Services.AddSingleton<WatchlistsService>();

builder.Services.AddSingleton<PublisherService>();

builder.Services.AddHostedService<SubscriberService>();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins(
                "http://localhost:2070",
                "http://localhost:5255" // Add other origins if needed
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply the CORS policy
app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
