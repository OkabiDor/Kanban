using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.BusinessLayer;
using log4net;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class BoardService
    {
        private BoardFacade boardFacade;
        private static readonly ILog log = LogManager.GetLogger(typeof(BoardService));

        internal BoardService(BoardFacade boardFacade)
        {
            this.boardFacade = boardFacade;
        }

        /// <summary>
        /// Creates a new board and adds the board to the boards list of the user who created it.
        /// </summary>
        /// <param name="userEmail">Email of a user</param>
        /// <param name="boardName">Name for the new board</param>
        /// <returns>JSON string of the new board, or exception if raised</returns>
        public string CreateBoard(string userEmail, string boardName)
        {
            log.Info($"CreateBoard called with userEmail: {userEmail}, boardName: {boardName}");
            try
            {
                boardFacade.CreateBoard(userEmail, boardName);
                Response response = new Response(null, null);
                log.Info($"CreateBoard successful user: {userEmail}");
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("CreateBoard failed", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }
        public string LoadBoardsData()
        {
            log.Info("called LoadBoardsData");
            try
            {
                boardFacade.LoadBoards();
                Response response = new Response(null, null);
                log.Info("LoadBoardsData succeeded");
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("LoadBoardsData failed", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }

        public string DeleteAllBoards()
        {
            log.Info($"attempting delete all boards");
            try
            {
                boardFacade.DeleteBoards();
                Response response = new Response(null, null);
                log.Info($"deleted all boards successfully");
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("deletedAllUsers failed", ex);
                Response response= new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }

        /// <summary>
        /// Deletes a board from user's list
        /// </summary>
        /// <param name="userEmail">Email of a user</param>
        /// <param name="boardName">Name of the board that's about to be deleted</param>
        /// <returns>JSON string with result or exception if raised</returns>
        public string DeleteBoard(string userEmail, string boardName)
        {
            log.Info($"DeleteBoard called with userEmail: {userEmail}, boardName: {boardName}");
            try
            {
                boardFacade.DeleteBoard(userEmail, boardName);
                Response response = new Response(null, null);
                log.Info($"DeleteBoard successful user{userEmail}");
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("DeleteBoard failed", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }

        /// <summary>
        /// Updates the limit of tasks per list
        /// </summary>
        /// <param name="userEmail">Email of a user</param>
        /// <param name="boardName">Name of a board</param>
        /// <param name="columnNum">Num of the list</param>
        /// <param name="maxTasks">Number to be max</param>
        /// <returns>JSON string with result or exception if raised</returns>
        public string LimitColumn(string userEmail, string boardName, int columnNum, int maxTasks)
        {
            string columnName = "";
            switch (columnNum)
            {
                case 0:
                    columnName = "backlog";
                    break;
                case 1:
                    columnName = "inprogress";
                    break;
                case 2:
                    columnName = "done";
                    break;
                default:
                    break;
            }
            log.Info($"LimitColumns called with userEmail: {userEmail}, boardName: {boardName}, columnName: {columnName}, maxTasks: {maxTasks}");
            try
            {
                boardFacade.LimitColumn(userEmail, boardName, columnName, maxTasks);
                Response response = new Response(null, null);
                log.Info($"LimitColumns successful for user{userEmail}");
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("LimitColumns failed", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }

        /// <summary>
        /// Change list from 'backlog' to 'in progress' or 'in progress' to 'done'
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <param name="boardName">Name of a board</param>
        /// <param name="taskID">ID number of a task</param>
        /// <returns>JSON string with result or exception if raised</returns>
        public string UpdateColumnOfTask(string email, string boardName, int taskID)
        {
            log.Info($"UpdateColumnOfTask called with email: {email}, boardName: {boardName}, taskID: {taskID}");
            try
            {
                boardFacade.UpdateColumnOfTask(email, boardName, taskID);
                Response response = new Response(null, null);
                log.Info($"UpdateColumnOfTask successful user{email}");
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("UpdateColumnOfTask failed", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }
        /// <summary>
        /// returns a list of all the tasks in a specific column
        /// </summary>
        /// <param name="email"></param>
        /// <param name="boardName"></param>
        /// <param name="columnOrdinal"></param>
        /// <returns></returns>
        public string GetColumn(string email, string boardName, int columnOrdinal)
        {
            string str = "";
            switch (columnOrdinal)
            {
                case 0:
                    str = "backlog";
                    break;
                case 1:
                    str = "inprogress";
                    break;
                case 2:
                    str = "done";
                    break;
                default:
                    break;
            }
            try
            {
                List<TaskBL> tasksBL = boardFacade.GetColumn(email, boardName, str);
                List<TaskSL> tasksSL = new List<TaskSL>();
                foreach (TaskBL task in tasksBL)
                {
                    tasksSL.Add(new TaskSL(task));
                }
                Response response = new Response(null, tasksSL);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("GetColumn failed", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }

        /// <summary>
        /// Gets the limit of tasks per column
        /// </summary>
        /// <param name="email">Email of a user</param>
        /// <param name="boardName">Name of a board</param>
        /// <param name="columnName">Name of the list</param>
        /// <returns>JSON string with result or exception if raised</returns>
        public string GetColumnLimit(string email, string boardName, int columnNum)
        {
            string columnName = "";
            switch (columnNum)
            {
                case 0:
                    columnName = "backlog";
                    break;
                case 1:
                    columnName = "inprogress";
                    break;
                case 2:
                    columnName = "done";
                    break;
                default:
                    break;
            }
            log.Info($"GetColumnLimit called with email: {email}, boardName: {boardName}, columnName: {columnName}");
            Response response;
            try
            {
                int limit = boardFacade.GetColumnLimit(email, boardName, columnName);
                response = new Response(null, limit);
                log.Info($"GetColumnLimit successful user{email}");
            }
            catch (Exception ex)
            {
                log.Error("GetColumnLimit failed", ex);
                response = new Response(ex.Message, null);
            }
            return JsonSerializer.Serialize(response);
        }
        /// <summary>
        /// function that returns the column name based on the number given.
        /// </summary>
        /// <param name="columnNum"></param>
        /// <returns></returns>
        public string GetColumnName(int columnNum)
        {
            Response response;
            switch (columnNum)
            {
                case 0:
                    response = new Response(null, "backlog");
                    break;
                case 1:
                    response = new Response(null, "in progress");
                    break;
                case 2:
                    response = new Response(null, "done");
                    break;
                default:
                    response = new Response("Invalid column ordinal value", null);
                    break;
            }
            return JsonSerializer.Serialize(response);
        }

        public string JoinBoard(string email, int boardID)
        {
            log.Info($"JoinBoard called with email: {email}, boardID: {boardID}");
            try
            {
                boardFacade.JoinBoard(email, boardID);
                Response response = new Response(null, null);
                log.Info($"LimitColumns successful for user{email}");
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("JoinBoard failed", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }
        public string TransferOwnership(string email, string recieverEmail, string boardName)
        {
            log.Info($"TransferOwnership called with email: {email}, recieverEmail: {recieverEmail}, boardName: {boardName}");
            try
            {
                boardFacade.TransferOwnership(email, recieverEmail, boardName);
                Response response = new Response(null, null);
                log.Info($"TransferOwnership successful for user{email}");
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("TransferOwnership failed", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }
        public string LeaveBoard(string email, int boardID)
        {
            log.Info($"LeaveBoard called with email: {email}, boardName: {boardID}");
            try
            {
                boardFacade.LeaveBoard(email, boardID);
                Response response = new Response(null, null);
                log.Info($"LeaveBoard successful for user{email}");
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("LeaveBoard failed", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }

        public string GetBoardName(int BoardID)
        {
            log.Info($"GetBoardName called with ID {BoardID}");
            try
            {
                string name = boardFacade.GetBoardName(BoardID); //TDA
                Response respons = new Response(null, name);
                log.Info($"got successfully the name of board with ID {BoardID}");
                return JsonSerializer.Serialize(respons);
            }
            catch (Exception ex)
            {
                log.Error("GetBoardName failed", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }
        public string GetUserBoards(string userEmail)
        {
            log.Info($"GetUserBoards called with email {userEmail}");
            try
            {
                List<int> userBords = boardFacade.GetUserBoards(userEmail);
                Response respons = new Response(null, userBords);
                log.Info($"got successfully the boards of the user with email {userEmail}");
                return JsonSerializer.Serialize(respons);
            }
            catch (Exception ex)
            {
                log.Error("GetUserBoards failed", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }
    }

    
}