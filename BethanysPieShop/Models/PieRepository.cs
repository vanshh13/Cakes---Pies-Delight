using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BethanysPieShop.Models
{
    public class PieRepository : IPieRepository
    {
        private readonly AppDbContext _appDbContext;

        public PieRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<IEnumerable<Pie>> GetAllPiesAsync()
        {
            return await _appDbContext.Pies.ToListAsync();
        }

        public async Task<IEnumerable<Pie>> SearchPiesAsync(string searchQuery)
        {
            return await _appDbContext.Pies
                .Where(p => p.Name.Contains(searchQuery) || p.Price.ToString().Contains(searchQuery))
                .ToListAsync();
        }
        public IEnumerable<Pie> AllPies
        {
            get
            {
                return _appDbContext.Pies.Include(c => c.Category);
            }
        }

        public IEnumerable<Pie> PiesOfTheWeek
        {
            get
            {
                return _appDbContext.Pies.Include(c => c.Category).Where(p => p.IsPieOfTheWeek);
            }
        }

        public Pie GetPieById(int pieId)
        {
            
            return _appDbContext.Pies.FirstOrDefault(p => p.PieId == pieId);
            
        }
        public async Task<Pie> AddPieAsync(Pie pie)
        {
            _appDbContext.Pies.Add(pie);
            await _appDbContext.SaveChangesAsync();
            return pie;
        }
        public async Task UpdatePieAsync(Pie pie)
        {
            var existingPie = await _appDbContext.Pies.FirstOrDefaultAsync(p => p.PieId == pie.PieId);

            if (existingPie != null)
            {
                // Update existing pie fields
                existingPie.Name = pie.Name;
                existingPie.Price = pie.Price;
                existingPie.ShortDescription = pie.ShortDescription;
                existingPie.LongDescription = pie.LongDescription;
                existingPie.CategoryId = pie.CategoryId; // Update Category ID (FK)
                existingPie.IsPieOfTheWeek = pie.IsPieOfTheWeek;
                existingPie.InStock = pie.InStock;
                existingPie.Notes = pie.Notes;

                // Log existing and new ImageUrl for debugging
                Console.WriteLine($"Old ImageUrl: {existingPie.ImageUrl}");
                Console.WriteLine($"New ImageUrl: {pie.ImageUrl}");

                // If an image is being updated
                if (!string.IsNullOrEmpty(pie.ImageUrl))
                {
                    existingPie.ImageUrl = pie.ImageUrl;
                }

                // Save changes to the database
                await _appDbContext.SaveChangesAsync();

                // Log successful update
                Console.WriteLine("Pie updated successfully");
            }
            else
            {
                // Log if pie was not found
                Console.WriteLine($"Pie with ID {pie.PieId} not found.");
            }
        }

        public void UpdatePie(Pie pie)
        {
            var existingPie = _appDbContext.Pies.FirstOrDefault(p => p.PieId == pie.PieId);
            if (existingPie != null)
            {
                existingPie.Name = pie.Name;
                existingPie.Price = pie.Price;
                existingPie.ShortDescription = pie.ShortDescription;
                existingPie.LongDescription = pie.LongDescription;
                existingPie.Category = pie.Category;
                existingPie.IsPieOfTheWeek = pie.IsPieOfTheWeek;

                _appDbContext.SaveChanges();  // Commit changes
            }
        }

        public void DeletePie(int pieId)
        {
            var pieToDelete = _appDbContext.Pies.FirstOrDefault(p => p.PieId == pieId);
            if (pieToDelete != null)
            {
                _appDbContext.Pies.Remove(pieToDelete);
                _appDbContext.SaveChanges();  // Commit changes
            }
        }
        public async Task<Pie> GetPieByIdAsync(int pieId)
        {
            return await _appDbContext.Pies
                .Include(p => p.Category) // Include the category data if needed
                .FirstOrDefaultAsync(p => p.PieId == pieId);
        }

    }
}
