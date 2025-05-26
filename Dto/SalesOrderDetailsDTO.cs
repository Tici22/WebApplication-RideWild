namespace Adventure19.Dto
{
    public class SalesOrderDetailsDTO
    {
        public int SalesOrderId { get; set; }
        public int SalesOrderDetailId { get; set; }
        public int OrderQty { get; set; }
        public int ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPriceDiscount { get; set; }
        public decimal LineTotal { get; set; }

    }
}
