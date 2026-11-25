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

builder.Services.AddSingleton<IOrderStore, InMemoryOrderStore>();
builder.Services.AddSingleton<IAsyncOrderStore, AsyncOrderStoreAdapter>();

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
