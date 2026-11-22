using MiniCapstone_TechStoreApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCapstone_TechStoreApp.Helper
{
    public static class UserSession
    {
        public static UserDB CurrentUser { get; private set; }

        public static bool IsLoggedIn
        {
            get { return CurrentUser != null; }
        }

        public static bool IsBuyer
        {
            get { return IsLoggedIn && CurrentUser.Role == "Buyer"; }
        }

        public static bool IsAdmin
        {
            get { return IsLoggedIn && CurrentUser.Role == "Admin"; }
        }

        public static void Login(UserDB user)
        {
            CurrentUser = user;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
