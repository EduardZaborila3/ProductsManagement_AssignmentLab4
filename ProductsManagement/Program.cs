using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductsManagement.Common.Middleware;
using ProductsManagement.Features.Products;
using ProductsManagement.Persistence;
using ProductsManagement.Validators;
using ProductsManagement.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProductManagementContext>(options =>
    options.UseInMemoryDatabase("ProductDb"));

builder.Services.AddMemoryCache();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddValidatorsFromAssemblyContaining<CreateProductProfileValidator>();

builder.Services.AddScoped<CreateProductHandler>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseMiddleware<CorrelationMiddleware>();

app.UseHttpsRedirection();

app.MapPost("/products", async (
    CreateProductProfileRequest request, 
    CreateProductHandler handler) =>
{
    return await handler.Handle(request);
})
.WithName("CreateProduct")
    .WithTags("Products");

app.Run();