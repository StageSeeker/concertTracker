using System.Net;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder; 
using StageSeeker.Controllers;
using StageSeeker.MiddleWare;
using StageSeeker.Models;
using StageSeeker.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"] ?? throw new Exception("Missing 'Domain' setting in Auth0 configuration");;
    options.ClientId = builder.Configuration["Auth0:ClientId"] ?? throw new Exception("Missing 'ClientId' setting in Auth0 configuration");;
    options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
    options.CallbackPath = new PathString("/callback");
});
builder.Services.AddControllersWithViews();
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

builder.Services.AddHttpsRedirection(options=>{
    options.HttpsPort = 7290;
});

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StageSeeker V1");
        // Require authentication for Swagger UI
        c.RoutePrefix = "swagger"; // Remove base path for easier integration
        app.UseMiddleware<SwaggerMiddleWare>();
    });
    app.UseHttpsRedirection();



app.MapControllers();
app.Run();

