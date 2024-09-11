using IntroSE.Kanban.Backend.DataAccessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer.DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class BoardFacade
    {
        private readonly Dictionary<int, BoardBL> boardlist;
        private UserFacade userFacade;
        private BoardController boardController;
        private TaskController taskController;
        public BoardFacade(UserFacade userF)
        {
            userFacade = userF;
            boardlist = new Dictionary<int, BoardBL>();
            boardController = new BoardController();
            taskController = new TaskController();
        }
        public BoardBL CreateBoard(string email, string boardName) {
            userFacade.isConnected(email);
            foreach (var board in boardlist.Values)
            {
                if (board.BoardName == boardName && board.Users.Contains(email))
                {
                    throw new Exception("User cannot be part of two boards with the same name.");
                }
            }
            BoardBL newboard = new BoardBL(email, boardName);
            boardlist.Add(newboard.BoardID, newboard); 
            return newboard;
        }
        public void DeleteBoard(string email, string boardName) {
            userFacade.isConnected(email);
            BoardBL board = boardlist[GetBoardID(email, boardName)];
            board.CheckOwnership(email);
            List<TaskBL> tasks = board.Backlog;
            foreach (TaskBL task in tasks) { taskController.DeleteTask(board.BoardID, task.TaskId); }
            tasks = board.InProgress;
            foreach (TaskBL task in tasks) { taskController.DeleteTask(board.BoardID, task.TaskId); }
            tasks = board.Done;
            foreach (TaskBL task in tasks) { taskController.DeleteTask(board.BoardID, task.TaskId); }
            boardController.DeleteBoard(board.BoardID);
            boardlist.Remove(board.BoardID);
        }
        public List<TaskBL> GetColumn(string email, string boardName,  string columnName)
        {
            BoardBL board = boardlist[GetBoardID(email, boardName)];
            userFacade.DoesExist(email);
            switch (columnName)
            {
                case "backlog":
                    return board.Backlog;
                case "inprogress":
                    return board.InProgress;
                case "done":
                    return board.Done;
                default:
                    throw new Exception("Not an actual name of column");
            }
        }
        public void LimitColumn(string email, string boardName, string columnName, int maxSize)
        {
            userFacade.isConnected(email);
            BoardBL board = boardlist[GetBoardID(email, boardName)];
            switch (columnName)
            {
                case "backlog":
                    board.BacklogLimiter = maxSize;
                    break;

                case "inprogress":
                    board.InProgressLimiter = maxSize;
                    break;

                case "done":
                    board.DoneLimiter = maxSize;
                    break;
                default:
                    throw new Exception("Inserted wrong name of list");
            }
        }
        public void UpdateColumnOfTask(string email, string boardName, int taskId) {
            userFacade.isConnected(email);
            BoardBL board = boardlist[GetBoardID(email, boardName)];
            board.UpdateColumnOfTask(email, taskId);
        }
        public TaskBL EditTaskDesc(string email, string boardName, int taskId, string description)
        {
            userFacade.isConnected(email);
            BoardBL board = boardlist[GetBoardID(email, boardName)];
            TaskBL task = board.FindTask(taskId);
            task.EditTaskDesc(email, description);
            return task;
        }
        public TaskBL EditTaskTitle(string email, string boardName, int taskId, string title)
        {
            userFacade.isConnected(email);
            BoardBL board = boardlist[GetBoardID(email, boardName)];
            TaskBL task = board.FindTask(taskId);
            task.EditTaskTitle(email, title);
            return task;
        }
        public TaskBL EditTaskDate(string email, string boardName, int taskId, DateTime date) 
        {
            userFacade.isConnected(email);
            BoardBL board = boardlist[GetBoardID(email, boardName)];
            TaskBL task = board.FindTask(taskId);
            task.EditTaskDate(email, date);
            return task;
        }
        public TaskBL CreateTask(string email, string boardName, string title, string description, DateTime duedate) {
            userFacade.isConnected(email);
            int boardID = GetBoardID(email, boardName);
            BoardBL board = boardlist[boardID];
            TaskBL task = new TaskBL(email, boardID, duedate, title, description);
            board.AddTask(email, task);
            return task;
        }
        public List<TaskBL> TasksInProgress(string userEmail) {
            userFacade.isConnected(userEmail);
            List<TaskBL> inProgressList = new List<TaskBL>();
            foreach (BoardBL board in boardlist.Values)
            {
                if (board.Users.Contains(userEmail))
                {
                    List<TaskBL> inProgress = board.InProgress;
                    foreach (TaskBL task in inProgress)
                    {
                        if (task.Assignee == userEmail)
                        {
                            inProgressList.Add(task);
                        }
                    }
                }
            }
            return inProgressList;
        }
        public TaskBL FindTask(string email, string boardName, int taskId)
        {
            BoardBL board = boardlist[GetBoardID(email, boardName)];
            TaskBL task = board.FindTask(taskId);
            if (task == null) { throw new Exception("Task wasnt found"); }
            return task;
        }
        public int GetColumnLimit(string email, string boardName, string columnName)
        {
            BoardBL board = boardlist[GetBoardID(email, boardName)];
            return board.GetColumnLimit(columnName);
        }
        internal void TransferOwnership(string email, string recieverEmail, string boardName)
        {
            userFacade.isConnected(email);
            BoardBL board = boardlist[GetBoardID(email, boardName)];
            board.TransferOwnership(email, recieverEmail);
        }
        internal void JoinBoard(string email, int boardID)
        {
            userFacade.isConnected(email);
            BoardBL board = boardlist[boardID];
            board.JoinBoard(email);
        }
        internal void LeaveBoard(string email, int boardID)
        {
            userFacade.isConnected(email);
            BoardBL board = boardlist[boardID];
            board.LeaveBoard(email);
        }
        internal void AssignTask(string email, string emailAssignee, string boardName, int taskId)
        {
            userFacade.isConnected(email);
            BoardBL board = boardlist[GetBoardID(email, boardName)];
            board.AssignTask(email, emailAssignee, taskId);
        }
        internal void LoadBoards()
        {
            List<BoardDAO> boardsDAO = boardController.SelectAllBoards();
            foreach (BoardDAO boardDAO in boardsDAO)
            {
                List<string> users = boardController.GetBoardsUsers((int)boardDAO.BoardId);
                List<TaskDAO> backlogTasks = taskController.SelectTasksByBoardAndColumn((int)boardDAO.BoardId, 0);
                List<TaskDAO> inProgressTasks = taskController.SelectTasksByBoardAndColumn((int)boardDAO.BoardId, 1);
                List<TaskDAO> doneTasks = taskController.SelectTasksByBoardAndColumn((int)boardDAO.BoardId, 2);
                BoardBL newBoard = new BoardBL(boardDAO, users, backlogTasks, inProgressTasks, doneTasks);
                boardlist.Add(newBoard.BoardID, newBoard);
            }
        }
        internal void DeleteBoards()
        {
            taskController.DeleteAllTasks();
            boardController.DeleteAllBoards();
            boardlist.Clear();
        }

        internal int GetBoardID(string email, string boardName)
        {
            foreach(BoardBL board in boardlist.Values)
            {
                if (board.BoardName == boardName && board.Users.Contains(email))
                {
                    return board.BoardID;
                }
            }
            throw new Exception($"Board {boardName} with user {email} not found");
        }

        internal List<int> GetUserBoards(string email)
        {
            List<int> boards = new List<int>();
            userFacade.DoesExist(email);
            foreach (BoardBL board in boardlist.Values)
            {
                if (board.Users.Contains(email))
                {
                    boards.Add(board.BoardID);
                }
            }
            return boards;
        }

        internal string GetBoardName(int boardID)
        {
            BoardBL board = boardlist[boardID];
            return board.BoardName;
        }
    }
}
