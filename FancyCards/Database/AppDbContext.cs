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
        public DbSet<TrainingSession> TrainingSessions { get; set; }
        public DbSet<TrainingSessionCard> TrainingSessionCards { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<ReviewProfile> ReviewProfiles { get; set; }

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

            //modelBuilder.Entity<Deck>().HasData(
            //[
            //    new Deck(){ Cards = new List<Card>(), DateCreated = DateTime.Now, Name = "Test Deck", Description = "Test Description", Id = 1 }
            //]);

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

            modelBuilder.Entity<ReviewProfile>().HasData(
            [
                new ReviewProfile
                {
                    Id = 1,
                    Name = "Default",
                    StartEF = 2.1,
                    EasyRatioEF = 0.08,
                    NormalRatioEF = -0.05,
                    HardRatioEF = -0.15,
                    ErrorRatioEF = -0.25,
                    MinEF = 1.3,
                    MaxEF = 2.5,
                    SecondRepetitionInterval = 3
                },
                new ReviewProfile
                {
                    Id = 2,
                    Name = "Default 2",
                    StartEF = 1.3,
                    EasyRatioEF = 0.12,
                    NormalRatioEF = 0.05,
                    HardRatioEF = -0.10,
                    ErrorRatioEF = -0.15,
                    MinEF = 1.3,
                    MaxEF = 2.5,
                    SecondRepetitionInterval = 3
                }
            ]);
        }
    }
}
