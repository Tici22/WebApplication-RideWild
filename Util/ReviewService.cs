using Adventure19.AuthModels; 
using Adventure19.Models;   
using Adventure19.Util;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adventure19.Util
{
    public class ReviewService
    {
        private readonly AuthDbContext _authContext; // Per salvare e leggere recensioni
        private readonly AdventureWorksLt2019Context _adventureContext; // Per verificare gli acquisti

        public ReviewService(AuthDbContext authContext, AdventureWorksLt2019Context adventureContext)
        {
            _authContext = authContext;
            _adventureContext = adventureContext;
        }

        public async Task<List<Review>> GetReviewsByProductIdAsync(int productId)
        {
            return await _authContext.Reviews
                .Where(r => r.ProductId == productId)
                .Include(r => r.User) // Per ottenere UserName/Email da mostrare
                .OrderByDescending(r => r.ReviewDate)
                .Select(r => new Review // Proiezione per non esporre l'intera entità User se non necessario
                {
                    ReviewId = r.ReviewId,
                    ProductId = r.ProductId,
                    UserId = r.UserId,
                    UserName = r.User.FullName ?? r.User.Email, // Scegli cosa mostrare
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate
                })
                .ToListAsync();
        }

        public async Task<(Review? Review, string? Error)> CreateReviewAsync(Review review, int userId)
        {
            // 1. Verifica se l'utente ha acquistato il prodotto
            if (!await CanUserReviewProductAsync(userId, review.ProductId))
            {
                return (null, "Puoi recensire solo i prodotti che hai acquistato.");
            }

            // 2. Verifica se l'utente ha già recensito questo prodotto
            var existingReview = await _authContext.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == review.ProductId);

            if (existingReview != null)
            {
                return (null, "Hai già recensito questo prodotto.");
            }

            review.UserId = userId; // Assicura che l'UserId sia corretto
            // UserName sarà popolato dal controller o qui se hai accesso ai dati utente

            _authContext.Reviews.Add(review);
            await _authContext.SaveChangesAsync();

            // Ricarica la recensione con i dati utente per la risposta
            var createdReviewWithUser = await _authContext.Reviews
                .Where(r => r.ReviewId == review.ReviewId)
                .Include(r => r.User)
                .Select(r => new Review
                {
                    ReviewId = r.ReviewId,
                    ProductId = r.ProductId,
                    UserId = r.UserId,
                    UserName = r.User.FullName ?? r.User.Email,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate
                })
                .FirstOrDefaultAsync();

            return (createdReviewWithUser, null);
        }

        public async Task<bool> CanUserReviewProductAsync(int userId, int productId)
        {
            // Trova il CustomerID nel vecchio DB corrispondente all'UserId del nuovo DB.
            // Questo assume che l'Email sia l'identificatore comune.
            var userAuth = await _authContext.Users.FindAsync(userId);
            if (userAuth == null) return false; // Utente non trovato nel sistema di autenticazione

            var customer = await _adventureContext.Customers
                                 .FirstOrDefaultAsync(c => c.EmailAddress == userAuth.Email);
            if (customer == null) return false; // Cliente non trovato nel vecchio DB (nessun acquisto possibile)

            // Verifica se esiste un ordine per questo CustomerId che include il ProductId
            var hasPurchased = await _adventureContext.SalesOrderHeaders
                .Where(soh => soh.CustomerId == customer.CustomerId)
                .SelectMany(soh => soh.SalesOrderDetails) // Appiattisce gli ordini nei loro dettagli
                .AnyAsync(sod => sod.ProductId == productId);

            return hasPurchased;
        }
    }
}
