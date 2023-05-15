using System.Configuration;
using MiroslavGPT.Admin.API.Middlewares;
using MiroslavGPT.Admin.API.Settings;
using MiroslavGPT.Admin.Domain.Azure.Extensions;
using MiroslavGPT.Admin.Domain.Interfaces.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAzureAdminDomainServices();

var config = builder.Configuration;
var settings = new ApiSettings
{
    Secret = config["API_SECRET_KEY"],
    PersonalityDatabaseName = config["PERSONALITY_DATABASE_NAME"],
    PersonalityContainerName = config["PERSONALITY_CONTAINER_NAME"],
};

builder.Services.AddSingleton<IPersonalitySettings>(settings);
builder.Services.AddSingleton<IAuthSettings>(settings);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSingleKeyAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();