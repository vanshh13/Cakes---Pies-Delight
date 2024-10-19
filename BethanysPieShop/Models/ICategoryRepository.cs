using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BethanysPieShop.Models
{
    public interface ICategoryRepository
    {
        IEnumerable<Category> AllCategories { get; }
        IEnumerable<Category> GetAllCategories();
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task AddCategoryAsync(Category category);  // Add this method
        Task DeleteCategoryAsync(int categoryId);  // Add this method
        Task<Category> GetCategoryByIdAsync(int categoryId);  // This method fetches a category by its ID
        Task<IEnumerable<Category>> SearchCategoriesAsync(string searchQuery);

    }
}
