using FancyCards.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace FancyCards.Database
{
    public class AppDbContext : DbContext
    {
        public DbSet<Deck> Decks { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<TextReplacementRule> TextReplacementRules { get; set; }

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

            modelBuilder.Entity<TextReplacementRule>().HasData(
            [
                new TextReplacementRule("."){ Id = 1},
                new TextReplacementRule(","){ Id = 2},
                new TextReplacementRule("!"){ Id = 3},
                new TextReplacementRule("?"){ Id = 4},
                new TextReplacementRule(";"){ Id = 5},
                new TextReplacementRule(":"){ Id = 6},
                new TextReplacementRule("\""){ Id = 7},
                new TextReplacementRule("("){ Id = 8},
                new TextReplacementRule(")"){ Id = 9},
                new TextReplacementRule("_"){ Id = 10},
                new TextReplacementRule("`", "'"){ Id = 11},
                new TextReplacementRule("-"){ Id = 12},
            ]);

        }
    }
}
