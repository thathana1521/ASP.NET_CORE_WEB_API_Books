using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApiProject.DTOs;
using BookApiProject.Models;
using BookApiProject.Services;
using BookApiProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : Controller
    {
        private readonly IBookRepository _iBookRepository;
        private readonly IAuthorRepository _iAuthorRepository;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly IReviewRepository _iReviewRepository;

        public BookController(IBookRepository iBookRepository, IAuthorRepository iAuthorRepository, ICategoryRepository iCategoryRepository, IReviewRepository iReviewRepository)
        {
            _iBookRepository = iBookRepository;
            _iAuthorRepository = iAuthorRepository;
            _iCategoryRepository = iCategoryRepository;
            _iReviewRepository = iReviewRepository;
        }
        
        //api/book
        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(ICollection<BookDTO>))]
        public IActionResult GetBooks()
        {
            var books = _iBookRepository.GetBooks();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var booksDto = new List<BookDTO>();

            foreach (var book in books)
            {
                booksDto.Add(new BookDTO()
                {
                    Id = book.Id,
                    DatePublished = book.DatePublished,
                    Isbn = book.Isbn,
                    Title = book.Title
                });
            }

            return Ok(booksDto);
        }

        //api/book/{bookId}
        [HttpGet("{bookId}", Name = "GetBook")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(BookDTO))]
        public IActionResult GetBook(int bookId)
        {
            if (!_iBookRepository.BookExists(bookId))
                return NotFound();

            var book = _iBookRepository.GetBook(bookId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var bookDto = new BookDTO()
            {
                Id = book.Id,
                DatePublished = book.DatePublished,
                Isbn = book.Isbn,
                Title = book.Title
            };
            return Ok(bookDto);
        }

        //api/book/isbn/{bookIsbn}
        [HttpGet("isbn/{bookIsbn}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(BookDTO))]
        public IActionResult GetBook(string bookIsbn)
        {
            if (!_iBookRepository.BookExists(bookIsbn))
                return NotFound();

            var book = _iBookRepository.GetBook(bookIsbn);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var bookDto = new BookDTO()
            {
                Id = book.Id,
                DatePublished = book.DatePublished,
                Isbn = book.Isbn,
                Title = book.Title
            };
            return Ok(bookDto);
        }

        //api/book/{bookId}/rating
        [HttpGet("{bookId}/rating")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(decimal))]
        public IActionResult GetBookRating(int bookId)
        {
            if (!_iBookRepository.BookExists(bookId))
                return NotFound();

            var rating = _iBookRepository.GetBookRating(bookId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            return Ok(rating);
        }

        //api/books?authId=1&authId=2&catId=1&catId=2
        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(422)]
        [ProducesResponseType(201, Type = typeof(Book))]
        public IActionResult CreateBook([FromQuery] List<int> authId, [FromQuery] List<int> catId,[FromBody] Book book)
        {
            var statusCode = ValidateBook(authId, catId, book);
            if (!ModelState.IsValid)
                return StatusCode(statusCode.StatusCode);

            if (!_iBookRepository.CreateBook(authId, catId, book))
            {
                ModelState.AddModelError("","Something went wrong saving book");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetBook", new {bookId = book.Id}, book);
        }

        //api/book/bookId?authId=1&authId=2&catId=1&catId=2
        [HttpPut("{bookId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(422)]
        [ProducesResponseType(204)]
        public IActionResult UpdateBook(int bookId, [FromQuery] List<int> authId, [FromQuery] List<int> catId, [FromBody] Book bookToUpdate)
        {
            if (bookId != bookToUpdate.Id)
                return BadRequest(ModelState);

            if (!_iBookRepository.BookExists(bookId))
                return NotFound();

            var statusCode = ValidateBook(authId, catId, bookToUpdate);
            if (!ModelState.IsValid)
                return StatusCode(statusCode.StatusCode);

            if (!_iBookRepository.UpdateBook(authId, catId, bookToUpdate))
            {
                ModelState.AddModelError("", "Something went wrong updating book");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        //api/book/{bookId}
        [HttpDelete("{bookId}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        public IActionResult DeleteBook(int bookId)
        {
            if (!_iBookRepository.BookExists(bookId))
                return NotFound();

            
            var reviewsToDelete = _iReviewRepository.GetReviewsOfABook(bookId);
            var bookToDelete = _iBookRepository.GetBook(bookId);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_iReviewRepository.DeleteReviews(reviewsToDelete.ToList()))
            {
                ModelState.AddModelError("", "Something went wrong deleting the reviews by this book");
                return StatusCode(500, ModelState);
            }

            if (!_iBookRepository.DeleteBook(bookToDelete))
            {
                ModelState.AddModelError("", "Something went wrong deleting the book");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        private StatusCodeResult ValidateBook(List<int> authorId, List<int> categoryId, Book book)
        {
            if (book == null || authorId.Count <= 0 || categoryId.Count <= 0)
            {
                ModelState.AddModelError("","Missing book, author or category ");
                return BadRequest();
            }

            if (_iBookRepository.IsDuplicateISBN(book.Id, book.Isbn))
            {
                ModelState.AddModelError("","Duplicate ISBN");
                return StatusCode(422);
            }

            foreach (var id in authorId)
            {
                if (!_iAuthorRepository.AuthorExists(id))
                {
                    ModelState.AddModelError("","Author is not found");
                    return StatusCode(404);
                }
            }

            foreach (var id in categoryId)
            {
                if(!_iCategoryRepository.CategoryExists(id))
                {
                    ModelState.AddModelError("", "Category is not found");
                    return StatusCode(404);
                }
            }

            if (!ModelState.IsValid)
                return BadRequest();

            return NoContent();
        }
    }
}