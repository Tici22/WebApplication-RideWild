using Adventure19.Models;

namespace Adventure19.Dto
{
    public class CustomerDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateOnly BirthDate { get; set; }
        public string? CompanyName { get; set; }
        public string? SalesPerson { get; set; }
       
        public string? Phone { get; set; }
        public Guid? Rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsMigrated { get; set; }


        List<Address>? addresses { get; set; } //Address per prendere gli indirizzi
    }
}
