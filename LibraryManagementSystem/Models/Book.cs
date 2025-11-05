using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Publication date is required")]
        public DateTime PublicationDate { get; set; }

        [Required(ErrorMessage = "Genre is required")]
        [StringLength(50, ErrorMessage = "Genre cannot exceed 50 characters")]
        public string Genre { get; set; } = string.Empty;

        public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    }
}