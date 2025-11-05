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
    public class AuthorsController : ControllerBase
    {
        private readonly LibraryContext _context;
        private readonly ILogger<AuthorsController> _logger;

        public AuthorsController(LibraryContext context, ILogger<AuthorsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets all authors
        /// </summary>
        /// <returns>List of all authors</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AuthorReadDto>>> GetAuthors()
        {
            _logger.LogInformation("Retrieving all authors");

            var authors = await _context.Authors.ToListAsync();

            var authorDtos = authors.Select(a => new AuthorReadDto
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName
            });

            return Ok(authorDtos);
        }

        /// <summary>
        /// Gets a specific author by ID
        /// </summary>
        /// <param name="id">Author ID</param>
        /// <returns>Author details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthorReadDto>> GetAuthor(int id)
        {
            _logger.LogInformation("Retrieving author with ID: {AuthorId}", id);

            var author = await _context.Authors.FindAsync(id);

            if (author == null)
            {
                _logger.LogWarning("Author with ID {AuthorId} not found", id);
                return NotFound(new { message = $"Author with ID {id} not found" });
            }

            var authorDto = new AuthorReadDto
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName
            };

            return Ok(authorDto);
        }

        /// <summary>
        /// Gets all books by a specific author
        /// </summary>
        /// <param name="id">Author ID</param>
        /// <returns>List of books by the author</returns>
        [HttpGet("{id}/books")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BookReadDto>>> GetAuthorBooks(int id)
        {
            _logger.LogInformation("Retrieving books for author with ID: {AuthorId}", id);

            var author = await _context.Authors.FindAsync(id);

            if (author == null)
            {
                _logger.LogWarning("Author with ID {AuthorId} not found", id);
                return NotFound(new { message = $"Author with ID {id} not found" });
            }

            var books = await _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Where(b => b.BookAuthors.Any(ba => ba.AuthorId == id))
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
        /// Creates a new author
        /// </summary>
        /// <param name="authorDto">Author creation data</param>
        /// <returns>Created author</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthorReadDto>> CreateAuthor(AuthorCreateDto authorDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for author creation");
                return BadRequest(ModelState);
            }

            var author = new Author
            {
                FirstName = authorDto.FirstName,
                LastName = authorDto.LastName
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Author created with ID: {AuthorId}", author.Id);

            var resultDto = new AuthorReadDto
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName
            };

            return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, resultDto);
        }

        /// <summary>
        /// Updates an existing author
        /// </summary>
        /// <param name="id">Author ID</param>
        /// <param name="authorDto">Updated author data</param>
        /// <returns>No content</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAuthor(int id, AuthorUpdateDto authorDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for author update");
                return BadRequest(ModelState);
            }

            var author = await _context.Authors.FindAsync(id);

            if (author == null)
            {
                _logger.LogWarning("Author with ID {AuthorId} not found for update", id);
                return NotFound(new { message = $"Author with ID {id} not found" });
            }

            author.FirstName = authorDto.FirstName;
            author.LastName = authorDto.LastName;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Author with ID {AuthorId} updated", id);

            return NoContent();
        }

        /// <summary>
        /// Deletes an author
        /// </summary>
        /// <param name="id">Author ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            var author = await _context.Authors
                .Include(a => a.BookAuthors)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
            {
                _logger.LogWarning("Author with ID {AuthorId} not found for deletion", id);
                return NotFound(new { message = $"Author with ID {id} not found" });
            }

            if (author.BookAuthors.Any())
            {
                _logger.LogWarning("Cannot delete author with ID {AuthorId} because they have associated books", id);
                return BadRequest(new { message = $"Cannot delete author with ID {id} because they have associated books. Please remove the author from all books first." });
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Author with ID {AuthorId} deleted", id);

            return NoContent();
        }
    }
}