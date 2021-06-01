using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using static SkBootsBot.Config;


namespace SkBootsBot
{
    public static class Sqliter
    {
        public static void DB_init()
        {

            using (var connection = new SqliteConnection($"Data Source={DB_PATH}"))
            {
                connection.Open();

                String tableCommand = "CREATE TABLE IF NOT " +
                    "EXISTS favouriteBooks (user_id INTEGER, book_id TEXT, " +
                    "title TEXT, link TEXT)";

                SqliteCommand createTable = new SqliteCommand(tableCommand, connection);

                createTable.ExecuteReader();
            }
        }

        public static Dictionary<string, List<String>> getFavourites(int user_id)
        {
            Dictionary<string, List<String>> entries = new Dictionary<string, List<string>>();

            using (var connection = new SqliteConnection("Data Source=books.db"))
            {
                connection.Open();

                SqliteCommand selectCommand = new SqliteCommand();
                selectCommand.Connection = connection;

                selectCommand.CommandText = $"SELECT book_id, title, link FROM favouriteBooks WHERE user_id = {user_id}";

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {   
                    List<String> titledLinks = new List<string>();
                    titledLinks.Add(query.GetString(1));
                    titledLinks.Add(query.GetString(2));

                    entries.Add(query.GetString(0), titledLinks);
                }

                connection.Close();
            }
            
            return entries;
        }

        public static void addToFavourites(int user_id, string book_id, string title, string link)
        {   
            using (var connection = new SqliteConnection("Data Source=books.db"))
            {
                connection.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = connection;

                insertCommand.CommandText = "INSERT INTO favouriteBooks(user_id,book_id,title,link) " +
                    $"VALUES ({user_id},'{book_id}','{title}','{link}')";

                insertCommand.ExecuteReader();
            }
        }

        public static void removeFromFavourites(int user_id, string book_id)
        {
            using (var connection = new SqliteConnection("Data Source=books.db"))
            {
                connection.Open();

                SqliteCommand deleteCommand = new SqliteCommand();
                deleteCommand.Connection = connection;

                deleteCommand.CommandText = "DELETE FROM favouriteBooks " +
                    $"WHERE user_id = {user_id} AND book_id = '{book_id}'";

                deleteCommand.ExecuteReader();
            }
        }

        public static bool checkIfInFavourites(int user_id, string book_id)
        {
            Dictionary<string, List<String>> favourites = getFavourites(user_id);

            bool inFavourites = favourites.ContainsKey(book_id);
            return inFavourites;
        }
    }
}