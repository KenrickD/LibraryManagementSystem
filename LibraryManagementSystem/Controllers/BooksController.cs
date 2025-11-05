using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.DTOs;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BooksController : ControllerBase
    {
        private readonly LibraryContext _context;
        private readonly ILogger<BooksController> _logger;

        public BooksController(LibraryContext context, ILogger<BooksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets all books
        /// </summary>
        /// <returns>List of all books with their authors</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookReadDto>>> GetBooks()
        {
            _logger.LogInformation("Retrieving all books");

            var books = await _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .ToListAsync();

            var bookDtos = books.Select(b => new BookReadDto
            {
                Id = b.Id,
                Title = b.Title,
                PublicationDate = b.PublicationDate,
                Genre = b.Genre,
                Authors = b.BookAuthors.Select(ba => new AuthorReadDto
                {
                    Id = ba.Author.Id,
                    FirstName = ba.Author.FirstName,
                    LastName = ba.Author.LastName
                }).ToList()
            });

            return Ok(bookDtos);
        }

        /// <summary>
        /// Gets a specific book by ID
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <returns>Book details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookReadDto>> GetBook(int id)
        {
            _logger.LogInformation("Retrieving book with ID: {BookId}", id);

            var book = await _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                _logger.LogWarning("Book with ID {BookId} not found", id);
                return NotFound(new { message = $"Book with ID {id} not found" });
            }

            var bookDto = new BookReadDto
            {
                Id = book.Id,
                Title = book.Title,
                PublicationDate = book.PublicationDate,
                Genre = book.Genre,
                Authors = book.BookAuthors.Select(ba => new AuthorReadDto
                {
                    Id = ba.Author.Id,
                    FirstName = ba.Author.FirstName,
                    LastName = ba.Author.LastName
                }).ToList()
            };

            return Ok(bookDto);
        }

        /// <summary>
        /// Creates a new book
        /// </summary>
        /// <param name="bookDto">Book creation data</param>
        /// <returns>Created book</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookReadDto>> CreateBook(BookCreateDto bookDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for book creation");
                return BadRequest(ModelState);
            }

            if (bookDto.AuthorIds.Any())
            {
                var existingAuthorIds = await _context.Authors
                    .Where(a => bookDto.AuthorIds.Contains(a.Id))
                    .Select(a => a.Id)
                    .ToListAsync();

                var invalidAuthorIds = bookDto.AuthorIds.Except(existingAuthorIds).ToList();
                if (invalidAuthorIds.Any())
                {
                    _logger.LogWarning("Invalid author IDs: {AuthorIds}", string.Join(", ", invalidAuthorIds));
                    return BadRequest(new { message = $"Invalid author IDs: {string.Join(", ", invalidAuthorIds)}" });
                }
            }

            var book = new Book
            {
                Title = bookDto.Title,
                PublicationDate = bookDto.PublicationDate,
                Genre = bookDto.Genre
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            foreach (var authorId in bookDto.AuthorIds)
            {
                _context.BookAuthors.Add(new BookAuthor
                {
                    BookId = book.Id,
                    AuthorId = authorId
                });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Book created with ID: {BookId}", book.Id);

            var createdBook = await _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .FirstAsync(b => b.Id == book.Id);

            var resultDto = new BookReadDto
            {
                Id = createdBook.Id,
                Title = createdBook.Title,
                PublicationDate = createdBook.PublicationDate,
                Genre = createdBook.Genre,
                Authors = createdBook.BookAuthors.Select(ba => new AuthorReadDto
                {
                    Id = ba.Author.Id,
                    FirstName = ba.Author.FirstName,
                    LastName = ba.Author.LastName
                }).ToList()
            };

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, resultDto);
        }

        /// <summary>
        /// Updates an existing book
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <param name="bookDto">Updated book data</param>
        /// <returns>No content</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBook(int id, BookUpdateDto bookDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for book update");
                return BadRequest(ModelState);
            }

            var book = await _context.Books
                .Include(b => b.BookAuthors)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                _logger.LogWarning("Book with ID {BookId} not found for update", id);
                return NotFound(new { message = $"Book with ID {id} not found" });
            }

            if (bookDto.AuthorIds.Any())
            {
                var existingAuthorIds = await _context.Authors
                    .Where(a => bookDto.AuthorIds.Contains(a.Id))
                    .Select(a => a.Id)
                    .ToListAsync();

                var invalidAuthorIds = bookDto.AuthorIds.Except(existingAuthorIds).ToList();
                if (invalidAuthorIds.Any())
                {
                    _logger.LogWarning("Invalid author IDs: {AuthorIds}", string.Join(", ", invalidAuthorIds));
                    return BadRequest(new { message = $"Invalid author IDs: {string.Join(", ", invalidAuthorIds)}" });
                }
            }

            book.Title = bookDto.Title;
            book.PublicationDate = bookDto.PublicationDate;
            book.Genre = bookDto.Genre;

            _context.BookAuthors.RemoveRange(book.BookAuthors);
            foreach (var authorId in bookDto.AuthorIds)
            {
                _context.BookAuthors.Add(new BookAuthor
                {
                    BookId = book.Id,
                    AuthorId = authorId
                });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Book with ID {BookId} updated", id);

            return NoContent();
        }

        /// <summary>
        /// Deletes a book
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                _logger.LogWarning("Book with ID {BookId} not found for deletion", id);
                return NotFound(new { message = $"Book with ID {id} not found" });
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Book with ID {BookId} deleted", id);

            return NoContent();
        }
    }
}