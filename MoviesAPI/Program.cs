using FootballAPI.Models;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<MovieContext>(opt =>opt.UseInMemoryDatabase("MovieList"));
var app = builder.Build();
// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();