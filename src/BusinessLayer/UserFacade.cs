using IntroSE.Kanban.Backend.DataAccessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer.DAOs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class UserFacade 
    {
        private readonly Dictionary<string, UserBL> users; //dictionary is mapped email to user
        private UserController userController;
        public UserFacade()
        {
            users = new Dictionary<string, UserBL>();
            userController = new UserController();
        }
        public UserBL UserRegistration(string email, string password)
        {
            if (users.ContainsKey(email)){ throw new Exception($"User {email} already exists"); }
            UserBL newUser = new UserBL(email, password);
            users.Add(email, newUser);
            return newUser;
        }
        public UserBL LogIn(string email, string password)
        {
            UserBL toConnectUser = CheckCredentials(email,password);
            toConnectUser.LogIn();
            return toConnectUser;
        }
        public void LogOut(string email)
        {
            UserBL toDisconnectPlayer = CheckEmailExists(email);
            toDisconnectPlayer.LogOut();
        }
        public UserBL CheckCredentials(string email, string password) 
        {
            UserBL user = CheckEmailExists(email);
            user.CheckCredentials(email, password);
            return user;
        }
        public UserBL CheckEmailExists(string email)
        {
            if (!users.ContainsKey(email)) { throw new Exception($"{email} is not found."); }
            users.TryGetValue(email, out UserBL user);
            return user;
        }
        public void isConnected(string email)
        {
            UserBL user = users[email];
            if (!user.IsConnected) { throw new Exception($"{user.Email} isn't connected"); }
        }
        internal void LoadUsers()
        {
            List<UserDAO> usersDAO = userController.SelectAllUsers();
            foreach (UserDAO userDAO in usersDAO)
            {
                UserBL newUser = new UserBL(userDAO);
                users.Add(newUser.Email, newUser);
            }
        }
        internal void DeleteUsers()
        {
            userController.DeleteAllUsers();
            users.Clear();
        }
        public void DoesExist(string email)
        {
            UserBL user = users[email];
            if (user == null) { throw new Exception($"{user.Email} doesnt exist"); }     
        }
    }
}
