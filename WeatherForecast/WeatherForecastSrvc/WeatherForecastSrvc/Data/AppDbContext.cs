using Microsoft.EntityFrameworkCore;
using System;
using WeatherForecastSrvc.Model;

namespace WeatherForecastSrvc.Data
{
    /// <summary>
    /// EF’s bridge between WeatherForcecastSrvc and the database. 
    /// It tracks changes and executes SQL under the hood.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        { }

        /// <summary>
        /// Setup model to database table name.
        /// </summary>
        public DbSet<Location> Locations => Set<Location>();

        /// <summary>
        /// Specifying the model to have unique latitude and longitude values.
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Latitude and Longitude values need to be unique in the list.
            //If duplicate the DB will reject it.
            modelBuilder.Entity<Location>()
                .HasIndex(l => new { l.Latitude, l.Longitude })
                .IsUnique();
        }
    }
}
