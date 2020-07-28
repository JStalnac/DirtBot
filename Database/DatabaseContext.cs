using System;
using DirtBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace DirtBot.Database
{
    public class DatabaseContext : DbContext
    {
        public virtual DbSet<GuildPrefix> Prefixes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            string mysqlConnectionString = new MySqlConnectionStringBuilder
            {
                // Don't hack my dev database lol
                Server = "localhost",
                UserID = "admin",
                Password = "1234",
                Database = "ef"
            }.ToString();
            options.UseMySql(mysqlConnectionString, o
                => o.ServerVersion(new Version(8, 0, 18), ServerType.MySql));
        }
    }
}