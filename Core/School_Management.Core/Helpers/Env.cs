
namespace School_Management.Core.Helpers
{
    public static class Env
    {
        private static readonly string envPath = ".env";
        public static string Get(string key)
        {
            DotNetEnv.Env.Load(envPath);
            string value = Environment.GetEnvironmentVariable(key) ?? throw new Exception($"Couldn't find the value of \"{key}\" inside the environment file!");
            return value;
        }
    }
}
