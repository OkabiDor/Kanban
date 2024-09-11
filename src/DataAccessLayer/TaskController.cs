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
    internal class TaskController
    {
        private readonly string _connectionString;
        private readonly string _tableName;
        private const string TableName = "Task";
        private const string taskBoardTable = "TaskBoardStatus";

        public TaskController()
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

        public bool InsertTaskWithStatus(TaskDAO taskDAO, int boardID)
        {
            using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
            {
                try
                {
                    connection.Open();

                    // Start a transaction to ensure atomicity
                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (SQLiteCommand command = new SQLiteCommand(connection))
                            {
                                // Insert into the Task table
                                string insertTask = "INSERT INTO Task (asignee, creationTime, dueDate, title, description, done) " +
                                                    "VALUES (@assignee, @creationTime, @dueDate, @title, @description, @done)";

                                command.CommandText = insertTask;
                                command.Parameters.AddWithValue("@assignee", taskDAO.Assignee);
                                command.Parameters.AddWithValue("@creationTime", taskDAO.CreationTime);
                                command.Parameters.AddWithValue("@dueDate", taskDAO.DueDate);
                                command.Parameters.AddWithValue("@title", taskDAO.Title);
                                command.Parameters.AddWithValue("@description", taskDAO.Description);
                                command.Parameters.AddWithValue("@done", taskDAO.Done);

                                command.ExecuteNonQuery();

                                // Retrieve the last inserted taskID
                                command.CommandText = "SELECT last_insert_rowid()";
                                int taskID = (int)(long)command.ExecuteScalar();

                                // Insert into the TaskBoardStatus table
                                string insertTaskBoardStatus = "INSERT INTO TaskBoardStatus (boardID, taskID, columnStatus, taskIDPerBoard) " +
                                                               "VALUES (@boardID, @taskID, @columnStatus, @taskIDPerBoard)";
                                command.CommandText = insertTaskBoardStatus;
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@boardID", boardID);
                                command.Parameters.AddWithValue("@taskID", taskID);
                                command.Parameters.AddWithValue("@columnStatus", 0); // Assuming default columnStatus is 0
                                command.Parameters.AddWithValue("@taskIDPerBoard", taskDAO.TaskID); // Ensure TaskDAO has TaskID property

                                int res = command.ExecuteNonQuery();

                                // Commit the transaction
                                transaction.Commit();

                                return res > 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            // Rollback the transaction if any error occurs
                            transaction.Rollback();
                            Console.WriteLine(ex.ToString());
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
        }





        public void Update(long taskID, string columnName, string newValue, TaskDAO taskDAO)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                try
                {
                    connection.Open();
                    string update = $"UPDATE {TableName} SET {columnName} = @newValue WHERE taskID = @taskID";
                    using (SQLiteCommand command = new SQLiteCommand(update, connection))
                    {
                        command.Parameters.AddWithValue("@newValue", newValue);
                        command.Parameters.AddWithValue("@taskID", taskID);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }


        public void DeleteTask(long taskID, long boardID)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
                try
                {
                    {
                        SQLiteCommand command = new SQLiteCommand(null, connection);
                        string delete = $"DELETE FROM {TableName} WHERE taskID = @taskID";
                        string deleteFromHelpere = $"DELETE FROM {taskBoardTable} WHERE taskID = @taskID AND boardID = @boardID";
                        SQLiteParameter columnNameParam = new SQLiteParameter(@"taskID", taskID);
                        command.CommandText = delete;
                        command.Parameters.Add(columnNameParam);
                        connection.Open();
                        command.ExecuteNonQuery();
                        SQLiteParameter columnIDParam = new SQLiteParameter(@"taskID", taskID);
                        SQLiteParameter columnNameParam2 = new SQLiteParameter(@"boardID", boardID);
                        command.Parameters.Add(columnNameParam);
                        command.Parameters.Add(columnNameParam2);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
        }
        public List<TaskDAO> SelectTasksByBoardAndColumn(int boardID, int columnStatus)
        {
            List<TaskDAO> result = new List<TaskDAO>();
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                try
                {
                    connection.Open();
                    command.CommandText = @"
                    SELECT t.*
                    FROM TaskBoardStatus tbs
                    JOIN Task t ON tbs.taskID = t.taskID
                    WHERE tbs.boardID = @boardID AND tbs.columnStatus = @columnStatus";
                    command.Parameters.AddWithValue("@boardID", boardID);
                    command.Parameters.AddWithValue("@columnStatus", columnStatus);
                    command.Prepare();

                    using (SQLiteDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            result.Add(ConvertReaderToObject(dataReader));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
            return result;
        }

        public TaskDAO ConvertReaderToObject(SQLiteDataReader dataReader)
        {
            return new TaskDAO((int)dataReader.GetValue(0), dataReader.GetString(1), (DateTime)dataReader.GetValue(2), (DateTime)dataReader.GetValue(3), dataReader.GetString(4), dataReader.GetString(5));
        }
        public void DeleteAllTasks()
        {
            using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
            {
                try
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand("DELETE FROM TaskBoardStatus", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    using (SQLiteCommand command = new SQLiteCommand("DELETE FROM Task", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        public int GetMaxTaskIDPerBoard(int boardID)
        {
            using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
            {
                try
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        string select = "SELECT IFNULL(MAX(taskIDperBoard), 0) FROM TaskBoardStatus WHERE boardID = @boardID";
                        command.CommandText = select;
                        command.Parameters.AddWithValue("@boardID", boardID);
                        object result = command.ExecuteScalar();
                        return Convert.ToInt32(result); // This will return 0 if there are no tasks
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return -1; // Indicate an error
                }
                finally
                {
                    connection.Close();
                }
            }
        }




    }
}
