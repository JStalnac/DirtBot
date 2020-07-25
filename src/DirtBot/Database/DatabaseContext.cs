using DirtBot.Database.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DirtBot.Database
{
    public class DatabaseContext : DbContext
    {
        public virtual DbSet<GuildPrefix> Prefixes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = "sqlite.db",
                Password = "abcd"
            };

            var connection = new SqliteConnection(connectionString.ToString());
            connection.Open();

            optionsBuilder.UseSqlite(connection);
        }
    }
}