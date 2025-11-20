using Microsoft.EntityFrameworkCore;
using ProductsManagement.Features.Products;

namespace ProductsManagement.Persistence;

public class ProductManagementContext(DbContextOptions<ProductManagementContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}