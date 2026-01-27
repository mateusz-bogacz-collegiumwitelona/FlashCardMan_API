namespace DTO.Response
{
    public class GetCardsForDeckResponse
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CardToken { get; set; }
    }
}
