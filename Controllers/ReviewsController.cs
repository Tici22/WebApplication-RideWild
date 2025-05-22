using Adventure19.AuthModels;
using Adventure19.Models;
using Adventure19.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Adventure19.Dto;

namespace Adventure19.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly AuthDbContext _authContext;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IReviewService reviewService, AuthDbContext authContext, ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _authContext = authContext;
            _logger = logger;
        }

        // GET: api/reviews/product/{productId}
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<List<Review>>> GetReviewsForProduct(int productId)
        {
            if (productId <= 0)
            {
                return BadRequest(new { message = "ID Prodotto non valido." });
            }
            var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);
            return Ok(reviews);
        }

        // POST: api/reviews
        [HttpPost]
        [Authorize] // Richiede che l'utente sia autenticato
        public async Task<ActionResult<Review>> CreateReview([FromBody] ReviewDTO reviewInput)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { message = "ID Utente non valido o mancante nel token." });
            }

            // Ottieni i dati dell'utente per UserName
            var user = await _authContext.Users.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized(new { message = "Utente non trovato." });
            }

            var review = new Review
            {
                ProductId = reviewInput.ProductId,
                UserId = userId, // Già ottenuto e validato come int
                // UserName sarà popolato dal servizio o dalla proiezione in Get
                Rating = reviewInput.Rating,
                Comment = reviewInput.Comment,
                ReviewDate = DateTime.UtcNow
                // User (proprietà di navigazione) non è necessario impostarlo qui, UserId è sufficiente per la FK
            };

            var (createdReview, error) = await _reviewService.CreateReviewAsync(review, userId);

            if (error != null)
            {
                // Potrebbe essere Conflict (409) se l'utente ha già recensito o non può recensire
                // o Forbidden (403) se non ha acquistato
                if (error.Contains("acquistato") || error.Contains("già recensito"))
                {
                    return Conflict(new { message = error }); // O BadRequest
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = error });
            }

            if (createdReview == null) // Controllo aggiuntivo
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Errore imprevisto durante la creazione della recensione." });
            }

            // La recensione restituita da CreateReviewAsync ora include UserName
            return CreatedAtAction(nameof(GetReviewByIdPlaceholder), new { id = createdReview.ReviewId }, createdReview);
        }

        // Metodo placeholder per CreatedAtAction.
        [HttpGet("{id:int}", Name = "GetReviewByIdPlaceholder")] // Id ora è int
        [ApiExplorerSettings(IgnoreApi = true)]
        public ActionResult<Review> GetReviewByIdPlaceholder(int id)
        {
            return NotFound(new { message = "Endpoint GetReviewById non implementato completamente." });
        }
    }
}
