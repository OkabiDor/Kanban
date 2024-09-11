using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.DataAccessLayer.DAOs
{
    internal class UserDAO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public UserController userController { get; set; }
        private bool isPersistent;
        public string UserColumnEmail = "email";
        public string UserColumnPassword = "password";

        public UserDAO(string email, string password)
        {
            Email = email;
            Password = password;
            userController = new UserController();
            isPersistent = false;
        }
        public UserDAO(string email, string password, bool isPersistent)
        {
            Email = email;
            Password = password;
            this.isPersistent = isPersistent;
        }
        public void persist()
        {
            if (!isPersistent)
            {
                userController.Insert(this);
                isPersistent = true;
            }

        }
        public void DeleteUser()
        {
            if (isPersistent)
            {
                userController.DeleteUser(Email);
            }
        }

    }
}
