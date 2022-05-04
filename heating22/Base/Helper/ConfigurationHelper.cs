using Microsoft.Extensions.Configuration;

using System;

namespace Base.Helper
{
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Liefert die aktuelle aus den appsettings ermittelte Konfiguration.
        /// </summary>
        /// <returns></returns>
        public static IConfiguration GetConfiguration()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("appsettings.json")
            .Build();
            return configuration;
        }

        /// <summary>
        /// ConfigurationItem aus den Applicationsettings über Key identifiziert
        /// Optional auch über Section adressiert
        /// </summary>
        /// <param name="key"></param>
        /// <param name="section"></param>
        /// <returns>Gewählten Konfigurationseintrag</returns>
        /// <exception cref="ApplicationException"></exception>
        public static string GetConfiguration(string key, string section = null)
        {
            var configuration = GetConfiguration();
            if (section != null)
            {
                var appSettingsSection = configuration.GetSection(section);
                if (!appSettingsSection.Exists())
                {
                    throw new ApplicationException($"GetConfiguration; Section: {section} doesn't exist");
                }
                var sectionValue = appSettingsSection[key];
                if (string.IsNullOrEmpty(sectionValue))
                {
                    throw new ApplicationException($"GetConfiguration; Section: {section}, Key: {key} doesn't exist");
                }
                return sectionValue;
                //var key = Encoding.ASCII.GetBytes();
            }
            var value = configuration[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new ApplicationException($"GetConfiguration; Section: {section}, Key: {key} doesn't exist");
            }
            return value;
        }

        /// <summary>
        /// Liefert den aus den appsettings ermittelten Connectionstring
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString() => GetConfiguration("DefaultConnection", "ConnectionStrings");
    }
}
