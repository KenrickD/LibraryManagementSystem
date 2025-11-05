using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace LibraryManagementSystem.Data
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<BookAuthor> BookAuthors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BookAuthor>()
                .HasKey(ba => new { ba.BookId, ba.AuthorId });

            modelBuilder.Entity<BookAuthor>()
                .HasOne(ba => ba.Book)
                .WithMany(b => b.BookAuthors)
                .HasForeignKey(ba => ba.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookAuthor>()
                .HasOne(ba => ba.Author)
                .WithMany(a => a.BookAuthors)
                .HasForeignKey(ba => ba.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seeder data
            modelBuilder.Entity<Author>().HasData(
                new Author { Id = 1, FirstName = "J.K.", LastName = "Rowling" },
                new Author { Id = 2, FirstName = "George", LastName = "Orwell" },
                new Author { Id = 3, FirstName = "Jane", LastName = "Austen" }
            );

            modelBuilder.Entity<Book>().HasData(
                new Book { Id = 1, Title = "Harry Potter and the Philosopher's Stone", PublicationDate = new DateTime(1997, 6, 26), Genre = "Fantasy" },
                new Book { Id = 2, Title = "1984", PublicationDate = new DateTime(1949, 6, 8), Genre = "Dystopian" },
                new Book { Id = 3, Title = "Pride and Prejudice", PublicationDate = new DateTime(1813, 1, 28), Genre = "Romance" }
            );

            modelBuilder.Entity<BookAuthor>().HasData(
                new BookAuthor { BookId = 1, AuthorId = 1 },
                new BookAuthor { BookId = 2, AuthorId = 2 },
                new BookAuthor { BookId = 3, AuthorId = 3 }
            );
        }
    }
}