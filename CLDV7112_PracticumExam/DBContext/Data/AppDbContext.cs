using CLDV7112_PracticumExam.Models;
using Microsoft.EntityFrameworkCore;

namespace CLDV7112_PracticumExam.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Device> Devices => Set<Device>();
}