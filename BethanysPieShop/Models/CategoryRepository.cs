using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BethanysPieShop.Models
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _appDbContext;

        public CategoryRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public IEnumerable<Category> GetAllCategories()
        {
            return _appDbContext.Categories.ToList();
        }
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _appDbContext.Categories.ToListAsync(); // Fetches all categories from the database
        }
        public async Task AddCategoryAsync(Category category)
        {
            _appDbContext.Categories.Add(category);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            var category = await _appDbContext.Categories.FindAsync(categoryId);
            if (category != null)
            {
                _appDbContext.Categories.Remove(category);
                await _appDbContext.SaveChangesAsync();
            }
        }
        public async Task<Category> GetCategoryByIdAsync(int categoryId)
        {
            return await _appDbContext.Categories.FindAsync(categoryId);  // Fetch the category using its primary key
        }
        public void UpdateOrder(Order order)
        {
            _appDbContext.Orders.Update(order);  // Assuming _context is your DbContext
        }
        public IEnumerable<Category> AllCategories => _appDbContext.Categories;

        public async Task<IEnumerable<Category>> SearchCategoriesAsync(string searchQuery)
        {
            return await _appDbContext.Categories
                .Where(c => c.CategoryName.Contains(searchQuery))
                .ToListAsync();
        }
    }
}
