using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Adventure19.Models;
using System.ComponentModel.DataAnnotations;
using Adventure19.AuthModels;
using Microsoft.CodeAnalysis.Scripting;

namespace Adventure19.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _oldcontext; // Vecchio Db
        private readonly AuthDbContext _context; // Nuovo Db
        public CustomersController(AdventureWorksLt2019Context oldcontext,AuthDbContext context)
        {
            _oldcontext = oldcontext;
            _context = context;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _oldcontext.Customers.Include(c => c.CustomerAddresses)
                    .ThenInclude(ca => ca.Address).ToListAsync();
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _oldcontext.Customers.Include(c => c.CustomerAddresses).ThenInclude(ca => ca.Address)
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

            var customerToUpdate = await _oldcontext.Customers.Include(c => c.CustomerAddresses).FirstOrDefaultAsync(c => c.CustomerId == id);

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
                        var address = await _oldcontext.Addresses.FindAsync(addressId);
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
                await _oldcontext.SaveChangesAsync();
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

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            // 1. Verifica se esiste nel nuovo DB 
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                // Verifica la password con BCrypt
                if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                    return Unauthorized("Password errata.");

                return Ok("Accesso riuscito (già migrato).");
            }

            // 2. Verifica se esiste nel vecchio DB (AdventureDb)
            var oldCustomer = await _oldcontext.Customers.FirstOrDefaultAsync(c => c.EmailAddress == email);

            if (oldCustomer == null)
                return NotFound("Utente non trovato in nessun database.");

            if (!BCrypt.Net.BCrypt.Verify(password, oldCustomer.PasswordHash))
                return Unauthorized("Password errata nel vecchio database.");

            // 3. Salvataggio nel nuovo DB (migrazione)
            var newUser = new User
            {
                FullName = $"{oldCustomer.FirstName} {oldCustomer.LastName}",
                Email = oldCustomer.EmailAddress!,
                Password = oldCustomer.PasswordHash 
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // 4. Aggiornamento flag nel vecchio DB
            oldCustomer.IsMigrated = true;
            _oldcontext.Customers.Update(oldCustomer);
            await _oldcontext.SaveChangesAsync();

            return Ok("Accesso riuscito e utente migrato nel nuovo sistema.");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(string email, string password, string fullName)
        {
            try
            {
                // 1. Controllo nel nuovo DB
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                return Conflict("Email già registrata nel nuovo database.");
            }

            // 2. Controllo nel vecchio DB
            var existingOldCustomer = await _oldcontext.Customers.FirstOrDefaultAsync(c => c.EmailAddress == email);
            if (existingOldCustomer != null)
            {
                return Conflict("Email già esistente nel vecchio sistema. Effettua il login per migrare.");
            }

            // 3. Hash e salt della password
            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password + salt);

            // 4. Salva nel nuovo DB (Users)
            var newUser = new User
            {
                Email = email,
                Password = hashedPassword,
                FullName = fullName
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // 5. Salva nel vecchio DB (Customers)
            var nameParts = fullName.Split(' ', 2);
            string firstName = nameParts.Length > 0 ? nameParts[0] : fullName;
            string lastName = nameParts.Length > 1 ? nameParts[1] : "";

            var newCustomer = new Customer
            {
                FirstName = firstName,
                LastName = lastName,
                EmailAddress = email,
                ModifiedDate = DateTime.UtcNow,
                
                IsMigrated = true, // oppure false se preferisci segnare che è nato dal nuovo sistema
                NameStyle = false // imposta un default
                                  // Aggiungi altri campi obbligatori con valori default o logici
            };

            _oldcontext.Customers.Add(newCustomer);
            await _oldcontext.SaveChangesAsync();

            return Ok("Registrazione completata con successo e sincronizzata.");
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
            
        }






        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _oldcontext.Customers
        .Include(c => c.CustomerAddresses)
        .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
            {
                return NotFound(); 
            }

            customer.CustomerAddresses.Clear(); 
            _oldcontext.Customers.Remove(customer); 
            await _oldcontext.SaveChangesAsync();   

            return NoContent(); 
        }

        private bool CustomerExists(int id)
        {
            return _oldcontext.Customers.Any(e => e.CustomerId == id);
        }


        #region Models Che non usiamo
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

        #endregion
    }
}

