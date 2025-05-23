using Adventure19.AuthModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Adventure19.Models
{
    public class Review
    {
            [Key]
            public int ReviewId { get; set; }

            [Required]
            public int ProductId { get; set; } // FK verso la tabella Product in AdventureWorksLT2019Context

            [Required]
            public int UserId { get; set; } // FK verso la tabella User in AuthDbContext

            public string UserName { get; set; } = null!;

            [Required]
            [Range(1, 5)]
            public int Rating { get; set; }

            [Required]
            [StringLength(1000, MinimumLength = 5)]
            public string Comment { get; set; } = null!;

            public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

            [ForeignKey("UserId")]
            public virtual User User { get; set; } = null!;

        
    }
}
