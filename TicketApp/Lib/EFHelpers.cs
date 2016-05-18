using System;
using System.Data.Entity;

namespace TicketApp
{
    public static class EFHelpers
    {
        public static void Rebuild(this DbContext db)
        {
            db.Database.Delete();
            db.Database.Create();
        }
    }
}