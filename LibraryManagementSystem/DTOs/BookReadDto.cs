namespace LibraryManagementSystem.DTOs
{
    public class BookReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime PublicationDate { get; set; }
        public string Genre { get; set; } = string.Empty;
        public List<AuthorReadDto> Authors { get; set; } = new List<AuthorReadDto>();
    }
}
