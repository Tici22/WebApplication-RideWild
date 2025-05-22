using System.ComponentModel.DataAnnotations;

namespace Adventure19.Dto
{
    public class ReviewDTO
    {
        [Required(ErrorMessage = "L'ID del prodotto è obbligatorio.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Il voto è obbligatorio.")]
        [Range(1, 5, ErrorMessage = "Il voto deve essere compreso tra 1 e 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Il commento è obbligatorio.")]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Il commento deve essere tra 5 e 1000 caratteri.")]
        public string Comment { get; set; } = null!;
    }
}
