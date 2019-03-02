# JIRA to Azure Issue Migrator

If you or your company is moving to Azure Devops from JIRA, and want to import all issues, this JIRA to Azure issue migrator will take issues from any given project in your JIRA instance and import them as work items to Azure Boards.

This is a .NET Framework 4.7.2 console application, supporting C# 7.3. It uses the following first party libraries to communicate with JIRA and Azure's REST API respectively:

- [Atlassian.NET SDK](https://bitbucket.org/farmas/atlassian.net-sdk/wiki/Home)
- [Microsoft TFS Extended Client](https://www.nuget.org/packages/Microsoft.TeamFoundationServer.ExtendedClient/)

Both have smaller dependencies of their own. No other dependencies are required.

### How to use it

To use it, either clone/fork/download this repository and build for yourself in Visual Studio or your favourite IDE that supports .NET Framework or find a standalone release on the releases page of this repository, here.

The application requires for you to setup the `app.config` with the following keys:

- `JiraInstanceUrl` URL to your JIRA instance, for example: https://my.jira.com
- `JiraUsername` Username the account that will be exporting issues uses to log in
- `JiraPassword` Password the account that will be exporting issues uses to log in
- `JiraProjectKey` Key of the project you want to export issues from
- `JiraMaximumToQuery` Maximum number of issues to query per request, JIRA's default is 20
- `AzureOrganizationUrl` URL to your organization, for example: https://dev.azure.com/my-great-code
- `AzureProjectName` Name of the project you want to import the issues to
- `MillisecondsBetweenAzureRequests` Number of milliseconds to delay between each request, as a buffer

Only when all of these configurations are set can the console application run.

Why not arguments? App config is nicer to configure.

This was tested on an on-premise JIRA Server installation, and has not been tested on the hosted version of JIRA, but should work the same way.

### What does it do

The application will query for **all** issues in the provided JIRA project, be it open or closed/done issues. There are a number of assumptions made as to the configuration of your JIRA instance therefore, if you have any custom statuses, workflows or issue types, the code will need to be customised before running.

It will optimistically attempt to map open statuses to the "New" status in Azure. It will optimistically attempt to map in progress statuses to "Active" in Azure. It will optimistically attempt to map closed/resolved/done to "Closed" in Azure.

If you have your own concepts/workflows in either JIRA or Azure, you will need to modify this code.

As users cannot be guaranteed to exist in both JIRA and Azure Devops, all imported issues will be created as unassigned. You will need to customise the code to make the relevant mappings from JIRA user to Azure user. To do this, supply the email of the assignee to Azure with the following JSON patch operation when creating or updating the work item:

```cs
new JsonPatchOperation
{
    Operation = Operation.Add,
    Path = "/fields/System.AssignedTo",
    Value = "user@organization.com"
}
```

### Do with it what you want

JIRAToAzure is licensed under MIT, you're free to do with it what you want. Contribute, download it, fork it, steal it, sell it, repurpose it.

### Issue or question?

While this was a one time use project, issues or questions can be put up in the repository and they'll be supported.