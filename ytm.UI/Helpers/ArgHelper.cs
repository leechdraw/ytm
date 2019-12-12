namespace ytm.UI.Helpers
{
    public static class ArgHelper
    {
        public static string GetPathToMainConfig(this string[] args)
        {
            return args.Length < 1 
                ? GetDefaultMainConfigPath() 
                : args[0];
        }

        private static string GetDefaultMainConfigPath()
        {
            return "main_config.json";
        }
    }
}