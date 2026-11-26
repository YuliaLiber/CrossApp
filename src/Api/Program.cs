using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Core.Abstractions;
using Core.Storage;
using Scalar.AspNetCore;
using Api.Endpoints;
using Api.Validation;
using Api.Contracts;
using FluentValidation;
using Api.Errors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<BadHttpRequestExceptionHandler>();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder.Services.AddScoped<
    IValidator<SaveOrderRequest>,
    SaveOrderRequestValidator>();



var connectionString = builder.Configuration.GetConnectionString("AppDb")
    ?? throw new InvalidOperationException("Не задано ConnectionStrings:AppDb");

var sqlite = new SqliteConnectionStringBuilder(connectionString);

if (!Path.IsPathRooted(sqlite.DataSource))
    sqlite.DataSource = Path.GetFullPath(
        Path.Combine(builder.Environment.ContentRootPath, sqlite.DataSource));

Directory.CreateDirectory(Path.GetDirectoryName(sqlite.DataSource)!);

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlite(sqlite.ConnectionString));

builder.Services.AddScoped<IAsyncOrderStore, EfOrderStore>();


var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health", () =>
    TypedResults.Ok(new { status = "ok" }))
    .WithName("Health")
    .WithTags("Service");

app.MapOrders();

app.Run();

public partial class Program { }
