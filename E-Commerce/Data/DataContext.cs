using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
    }
}
