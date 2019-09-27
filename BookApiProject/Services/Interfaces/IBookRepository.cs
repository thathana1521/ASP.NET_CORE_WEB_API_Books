using System.Collections.Generic;
using BookApiProject.Models;

namespace BookApiProject.Services.Interfaces
{
    public interface IBookRepository
    {
        ICollection<Book> GetBooks();
        Book GetBook(int bookId);
        Book GetBook(string isbn);
        bool BookExists(int bookId);
        bool BookExists(string isbn);
        bool IsDuplicateISBN(int bookId, string bookIsbn);
        decimal GetBookRating(int bookId);
        bool CreateBook(List<int> authorsId, List<int> categoriesId, Book book);
        bool UpdateBook(List<int> authorsId, List<int> categoriesId, Book book);
        bool DeleteBook(Book book);
        bool Save();
    }
}
