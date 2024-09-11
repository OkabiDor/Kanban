using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using IntroSE.Kanban.Backend.DataAccessLayer.DAOs;
using System.Linq.Expressions;

namespace IntroSE.Kanban.Backend.DataAccessLayer
{
    internal class BoardController
    {
        private readonly string _connectionString;
        private readonly string _tableName;
        private const string TableName = "Board";
        private const string userBoardTable = "UserBoardRole";
        private const string taskBoardTable = "TaskBoardStatus";

        public BoardController()
        {
            // Get the directory of the executing assembly
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Adjust the path to point to the correct location of the database file
            string relativePath = Path.Combine(baseDirectory, "../../../../Backend/DataAccessLayer/kanban.db");
            string path = Path.GetFullPath(relativePath);
            Console.WriteLine("Database Path: " + path); // Log the database path

            this._connectionString = $"Data Source={path};Version=3;";
            this._tableName = TableName;
        }
        public bool Insert(BoardDAO boardDAO)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                try
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        // Insert the board
                        using (var insertCommand = new SQLiteCommand(connection))
                        {
                            insertCommand.CommandText = $"INSERT INTO {TableName} (boardName, limitBackLog, limitProgress, limitDone) " +
                                                        $"VALUES (@name, @backlogLimiter, @inProgressLimiter, @doneLimiter)";
                            insertCommand.Parameters.AddWithValue("@name", boardDAO.Name);
                            insertCommand.Parameters.AddWithValue("@backlogLimiter", boardDAO.BacklogLimiter);
                            insertCommand.Parameters.AddWithValue("@inProgressLimiter", boardDAO.InProgressLimiter);
                            insertCommand.Parameters.AddWithValue("@doneLimiter", boardDAO.DoneLimiter);
                            insertCommand.ExecuteNonQuery();

                            // Retrieve the last inserted ID
                            int newID = -1;
                            using (var cmd = new SQLiteCommand(connection))
                            {
                                cmd.CommandText = "SELECT id FROM Board WHERE boardName = @boardName";
                                cmd.Parameters.AddWithValue("@boardName", boardDAO.Name);
                                object result = cmd.ExecuteScalar();
                                newID = Convert.ToInt32(result);
                            }
                            using (var checkzero = new SQLiteCommand(connection))
                            {
                                checkzero.CommandText = $"SELECT COUNT(*) FROM {TableName}";
                                object result = checkzero.ExecuteScalar();
                                if (Convert.ToInt32(result) == 1)
                                {
                                    boardDAO.BoardId = (newID - 1);
                                }
                                else
                                {
                                    boardDAO.BoardId = newID;
                                }
                            }

                            // Accurate the ID to minus one
                            string updateID = $"UPDATE {TableName} SET id = @newID WHERE boardName = @boardName";
                            using (var updateIDCommand = new SQLiteCommand(updateID, connection))
                            {
                                SQLiteParameter newIDParam = new SQLiteParameter("@newID", boardDAO.BoardId);
                                SQLiteParameter oldIDParam = new SQLiteParameter("@boardName", boardDAO.Name);
                                updateIDCommand.Parameters.Add(newIDParam);
                                updateIDCommand.Parameters.Add(oldIDParam);
                                updateIDCommand.ExecuteNonQuery();
                            }
                        }

                        // Insert into UserBoardRole table
                        using (var userBoardRoleCommand = new SQLiteCommand(connection))
                        {
                            userBoardRoleCommand.CommandText = $"INSERT INTO {userBoardTable} (email, boardID, role) VALUES (@Email, @BoardID, @Role)";
                            userBoardRoleCommand.Parameters.AddWithValue("@Email", boardDAO.OwnerEmail);
                            userBoardRoleCommand.Parameters.AddWithValue("@BoardID", boardDAO.BoardId);
                            userBoardRoleCommand.Parameters.AddWithValue("@Role", "owner");
                            userBoardRoleCommand.ExecuteNonQuery();
                        }

                        // Commit transaction
                        transaction.Commit();
                    }
                    connection.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to insert board: " + ex.ToString());
                    return false;
                }
            }
        }


        public void Update(long boardID, string columnName, string newValue)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                try
                {
                    SQLiteCommand command = new SQLiteCommand(null, connection);
                    string update = $"UPDATE {TableName} " + $"SET @columnName = @newValue" + $"WHERE id = @boardID";
                    SQLiteParameter columnNameParam = new SQLiteParameter(@"columnName", columnName);
                    SQLiteParameter newValueParam = new SQLiteParameter(@"newValue", newValue);
                    SQLiteParameter boardIdParam = new SQLiteParameter(@"boardID", boardID);
                    command.CommandText = update;
                    command.Parameters.Add(columnNameParam);
                    command.Parameters.Add(newValueParam);
                    command.Parameters.Add(boardIdParam);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return;
                }
            }
        }
        public void UpdateColumnLimit(long boardID, string columnName, int newLimit)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(connection))
                    {
                        // Ensure columnName is sanitized to avoid SQL injection
                        string update = $"UPDATE {TableName} SET {columnName} = @newLimit WHERE id = @boardID";
                        command.CommandText = update;
                        command.Parameters.AddWithValue("@newLimit", newLimit);
                        command.Parameters.AddWithValue("@boardID", boardID);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public bool IsUserAssociatedWithBoard(string email, string boardName)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();
                string query = @"
            SELECT COUNT(*)
            FROM UserBoardRole ubr
            JOIN Board b ON ubr.boardID = b.id
            WHERE ubr.email = @Email AND b.boardName = @BoardName";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@BoardName", boardName);
                    long count = (long)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public List<BoardDAO> SelectAllBoards()
        {
            List<BoardDAO> result = new List<BoardDAO>();
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
        public BoardDAO SelectBoard(long boardId)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                command.CommandText = $"SELECT * FROM {TableName}" + $"WHERE id = @boardID";
                command.Parameters.Add(new SQLiteParameter(@"boardID", boardId));
                using (SQLiteDataReader dataReader = command.ExecuteReader())
                {
                    try
                    {
                        connection.Open();
                        if (dataReader.Read())
                        {
                            return ConvertReaderToObject(dataReader);
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
            return null;
        }
        public void DeleteBoard(long boardID)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                try
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        // Delete from UserBoardRole table
                        using (var deleteUserBoardRoleCommand = new SQLiteCommand(connection))
                        {
                            deleteUserBoardRoleCommand.CommandText = $"DELETE FROM {userBoardTable} WHERE boardID = @boardID";
                            deleteUserBoardRoleCommand.Parameters.AddWithValue("@boardID", boardID);
                            deleteUserBoardRoleCommand.ExecuteNonQuery();
                        }

                        // Delete from Board table
                        using (var deleteBoardCommand = new SQLiteCommand(connection))
                        {
                            deleteBoardCommand.CommandText = $"DELETE FROM {TableName} WHERE id = @boardID";
                            deleteBoardCommand.Parameters.AddWithValue("@boardID", boardID);
                            deleteBoardCommand.ExecuteNonQuery();
                        }

                        // Commit transaction
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public BoardDAO ConvertReaderToObject(SQLiteDataReader dataReader)
        {
            return new BoardDAO(dataReader.GetString(0), (int)dataReader.GetValue(1), (int)dataReader.GetValue(2), (int)dataReader.GetValue(3), (int)dataReader.GetValue(4));
        }
        public void PromoteTask(long boardID, int newColumn, long taskID)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                try
                {
                    SQLiteCommand command = new SQLiteCommand(null, connection);
                    string update = $"UPDATE TaskBoardStatus SET columnStatus = @newColumn WHERE boardID = @boardID AND taskID = @taskID";
                    SQLiteParameter newColumnParam = new SQLiteParameter("@newColumn", newColumn);
                    SQLiteParameter boardIDParam = new SQLiteParameter("@boardID", boardID);
                    SQLiteParameter taskIDParam = new SQLiteParameter("@taskID", taskID);
                    command.CommandText = update;
                    command.Parameters.Add(newColumnParam);
                    command.Parameters.Add(boardIDParam);
                    command.Parameters.Add(taskIDParam);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }



        public void AddUserToBoard(string email, long boardId, string role)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                try
                {
                    connection.Open();

                    // Insert into UserBoardRole table
                    string insert = $"INSERT INTO {userBoardTable} (email, boardID, role) VALUES (@Email, @BoardID, @Role)";
                    using (var command = new SQLiteCommand(insert, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@BoardID", boardId);
                        command.Parameters.AddWithValue("@Role", role);

                        command.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Constraint)
                {
                    Console.WriteLine($"Constraint error: {ex.Message}");
                    throw new Exception("User is already associated with this board.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw;
                }
            }
        }



        internal void RemoveUserFromBoard(string email, long boardId)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                string delete = $"DELETE FROM {userBoardTable} WHERE email = @email AND boardID = @boardID";
                SQLiteParameter emailParam = new SQLiteParameter(@"email", email);
                SQLiteParameter boardIDParam = new SQLiteParameter(@"boardID", boardId);
                command.CommandText = delete;
                command.Parameters.Add(emailParam);
                command.Parameters.Add(boardIDParam);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteAllBoards()
        {
            using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
            {
                try
                {
                    connection.Open();

                    // Start a transaction
                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Delete all entries from TaskBoardStatus related to the boards
                            using (SQLiteCommand deleteTaskBoardStatusCommand = new SQLiteCommand("DELETE FROM TaskBoardStatus WHERE boardID IN (SELECT boardID FROM Board)", connection))
                            {
                                deleteTaskBoardStatusCommand.ExecuteNonQuery();
                            }

                            // Delete all entries from Task related to the boards
                            using (SQLiteCommand deleteTasksCommand = new SQLiteCommand("DELETE FROM Task WHERE taskID IN (SELECT taskID FROM TaskBoardStatus)", connection))
                            {
                                deleteTasksCommand.ExecuteNonQuery();
                            }

                            // Delete all entries from UserBoardRole related to the boards
                            using (SQLiteCommand deleteUserBoardRoleCommand = new SQLiteCommand("DELETE FROM UserBoardRole WHERE boardID IN (SELECT boardID FROM Board)", connection))
                            {
                                deleteUserBoardRoleCommand.ExecuteNonQuery();
                            }

                            // Delete all boards
                            using (SQLiteCommand deleteBoardsCommand = new SQLiteCommand("DELETE FROM Board", connection))
                            {
                                deleteBoardsCommand.ExecuteNonQuery();
                            }

                            // Commit the transaction
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            // Rollback the transaction if an error occurs
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



        public List<string> GetBoardsUsers(int boardID)
        {
            List<string> users = new List<string>();
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(connection))
                    {
                        // Construct the SQL query to get the users of a specific board
                        command.CommandText = @"
                         SELECT u.email
                         FROM UserBoardRole ubr
                         JOIN User u ON ubr.email = u.email
                         WHERE ubr.boardID = @boardID";
                        command.Parameters.AddWithValue("@boardID", boardID);

                        using (SQLiteDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                users.Add(dataReader["email"].ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions (e.g., log the error)
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
            return users;
        }

        public void UpdateBoardRole(string email, string columnName, string newValue)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                try
                {
                    SQLiteCommand command = new SQLiteCommand(null, connection);
                    string update = $"UPDATE {userBoardTable} " + $"SET @columnName = @newValue" + $"WHERE email = @email";
                    SQLiteParameter columnNameParam = new SQLiteParameter(@"columnName", columnName);
                    SQLiteParameter newValueParam = new SQLiteParameter(@"newValue", newValue);
                    SQLiteParameter boardIdParam = new SQLiteParameter(@"email", email);
                    command.CommandText = update;
                    command.Parameters.Add(columnNameParam);
                    command.Parameters.Add(newValueParam);
                    command.Parameters.Add(boardIdParam);
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

        internal int GetMaxID()
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                try
                {
                    SQLiteCommand command = new SQLiteCommand(null, connection);
                    string select = $"SELECT MAX(boardID) FROM {TableName}";
                    command.CommandText = select;
                    connection.Open();
                    SQLiteDataReader reader = command.ExecuteReader();

                    if (reader.Read() && !reader.IsDBNull(0))
                    {
                        return reader.GetInt32(0);
                    }
                    else
                    {
                        return 0; // Return 0 if no rows are found
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return 0; // Return 0 in case of an error
                }
                finally
                {
                    connection.Close();
                }
            }
        }

    }
}
 