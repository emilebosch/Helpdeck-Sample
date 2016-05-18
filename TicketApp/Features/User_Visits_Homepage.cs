using Catalyst;

namespace TicketApp
{
    public class Homepage_Feature : Feature
    {
        public Homepage_Feature()
        {
            Scenario("Visit homepage", () =>
            {
                this.Visit("/");
                this.HasText("Welcome");
            });
        }
    }
}
