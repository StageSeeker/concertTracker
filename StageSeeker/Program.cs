using StageSeeker.Controllers;
using StageSeeker.Models;
using StageSeeker.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();


// Add services to the container.
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDataBase"));
builder.Services.AddSingleton<UsersService>();
builder.Services.AddSingleton<WatchListService>();
builder.Services.AddSingleton<ConcertService>();

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

app.UseHttpsRedirection();


app.MapControllers();
app.Run();

