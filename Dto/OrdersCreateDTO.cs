namespace Adventure19.Dto
{
    public class OrdersCreateDTO
    {
        public int CustomerId { get; set; } // ID per il cliente
        public List<OrderItemCreateDTO> orderItems { get; set; } // Lista degli articoli dell'ordine
    }



    public class OrderItemCreateDTO
    {
        public int ProductId { get; set; } // ID del prodotto
        public int Quantity { get; set; } // Quantità del prodotto nell'ordine
        public decimal UnitPrice { get; set; } //Prezzo Unitario
    }



    public class OrderDTO
    {
        public int OrderId { get; set; } // ID dell'ordine
        public int CustomerId { get; set; } // ID del cliente
        public DateTime OrderDate { get; set; } // Data dell'ordine
        public int Quantity { get; set; } // Quantità totale degli articoli nell'ordine
        public decimal UnitedPrice { get; set; } // Prezzo unitario degli articoli
        public decimal LineTotal { get; set; } // Prezzo totale dell'ordine
    }
}
