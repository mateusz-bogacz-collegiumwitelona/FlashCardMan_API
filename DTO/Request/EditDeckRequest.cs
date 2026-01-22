namespace DTO.Request
{
    public class EditDeckRequest
    {
        public string Token { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
