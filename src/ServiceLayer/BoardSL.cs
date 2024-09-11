using IntroSE.Kanban.Backend.BusinessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class BoardSL
    {
        public string boardName;
        public List<TaskSL> backlog;
        public List<TaskSL> inProgress;
        public List<TaskSL> done;
        public string ownerEmail;

        internal BoardSL(BoardBL boardBL)
        {
            this.boardName = boardBL.BoardName;
            foreach(TaskBL t in boardBL.Backlog)
            {
                this.backlog.Add(new TaskSL(t));
            }
            foreach (TaskBL t in boardBL.InProgress)
            {
                this.inProgress.Add(new TaskSL(t));
            }
            foreach (TaskBL t in boardBL.Done)
            {
                this.done.Add(new TaskSL(t));
            }
            this.ownerEmail = boardBL.OwnerEmail;
        }
        /// <summary>
        /// returns the name of this board
        /// </summary>
        /// <returns></returns>
        public string GetName() { return this.boardName; }
        /// <summary>
        /// returns the BackLog column
        /// </summary>
        /// <returns></returns>
        public List<TaskSL> GetBackLog() {  return this.backlog; }
        /// <summary>
        /// returns the inProgress column
        /// </summary>
        /// <returns></returns>
        public List<TaskSL> GetInProgress() { return this.inProgress; }
        /// <summary>
        /// returns the done column
        /// </summary>
        /// <returns></returns>
        public List<TaskSL> GetDone() { return this.done; }
        /// <summary>
        /// returns the email of the owner of this board
        /// </summary>
        /// <returns></returns>
        public string GetOwnerEmail() { return this.ownerEmail; }
    }
}
