using System;
using System.Configuration;

namespace JiraToAzure
{
    internal class Configuration
    {
        private Configuration()
        {
            
        }

        internal static Configuration Build()
        {
            var jiraInstanceUrl = ConfigurationManager.AppSettings["JiraInstanceUrl"];
            var jiraUsername = ConfigurationManager.AppSettings["JiraUsername"];
            var jiraPassword = ConfigurationManager.AppSettings["JiraPassword"];
            var jiraProjectKey = ConfigurationManager.AppSettings["JiraProjectKey"];
            var jiraMaximumToQueryUnparsed = ConfigurationManager.AppSettings["JiraMaximumToQuery"];
            var azureOrganizationUrl = ConfigurationManager.AppSettings["AzureOrganizationUrl"];
            var azureProjectName = ConfigurationManager.AppSettings["AzureProjectName"];
            var azureUserPat = ConfigurationManager.AppSettings["AzureUserPAT"];
            var millisecondsBetweenAzureRequestsUnparsed = ConfigurationManager.AppSettings["MillisecondsBetweenAzureRequests"];

            if (string.IsNullOrWhiteSpace(jiraInstanceUrl))
                throw new InvalidOperationException("Cannot migrate with missing JIRA Instance URL, please configure it in the app.config");

            if (string.IsNullOrWhiteSpace(jiraUsername))
                throw new InvalidOperationException("Cannot migrate with missing JIRA username, please configure it in the app.config");

            if (string.IsNullOrWhiteSpace(jiraPassword))
                throw new InvalidOperationException("Cannot migrate with missing JIRA password, please configure it in the app.config");

            if (string.IsNullOrWhiteSpace(jiraProjectKey))
                throw new InvalidOperationException("Cannot migrate with missing JIRA project key, please configure it in the app.config");

            if (string.IsNullOrWhiteSpace(jiraMaximumToQueryUnparsed) || !int.TryParse(jiraMaximumToQueryUnparsed, out var jiraMaximumToQuery))
                throw new InvalidOperationException("A maximum number of issues to query from JIRA must be provided and must be a valid integer, please configure it in the app.config");

            if (string.IsNullOrWhiteSpace(azureOrganizationUrl))
                throw new InvalidOperationException("Cannot migrate with missing Azure organization URL, please configure it in the app.config");

            if (string.IsNullOrWhiteSpace(azureProjectName))
                throw new InvalidOperationException("Cannot migrate with missing Azure project name, please configure it in the app.config");

            if (string.IsNullOrWhiteSpace(azureUserPat))
                throw new InvalidOperationException("Cannot migrate with missing Azure personal access token (PAT), please configure it in the app.config");

            if (string.IsNullOrWhiteSpace(millisecondsBetweenAzureRequestsUnparsed) || !int.TryParse(millisecondsBetweenAzureRequestsUnparsed, out var millisecondsBetweenAzureRequests))
                throw new InvalidOperationException("Milliseconds between Azure requests has not been configured, please configure it in the app.config");

            return new Configuration
            {
                JiraInstanceUrl = jiraInstanceUrl,
                JiraUsername = jiraUsername,
                JiraPassword = jiraPassword,
                JiraProjectKey = jiraProjectKey,
                JiraMaximumToQuery = jiraMaximumToQuery,
                AzureOrganizationUrl = azureOrganizationUrl,
                AzureProjectName = azureProjectName,
                AzureUserPAT = azureUserPat,
                MillisecondsBetweenAzureRequests = millisecondsBetweenAzureRequests,
            };
        }

        internal string JiraInstanceUrl { get; private set; }
        internal string JiraUsername { get; private set; }
        internal string JiraPassword { get; private set; }
        internal string JiraProjectKey { get; private set; }
        internal int JiraMaximumToQuery { get; private set; }
        public string AzureOrganizationUrl { get; private set; }
        public string AzureProjectName { get; private set; }
        public string AzureUserPAT { get; private set; }
        public int MillisecondsBetweenAzureRequests { get; private set; }
    }
}