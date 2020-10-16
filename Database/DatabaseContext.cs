using DirtBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace DirtBot.Database
{
    public class DatabaseContext : DbContext
    {
        public virtual DbSet<GuildPrefix> Prefixes { get; set; }

        public virtual DbSet<LanguageData> Languages { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
    }
}