using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.DataAccessLayer.DAOs;
using IntroSE.Kanban.Backend.ServiceLayer;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class BoardBL
    {
        private string boardName;
        private List<string> users;
        private List<TaskBL> backlog;
        private List<TaskBL> inProgress;
        private List<TaskBL> done;
        private int backlogLimiter;
        private int inProgressLimiter;
        private int doneLimiter;
        private BoardDAO daoBoard;
        private int boardID;
        private string ownerEmail;
        internal string BoardName
        {
            get => boardName;
            set
            {
                if (value == null) { throw new ArgumentNullException("Board name is null"); }
                daoBoard.Name = value;
                boardName = value;
            }
        }
        internal List<string> Users { get => users; set 
            {
                if (value == null) { throw new ArgumentNullException("Users is null"); }
                users = value; 
            } 
        }
        internal List<TaskBL> Backlog
        {
            get => backlog;
            set
            {
                if (value == null) { throw new ArgumentNullException("Backlog is null"); }
                backlog = value;
            }
        }
        internal List<TaskBL> InProgress
        {
            get => inProgress;
            set
            {
                if (value == null) { throw new ArgumentNullException("InProgress is null"); }
                inProgress = value;
            }
        }
        internal List<TaskBL> Done
        {
            get => done;
            set
            {
                if (value == null) { throw new ArgumentNullException("Done is null"); }
                done = value;
            }
        }
        internal int BacklogLimiter
        {
            get => backlogLimiter;
            set
            {
                if (value < backlog.Count && value != -1)
                {
                    throw new ArgumentOutOfRangeException("limit is lower than actual size");
                }
                backlogLimiter = value;
                daoBoard.UpdateColumnLimit("limitBackLog", value);
            }
        }

        internal int InProgressLimiter
        {
            get => inProgressLimiter;
            set
            {
                if (value < inProgress.Count && value != -1)
                {
                    throw new ArgumentOutOfRangeException("limit is lower than actual size");
                }
                inProgressLimiter = value;
                daoBoard.UpdateColumnLimit("limitProgress", value);
            }
        }

        internal int DoneLimiter
        {
            get => doneLimiter;
            set
            {
                if (value < done.Count && value != -1)
                {
                    throw new ArgumentOutOfRangeException("limit is lower than actual size");
                }
                doneLimiter = value;
                daoBoard.UpdateColumnLimit("limitDone", value);
            }
        }

        internal int BoardID { get => boardID; set 
            { 
                if (value < 0) { throw new ArgumentOutOfRangeException("BoardID is negative"); }
                boardID = value; 
            }
        }
        internal string OwnerEmail { get => ownerEmail; set 
            {
                if (value == null) { throw new ArgumentNullException("OwnerEmail is null"); }
                ownerEmail = value; 
            } 
        }
        internal BoardBL(string email, string boardName)
        {

            daoBoard = new BoardDAO(email, boardName);
            BoardName = boardName;
            OwnerEmail = email;
            Backlog = new List<TaskBL>();
            InProgress = new List<TaskBL>();
            Done = new List<TaskBL>();
            Users = new List<string>();
            users.Add(email);
            BacklogLimiter = -1;
            InProgressLimiter = -1;
            DoneLimiter = -1;
            daoBoard.persist();
            BoardID = daoBoard.BoardId;
        }
        internal BoardBL(BoardDAO dao, List<string> usersList, List<TaskDAO> backlogList, List<TaskDAO> inProgressList, List<TaskDAO> doneList)
        {
            BoardName = dao.Name;
            Users = usersList;
            OwnerEmail = dao.OwnerEmail;
            Backlog = backlogList.Select(task => new TaskBL(task)).ToList();
            InProgress = inProgressList.Select(task => new TaskBL(task)).ToList();
            Done = doneList.Select(task => new TaskBL(task)).ToList();
            BacklogLimiter = dao.BacklogLimiter;
            InProgressLimiter = dao.InProgressLimiter;
            DoneLimiter = dao.DoneLimiter;
            daoBoard = dao;
            BoardID = daoBoard.BoardId;
        }
        public void AddTask(string email, TaskBL newTask)
        {
            if (!users.Contains(email)) { throw new Exception($"User {email} isn't in board {boardName}"); }
            if (backlog.Count < backlogLimiter || backlogLimiter==-1)
            {
                backlog.Add(newTask);
            }
            else { throw new Exception($"In board {boardName}, backlog is full"); }
        }
        public TaskBL FindTask(int taskID)
        {
            foreach (TaskBL task in backlog)
            {
                if (task.TaskId == taskID)
                {
                    return task;
                }
            }
            foreach (TaskBL task in inProgress)
            {
                if (task.TaskId == taskID)
                {
                    return task;
                }
            }
            foreach (TaskBL task in done)
            {
                if (task.TaskId == taskID)
                {
                    return task;
                }
            }
            throw new Exception($"Task {taskID} doesn't exists in board {boardName}");
        }
        public void UpdateColumnOfTask(string email, int taskID)
        {
            TaskBL task = FindTask(taskID);
            task.CheckAssignee(email);
            if (backlog.Contains(task)) 
            { 
                if(inProgressLimiter != -1 && inProgressLimiter <= inProgress.Count)
                {
                    throw new Exception($"In board {boardName}, inProgress is full");
                }
                daoBoard.UpdateColumnToInProgress(taskID);
                inProgress.Add(task);
                backlog.Remove(task);
            }
            else if (inProgress.Contains(task)) 
            {
                if (doneLimiter != -1 && doneLimiter <= done.Count)
                {
                    throw new Exception($"In board {boardName}, done is full");
                }
                daoBoard.UpdateColumnToDone(taskID);
                inProgress.Remove(task);
                task.Done = true;
                done.Add(task);
            }
            else if(done.Contains(task)) { throw new Exception($"Task {taskID} is in done"); }
            else { throw new Exception($"Task {taskID} doesn't exists in board {boardName}"); }
        }
        public void CheckOwnership(string email)
        {
            if (!users.Contains(email)) { throw new Exception($"User {email} isn't in board {boardName}"); }
            if (email != OwnerEmail) { throw new Exception($"User {email} isn't the owner of board {boardName}"); }
        }
        internal void TransferOwnership(string email, string recieverEmail)
        {
            CheckOwnership(email);
            if(!users.Contains(recieverEmail)) { throw new Exception($"User {recieverEmail} isn't in board {boardName}"); }
            OwnerEmail = recieverEmail;
        }
        internal void JoinBoard(string email)
        {
            if (users.Contains(email))
            {
                throw new Exception($"User {email} is already in board {boardName}");
            }
            daoBoard.AddUserToBoard(email, "User");
            users.Add(email);
        }

        internal void LeaveBoard(string email)
        {
            if (!users.Contains(email)) { throw new Exception($"User {email} isn't in board {boardName}"); }
            if (email == OwnerEmail) { throw new Exception($"User {email} is the owner of board {boardName}"); }
            daoBoard.RemoveUserFromBoard(email);
            users.Remove(email);
            foreach (TaskBL task in backlog) { if (task.Assignee == email) { task.DeleteAssignee(); } }
            foreach (TaskBL task in inProgress) { if (task.Assignee == email) { task.DeleteAssignee(); } }
        }
        internal int GetColumnLimit(string columnName)
        {
            switch (columnName)
            {
                case "backlog":
                    return BacklogLimiter;
                case "inprogress":
                    return InProgressLimiter;
                case "done":
                    return DoneLimiter;
                default:
                    throw new ArgumentOutOfRangeException(nameof(columnName), "Invalid column ordinal value");
            }
        }
        internal void AssignTask(string email, string assignee, int taskID)
        {
            if (!users.Contains(email))
            {
                throw new Exception($"User {email} isn't in board {boardName}");
            }
            if (!users.Contains(assignee))
            {
                throw new Exception($"User {assignee} isn't in board {boardName}");
            }
            TaskBL task = FindTask(taskID);
            if (task == null)
            {
                throw new Exception($"Task with ID {taskID} not found");
            }
            task.EditAssignee(email, assignee);
        }

    }
}
