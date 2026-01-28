namespace Data.Models
{
    public class Tags
    {
        public Guid Id { get; set; }
        public string Tag { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Token { get; set; }
        public Guid? CardId { get; set; }
        public FlashCards? Card { get; set; }
    }
}