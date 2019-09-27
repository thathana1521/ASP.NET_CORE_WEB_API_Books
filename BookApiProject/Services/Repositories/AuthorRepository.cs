using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApiProject.Models;
using BookApiProject.Services.Interfaces;

namespace BookApiProject.Services.Repositories
{

    public class AuthorRepository : IAuthorRepository
    {
        private readonly BookDbContext _bookDbContext;

        public AuthorRepository(BookDbContext bookDbContext)
        {
            _bookDbContext = bookDbContext;
        }

        public ICollection<Author> GetAuthors()
        {
            return _bookDbContext.Authors.OrderBy(a => a.LastName).ToList();
        }

        public Author GetAuthor(int authorId)
        {
            return _bookDbContext.Authors.FirstOrDefault(a => a.Id == authorId);
        }

        public ICollection<Author> GetAuthorsOfABook(int bookId)
        {
            return _bookDbContext.BookAuthors.Where(ba => ba.Book.Id == bookId).Select(a=>a.Author).ToList();
        }

        public ICollection<Book> GetBooksByAuthor(int authorId)
        {
            return _bookDbContext.BookAuthors.Where(ba => ba.Author.Id == authorId).Select(b => b.Book).ToList();
        }

        public bool AuthorExists(int authorId)
        {
            return _bookDbContext.Authors.Any(a => a.Id == authorId);
        }

        public bool CreateAuthor(Author author)
        {
            _bookDbContext.Add(author);
            return Save();
        }

        public bool UpdateAuthor(Author author)
        {
            _bookDbContext.Update(author);
            return Save();
        }

        public bool DeleteAuthor(Author author)
        {
            _bookDbContext.Remove(author);
            return Save();
        }

        public bool Save()
        {
            var savedChanges = _bookDbContext.SaveChanges();
            return savedChanges >= 0;
        }
    }
}
