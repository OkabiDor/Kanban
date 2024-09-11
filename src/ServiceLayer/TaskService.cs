using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.BusinessLayer;
using System;
using System.Collections.Generic;
using System.Text.Json;
using log4net;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class TaskService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TaskService));
        private BoardFacade boardFacade;

        internal TaskService(BoardFacade boardFacade)
        {
            this.boardFacade = boardFacade;
        }
        /// <summary>
        /// edits a specific tasks' description
        /// </summary>
        /// <param name="email"></param>
        /// <param name="boardName"></param>
        /// <param name="taskID"></param>
        /// <param name="description"></param>
        /// <returns>returns null if edit was succesful</returns>
        public string EditTaskDesc(string email, string boardName, int taskID, string description)
        {
            try
            {
                log.Info($"EditTaskDesc called with email: {email}, boardName: {boardName}, taskID: {taskID}, description: {description}");
                boardFacade.EditTaskDesc(email, boardName, taskID, description);
                Response response = new Response(null, null);
                log.Info($"EditTaskDesc{taskID} successful at {boardName}");
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("Error in EditTaskDesc", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }
        /// <summary>
        /// edits a specific tasks's due date
        /// </summary>
        /// <param name="email"></param>
        /// <param name="boardName"></param>
        /// <param name="taskID"></param>
        /// <param name="date"></param>
        /// <returns>returns null if edit was succesful</returns>
        public string EditTaskDate(string email, string boardName, int taskID, DateTime date)
        {
            try
            {
                log.Info($"EditTaskDate called with email: {email}, boardName: {boardName}, taskID: {taskID}, date: {date}");
                boardFacade.EditTaskDate(email, boardName, taskID, date);
                Response response = new Response(null, null);
                log.Info($"EditTaskDate{taskID} successful at {boardName}");
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("Error in EditTaskDate", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }
        /// <summary>
        /// edits a specific tasks's title
        /// </summary>
        /// <param name="email"></param>
        /// <param name="boardName"></param>
        /// <param name="taskID"></param>
        /// <param name="title"></param>
        /// <returns>returns null if edit was succesful</returns>
        public string EditTaskTitle(string email, string boardName, int taskID, string title)
        {
            try
            {
                log.Info($"EditTaskTitle called with email: {email}, boardName: {boardName}, taskID: {taskID}, title: {title}");
                boardFacade.EditTaskTitle(email, boardName, taskID, title);
                Response response = new Response(null, null);
                log.Info($"EditTaskDesc{taskID} successful at {boardName}");
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("Error in EditTaskTitle", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }

        /// <summary>
        /// Creates a new task and add to 'backlog' list in a board
        /// </summary>
        /// <param name="boardName">name of a board</param>
        /// <param name="title">title of the task</param>
        /// <param name="description">description of the task</param>
        /// <param name="dueDate">the task due date</param>
        /// <returns>a task in JSON format, or exception if didn't meet the requirements</returns>
        public string CreateTask(string email, string boardName, string title, string description, DateTime dueDate)
        {
            try
            {
                log.Info($"CreateTask called with email: {email}, boardName: {boardName}, title: {title}, description: {description}, dueDate: {dueDate}");
                boardFacade.CreateTask(email, boardName, title, description, dueDate);
                Response response = new Response(null, null);
                log.Info($"CreateTask successful user{email} at {boardName}");
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("Error in CreateTask", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }

        /// <summary>
        /// make a list of all task that in 'in progress' list of a user from whole boards
        /// </summary>
        /// <param name="userEmail">email of a user</param>
        /// <returns>list of tasks in JSON format or exception if raised</returns>
        public string TasksInProgress(string userEmail)
        {
            Response response;
            try
            {
                log.Info($"TasksInProgress called with userEmail: {userEmail}");
                List<TaskBL> tasksBL = boardFacade.TasksInProgress(userEmail);
                List<TaskSL> tasksSL = new List<TaskSL>();
                foreach (TaskBL taskBL in tasksBL)
                {
                    tasksSL.Add(new TaskSL(taskBL));
                }
                response = new Response(null, tasksSL);
                log.Info($"TasksInProgress successful of user {userEmail}");
            }
            catch (Exception ex)
            {
                log.Error("Error in TasksInProgress", ex);
                response = new Response(ex.Message, null);
            }
            return JsonSerializer.Serialize(response);
        }

        public string AssignUserToTask(string userEmail, string reciverEmail, int columnOrdinal, string boardName, int taskID)
        {
            log.Info($"AssignUserToTask called with userEmail: {userEmail}, reciverEmail: {reciverEmail}, columnOrdinal: {columnOrdinal}, boardName: {boardName}, taskID: {taskID}");
            try
            {
                boardFacade.AssignTask(userEmail, reciverEmail, boardName, taskID);
                Response response = new Response(null, null);
                log.Info($"AssignUserToTask successful of user {userEmail}");
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("Error in AssignUserToTask", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }

        }
    }
}