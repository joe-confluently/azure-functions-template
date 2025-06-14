namespace Confluent.Configuration
{
    public class ConfigurationService
    {
        public IConfigurationBuilder ConfigurationBuilder { get; }
        private static IConfigurationBuilder _configurationBuilder { get; } = GetConfigurationBuilder();

        public ConfigurationService()
        {
            ConfigurationBuilder = _configurationBuilder;
        }

        private static IConfigurationBuilder GetConfigurationBuilder()
        {
            string basePath = GetCustomSettingsBasePath();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            return builder;
        }
        private static string GetCustomSettingsBasePath()
        {
            string path = "";
            string home = Environment.GetEnvironmentVariable("HOME");

            if (home != null)
            {
                // We're on Azure
                return Path.Combine(Environment.GetEnvironmentVariable("HOME"), "site", "wwwroot", "CustomSettings");
            }
            else
            {
                // Running locally
                return GetCurrentDirectoryCustomSettingsPath();
            }

        }

        private static string GetCurrentDirectoryCustomSettingsPath()
        {
            path = new Uri(Assembly.GetExecutingAssembly().Location).LocalPath;
            path = Path.GetDirectoryName(path);
            DirectoryInfo parentDir = Directory.GetParent(path);
            path = parentDir.FullName;
            return path;
        }

        private static IConfigurationBuilder AddAppSettingsToConfigBuilder(string basePath, IConfigurationBuilder builder)
        {
            if (basePath != null)
            {
                builder.SetBasePath(basePath);
            }
            else
            {
                basePath = Directory.GetCurrentDirectory();
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(basePath);
            DirectoryInfo[] directories = directoryInfo.GetDirectories("Settings", SearchOption.AllDirectories);
            foreach (DirectoryInfo directory in directories)
            {
                builder.AddJsonFile($@"{directory.FullName}\appsettings.json");

            }
            directoryInfo.GetDirectories("Global", SearchOption.AllDirectories).ToList().ForEach((directory) =>
            {
                builder.AddJsonFile($@"{directories[0].FullName}\Settings\localsettings.json", optional: true, reloadOnChange: true);
            });

            return builder;
        }
   
    }
}