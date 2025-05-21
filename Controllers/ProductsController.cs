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

        public ProductsController(AdventureWorksLt2019Context context)
        {
            _context = context;
        }

        // GET: api/Products?pageNumber=1&pageSize=20
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(int pageNumber = 1, int pageSize = 20)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
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

            return Ok(productsDto); 
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
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
                return NotFound(); 
            }

            return Ok(productDto); 
        }


        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("by-category/{productCategory}")]
        public async Task<IActionResult> GetProductsForCategory(int productCategory)
        {
            var products = await _context.Products
                .Where(p => p.ProductCategory != null && p.ProductCategory.ParentProductCategoryId == productCategory)
                .Select(p => new ProductDto
                {
                    Name = p.Name

                }).ToListAsync();

            return Ok(products);
        }


        [HttpGet("by-parent/{parentId}")]
        public async Task<IActionResult> GetByParentCategory(int parentId)
        {
            var result = await _context.ProductCategories
                .Where(p => p.ParentProductCategoryId == parentId) // Fix CS1061: Correct property name
                .Where(p => _context.Products
                            .Any(s => s.ProductCategoryId == p.ProductCategoryId)) // Fix CS8602: Check for null reference
                .GroupBy(p => new { p.ParentProductCategoryId, p.Name }) // Fix CS1061: Correct property name
                .Select(g => new
                {
                    
                    g.Key.Name
                })
                .ToListAsync();

            return Ok(result);
        }
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }


    }
}
