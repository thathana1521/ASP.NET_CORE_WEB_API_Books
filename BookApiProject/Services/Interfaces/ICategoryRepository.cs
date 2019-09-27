using System.Collections.Generic;
using BookApiProject.Models;

namespace BookApiProject.Services.Interfaces
{
    public interface ICategoryRepository
    {
        ICollection<Category> GetCategories();
        Category GetCategory(int categoryId);
        ICollection<Category> GetCategoriesOfABook(int bookId);
        ICollection<Book> GetBooksForCategory(int categoryId);
        bool CategoryExists(int categoryId);
        bool IsDuplicateCategoryName(int categoryId, string categoryName);
        bool CreateCategory(Category category);
        bool UpdateCategory(Category category);
        bool DeleteCategory(Category category);
        bool Save();
    }
}
