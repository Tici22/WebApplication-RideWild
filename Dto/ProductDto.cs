using Adventure19.Models;

namespace Adventure19.Dto
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string ProductNumber { get; set; } = null!;
        public string? Color { get; set; }
        public decimal StandardCost { get; set; }
        public decimal ListPrice { get; set; }
        public string? Size { get; set; }
        public decimal? Weight { get; set; }
        public int? ProductCategoryId { get; set; }
        public int? ProductModelId { get; set; }
        public string ? ProductModelName { get; set; }
        public DateTime SellStartDate { get; set; }
        public DateTime? SellEndDate { get; set; }
        public DateTime? DiscontinuedDate { get; set; }

        public Guid Rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }
        public decimal effectivePrice { get; set; }
        public List<ProductCategory>? ProductCategories { get; set; }

    public List<SalesOrderDetailsDTO> SalesOrderDetails { get; set; }

    }

}
