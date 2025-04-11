using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Adventure19.Models;
using System.ComponentModel.DataAnnotations;

namespace Adventure19.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;

        public CustomersController(AdventureWorksLt2019Context context)
        {
            _context = context;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers.Include(c => c.CustomerAddresses)
                    .ThenInclude(ca => ca.Address).ToListAsync();
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers.Include(c => c.CustomerAddresses).ThenInclude(ca => ca.Address)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, CustomerUpdateModel customerUpdate)
        {
            if (id != customerUpdate.CustomerId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }

            var customerToUpdate = await _context.Customers.Include(c => c.CustomerAddresses).FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customerToUpdate == null)
            {
                return NotFound();
            }

            customerToUpdate.NameStyle = customerUpdate.NameStyle;
            customerToUpdate.Title = customerUpdate.Title;
            customerToUpdate.FirstName = customerUpdate.FirstName;
            customerToUpdate.MiddleName = customerUpdate.MiddleName;
            customerToUpdate.LastName = customerUpdate.LastName;
            customerToUpdate.Suffix = customerUpdate.Suffix;
            customerToUpdate.CompanyName = customerUpdate.CompanyName;
            customerToUpdate.SalesPerson = customerUpdate.SalesPerson;
            customerToUpdate.EmailAddress = customerUpdate.EmailAddress;
            customerToUpdate.Phone = customerUpdate.Phone;
            customerToUpdate.PasswordHash = customerUpdate.PasswordHash;
            customerToUpdate.PasswordSalt = customerUpdate.PasswordSalt;
            customerToUpdate.ModifiedDate = DateTime.Now;

            if (customerUpdate.AddressIds != null)
            {
                customerToUpdate.CustomerAddresses = customerToUpdate.CustomerAddresses.Where(ca => customerUpdate.AddressIds.Contains(ca.AddressId)).ToList();

                foreach (var addressId in customerUpdate.AddressIds)          
                {
                    if (!customerToUpdate.CustomerAddresses.Any(ca => ca.AddressId == addressId))
                    {
                        var address = await _context.Addresses.FindAsync(addressId);
                        if (address != null)
                        {
                            customerToUpdate.CustomerAddresses.Add(new CustomerAddress
                            {
                                AddressId = addressId,
                                AddressType = "Main",
                                Rowguid = Guid.NewGuid(),
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }
                }
            }
            else
            {
                customerToUpdate.CustomerAddresses.Clear();
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
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

        // POST: api/Customers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer(CustomerCreateModel customerCreate)
        {
            var customer = new Customer
            {
                NameStyle = customerCreate.NameStyle,
                Title = customerCreate.Title,
                FirstName = customerCreate.FirstName,
                MiddleName = customerCreate.MiddleName,
                LastName = customerCreate.LastName,
                Suffix = customerCreate.Suffix,
                CompanyName = customerCreate.CompanyName,
                SalesPerson = customerCreate.SalesPerson,
                EmailAddress = customerCreate.EmailAddress,
                Phone = customerCreate.Phone,
                PasswordHash = customerCreate.PasswordHash,
                PasswordSalt = customerCreate.PasswordSalt,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.Now,
                CustomerAddresses = new List<CustomerAddress>()
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(); 

            if (customerCreate.AddressIds != null)
            {
                foreach (var addressId in customerCreate.AddressIds)
                {
                    var address = await _context.Addresses.FindAsync(addressId);
                    if (address != null)
                    {
                        customer.CustomerAddresses.Add(new CustomerAddress 
                        {
                            CustomerId = customer.CustomerId, 
                            AddressId = addressId,
                            AddressType = "Main", 
                            Rowguid = Guid.NewGuid(),
                            ModifiedDate = DateTime.Now
                        });
                    }
                    else
                    {
                        return BadRequest($"Address with ID {addressId} not found.");
                    }
                }
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, customer);
        }

        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers
        .Include(c => c.CustomerAddresses)
        .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
            {
                return NotFound(); 
            }

            customer.CustomerAddresses.Clear(); 
            _context.Customers.Remove(customer); 
            await _context.SaveChangesAsync();   

            return NoContent(); 
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }

        public class CustomerCreateModel
        {
            public bool NameStyle { get; set; }
            [MaxLength(8)]
            public string? Title { get; set; }
            [MaxLength(50)]
            public string FirstName { get; set; } = null!;
            [MaxLength(50)]
            public string? MiddleName { get; set; }
            [MaxLength(50)]
            public string LastName { get; set; } = null!;
            [MaxLength(10)]
            public string? Suffix { get; set; }
            [MaxLength(128)]
            public string? CompanyName { get; set; }
            [MaxLength(256)]
            public string? SalesPerson { get; set; }
            [MaxLength(50)]
            public string? EmailAddress { get; set; }
            [MaxLength(25)]
            public string? Phone { get; set; }
            [MaxLength(128)]
            public string PasswordHash { get; set; } = null!;
            [MaxLength(10)]
            public string PasswordSalt { get; set; } = null!;
            public List<int>? AddressIds { get; set; }
        }

        public class CustomerUpdateModel
        {
            public int CustomerId { get; set; }
            public bool NameStyle { get; set; }
            [MaxLength(8)]
            public string? Title { get; set; }
            [MaxLength(50)]
            public string FirstName { get; set; } = null!;
            [MaxLength(50)]
            public string? MiddleName { get; set; }
            [MaxLength(50)]
            public string LastName { get; set; } = null!;
            [MaxLength(10)]
            public string? Suffix { get; set; }
            [MaxLength(128)]
            public string? CompanyName { get; set; }
            [MaxLength(256)]
            public string? SalesPerson { get; set; }
            [MaxLength(50)]
            public string? EmailAddress { get; set; }
            [MaxLength(25)]
            public string? Phone { get; set; }
            [MaxLength(128)]
            public string PasswordHash { get; set; } = null!;
            [MaxLength(10)]
            public string PasswordSalt { get; set; } = null!;
            public List<int>? AddressIds { get; set; }
        }
    }
}

