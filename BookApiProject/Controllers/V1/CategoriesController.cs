using System.Collections.Generic;
using System.Linq;
using BookApiProject.Contracts.V1;
using BookApiProject.DTOs;
using BookApiProject.Models;
using BookApiProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookApiProject.Controllers.V1
{
    [Route(ApiRoutes.CategoriesRoot)]
    [ApiController]
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBookRepository _iBookRepository;

        public CategoriesController(ICategoryRepository categoryRepository, IBookRepository iBookRepository)
        {
            _categoryRepository = categoryRepository;
            _iBookRepository = iBookRepository;
        }

        //api/categories
        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(ICollection<CategoryDTO>))]
        public IActionResult GetCategories()
        {
            var categories = _categoryRepository.GetCategories();

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var categoriesDTO = new List<CategoryDTO>();
            foreach (var category in categories)
            {
                categoriesDTO.Add(new CategoryDTO
                {
                    Id = category.Id,
                    Name = category.Name
                });
            }

            return Ok(categoriesDTO);
        }

        //api/categories/{categoryId}
        [HttpGet("{categoryId}",Name = "GetCategory")]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(CategoryDTO))]
        public IActionResult GetCategory(int categoryId)
        {
            if (!_categoryRepository.CategoryExists(categoryId))
                return NotFound();

            var category = _categoryRepository.GetCategory(categoryId);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            
            var categoryDTO= new CategoryDTO()
            {
                Id = category.Id,
                Name = category.Name
            };

            return Ok(categoryDTO);
        }

        //api/categories/books/{bookId}
        [HttpGet("books/{bookId}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CategoryDTO>))]
        public IActionResult GetCategoriesOfABook(int bookId)
        {
            if (!_iBookRepository.BookExists(bookId))
                return NotFound();

            var categoriesOfBook = _categoryRepository.GetCategoriesOfABook(bookId);

            var categoriesOfBookDTO = new List<CategoryDTO>();

            foreach (var category in categoriesOfBook)
            {
                categoriesOfBookDTO.Add(new CategoryDTO()
                {
                    Id = category.Id,
                    Name = category.Name
                });
            }
            return Ok(categoriesOfBookDTO);
        }

        //api/categories/{categoryId}/books
        [HttpGet("{categoryId}/books")]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<BookDTO>))]
        public IActionResult GetBooksForCategory(int categoryId)
        {
            if (!_categoryRepository.CategoryExists(categoryId))
                return NotFound();

            var books = _categoryRepository.GetBooksForCategory(categoryId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var booksDto = new List<BookDTO>();

            foreach (var book in books)
            {
                booksDto.Add(new BookDTO
                {
                    Id = book.Id,
                    Isbn = book.Isbn,
                    DatePublished = book.DatePublished,
                    Title = book.Title
                });
            }

            return Ok(booksDto);
        }

        //api/categories
        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        [ProducesResponseType(201, Type = typeof(Category))]
        public IActionResult CreateCategory([FromBody] Category categoryToCreate)
        {
            if (categoryToCreate == null)
                return BadRequest(ModelState);

            var categoryExists = _categoryRepository.GetCategories()
                .Any(c => c.Name.Trim().ToUpper().Equals(categoryToCreate.Name.Trim().ToUpper()));

            if (categoryExists)
            {
                ModelState.AddModelError("",$"Category {categoryToCreate.Name} already exists");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_categoryRepository.CreateCategory(categoryToCreate))
            {
                ModelState.AddModelError("",$"Something went wrong saving {categoryToCreate.Name}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetCategory", new {categoryId = categoryToCreate.Id}, categoryToCreate);
        }

        //api/categories/{categoryId}
        [HttpPut("{categoryId}")]
        [ProducesResponseType(400)]//Bad request
        [ProducesResponseType(404)]//Not found
        [ProducesResponseType(422)]//Unprocessable
        [ProducesResponseType(204)]//OK, no content
        [ProducesResponseType(500)]//Server Error
        public IActionResult UpdateCategory(int categoryId, [FromBody]Category categoryToUpdate)
        {
            if (categoryToUpdate == null)
                return BadRequest(ModelState);

            if (categoryToUpdate.Id != categoryId)
                return BadRequest(ModelState);

            if (!_categoryRepository.CategoryExists(categoryId))
                return NotFound();

            if (_categoryRepository.IsDuplicateCategoryName(categoryId, categoryToUpdate.Name))
            {
                ModelState.AddModelError("",$"Category {categoryToUpdate.Name} already exists");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_categoryRepository.UpdateCategory(categoryToUpdate))
            {
                ModelState.AddModelError("",$"Something went wrong updating {categoryToUpdate.Name} category");
                return StatusCode(500, ModelState);
            }
            
            return NoContent();
        }

        //api/categories/{categoryId}
        [HttpDelete("{categoryId}")]
        [ProducesResponseType(204)] //no content
        [ProducesResponseType(400)] //Bad request
        [ProducesResponseType(404)] //Not Found
        [ProducesResponseType(500)] //Server error
        [ProducesResponseType(409)] //Conflict
        public IActionResult DeleteCategory(int categoryId)
        {
            if (!_categoryRepository.CategoryExists(categoryId))
                return NotFound();

            var categoryToDelete = _categoryRepository.GetCategory(categoryId);

            //check if there are any books with this category
            if (_categoryRepository.GetBooksForCategory(categoryId).Count > 0)
            {
                ModelState.AddModelError("",
                    $"Category {categoryToDelete.Name} cannot be deleted because there are some books with its name");
                return StatusCode(409, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_categoryRepository.DeleteCategory(categoryToDelete))
            {
                ModelState.AddModelError("", $"Something went wrong with deleting {categoryToDelete.Name} category");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

    }
}