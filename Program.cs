using SHLAPI.Database;
using SHLAPI.Middlewares;
using SHLAPI.Utilities;
using FluentValidation.AspNetCore;
using Mapster;
using Serilog;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;
var builder = WebApplication.CreateBuilder(args);

IFileProvider fileProvider = builder.Environment.ContentRootFileProvider;
builder.Configuration.AddEnvironmentVariables();
TypeAdapterConfig.GlobalSettings.Default.ShallowCopyForSameType(true);
TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(options =>
// {
//     options.CustomSchemaIds(type => type.FullName);
// });
builder.Services.AddControllers();
builder.Services.AddResponseCompression();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IShamelDatabase, ShamelDatabase>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("*")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors("AllowReactApp");
// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

Log.Logger = new LoggerConfiguration().MinimumLevel
    .Information()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine("Log", "log.txt"), rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
    .CreateLogger();

app.UseMiddleware<LoggingMiddleware>();
//app.UseMiddleware<HaspMiddleware>();

// TODO Authentication Middleware
// app.UseCors(builder =>
// {
//     builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetPreflightMaxAge(TimeSpan.FromSeconds(86400));
// });
app.MapControllers();
app.UseMiddleware<AuthenticationMiddleware>();

app.Run();
