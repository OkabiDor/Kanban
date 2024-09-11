using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using IntroSE.Kanban.Backend.DataAccessLayer.DAOs;

namespace IntroSE.Kanban.Backend.DataAccessLayer
{
    internal class UserController
    {
        private readonly string _connectionString;
        private readonly string TableName;
        private readonly string userBoardTable;
        private const string UserTableName = "User";
        private const string UserBoardTableName = "UserBoardRole";

        public UserController()
        {
            // Get the directory of the executing assembly
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Adjust the path to point to the correct location of the database file
            string relativePath = Path.Combine(baseDirectory, "../../../../Backend/DataAccessLayer/kanban.db");
            string path = Path.GetFullPath(relativePath);
            Console.WriteLine("Database Path: " + path); // Log the database path

            this._connectionString = $"Data Source={path};Version=3;";
            this.TableName = UserTableName;
            this.userBoardTable = UserBoardTableName;
        }
        public bool Insert(UserDAO userDAO)
        {
            int res = -1;
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(connection))
                    {
                        // Define the SQL command with parameters
                        string insert = "INSERT INTO User (email, password) VALUES (@Email, @Password)";
                        command.CommandText = insert;

                        // Add parameters to the command
                        command.Parameters.AddWithValue("@Email", userDAO.Email);
                        command.Parameters.AddWithValue("@Password", userDAO.Password);

                        // Execute the command
                        res = command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            return res > 0;
        }
        public void Update(string userEmail, string columnName, string newValue, UserDAO userDAO)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                try
                {
                    SQLiteCommand command = new SQLiteCommand(null, connection);
                    string update = $"UPDATE {TableName} " + $"SET @columnName = @newValue" + $"WHERE {userDAO.Email} = @userEmail";
                    SQLiteParameter columnNameParam = new SQLiteParameter(@"columnName", columnName);
                    SQLiteParameter newValueParam = new SQLiteParameter(@"newValue", newValue);
                    SQLiteParameter userEmailParam = new SQLiteParameter(@"userEmail", userEmail);
                    command.CommandText = update;
                    command.Parameters.Add(columnNameParam);
                    command.Parameters.Add(newValueParam);
                    command.Parameters.Add(userEmailParam);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return;
                }
            }
        }
        public void DeleteUser(string userEmail)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
                try
                {
                    {
                        SQLiteCommand command = new SQLiteCommand(null, connection);
                        string delete = $"DELETE FROM {TableName} WHERE email = @Email";
                        SQLiteParameter columnNameParam = new SQLiteParameter(@"email", userEmail);
                        command.CommandText = delete;
                        command.Parameters.Add(columnNameParam);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
        }
        public List<UserDAO> SelectAllUsers()
        {
            List<UserDAO> result = new List<UserDAO>();
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                command.CommandText = $"SELECT * FROM {TableName}";
                using (SQLiteDataReader dataReader = command.ExecuteReader())
                {
                    try
                    {
                        connection.Open();
                        while (dataReader.Read())
                        {
                            result.Add(ConvertReaderToObject(dataReader));
                        }
                    }
                    finally
                    {
                        if (dataReader != null)
                        {
                            dataReader.Close();
                        }
                    }
                }
            }
            return result;
        }
        public UserDAO ConvertReaderToObject(SQLiteDataReader dataReader)
        {
            return new UserDAO(dataReader.GetString(0), dataReader.GetString(1), true);
        }
        public void TranswerOwnership(string email, string boardName) { }

        public void DeleteAllUsers()
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                try
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Delete all entries from UserBoardRole related to the users
                            using (var deleteUserBoardRoleCommand = new SQLiteCommand($"DELETE FROM {userBoardTable} WHERE email IN (SELECT email FROM {TableName})", connection))
                            {
                                deleteUserBoardRoleCommand.ExecuteNonQuery();
                            }

                            // Delete all users
                            using (var deleteUserCommand = new SQLiteCommand($"DELETE FROM {TableName}", connection))
                            {
                                deleteUserCommand.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

    }
}
