using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.DataAccessLayer.DAOs
{
    internal class BoardDAO
    {
        public string Name
        {
            get => name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Board name cannot be empty.");
                }

                name = value;

                if (isPersistent)
                {
                    boardController.Update(BoardId, BoardColumnBoardName, value);
                }

            }
        }
        private string name;
        public int BoardId { get; set; }
        public int BacklogLimiter
        {
            get => backLogLimiter;
            set
            {
                backLogLimiter = value;
                if (isPersistent)
                {
                    boardController.Update(BoardId, BoardColumnLimitBacklog, value.ToString());
                }
            }
        }
        private int backLogLimiter;
        public int InProgressLimiter 
        {
            get => inProgressLimiter;
            set 
            {
                inProgressLimiter = value;
                if (isPersistent)
                {
                    boardController.Update(BoardId, BoardColumnLimitProgress, value.ToString());
                }
            } 
        }
        private int inProgressLimiter;
        public int DoneLimiter 
        {   get => doneLimiter;
            set 
            {
                doneLimiter = value;
                if (isPersistent)
                {
                    boardController.Update(BoardId, BoardColumnLimitDone, value.ToString());
                }
            }
        }
        private int doneLimiter;
        public BoardController boardController { get; set; }
        public string OwnerEmail
        { get => ownerEmail; }
            
        private bool isPersistent;
        public string BoardColumnBoardID = "id";
        public string BoardColumnBoardName = "boardName";
        public string BoardColumnLimitBacklog = "limitBacklog";
        public string BoardColumnLimitProgress = "limitProgress";
        public string BoardColumnLimitDone = "limitDone";
        public string ownerEmail;

        public BoardDAO( string ownerEmail, string name)
        {
            Name = name;
            this.BacklogLimiter = -1;
            this.InProgressLimiter = -1;
            this.DoneLimiter = -1;
            this.boardController = new BoardController();
            isPersistent = false;
            this.ownerEmail = ownerEmail;   
        }
        public BoardDAO(string name, int boardID, int backlogLimiter, int  inProgressLimiter, int doneLimiter)
        {
            Name = name;
            BoardId = boardID;
            this.BacklogLimiter = backlogLimiter;
            this.InProgressLimiter= inProgressLimiter;
            this.DoneLimiter = doneLimiter;
            boardController = new BoardController();
            isPersistent = true;
        }
        public void persist()
        {
            if (!isPersistent)
            {
                boardController.Insert(this);
                isPersistent = true;
            }
        }
        public void DeleteBoard()
        {
            if (isPersistent)
            {
                boardController.DeleteBoard(this.BoardId);
            }
        }
        public void UpdateColumnLimit(string columnName, int newLimit)
        {
            boardController.UpdateColumnLimit(BoardId, columnName, newLimit);
        }

        public void UpdateColumnToInProgress(long taskID)
        {
            boardController.PromoteTask(this.BoardId, 1, taskID);
        }
        public void UpdateColumnToDone(long taskID)
        {
            boardController.PromoteTask(this.BoardId, 2, taskID);
        }
        public void AddUserToBoard(string email, string role)
        {
            boardController.AddUserToBoard(email, this.BoardId, role);
        }
        public void RemoveUserFromBoard(string email)
        {
            boardController.RemoveUserFromBoard(email, this.BoardId);
        }
    }
}
