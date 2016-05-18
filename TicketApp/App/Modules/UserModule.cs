using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;

namespace TicketApp
{
    public class UserModule : NancyModule
    {
        public UserModule()
        {
            Get["/users/1"] = t =>
            {
                return View["app/views/users/view.cshtml"];
            };
        }
    }
}
