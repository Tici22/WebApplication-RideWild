using Adventure19.AuthModels;
using Adventure19.Dto;
using Adventure19.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Adventure19.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _oldContext; // Vecchio Db
        private readonly ILogger<OrderController> _logger; // Logger per il vecchio Db
        private readonly AuthDbContext _context; // Nuovo Db
        public OrderController(AdventureWorksLt2019Context oldContext, AuthDbContext context, ILogger<OrderController> logger)
        {
            _oldContext = oldContext;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Crea un nuovo ordine per un cliente specifico.
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateOrder(OrdersCreateDTO orders)
        {
            var order = new SalesOrderHeader
            {
                CustomerId = orders.CustomerId,
                OrderDate = DateTime.UtcNow,
                Status = 1, // Stato dell'ordine, ad esempio "In lavorazione"
                SubTotal = 0, // Prezzo totale prima delle tasse e delle spese di spedizione
                TaxAmt = 0, // Tasse calcolate sull'ordine
                Freight = 5, // Spese di spedizione Default 5 euro
                ModifiedDate = DateTime.UtcNow,
                SalesOrderNumber = $"SO-{Guid.NewGuid().ToString().Substring(0, 8)}",
                SalesOrderDetails = new List<SalesOrderDetail>()


            };

            decimal subTotal = 0;
            foreach (var item in orders.orderItems)
            {
                var lineTotal = item.UnitPrice * item.Quantity; // Calcolo del totale della riga (UnitPrice * Quantity)
                subTotal += lineTotal; // Calcolo del subtotale per l'ordine

                var orderDetail = new SalesOrderDetail
                {
                    ProductId = item.ProductId,
                    OrderQty = (short)item.Quantity, // Quantità dell'articolo
                    UnitPrice = item.UnitPrice, // Prezzo unitario dell'articolo
                    LineTotal = lineTotal, // Totale della riga
                    SalesOrder = order, // Associazione all'ordine
                    
                };
            }
            order.SubTotal = subTotal; // Imposta il subtotale dell'ordine
            order.TaxAmt = subTotal * 0.22m; // Calcolo delle tasse (22% del subtotale)
            order.TotalDue = order.SubTotal + order.TaxAmt + order.Freight; // Calcolo del totale dovuto (SubTotal + TaxAmt + Freight)

            _oldContext.SalesOrderHeaders.Add(order); // Aggiungi l'ordine al contesto del vecchio Db

            await _oldContext.SaveChangesAsync(); // Salva le modifiche al vecchio Db

            return Ok (new {message="Order Creato Con successo", orderId = order.SalesOrderId }); // Restituisci una risposta di successo con l'ID dell'ordine creato
        }
    }
}
