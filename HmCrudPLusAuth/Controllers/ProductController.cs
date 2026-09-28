using HmCrudPLusAuth.Common;
using HmCrudPLusAuth.Data;
using HmCrudPLusAuth.DTOs.Product;
using HmCrudPLusAuth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HmCrudPLusAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly DataContext _db;

        public ProductController(DataContext db)
        {
            _db = db;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var userId = GetUserId();
            var products = _db.Products
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .ToList();

            return Ok(Result<List<Product>>.Ok(products));
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var userId = GetUserId();
            var product = _db.Products
                .FirstOrDefault(p => p.Id == id && p.UserId == userId && !p.IsDeleted);

            if (product == null)
                return NotFound(Result<Product>.NotFound("Product not found"));

            return Ok(Result<Product>.Ok(product));
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateProductDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.Price <= 0)
                return BadRequest(Result<Product>.BadRequest("Name and valid price are required"));

            var userId = GetUserId();

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Products.Add(product);
            _db.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, Result<Product>.Ok(product));
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateProductDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.Price <= 0)
                return BadRequest(Result<Product>.BadRequest("Name and valid price are required"));

            var userId = GetUserId();
            var product = _db.Products
                .FirstOrDefault(p => p.Id == id && p.UserId == userId && !p.IsDeleted);

            if (product == null)
                return NotFound(Result<Product>.NotFound("Product not found"));

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.UpdatedAt = DateTime.UtcNow;

            _db.SaveChanges();

            return Ok(Result<Product>.Ok(product));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var userId = GetUserId();
            var product = _db.Products
                .FirstOrDefault(p => p.Id == id && p.UserId == userId && !p.IsDeleted);

            if (product == null)
                return NotFound(Result<string>.NotFound("Product not found"));

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;

            _db.SaveChanges();

            return Ok(Result<string>.Ok("Product deleted successfully"));
        }
    }
}
