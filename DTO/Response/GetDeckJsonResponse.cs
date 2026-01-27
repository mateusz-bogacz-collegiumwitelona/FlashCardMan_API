namespace DTO.Response
{
    public class GetDeckJsonResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<GetCardJsonResponse> Cards { get; set; }
    }
}
