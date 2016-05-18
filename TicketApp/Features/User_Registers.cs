using System;
using System.Linq;
using NUnit.Framework;
using Catalyst;

namespace TicketApp
{
    public class User_Registration_Feature : Feature
    {
        public User_Registration_Feature()
        {
            Background(() =>
            {
                using (var db = new Db())
                {
                    db.Rebuild();
                }
                App.Add<UserService, ConcreteUserLoginService>();
            });

            Scenario("Register a user", () =>
            {
                //Go to the register page
                this.Visit("/account/signup");

                //Fil in the register form
                this.Fill("Email", with: "emilebosch@hotmail.com");
                this.Fill("Password", with: "Aap");
                this.Submit("Register");
                this.HasText("You are registered!");

                //Check if we got send an confirm e-mail
                var mail = App.Get<EmailService>().Read().Last();
                Assert.NotNull(mail, "No confirmation mail send!");

                //Click the link in the e-mail 
                var confirmUrl = mail.GetTextFromHtml();
                this.Visit(confirmUrl);

                //Check whether the text is present
                this.HasText("Thanks for signing up with us!");
            });

            Scenario("Login as a user", () =>
            {
                this.Visit("/account/login");
                this.Fill("Email", with: "emilebosch@hotmail.com");
                this.Fill("Password", with: "Aap");
                this.Submit("Login!");

                //Check to see if we are logged in
                this.HasText("Logged in!");

                //Actual log in check
                bool loggedIn = App.Get<UserService>().IsLoggedIn();
                Assert.IsTrue(loggedIn, "User should be logged in.");
            });

            Scenario("Log out as a user", () =>
            {
                this.Visit("/account/logout");
                this.HasText("Logged out");

                //Actual log in check
                bool loggedIn = App.Get<UserService>().IsLoggedIn();
                Assert.IsFalse(loggedIn, "User should be logged out");
            });

            Scenario("Request forgotten password", () =>
            {
                App.Get<EmailService>().Clear();

                //Make sure
                this.Visit("/account/signup");
                this.Click("Forgot password?");

                //Request a password reset
                this.Visit("/account/resetpassword");
                this.HasText("Request password reset");
                this.Fill("Email", with: "emilebosch@hotmail.com");
                this.Submit("Request");
                this.HasText("Check your mail to confirm the reset");

                //Check if an e-mail has been send to confirm the reset
                var requestEmailPassword = App.Get<EmailService>().Read().Last();
                Assert.NotNull(requestEmailPassword, "No request password mail send!");

                App.Get<EmailService>().Clear();

                //Get the token from the e-mail
                var resetUrl = requestEmailPassword.GetTextFromHtml();

                //Visit our reset password page with our token
                this.Visit(resetUrl);
                this.HasText("emilebosch@hotmail.com");
                this.Fill("Password", with: "MyNewPassword");
                this.Submit("Change");

                this.HasText("Your password has been reset");

                //Check if the password mail has been send
                var passwordChangedMail = App.Get<EmailService>().Read().Last();
                Assert.NotNull(passwordChangedMail, "No password reset mail send");

                //Check if we can login
                var loggedin = App.Get<UserService>().LogIn("emilebosch@hotmail.com", "MyNewPassword");
                Assert.IsTrue(loggedin.Success, "Password has not changed to the right user");
            });
        }
    }
}
