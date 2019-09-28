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
    public class AuthorsController : Controller
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IBookRepository _iBookRepository;
        private readonly ICountryRepository _iCountryRepository;

        public AuthorsController(IAuthorRepository authorRepository, IBookRepository iBookRepository, ICountryRepository iCountryRepository)
        {
            _authorRepository = authorRepository;
            _iBookRepository = iBookRepository;
            _iCountryRepository = iCountryRepository;
        }

        //api/
        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(200,Type = typeof(IEnumerable<AuthorDTO>))]
        public IActionResult GetAuthors()
        {
            var authors = _authorRepository.GetAuthors();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authorsDto = new List<AuthorDTO>();

            foreach (var author in authors)
            {
                authorsDto.Add(new AuthorDTO()
                {
                    Id = author.Id,
                    FirstName = author.FirstName,
                    LastName = author.LastName
                });
            }

            return Ok(authorsDto);
        }

        [HttpGet("{authorId}", Name = "GetAuthor")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(AuthorDTO))]
        public IActionResult GetAuthor(int authorId)
        {
            if (!_authorRepository.AuthorExists(authorId))
                return NotFound();

            var author = _authorRepository.GetAuthor(authorId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authorDto = new AuthorDTO()
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName
            };
            return Ok(authorDto);
        }

        [HttpGet("book/{bookId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(ICollection<AuthorDTO>))]
        public IActionResult GetAuthorsOfABook(int bookId)
        {
            if (!_iBookRepository.BookExists(bookId))
                return NotFound();

            var authors = _authorRepository.GetAuthorsOfABook(bookId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authorsDto = new List<AuthorDTO>();

            foreach (var author in authors)
            {
                authorsDto.Add(new AuthorDTO()
                {
                    Id = author.Id,
                    FirstName = author.FirstName,
                    LastName = author.LastName
                });
            }

            return Ok(authorsDto);
        }

        [HttpGet("{authorId}/books")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(ICollection<BookDTO>))]
        public IActionResult GetBooksByAuthor(int authorId)
        {
            if (!_authorRepository.AuthorExists(authorId))
                return NotFound();

            var books = _authorRepository.GetBooksByAuthor(authorId);

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

        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(201,Type = typeof(Author))]
        public IActionResult CreateAuthor([FromBody] Author authorToCreate)
        {
            if (authorToCreate == null)
                return BadRequest(ModelState);

            //we must have a valid country for the author
            if (!_iCountryRepository.CountryExists(authorToCreate.Country.Id))
            {
                ModelState.AddModelError("","Country does not exist.");
                return StatusCode(404, ModelState);
            }

            authorToCreate.Country = _iCountryRepository.GetCountry(authorToCreate.Country.Id);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_authorRepository.CreateAuthor(authorToCreate))
            {
                ModelState.AddModelError("",$"Something went wrong saving {authorToCreate.FirstName} {authorToCreate.LastName}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetAuthor", new {authorId = authorToCreate.Id}, authorToCreate);
        }

        //api/author/{authroId}
        [HttpPut("{authorId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        public IActionResult UpdateAuthor(int authorId,[FromBody] Author authorToUpdate)
        {
            if (authorToUpdate == null || authorId!=authorToUpdate.Id)
                return BadRequest(ModelState);

            if (_authorRepository.AuthorExists(authorId))
                return NotFound();

            //we must have a valid country for the author
            if (!_iCountryRepository.CountryExists(authorToUpdate.Country.Id))
            {
                ModelState.AddModelError("", "Country does not exist.");
                return StatusCode(404, ModelState);
            }

            authorToUpdate.Country = _iCountryRepository.GetCountry(authorToUpdate.Country.Id);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_authorRepository.UpdateAuthor(authorToUpdate))
            {
                ModelState.AddModelError("", $"Something went wrong updating {authorToUpdate.FirstName} {authorToUpdate.LastName}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        //api/author/{authorId}
        [HttpDelete("{authorId}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        [ProducesResponseType(409)]
        public IActionResult DeleteAuthor(int authorId)
        {
            if (!_authorRepository.AuthorExists(authorId))
                return NotFound();

            var authorToDelete = _authorRepository.GetAuthor(authorId);
            var books = _authorRepository.GetBooksByAuthor(authorId).ToList();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (books.Count > 0)
            {
                ModelState.AddModelError("",
                    $"Author {authorToDelete.FirstName} {authorToDelete.LastName} cannot be deleted because there are {books.Count} books associated");
                return StatusCode(409, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_authorRepository.DeleteAuthor(authorToDelete))
            {
                ModelState.AddModelError("", $"Something went wrong deleting {authorToDelete.FirstName} {authorToDelete.LastName}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

    }
}