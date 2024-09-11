using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.DataAccessLayer.DAOs
{
    internal class TaskDAO
    {
        public int TaskID 
        {
            get => taskID;
            set
                {
                taskID = value;
                if (isPersistent)
                {
                    taskController.Update(taskID, TaskColumnTaskID, taskID.ToString(), this);
                }
            }
                }
        private int taskID;
        public DateTime CreationTime 
        {   get => creationTime;
            set
            {
                creationTime = value;
                if (isPersistent)
                {
                    taskController.Update(taskID, TaskColumnCreationTime, value.ToString(), this);
                }
            }
        }
        private DateTime creationTime;
        public DateTime DueDate 
        {   get=> dueDate;
            set 
            {
                dueDate = value;
                if (!isPersistent)
                {
                    taskController.Update(taskID, TaskColumnDueDate, value.ToString(), this);
                }
            }
        }
        private DateTime dueDate;
        public string Title { get => title;
            set
            {
                title = value;
                if (isPersistent)
                {
                    taskController.Update(taskID, TaskColumnTitle, value.ToString(), this);
                }
            }
        }
        private string title;
        public string Description { get=>desc; 
            set
            {
                desc = value;
                if (isPersistent)
                {
                    taskController.Update(taskID, TaskColumnDesc, value.ToString(), this);
                }
            }
        }
        private string desc;
        public TaskController taskController { get; set; }
        private bool isPersistent;
        public string TaskColumnTaskID = "taskID";
        public string TaskColumnAsignee = "asignee";
        public string TaskColumnCreationTime = "creationTime";
        public string TaskColumnDueDate = "dueDate";
        public string TaskColumnDesc = "description";
        public string TaskColumnTitle = "title";
        public string TaskColumnDone = "done";

        private bool done;
        public bool Done { get => done; 
            set
            {
                done = value;
                if (isPersistent)
                {
                    taskController.Update(taskID, TaskColumnDone, value.ToString(), this);
                }
            }
        }
        public string Assignee { get=> asignee; 
            set 
            {
                asignee = value;
                if (isPersistent) 
                {
                    taskController.Update(taskID, TaskColumnAsignee, value.ToString(), this);
                }
            }
        }
        private string asignee;
        public TaskDAO(string assignee, DateTime creationTime, DateTime dueDate, string title, string description, int boardID)
        {
            isPersistent = false; // Initially not persistent
            taskController = new TaskController();
            Assignee = assignee;
            CreationTime = creationTime;
            DueDate = dueDate;
            Title = title;
            Description = description;
            TaskID = taskController.GetMaxTaskIDPerBoard(boardID) + 1; // Unique per board
            Done = false;
        }



        public void Persist(int boardID)
        {
            if (!isPersistent)
            {
                taskController.InsertTaskWithStatus(this, boardID);
                isPersistent = true;
            }
        }
        public TaskDAO(int taskID, string assignee, DateTime creationTime, DateTime dueDate, string description, string title)
        {
            Assignee = assignee;
            CreationTime = creationTime;
            DueDate = dueDate;
            Title = title;
            Description = description;
            taskController = new TaskController();
            TaskID = taskID;
            isPersistent = true;
        }
        public int GetID()
        {
            return TaskID;
        }
    }
}
