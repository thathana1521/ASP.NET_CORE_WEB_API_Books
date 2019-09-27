using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApiProject.Models;
using BookApiProject.Services;
using BookApiProject.Services.Interfaces;

namespace BookApiProject.Services.Repositories
{

    public class BookRepository : IBookRepository
    {
        private readonly BookDbContext _bookDbContext;

        public BookRepository(BookDbContext bookDbContext)
        {
            _bookDbContext = bookDbContext;
        }
        public ICollection<Book> GetBooks()
        {
            return _bookDbContext.Books.OrderBy(b => b.Title).ToList();
        }

        public Book GetBook(int bookId)
        {
            return _bookDbContext.Books.FirstOrDefault(b => b.Id == bookId);
        }

        public Book GetBook(string isbn)
        {
            return _bookDbContext.Books.FirstOrDefault(b => b.Isbn == isbn);
        }

        public bool BookExists(int bookId)
        {
            return _bookDbContext.Books.Any(b => b.Id == bookId);
        }

        public bool BookExists(string isbn)
        {
            return _bookDbContext.Books.Any(b => b.Isbn == isbn);
        }

        public bool IsDuplicateISBN(int bookId, string bookIsbn)
        {
            var book = _bookDbContext.Books.FirstOrDefault(b =>b.Isbn.Trim().ToUpper().Equals(bookIsbn.Trim().ToUpper()) && b.Id != bookId);
            return book != null;
        }

        public decimal GetBookRating(int bookId)
        {
            var reviews = _bookDbContext.Reviews.Where(r => r.Book.Id == bookId).ToList();

            if (!(reviews.Count> 0))
                return 0;

            return ((decimal)reviews.Sum(r => r.Rating) / reviews.Count);
        }

        public bool CreateBook(List<int> authorsId, List<int> categoriesId, Book book)
        {
            var authors = _bookDbContext.Authors.Where(a => authorsId.Contains(a.Id)).ToList();
            var categories = _bookDbContext.Categories.Where(c => categoriesId.Contains(c.Id)).ToList();

            foreach (var author in authors)
            {
                BookAuthor bookAuthor = new BookAuthor()
                {
                    Author = author,
                    Book = book
                };
                _bookDbContext.Add(bookAuthor);
            }

            foreach (var category in categories)
            {
                BookCategory bookCategory = new BookCategory()
                {
                    Category = category,
                    Book = book
                };
                _bookDbContext.Add(bookCategory);
            }

            _bookDbContext.Add(book);
            return Save();

        }

        public bool UpdateBook(List<int> authorsId, List<int> categoriesId, Book book)
        {
            var authors = _bookDbContext.Authors.Where(a => authorsId.Contains(a.Id)).ToList();
            var categories = _bookDbContext.Categories.Where(c => categoriesId.Contains(c.Id)).ToList();

            var bookAuthorsToDelete = _bookDbContext.BookAuthors.Where(ba => ba.BookId == book.Id);
            var bookCategoriesToDelete = _bookDbContext.BookCategories.Where(c => c.BookId == book.Id);

            _bookDbContext.RemoveRange(bookAuthorsToDelete);
            _bookDbContext.RemoveRange(bookCategoriesToDelete);

            foreach (var author in authors)
            {
                BookAuthor bookAuthor = new BookAuthor()
                {
                    Author = author,
                    Book = book
                };
                _bookDbContext.Add(bookAuthor);
            }

            foreach (var category in categories)
            {
                BookCategory bookCategory = new BookCategory()
                {
                    Category = category,
                    Book = book
                };
                _bookDbContext.Add(bookCategory);
            }

            _bookDbContext.Update(book);
            return Save();
        }

        public bool DeleteBook(Book book)
        {
            _bookDbContext.Remove(book);
            return Save();
        }

        public bool Save()
        {
            var savedChanges = _bookDbContext.SaveChanges();
            return savedChanges >= 0 ? true : false;
        }
    }
}
