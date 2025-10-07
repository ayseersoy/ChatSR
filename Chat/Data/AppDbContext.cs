using Microsoft.EntityFrameworkCore;
using Chat.Models;
public class AppDbContext : DbContext
    {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

}

