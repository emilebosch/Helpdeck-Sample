using System;
using System.Linq;

namespace TicketApp
{
    public class RegisterResponse
    {
        public User User { get; set; }
        public bool Success { get; set; }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
    }

    public interface UserService
    {
        User GetLoggedInUser();
        RegisterResponse Register(string username, string password, string email, UserRole role, bool requireConfirm = true);
        LoginResponse LogIn(string username, string password);
        bool LogOut();
        bool IsLoggedIn();
        bool ConfirmUser(string verifycode);
    }

    public class ConcreteUserLoginService : UserService
    {
        User currentUser = null;

        public User GetLoggedInUser()
        {
            return currentUser;
        }

        public LoginResponse LogIn(string email, string password)
        {
            using (var context = new Db())
            {
                var user = context.Users.FirstOrDefault(a => a.Email == email && a.Password == password && a.Locked == false);
                if (user != null)
                {
                    currentUser = user;
                    return new LoginResponse { Success = true };
                }
            }
            return new LoginResponse { Success = false };
        }

        public bool LogOut()
        {
            currentUser = null;
            return true;
        }

        public RegisterResponse Register(string username, string password, string email, UserRole role = UserRole.Normal, bool lockedDefault = true)
        {
            using (var context = new Db())
            {
                var user = context.Users.Add(new User { Locked = lockedDefault, Name = username, Email = email, Password = password, Role = role, ConfirmationToken = Guid.NewGuid().ToString() });
                context.SaveChanges();
                return new RegisterResponse { User = user, Success = true };
            }
        }

        public bool IsLoggedIn()
        {
            return GetLoggedInUser() != null;
        }

        public bool ConfirmUser(string p)
        {
            using (var context = new Db())
            {
                var user = context.Users.FirstOrDefault(x => x.ConfirmationToken == p);
                user.Locked = false;
                context.SaveChanges();
                return true;
            }
        }
    }
}