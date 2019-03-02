using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atlassian.Jira;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace JiraToAzure
{
    public class Program
    {
        private static Configuration _configuration;

        public static async Task<int> Main(string[] args)
        {
            try
            {
                _configuration = Configuration.Build();

                Console.WriteLine("Getting JIRA issues...");

                var issues = await GetJiraIssues();

                Console.WriteLine("Finished querying JIRA");

                Console.WriteLine("Starting Azure import...");

                await CopyToAzure(issues);

                Console.WriteLine("Finished Azure import!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }

            return 1;
        }

        private static async Task<List<Issue>> GetJiraIssues()
        {
            bool hasProcessed;

            var maximumToQuery = _configuration.JiraMaximumToQuery;

            var client = Jira.CreateRestClient(_configuration.JiraInstanceUrl, _configuration.JiraUsername,
                _configuration.JiraPassword);

            var issueOffset = 0;

            var issues = new List<Issue>();

            do
            {
                Console.WriteLine($"Querying JIRA issues at offset {issueOffset}");

                var foundIssues = await client.Issues.GetIssuesFromJqlAsync($"PROJECT = '{_configuration.JiraProjectKey}' ORDER BY Key", startAt: issueOffset, maxIssues: maximumToQuery)
                    .ConfigureAwait(false);

                Console.WriteLine($"Found {issues.Count()} issues at offset");

                issues.AddRange(foundIssues);

                issueOffset += maximumToQuery;

                hasProcessed = !foundIssues.Any();

            } while (!hasProcessed);

            return issues;
        }

        private static async Task CopyToAzure(List<Issue> issues)
        {
            const string PAT_IDENTIFIER = "pat";

            var credentials = new VssClientCredentials(new VssBasicCredential(PAT_IDENTIFIER, _configuration.AzureUserPAT));

            var connection = new VssConnection(new Uri(_configuration.AzureOrganizationUrl), credentials);

            var client = connection.GetClient<WorkItemTrackingHttpClient>();

            foreach (var issue in issues)
            {
                Console.WriteLine($"Starting import of JIRA issue {issue.Key.Value}...");

                string typeName;
                JsonPatchDocument createDocument;

                if (issue.Type.Name == "Bug")
                {
                    Console.WriteLine("Issue recognised as bug...");

                    typeName = "Bug";
                    createDocument = BuildCreateBugDocument(issue);
                }
                else if (issue.Type.Name == "Story" || issue.Type.Name == "Task")
                {
                    Console.WriteLine("Issue recognised as story...");

                    typeName = "User Story";
                    createDocument = BuildCreateStoryDocument(issue);
                }
                else
                {
                    continue;
                }

                var createdWorkItem = await client.CreateWorkItemAsync(createDocument, _configuration.AzureProjectName, typeName).ConfigureAwait(false);

                Console.WriteLine("Work item created in Azure!");

                if (createdWorkItem?.Id == null)
                {
                    throw new Exception($"Failed creating JIRA issue {issue.Key} in Azure for an unknown reason...");
                }

                if (TryGetAzureStateFromJiraStatus(issue.Status, out var azureState))
                {
                    Console.WriteLine("Status needs to be updated...");

                    var patchDocument = BuildUpdateStateDocument(azureState);
                    await client.UpdateWorkItemAsync(patchDocument, createdWorkItem.Id.Value).ConfigureAwait(false);

                    Console.WriteLine("Azure work item status updated!");
                }

                Console.WriteLine($"Imported JIRA issue {issue.Key.Value}!");

                if(_configuration.MillisecondsBetweenAzureRequests > 0)
                    await Task.Delay(TimeSpan.FromMilliseconds(_configuration.MillisecondsBetweenAzureRequests)).ConfigureAwait(false);
            }
        }

        private static bool TryGetAzureStateFromJiraStatus(IssueStatus issueStatus, out string azureState)
        {
            azureState = null;

            switch (issueStatus.Name)
            {
                case "To do":
                    azureState = "New";
                    break;
                case "In Progress":
                    azureState = "Active";
                    break;
                case "Reopened":
                    azureState = "Active";
                    break;
                case "Review":
                    azureState = "Resolved";
                    break;
                case "Resolved":
                    azureState = "Resolved";
                    break;
                case "Closed":
                    azureState = "Closed";
                    break;
                case "Done":
                    azureState = "Closed";
                    break;
            }

            return azureState != null;
        }

        private static JsonPatchDocument BuildCreateBugDocument(Issue issue)
        {
            var patchDocument = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = issue.Summary,
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/Microsoft.VSTS.TCM.ReproSteps",
                    Value = issue.Description ?? "",
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.History",
                    Value = $"Imported from {issue.Key.Value}"
                }
            };

            return patchDocument;
        }

        private static JsonPatchDocument BuildCreateStoryDocument(Issue issue)
        {
            var patchDocument = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = issue.Summary,
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Description",
                    Value = issue.Description ?? "",
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.History",
                    Value = $"Imported from {issue.Key.Value}"
                }
            };

            return patchDocument;
        }

        private static JsonPatchDocument BuildUpdateStateDocument(string state)
        {
            return new JsonPatchDocument
            {
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.State",
                    Value = state
                }
            };
        }
    }
}
