using IntroSE.Kanban.Backend.BusinessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class TaskSL
    {
        public int Id { get; set; }
        public DateTime CreationTime { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string AsigneeEmail { get; set; }

        internal TaskSL(TaskBL taskBL)
        {
            Id = taskBL.TaskId;
            Title = taskBL.Title;
            Description = taskBL.Description;
            DueDate = taskBL.DueDate;
            CreationTime = taskBL.CreationTime;
        }
    }
}
