using MySql.Data.MySqlClient;

namespace DirtBot.Database
{
    public static class DatabaseUtils
    {
        public static MySqlConnection OpenConnection()
        {
            MySqlConnection connection = new MySqlConnection($"Server={Config.DatabaseAddress};Database={Config.DatabaseName};Uid={Config.DatabaseUserName};Pwd={Config.DatabasePassword};");
            connection.Open();
            return connection;
        }
    }
}
