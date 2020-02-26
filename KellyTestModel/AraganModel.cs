namespace KellyTestModel
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class AraganModel : DbContext
    {
        public AraganModel()
            : base("name=AraganDB")
        {
        }

        public virtual DbSet<TestCategory> TestCategories { get; set; }
        public virtual DbSet<TestOrderProduct> TestOrderProducts { get; set; }
        public virtual DbSet<TestOrder> TestOrders { get; set; }
        public virtual DbSet<TestProduct> TestProducts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestCategory>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<TestOrderProduct>()
                .Property(e => e.Price)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TestOrderProduct>()
                .Property(e => e.Total)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TestOrder>()
                .Property(e => e.FirstName)
                .IsUnicode(false);

            modelBuilder.Entity<TestOrder>()
                .Property(e => e.LastName)
                .IsUnicode(false);

            modelBuilder.Entity<TestOrder>()
                .Property(e => e.Address)
                .IsUnicode(false);

            modelBuilder.Entity<TestOrder>()
                .Property(e => e.City)
                .IsUnicode(false);

            modelBuilder.Entity<TestOrder>()
                .Property(e => e.State)
                .IsUnicode(false);

            modelBuilder.Entity<TestOrder>()
                .Property(e => e.Country)
                .IsUnicode(false);

            modelBuilder.Entity<TestProduct>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<TestProduct>()
                .Property(e => e.SKU)
                .IsUnicode(false);

            modelBuilder.Entity<TestProduct>()
                .Property(e => e.Description)
                .IsUnicode(false);
        }
    }
}
