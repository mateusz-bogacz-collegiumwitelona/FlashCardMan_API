using System.ComponentModel.DataAnnotations;

namespace DTO.Request
{
    public class ReviewCardRequest
    {
        [Required]
        public string CardToken { get; set; }

        [Range(0, 5, ErrorMessage = "Grade must be between 0 and 5.")]
        public int Grade { get; set; }
    }
}
