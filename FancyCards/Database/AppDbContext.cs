using FancyCards.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace FancyCards.Database
{
    public class AppDbContext : DbContext
    {
        public DbSet<Deck> Decks { get; set; }
        public DbSet<Card> Cards { get; set; }

        public AppDbContext()
        {
            Database.EnsureCreated();
        }

        public AppDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Deck>().HasData(
            [
                new Deck(){ Cards = new List<Card>(), DateCreated = DateTime.Now, Name = "Test Deck", Description = "Test Description", Id = 1 }
            ]);

        }
    }
}
