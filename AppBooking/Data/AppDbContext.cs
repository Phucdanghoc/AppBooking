using AppBooking.Model;
using Microsoft.EntityFrameworkCore;

namespace AppBooking.Data
{
    public class AppDbContext : DbContext
    {

        public DbSet<User> Users { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Seat> Seats { get; set; }

        public DbSet<Distance> Distances { get; set; }

        public AppDbContext(DbContextOptions
            <AppDbContext> options) : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("Server=localhost;Database=flymanagerment;User=root;Password=;",new MySqlServerVersion(new Version(8,0,30)));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<User>().HasData(
                 new User
                 {
                     UserId = 1,
                     Password =  BCrypt.Net.BCrypt.HashPassword("1"),
                     FirstName = "John",
                     LastName = "Doe",
                     Address = "123 Main St",
                     PhoneNumber = "123-456-7890",
                     Email = "john.doe@example.com",
                     Sex = "Male",
                     Role = "User",
                     Age = 30,
                     Credit = 1000,
                     SkyMiles = 500.5
                 }
             );
            modelBuilder.Entity<User>().HasData(
              new User
              {
                  UserId = 2,
                  Password = BCrypt.Net.BCrypt.HashPassword("1"),
                  FirstName = "Admin",
                  LastName = "Admin",
                  Address = "123 Main St",
                  PhoneNumber = "123-456-7890",
                  Email = "admin@admin.com",
                  Sex = "Male",
                  Role = "Admin",
                  Age = 30,
                  Credit = 1000,
                  SkyMiles = 500.5
              }
          );
            modelBuilder.Entity<Distance>().HasData(
                    new Distance { DistanceId = 1, DepartureCity = "HaNoi", DestinationCity = "HCM", Miles = 500 },
                    new Distance { DistanceId = 2, DepartureCity = "HaNoi", DestinationCity = "VINH", Miles = 300 },
                    new Distance { DistanceId = 3, DepartureCity = "HaNoi", DestinationCity = "CanTho", Miles = 700 },
                    new Distance { DistanceId = 4, DepartureCity = "HaNoi", DestinationCity = "DaNang", Miles = 400 },
                    new Distance { DistanceId = 5, DepartureCity = "HaNoi", DestinationCity = "PhuQuoc", Miles = 1000 },
                    new Distance { DistanceId = 6, DepartureCity = "HCM", DestinationCity = "HaNoi", Miles = 500 },
                    new Distance { DistanceId = 7, DepartureCity = "HCM", DestinationCity = "VINH", Miles = 800 },
                    new Distance { DistanceId = 8, DepartureCity = "HCM", DestinationCity = "CanTho", Miles = 200 },
                    new Distance { DistanceId = 9, DepartureCity = "HCM", DestinationCity = "DaNang", Miles = 600 },
                    new Distance { DistanceId = 10, DepartureCity = "HCM", DestinationCity = "PhuQuoc", Miles = 900 },
                    new Distance { DistanceId = 11, DepartureCity = "VINH", DestinationCity = "HaNoi", Miles = 300 },
                    new Distance { DistanceId = 12, DepartureCity = "VINH", DestinationCity = "HCM", Miles = 800 },
                    new Distance { DistanceId = 13, DepartureCity = "VINH", DestinationCity = "CanTho", Miles = 900 },
                    new Distance { DistanceId = 14, DepartureCity = "VINH", DestinationCity = "DaNang", Miles = 300 },
                    new Distance { DistanceId = 15, DepartureCity = "VINH", DestinationCity = "PhuQuoc", Miles = 1100 },
                    new Distance { DistanceId = 16, DepartureCity = "CanTho", DestinationCity = "HaNoi", Miles = 700 },
                    new Distance { DistanceId = 17, DepartureCity = "CanTho", DestinationCity = "HCM", Miles = 200 },
                    new Distance { DistanceId = 18, DepartureCity = "CanTho", DestinationCity = "VINH", Miles = 900 },
                    new Distance { DistanceId = 19, DepartureCity = "CanTho", DestinationCity = "DaNang", Miles = 800 },
                    new Distance { DistanceId = 20, DepartureCity = "CanTho", DestinationCity = "PhuQuoc", Miles = 600 },
                    new Distance { DistanceId = 21, DepartureCity = "DaNang", DestinationCity = "HaNoi", Miles = 400 },
                    new Distance { DistanceId = 22, DepartureCity = "DaNang", DestinationCity = "HCM", Miles = 600 },
                    new Distance { DistanceId = 23, DepartureCity = "DaNang", DestinationCity = "VINH", Miles = 300 },
                    new Distance { DistanceId = 24, DepartureCity = "DaNang", DestinationCity = "CanTho", Miles = 800 },
                    new Distance { DistanceId = 25, DepartureCity = "DaNang", DestinationCity = "PhuQuoc", Miles = 1200 },
                    new Distance { DistanceId = 26, DepartureCity = "PhuQuoc", DestinationCity = "HaNoi", Miles = 1000 },
                    new Distance { DistanceId = 27, DepartureCity = "PhuQuoc", DestinationCity = "HCM", Miles = 900 },
                    new Distance { DistanceId = 28, DepartureCity = "PhuQuoc", DestinationCity = "VINH", Miles = 1100 },
                    new Distance { DistanceId = 29, DepartureCity = "PhuQuoc", DestinationCity = "CanTho", Miles = 600 },
                    new Distance { DistanceId = 30, DepartureCity = "PhuQuoc", DestinationCity = "DaNang", Miles = 1200 }
                );
        }
    }
}
    