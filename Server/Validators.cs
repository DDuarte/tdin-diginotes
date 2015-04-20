namespace Server
{
    public static class Validators
    {
        public static bool ValidUsername(string username)
        {
            return !string.IsNullOrWhiteSpace(username) && username.Length >= 2 && username.Length <= 20;
        }

        public static bool ValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.Length >= 2 && name.Length <= 20;
        }

        public static bool ValidPassword(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 2 && password.Length <= 20;
        }
    }
}
