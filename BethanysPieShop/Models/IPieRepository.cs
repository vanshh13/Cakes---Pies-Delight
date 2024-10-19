using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BethanysPieShop.Models
{
    public interface IPieRepository
    {
        Task<Pie> AddPieAsync(Pie pie);
        IEnumerable<Pie> AllPies { get; }
        IEnumerable<Pie> PiesOfTheWeek { get; }
        Pie GetPieById(int pieId);
        void UpdatePie(Pie pie);  // Add this
        void DeletePie(int pieId);  // Add this
        Task<Pie> GetPieByIdAsync(int pieId);
        Task UpdatePieAsync(Pie pie);
        Task<IEnumerable<Pie>> GetAllPiesAsync();
        Task<IEnumerable<Pie>> SearchPiesAsync(string searchQuery);

    }
}
