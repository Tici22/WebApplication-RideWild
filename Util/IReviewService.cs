using System.Collections.Generic;
using System.Threading.Tasks;
using Adventure19.Models;

namespace Adventure19.Util
{
    public interface IReviewService
    {
        Task<List<Review>> GetReviewsByProductIdAsync(int productId);
        Task<(Review? Review, string? Error)> CreateReviewAsync(Review review, int userId);
        Task<bool> CanUserReviewProductAsync(int userId, int productId);
    }
}
