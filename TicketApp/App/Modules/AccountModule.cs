using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;
using Limilabs.Mail.Fluent;
using Catalyst;

namespace TicketApp
{
    public class AccountModule : NancyModule
    {
        public AccountModule()
        {
            Get["/account/login"] = p =>
            {
                return View["app/views/account/login.cshtml"];
            };

            Post["/account/login"] = p =>
            {
                var service = App.Get<UserService>();
                var response = service.LogIn((string)Request.Form.email, (string)Request.Form.password);
                return response.Success ? "Logged in!" : "Failed log in!";
            };

            Get["/account/signup"] = p =>
            {
                return View["app/views/account/signup.cshtml"];
            };

            Post["/account/signup"] = p =>
            {
                using (var context = new Db())
                {
                    var user = this.Bind<User>();
                    var service = App.Get<UserService>();
                    var mailer = App.Get<EmailService>();

                    var result = service.Register(user.Name, user.Password, user.Email, UserRole.Normal);
                    if (result.Success)
                    {
                        var email = Mail
                           .Html("/account/signup/confirm/{0}".Inject(result.User.ConfirmationToken))
                           .To(result.User.Email)
                           .Subject("Almost there!")
                           .Create();
                        mailer.Send(email);
                    }
                }
                return "You are registered!";
            };

            Get["/account/signup/confirm/{token}"] = (p) =>
            {
                bool confirmed = App.Get<UserService>().ConfirmUser((string)p.token);
                return confirmed ? "Thanks for signing up with us!" : "Unable to confirm";
            };

            Get["/account/logout"] = p =>
            {
                var service = App.Get<UserService>();
                var success = service.LogOut();
                return success ? "Logged out" : "Could not log out";
            };

            Get["/account/resetpassword"] = p =>
            {
                return View["app/views/account/requestpassword.cshtml"];
            };

            Post["/account/resetpassword"] = p =>
            {
                User user = null;
                string emailadres = this.Request.Form.email;

                using (var context = new Db())
                {
                    user = context.Users.FirstOrDefault(a => a.Email == emailadres);
                    if (user != null)
                    {
                        user.PasswordResetToken = Guid.NewGuid().ToString();
                        context.SaveChanges();
                    }
                }

                if (user != null)
                {
                    var mailer = App.Get<EmailService>();
                    var email = Mail
                        .Html("/account/resetpassword/confirm/{0}".Inject(user.PasswordResetToken))
                        .To(user.Email)
                        .Subject("Password request")
                        .Create();

                    mailer.Send(email);
                }
                return "Check your mail to confirm the reset";
            };

            Get["/account/resetpassword/confirm/{token}"] = p =>
            {
                string token = p.token;
                User user;
                using (var context = new Db())
                {
                    user = context.Users.FirstOrDefault(a => a.PasswordResetToken == token);
                }
                return View["app/views/account/resetpassword.cshtml", user];
            };

            Post["/account/resetpassword/confirm/{token}"] = p =>
            {
                string token = p.token;
                User user;
                using (var context = new Db())
                {
                    user = context.Users.FirstOrDefault(a => a.PasswordResetToken == token);
                    if (user != null)
                    {
                        user.Password = this.Request.Form.password;
                        context.SaveChanges();

                        var mailer = App.Get<EmailService>();
                        var email = Mail
                            .Html(String.Format("Your password is reset!"))
                            .To(user.Email)
                            .Subject("Password reset!")
                            .Create();

                        mailer.Send(email);
                    }
                }
                return "Your password has been reset";
            };

        }
    }
}