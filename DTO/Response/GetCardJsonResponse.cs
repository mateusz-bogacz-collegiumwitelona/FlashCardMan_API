namespace DTO.Response
{
    public class GetCardJsonResponse
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public List<string>? Tags { get; set; } = new();
    }
}
