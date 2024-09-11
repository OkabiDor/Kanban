using IntroSE.Kanban.Backend.BusinessLayer;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class ServiceFactory
    {
        public UserService userService;
        public BoardService boardService;
        public TaskService taskService;
        private static readonly ILog log = LogManager.GetLogger(typeof(ServiceFactory));

        public ServiceFactory()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetExecutingAssembly());
            log4net.Config.XmlConfigurator.Configure(logRepository, new System.IO.FileInfo("log4net.config"));
            UserFacade userFacade = new UserFacade();
            BoardFacade boardFacade = new BoardFacade(userFacade);
            userService = new UserService(userFacade);
            boardService = new BoardService(boardFacade);
            taskService = new TaskService(boardFacade);
        }

        /// <summary>
        /// Load all data from the persistence.
        /// </summary>
        public string LoadData()
        {
            log.Info("LoadData called.");
            try
            {
                log.Info("Starting to load data from persistence.");

                string userLoadResult = userService.LoadUserData();
                string boardLoadResult = boardService.LoadBoardsData();

                string result = userLoadResult + boardLoadResult;

                Response response = new Response(null, result);
                log.Info("Successfully loaded data from persistence.");
                Console.WriteLine(response);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Fatal("An error occurred while loading data from persistence.", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }


        /// <summary>
        /// Delete all data from the persistence.
        /// </summary>
        /// <summary>
        /// Delete all data from the persistence.
        /// </summary>
        public string DeleteData()
        {
            log.Info("DeleteData called.");
            try
            {
                log.Info("Starting to delete data from persistence.");

                string userDeletionResult = userService.DeleteAllUsers();
                string boardDeletionResult = boardService.DeleteAllBoards();

                string result = userDeletionResult + boardDeletionResult;

                Response response = new Response(null, null);
                log.Info("Successfully deleted data from persistence.");
                Console.WriteLine(response);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Fatal("An error occurred while deleting data from persistence.", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }

    }
}
