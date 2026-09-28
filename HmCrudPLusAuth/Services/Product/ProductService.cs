using HmCrudPLusAuth.Common;
using HmCrudPLusAuth.Data;
using HmCrudPLusAuth.Models;

namespace HmCrudPLusAuth.Services.Product
{
    public class ProductService : IProductService
    {
        private readonly DataContext _db;

        public ProductService(DataContext db)
        {
            _db = db;
        }

        public Result<List<Models.Product>> GetAll(int userId)
        {
            var products = _db.Products
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .ToList();

            return Result<List<Models.Product>>.Ok(products);
        }

        public Result<Models.Product> GetById(int productId, int userId)
        {
            var product = _db.Products
                .FirstOrDefault(p => p.Id == productId && p.UserId == userId && !p.IsDeleted);

            if (product == null)
                return Result<Models.Product>.NotFound("Product not found");

            return Result<Models.Product>.Ok(product);
        }

        public Result<Models.Product> Create(string name, string description, double price, int userId)
        {
            if (string.IsNullOrWhiteSpace(name) || price <= 0)
                return Result<Models.Product>.BadRequest("Name and valid price are required");

            var product = new Models.Product
            {
                Name = name,
                Description = description,
                Price = price,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Products.Add(product);
            _db.SaveChanges();

            return Result<Models.Product>.Ok(product);
        }

        public Result<Models.Product> Update(int productId, string name, string description, double price, int userId)
        {
            if (string.IsNullOrWhiteSpace(name) || price <= 0)
                return Result<Models.Product>.BadRequest("Name and valid price are required");

            var product = _db.Products
                .FirstOrDefault(p => p.Id == productId && p.UserId == userId && !p.IsDeleted);

            if (product == null)
                return Result<Models.Product>.NotFound("Product not found");

            product.Name = name;
            product.Description = description;
            product.Price = price;
            product.UpdatedAt = DateTime.UtcNow;

            _db.SaveChanges();

            return Result<Models.Product>.Ok(product);
        }

        public Result<string> Delete(int productId, int userId)
        {
            var product = _db.Products
                .FirstOrDefault(p => p.Id == productId && p.UserId == userId && !p.IsDeleted);

            if (product == null)
                return Result<string>.NotFound("Product not found");

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;

            _db.SaveChanges();

            return Result<string>.Ok("Product deleted successfully");
        }
    }
}
