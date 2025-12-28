using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace AlphaLogistics.API.Model
{
    public class AlphaLogisticsContext: DbContext
    {
        public virtual DbSet<ProductMaster> ProductMasters { get; set; }

        public virtual DbSet<OrderMaster> OrderMasters { get; set; }

        public virtual DbSet<CartMaster> CartMasters { get; set; }
        public virtual DbSet<OrderItems> OrderItems { get; set; }
        public virtual DbSet<UserMaster> UserMasters { get; set; }

        /* public AlphaLogisticsContext(DbContextOptions<AlphaLogisticsContext> options)
         : base(options)
         {
             
         }*/
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(
                "Host=zoumaapp.com;Port=5432;Database=AlphaLogisticsDb;Username=postgres;Password=zoumapg!@#admin;");
        }
    }
}
