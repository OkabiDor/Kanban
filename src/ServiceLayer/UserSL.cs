using IntroSE.Kanban.Backend.BusinessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class UserSL
    {
        private string email; 
        internal UserSL(UserBL userBL)
        {
            this.email = userBL.Email;
        }
        /// <summary>
        /// returns this user's email
        /// </summary>
        /// <returns></returns>
        public string GetEmail() { return email; }
    }
}
