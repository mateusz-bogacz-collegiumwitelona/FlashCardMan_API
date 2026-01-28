namespace Data.Models
{
    public class FlashCardTag
    {
        public Guid FlashCardId { get; set; }
        public FlashCards FlashCard { get; set; }

        public Guid TagId { get; set; }
        public Tags Tag { get; set; }
    }
}
