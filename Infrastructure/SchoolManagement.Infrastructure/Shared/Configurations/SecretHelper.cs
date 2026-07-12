namespace SchoolManagement.Infrastructure.Shared.Configurations
{
    public static class SecretHelper
    {
        private static readonly string envPath = ".env";

        /// <summary>
        /// Get the value from a .env file with the input key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>A <see cref="string"/> value</returns>
        /// <exception cref="Exception"></exception>
        public static string GetValueFromEnv(string key)
        {
            DotNetEnv.Env.Load(envPath);
            string value = Environment.GetEnvironmentVariable(key) 
                ?? throw new Exception($"Couldn't find the value of \"{key}\" inside the environment file!");
            return value;
        }
    }
}
