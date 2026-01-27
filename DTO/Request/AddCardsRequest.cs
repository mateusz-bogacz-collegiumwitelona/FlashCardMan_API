using System.ComponentModel.DataAnnotations;

namespace DTO.Request
{
    public class AddCardsRequest
    {
        [Required]
        public string Question { get; set; }
        [Required]
        public string Answer { get; set; }
    }
}
