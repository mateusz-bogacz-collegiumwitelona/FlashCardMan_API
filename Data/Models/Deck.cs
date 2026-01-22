using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class Deck
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string Description { get; set; } 
        public ICollection<FlashCards> FlashCards { get; set; } = new List<FlashCards>();
        public string Token { get; set; }
    }
}
