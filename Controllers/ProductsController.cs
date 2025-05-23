using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Adventure19.Models;
using Adventure19.Dto;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Adventure19.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(AdventureWorksLt2019Context context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Products?pageNumber=1&pageSize=20
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(int pageNumber = 1, int pageSize = 20)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                _logger.LogWarning("GetProducts: Invalid pagination parameters. pageNumber={PageNumber}, pageSize={PageSize}", pageNumber, pageSize);
                return BadRequest("pageNumber and pageSize must be greater than 0.");
            }

            var productsDto = await _context.Products
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    ProductNumber = p.ProductNumber,
                    Color = p.Color,
                    StandardCost = p.StandardCost,
                    ListPrice = p.ListPrice,
                    Size = p.Size,
                    Weight = p.Weight,
                    ProductCategoryId = p.ProductCategoryId,
                    ProductModelId = p.ProductModelId,
                    SellStartDate = p.SellStartDate,
                    SellEndDate = p.SellEndDate,
                    DiscontinuedDate = p.DiscontinuedDate,
                    Rowguid = p.Rowguid,
                    ModifiedDate = p.ModifiedDate
                })
                .ToListAsync();

            _logger.LogInformation("GetProducts: Returned {Count} products.", productsDto.Count);
            return Ok(productsDto);
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            _logger.LogInformation("GetProduct: Searching for product with ID {Id}", id);

            var productDto = await _context.Products
                .Where(p => p.ProductId == id)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    ProductNumber = p.ProductNumber,
                    Color = p.Color,
                    StandardCost = p.StandardCost,
                    ListPrice = p.ListPrice,
                    Size = p.Size,
                    Weight = p.Weight,
                    ProductCategoryId = p.ProductCategoryId,
                    ProductModelId = p.ProductModelId,
                    SellStartDate = p.SellStartDate,
                    SellEndDate = p.SellEndDate,
                    DiscontinuedDate = p.DiscontinuedDate,
                    Rowguid = p.Rowguid,
                    ModifiedDate = p.ModifiedDate
                })
                .FirstOrDefaultAsync();

            if (productDto == null)
            {
                _logger.LogWarning("GetProduct: Product with ID {Id} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("GetProduct: Product with ID {Id} retrieved successfully.", id);
            return Ok(productDto);
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.ProductId)
            {
                _logger.LogWarning("PutProduct: Mismatch between route ID {Id} and product ID {ProductId}", id, product.ProductId);
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("PutProduct: Product with ID {Id} updated successfully.", id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ProductExists(id))
                {
                    _logger.LogWarning("PutProduct: Product with ID {Id} not found during update.", id);
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "PutProduct: Concurrency error while updating product with ID {Id}", id);
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("PostProduct: Product with ID {Id} created successfully.", product.ProductId);

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("DeleteProduct: Product with ID {Id} not found.", id);
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("DeleteProduct: Product with ID {Id} deleted successfully.", id);

            return NoContent();
        }

        [HttpGet("by-category/{productCategory}")]
        public async Task<IActionResult> GetProductsForCategory(int productCategory)
        {
            _logger.LogInformation("GetProductsForCategory: Fetching products for category {CategoryId}", productCategory);

            var products = await _context.Products
                .Where(p => p.ProductCategory != null && p.ProductCategory.ParentProductCategoryId == productCategory)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    ProductNumber = p.ProductNumber,
                    Color = p.Color,
                    Size = p.Size,
                    StandardCost = p.StandardCost,
                    ListPrice = p.ListPrice,
                    ProductModelId = p.ProductModelId,
                    SellStartDate = p.SellStartDate,
                    SellEndDate = p.SellEndDate,
                    DiscontinuedDate = p.DiscontinuedDate,
                    ProductModelName = p.ProductModel != null ? p.ProductModel.Name : "Nessuna Categoria",
                }).ToListAsync();

            _logger.LogInformation("GetProductsForCategory: Returned {Count} products.", products.Count);

            return Ok(products);
        }

        [HttpGet("by-parent/{parentId}")]
        public async Task<IActionResult> GetByParentCategory(int parentId)
        {
            _logger.LogInformation("GetByParentCategory: Fetching child categories for parent ID {ParentId}", parentId);

            var result = await _context.ProductCategories
                .Where(p => p.ParentProductCategoryId == parentId)
                .Where(p => _context.Products.Any(s => s.ProductCategoryId == p.ProductCategoryId))
                .GroupBy(p => new { p.ParentProductCategoryId, p.Name })
                .Select(g => new
                {
                    g.Key.Name
                })
                .ToListAsync();

            _logger.LogInformation("GetByParentCategory: Found {Count} subcategories.", result.Count);

            return Ok(result);
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        //simulazione errore test

        [HttpGet("simulate-db-error")]
        public async Task<IActionResult> SimulateDbError()
        {
            var product = new Product
            {
                Name = "Test DB Error",
                ProductNumber = "1234-ERROR",
                StandardCost = 10,
                ListPrice = 20,
                SellStartDate = DateTime.UtcNow,
                ProductCategoryId = 99999, // categoria inesistente
                ModifiedDate = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(); // Questo dovrebbe fallire se 99999 non esiste come foreign key

            return Ok();
        }
    }
}
