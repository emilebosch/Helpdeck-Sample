namespace Catalyst
{
    public static class Flash
    {
        private static string message;

        public static void SetFlash(string msg)
        {
            message = msg;
        }

        public static string GetFlash()
        {
            var temp = message;
            message = null;
            return temp;
        }
    }
}
