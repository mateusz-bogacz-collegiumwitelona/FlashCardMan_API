namespace Data.Models
{
    public class FlashCards
    {
        public Guid Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid DeckId { get; set; }
        public Deck Deck { get; set; }
        public string Token { get; set; }

        public DateTime NextReviewAt { get; set; }

        public int Repetitions { get; set; }
        public double EasinessFactor { get; set; }
        public int IntervalDays { get; set; }

    }
}
