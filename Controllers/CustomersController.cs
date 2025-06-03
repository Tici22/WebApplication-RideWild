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
using Adventure19.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Globalization;
using Adventure19.Dto;

namespace Adventure19.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _oldcontext; // Vecchio Db
        private readonly AuthDbContext _context; // Nuovo Db
        private readonly JwtService _jwtService;

        public CustomersController(AdventureWorksLt2019Context oldcontext, AuthDbContext context, JwtService jwtService)
        {
            _oldcontext = oldcontext;
            _context = context;
            _jwtService = jwtService;
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

        [HttpGet("new/{id}")]
        public async Task<ActionResult<User>> GetUserId (int id)
        {
            try
            {
                var customer = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

                if (customer == null)
                    return NotFound("Utente not Found");
                return customer;
            }catch(Exception e)
            {
                return BadRequest($"Errore durante il recupero dell'utente: {e.Message}"); // Gestione degli errori generici

            }
           
        }

        
        [HttpPut("modify/{id}")]
        [Authorize]
        public async Task<IActionResult> ModifyCustomer(int id, UpdateCredentialDTO dto)
        {
            try
            {
                // Controllo di coerenza ID
                if (id != dto.Id)
                    return BadRequest("ID utente non valido.");

                // 1. Trova l'utente nel nuovo DB
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound(); // Utente non trovato:contentReference[oaicite:6]{index=6}

                // 2. Aggiorna FullName e Email nel nuovo DB
                user.FullName = dto.FullName;
                
                // EF Core segue le modifiche, non serve chiamare Update esplicitamente per entità tracciate

                // 3. Trova il cliente nel DB vecchio usando la nuova email
                var customer = await _oldcontext.Customers
                    .FirstOrDefaultAsync(c => c.EmailAddress == dto.Email);
                if (customer != null)
                {
                    var nameParts = dto.FullName.Split(' ', 2);
                    customer.FirstName = nameParts[0];
                    customer.LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

                    // 4. Aggiorna CompanyName e Phone se forniti
                    if (!string.IsNullOrEmpty(dto.CompanyName))
                        customer.CompanyName = dto.CompanyName;
                    if (!string.IsNullOrEmpty(dto.Phone))
                        customer.Phone = dto.Phone;
                    customer.ModifiedDate = DateTime.UtcNow;
                }

                // 5. Salva le modifiche su entrambi i contesti
                await _context.SaveChangesAsync();
                await _oldcontext.SaveChangesAsync();

                return Ok(new {message = "dati dell'utente aggiornati con successo" 
                ,customer
                }); // Operazione completata con successo:contentReference[oaicite:7]{index=7}
            }
            catch (Exception ex)
            {
                // Gestione degli errori generici
                return BadRequest($"Errore durante l'aggiornamento: {ex.Message}");
            }
        }

        // POST : api/Customers/login
        /// Login dell'utente:
        /// 1. Controlla se l'utente esiste nel nuovo DB (AuthDbContext). Se sì, verifica la password.
        /// 2. Se non esiste nel nuovo DB, controlla nel vecchio DB (AdventureWorksLt2019Context).
        /// 3. Se esiste nel vecchio DB e non è migrato, indica ad angular di avviare un set nuova password.
        /// 4. Se esiste nel vecchio DB ed è flaggato come migrato, ma non è nel nuovo DB: Conflict: error 409.

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            // MODIFICA: Validazione base per i parametri di input
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return BadRequest(new { message = "Email e password sono obbligatori." });
            }

            try
            {

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user != null)
                {
                    if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                    {
                        return Unauthorized(new { message = "Credenziali non valide." });
                    }
                    var token = _jwtService.GenerateToken(user.Id.ToString(), user.Email);
                    var fullname = user.FullName;
                    return Ok(new
                    {
                        message = "Accesso riuscito.",
                        token,
                        fullname
                    });
                }

                var oldCustomer = await _oldcontext.Customers.FirstOrDefaultAsync(c => c.EmailAddress == email);

                if (oldCustomer == null)
                {
                    return Unauthorized(new { message = "Credenziali non valide." });
                }

                if (!oldCustomer.IsMigrated)
                {
                    // MODIFICA: L'utente deve impostare una nuova password (migrazione forzata).
                    // Restituisce Conflict (409) per indicare al frontend un'azione specifica.
                    return Conflict(new
                    {
                        message = "Il tuo account necessita di un aggiornamento. Per favore, imposta una nuova password.",
                        action = "migrate_password" // Codice da fare frontend
                    });
                }
                else // oldCustomer.IsMigrated == true, MA user == null (non c'è nel nuovo DB) non serve altro if. 
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new
                    {
                        message = "Si è verificato un errore durante il tentativo di accesso. L'account potrebbe non essere stato migrato correttamente. Contattare l'assistenza.",
                        code = "MIGRATION_INCOMPLETE"
                    });
                }
                // MODIFICA: Rimosso 'newUser' perché la logica era errata, la migrazione deve avvenire tramite un set nuova password .
            }
            catch (Exception e)
            {
                Console.WriteLine($"Errore imprevisto Login: {e.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Errore interno del server durante il login. Riprova più tardi." });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromQuery] string email, [FromQuery] string password, [FromQuery] string fullname, [FromQuery] string date)
        {
            try
            {
                //Conversione della data in DateOnly
                if (!DateOnly.TryParse(date, out var parsedDate))
                {
                    return BadRequest("Formato data non valido. Usa yyyy-MM-dd.");
                }




                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                // Chek nel AUthDb
                if (user != null)
                {
                    return Conflict("Utente già esistente.");

                }
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                var newUser = new User
                {
                    FullName = fullname,
                    Email = email,
                    Password = hashedPassword,
                    DateBirth = parsedDate
                };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync(); // Salvataggio

                // Sincronizza con il vecchio DB
                var nameParts = fullname.Split(' ', 2);
                string firstName = nameParts[0];
                string lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

                var existOldCustomer = await _oldcontext.Customers.FirstOrDefaultAsync(c => c.EmailAddress == email);
                if (existOldCustomer == null)
                {
                    var newCustomer = new Customer
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        EmailAddress = email,
                        PasswordHash = hashedPassword,
                        IsMigrated = true, // Flag per indicare che è migrato
                        NameStyle = false,
                        ModifiedDate = DateTime.Now

                    };
                    _oldcontext.Customers.Add(newCustomer);
                }
                else
                {
                    existOldCustomer.IsMigrated = true; // Flag per indicare che è migrato
                    existOldCustomer.PasswordHash = hashedPassword;
                    existOldCustomer.ModifiedDate = DateTime.Now;
                    _oldcontext.Customers.Update(existOldCustomer);
                }
                await _oldcontext.SaveChangesAsync(); // Salvataggio nel vecchio DB



                return Ok(new { message = "Registrazione riuscita." });


            }
            catch (Exception e)
            {
                return BadRequest(e.InnerException?.Message ?? e.Message);
            }
        }


        /// <summary>
        /// aggiorna le credenziali del cliente.
        /// 
        /// </summary>
        /// 
        /// <returns></returns>
        





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


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(string email, string newPassword, string? currentPassword = null)
        {
            //Cerca prima nel nuovo DB =>utente già migrato 
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                // Se è currentPassword è null
                if (string.IsNullOrEmpty(currentPassword))
                    return BadRequest("Password attuale richiesta per il cambio password.");

                if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Password))
                    return Unauthorized("Password attuale errata.");

                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok("Password aggiornata con successo.");
            }

            //  cerca nel vecchio DB
            var oldCustomer = await _oldcontext.Customers.FirstOrDefaultAsync(c => c.EmailAddress == email);

            if (oldCustomer == null)
                return NotFound("Utente non trovato.");

            if (oldCustomer.IsMigrated)
                return Conflict("Utente già migrato, usare cambio password."); // sicurezza extra

            // Migrazione con nuova password
            var newUser = new User
            {
                FullName = $"{oldCustomer.FirstName} {oldCustomer.LastName}",
                Email = oldCustomer.EmailAddress!,
                Password = BCrypt.Net.BCrypt.HashPassword(newPassword)
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            //  Aggiorna flag nel vecchio DB
            oldCustomer.IsMigrated = true;
            _oldcontext.Customers.Update(oldCustomer);
            await _oldcontext.SaveChangesAsync();

            return Ok("Password impostata e utente migrato con successo.");
        }

        [HttpPost("forgotted-password")]
        public async Task<IActionResult> ForgotPassword([FromServices] EmailService emailService, string email)
        {
            try
            {
                // Verifica se esiste nei DB
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                var oldCustomer = await _oldcontext.Customers.FirstOrDefaultAsync(c => c.EmailAddress == email);

                if (user == null && oldCustomer == null)
                    return NotFound("Utente non trovato.");

                // Genera codice 8 cifre
                var random = new Random();
                var code = random.Next(10000000, 99999999).ToString();


                // Salva codice temporaneo
                PasswordResetStore.ResetCodes[email] = (code, DateTime.UtcNow);

                // Invia email
                await emailService.SendEmailAsync(email, "Codice recupero password", $"Il tuo codice di verifica è: {code}");

                return Ok("Codice di verifica inviato via email.");
            }
            catch (Exception e)
            {
                return BadRequest($"Errore => {e.Message} \n dettagli {e.StackTrace}");

            }
        }


        [HttpPost("verify-code-and-reset")]
        public async Task<IActionResult> VerifyCodeAndReset(string email, string code, string newPassword, string fullName = "")
        {
            if (!PasswordResetStore.ResetCodes.ContainsKey(email))
                return NotFound("Nessuna richiesta di reset trovata per questa email.");

            var (storedCode, createdAt) = PasswordResetStore.ResetCodes[email];
            Console.WriteLine(createdAt);

            if (storedCode != code)
                return Unauthorized("Codice non valido.");
            // Verifica scadenza del codice (2 minuti)
            if (DateTime.UtcNow - createdAt > TimeSpan.FromMinutes(1))
            {
                PasswordResetStore.ResetCodes.Remove(email);
                return Unauthorized("Codice scaduto. Richiedi un nuovo reset.");
            }

            PasswordResetStore.ResetCodes.Remove(email); // rimuovi codice usato

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return Ok("Password aggiornata con successo.");
            }

            var oldCustomer = await _oldcontext.Customers.FirstOrDefaultAsync(c => c.EmailAddress == email);

            if (oldCustomer == null)
                return NotFound("Utente non trovato.");

            if (oldCustomer.IsMigrated)
                return Conflict("Utente già migrato. Riprovare login/reset nel nuovo sistema.");

            // Migrazione con nuova password
            var newUser = new User
            {
                FullName = string.IsNullOrWhiteSpace(fullName) ? $"{oldCustomer.FirstName} {oldCustomer.LastName}" : fullName,
                Email = oldCustomer.EmailAddress!,
                Password = BCrypt.Net.BCrypt.HashPassword(newPassword)
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            oldCustomer.IsMigrated = true;
            _oldcontext.Customers.Update(oldCustomer);
            await _oldcontext.SaveChangesAsync();

            return Ok("Password aggiornata e utente migrato con successo.");
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
