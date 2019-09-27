using System.Collections.Generic;
using System.Linq;
using BookApiProject.Models;
using BookApiProject.Services.Interfaces;

namespace BookApiProject.Services.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly BookDbContext _categoryContext;

        public CategoryRepository(BookDbContext categoryContext)
        {
            _categoryContext = categoryContext;
        }
        public ICollection<Category> GetCategories()
        {
            return _categoryContext.Categories.OrderBy(c => c.Name).ToList();
        }

        public Category GetCategory(int categoryId)
        {
            return _categoryContext.Categories.FirstOrDefault(c => c.Id == categoryId);
        }

        public ICollection<Category> GetCategoriesOfABook(int bookId)
        {
            
            return _categoryContext.BookCategories.Where(b => b.BookId == bookId).Select(c=>c.Category).ToList();
        }

        public ICollection<Book> GetBooksForCategory(int categoryId)
        {
            return _categoryContext.BookCategories.Where(b => b.CategoryId == categoryId).Select(b => b.Book).ToList();
        }


        public bool CategoryExists(int categoryId)
        {
            return _categoryContext.Categories.Any(c => c.Id == categoryId);
        }

        public bool IsDuplicateCategoryName(int categoryId, string categoryName)
        {
            var category = _categoryContext.Categories.FirstOrDefault(c =>
                c.Name.Trim().ToUpper().Equals(categoryName.Trim().ToUpper()) && c.Id != categoryId);

            return category != null;
        }

        public bool CreateCategory(Category category)
        {
            _categoryContext.AddAsync(category);
            return Save();
        }

        public bool UpdateCategory(Category category)
        {
            _categoryContext.Update(category);
            return Save();
        }

        public bool DeleteCategory(Category category)
        {
            _categoryContext.Remove(category);
            return Save();
        }

        public bool Save()
        {
            var savedChanges = _categoryContext.SaveChanges();
            return savedChanges >= 0;
        }
    }
}
