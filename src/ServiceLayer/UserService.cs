using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.BusinessLayer;
using log4net;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class UserService
    {
        private UserFacade userFacade;
        private static readonly ILog log = LogManager.GetLogger(typeof(UserService));

        internal UserService(UserFacade userFacade)
        {
            this.userFacade = userFacade;
        }

        /// <summary>
        /// Registers a new user to the system
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>JSON string with error message and return value</returns>
        public string UserRegistration(string email, string password)
        {
            log.Info($"UserRegistration called with email: {email}");
            try
            {
                userFacade.UserRegistration(email, password);
                Response response = new Response(null, null);
                log.Info($"UserRegistration successful for{email} ");
                Console.WriteLine(response);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("UserRegistration failed", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }
        public string LoadUserData()
        {
            log.Info("called LoadUserData");
            try
            {
                userFacade.LoadUsers();
                Response response = new Response(null, null);
                log.Info("LoadUserData succeeded");
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("LoadUserData failed", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }

        public string DeleteAllUsers()
        {
            log.Info($"called delete all users");
            try
            {
                userFacade.DeleteUsers();
                Response response = new Response(null, null);
                log.Info($"delete all useres succeded");
                return JsonSerializer.Serialize(response);
                
            }
            catch(Exception ex) {
                log.Error($"delete all users failed");
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);

            }
        }

        /// <summary>
        /// Logging in an existing user
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>JSON string with error message and return value</returns>
        public string UserLogin(string email, string password)
        {
            log.Info($"UserLogin called with email: {email}");
            try
            {
                userFacade.LogIn(email, password);
                Response response = new Response(null, email);
                log.Info("UserLogin successful");
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("UserLogin failed", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }

        /// <summary>
        /// Logging out a logged in user
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <returns>JSON string with error message and return value</returns>
        public string UserLogOut(string email)
        {
            log.Info($"UserLogOut called with email: {email}");
            try
            {
                userFacade.LogOut(email);
                Response response = new Response(null, null);
                log.Info("UserLogOut successful");
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                log.Error("UserLogOut failed", ex);
                Response response = new Response(ex.Message, null);
                return JsonSerializer.Serialize(response);
            }
        }
    }
}
