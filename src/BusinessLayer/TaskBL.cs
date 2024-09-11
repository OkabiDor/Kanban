using IntroSE.Kanban.Backend.DataAccessLayer.DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class TaskBL
    {
        private int taskId;
        private DateTime creationTime;
        private DateTime dueDate;
        private string title;
        private string description;
        private bool done;
        private TaskDAO daoTask;
        private string assignee;
        public int TaskId { get => taskId;
            set
            {
                if (value < 0) { throw new ArgumentException("TaskId is negative"); }
                daoTask.TaskID = value;
                taskId = value;
            }
        }
        internal DateTime CreationTime { get => creationTime; }
        internal DateTime DueDate { get => dueDate; 
            set
            {
                if (value == null) { throw new ArgumentNullException("DueDate is null"); }
                if (done) { throw new Exception($"Task {taskId} is done and cannot be edited."); }
                if (creationTime > value) { throw new ArgumentException($"The dueDate: {value.ToString()} is before NOW DateTime"); }
                daoTask.DueDate = value;
                dueDate = value;
            }
        }
        internal string Title { get => title; 
            set
            {
                if (value == null) { throw new ArgumentNullException("Title is null"); }
                if (value.Length == 0) { throw new ArgumentException("Title is empty"); }
                if (value.Length > 50) { throw new ArgumentException("Title has more than 50 characters"); }
                daoTask.Title = value;
                title = value;
            }
        }
        internal string Description { get => description; 
            set
            {
                if (value == null) { throw new ArgumentNullException("Description is null"); }
                if (value.Length > 300) { throw new ArgumentException("Description has more than 300 characters"); }
                daoTask.Description = value;
                description = value;
            }
        }
        internal bool Done
        {
            get => done;
            set
            {
                daoTask.Done = value;
                done = value;
            }
        }

        internal string Assignee
        {
            get => assignee;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Assignee is null");
                }
                daoTask.Assignee = value;
                assignee = value;
            }
        }

        internal TaskDAO DaoTask { get => daoTask; }
        public TaskBL(string email, int boardID, DateTime dueDate, string title, string description)
        {
            daoTask = new TaskDAO(email, DateTime.Now, dueDate, title, description, boardID);
            Done = false;
            creationTime = DateTime.Now;
            DueDate = dueDate;
            Title = title;
            Description = description;
            Assignee = email;
            daoTask.Persist(boardID);
            TaskId = daoTask.TaskID; // Set the TaskId to be taskID
        }


        public TaskBL(TaskDAO dao)
        {
            creationTime = dao.CreationTime;
            Done = dao.Done;
            DueDate = dao.DueDate;
            Title = dao.Title;
            Description = dao.Description;
            Assignee = dao.Assignee;
            TaskId = dao.TaskID;
            daoTask = dao;
        }
        internal void DeleteAssignee()
        {
            daoTask.Assignee = null;
            Assignee = null;
            
        }
        internal void CheckAssignee(string email)
        {
            if (Assignee != email) { throw new Exception($"User {email} is not the assignee of the task {TaskId}"); }
        }
        internal void EditTaskDesc(string email, string description)
        {
            CheckAssignee(email);
            Description = description;
        }
        internal void EditTaskTitle(string email, string title)
        {
            CheckAssignee(email);
            Title = title;
        }
        internal void EditTaskDate(string email, DateTime date)
        {
            CheckAssignee(email);
            DueDate = date;
        }
        internal void EditAssignee(string email, string assignee)
        {
            if (Assignee == email || Assignee == null)
            {
                Assignee = assignee;
            }
            else
            {
                throw new Exception($"Current assignee is not the assignee of the task {TaskId} or null");
            }
        }

    }
}
