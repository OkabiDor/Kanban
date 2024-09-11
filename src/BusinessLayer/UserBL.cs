using System;
using System.Linq;
using IntroSE.Kanban.Backend.DataAccessLayer.DAOs;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class UserBL
    {
        private string email;
        private string password;
        private bool isConnected;
        private UserDAO daoUser;
        internal string Email
        {
            get => email;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) { throw new ArgumentNullException("Email is null or empty"); }
                if (value.Length > 50) { throw new Exception($"Email {value} exceeds the maximum length of 255 characters"); }
                email = value;
            }
        }

        internal string Password
        {
            get => password;
            set
            {
                if (value == null) { throw new ArgumentNullException("Password is null"); }
                if (value.Length < 6) { throw new Exception($"Password {value} was less than 6 characters"); }
                if (value.Length > 20) { throw new Exception($"Password {value} was more than 20 characters"); }
                if (!value.Any(char.IsUpper)) { throw new Exception($"Password {value} doesn't have upper case character"); }
                if (!value.Any(char.IsLower)) { throw new Exception($"Password {value} doesn't have lower case character"); }
                if (!value.Any(char.IsDigit)) { throw new Exception($"Password {value} doesn't have a number"); }
                if (value.Contains(" ")) { throw new Exception($"Password {value} contains spaces"); }
                password = value;
            }
        }
        internal bool IsConnected { get => isConnected; set { isConnected = value; } }
        internal UserDAO DaoUser { get => daoUser; }
        internal UserBL(string email, string password)
        {            
            Email = email;
            Password =password;
            daoUser = new UserDAO(email, password);
            IsConnected = true;
            daoUser.persist();
        }

        internal UserBL(UserDAO userDAO)
        {
            Email = userDAO.Email;
            Password = userDAO.Password;
            IsConnected = false;
            daoUser = userDAO;
        }
        public void LogIn()
        {
            if (isConnected)
            {
                throw new Exception($"User {this.email} is already logged in");
            }
            isConnected = true;
        }
        public void LogOut()
        {
            if (!isConnected) { throw new Exception($"User {this.email} is already logged out"); }
            isConnected = false;
        }
        public void CheckCredentials(string email, string password) 
        { 
            if(this.email != email || this.password != password) { throw new Exception("Email or password was incorrect"); }
        }
    }
}
