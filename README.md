
# sk-dev-team

# Build a Virtual AI Dev Team using Semantic Kernel Skills

# Goal

From a natural language specification, set out to integrate a team of AI copilot skills into your team’s dev process, either for discrete tasks on an existing repo (unit tests, pipeline expansions, PRs for specific intents), developing a new feature, or even building an application from scratch.  Starting from an existing repo and a broad statement of intent, work with multiple AI copilot dev skills, each of which has a different emphasis - from architecture, to task breakdown, to plans for individual tasks, to code output, code review, efficiency, documentation, build, writing tests, setting up pipelines, deployment, integration tests, and then validation.
The system will present a view that facilitates chain-of-thought coordination across multiple trees of reasoning with the dev team skills.

## Status

* You can iterate on building a workflow for your semantic kernel ai dev skills using Elsa Workflows designer and run these workflows to see the results.  The workflows do not yet support adding memory context. 
* You can use the CLI project to run the SK dev skills from the command line. The CLI supports using the [Microsoft Azure Well-Architected Frameworl](https://learn.microsoft.com/en-us/azure/well-architected/) as memory context for the skill invocations. 

## Trying it out

### Elsa.SemanticKernel

SemanticKernel Activity Provider for Elsa Workflows 3.x

The project supports running [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel) Skills as workflows using [Elsa Workflows](https://v3.elsaworkflows.io).  You can build the workflows as .NET code or in the visual designer.
To run the designer:

```bash
> cd WorkflowsApp
> cp .env_example .env
# Edit the .env file to choose your AI model, add your API Endpoint, and secrets.
> . ./.env
> dotnet build
> dotnet run
# Open browser to the URI in the console output
```

By Default you can use "admin" and "password" to login. Please review [Workflow Security](https://v3.elsaworkflows.io/docs/installation/aspnet-apps-workflow-server) for into on securing the app, using API tokens, and more.

To [invoke](https://v3.elsaworkflows.io/docs/guides/invoking-workflows) a workflow, first it must be "Published". If your workflow has a trigger activity, you can use that. When your workflow is ready, click the "Publish" button. You can also execute the workflow using the API. Then, find the Workflow Definition ID. From a command line, you can use "curl":

```bash
> curl --location 'https://localhost:5001/elsa/api/workflow-definitions/{workflow_definition_id}/execute' \
--header 'Content-Type: application/json' \
--header 'Authorization: ApiKey {api_key}' \
--data '{
}'
```

Once you have the app runing locally, you can login (admin/password - see the [Elsa Workflows](https://v3.elsaworkflows.io) for info about securing). Then you can click "new workflow" to begin building your workflow with semantic kernel skills.

1. Drag workflow Activity blocks into the designer, and examine the settings.
2. Connect the Activities to specify an order of operations.
3. You can use Workfflow Variables to pass state between activities.
   1. Create a Workflow Variable, "MyVariable"
   2. Click on the Activity that you want to use to populate the variable.
   3. In the Settings box for the Activity, Click "Output"
   4. Set the "Output" to the variable chosen.
   5. Click the Activity that will use the variable. Click on "Settings".
   6. Find the text box representing the variable that you want to populate, in this case usually "input".
   7. Click the "..." widget above the text box, and select "javascript"
   8. Set the value of the text box to

   ```javascript
   `${getMyVariable()}`
   ```

   9. Run the workflow.

## Via CLI

The easiest way to run the project is in Codespaces. Codespaces will start a qdrant instance for you.

1. Create a new codespace from the *code* button on the main branch.
2. Once the code space setup is finished, from the terminal:

```bash
> cd cli
cli> cp ../WorkflowsApp/.env_example . 
# Edit the .env file to choose your AI model, add your API Endpoint, and secrets.
cli> bash .env
cli> dotnet build
cli> dotnet run --file util/ToDoListSamplePrompt.txt do it
```

You will find the output in the *output/* directory.

## Proposed UX

* Possible UI: Start with an existing repo (GH or ADO), either populated or empty, and API Keys / config for access – once configured / loaded split view between three columns:
  * Settings/History/Tasks (allows browsing into each of the chats with a copilot dev team role) | [Central Window Chat interface with Copilot DevTeam] | Repo browsing/editing
  * Alternate interface will be via VS Code plugin/other IDE plugins, following the plugin idiom for each IDE
  * Settings include teams channel for conversations, repo config and api keys, model config and api keys, and any desired prompt template additions
* CLI: start simple with a CLI that can be passed a file as prompt input and takes optional arguments as to which skills to invoke
* User begins with specifying a repository and then statement of what they want to accomplish, natural language, as simple or as detailed as needed.
  * SK DevTeam skill will use dialog to refine the intent as needed, returns a plan, proposes necessary steps
  * User approves the plan or gives feedback, requests iteration
  * Plan is parceled out to the appropriate further skills
  * Eg, for a new app:
    * Architecture is passed to DevLead skill gives plan/task breakdown.
    * DevLead breaks down tasks into smaller tasks, each of these is fed to a skill to decide if it is a single code module or multiple
    * Each module is further fed to a dev lead to break down again or specify a prompt for a coder
    * Each code module prompt is fed to a coder
    * Each module output from a coder is fed to a code reviewer (with context, specific goals)
    * Each reviewer proposes changes, which result in a new prompt for the original coder
    * Changes are accepted by the coder
    * Each module fed to a builder
    * If it doesn’t build sent back to review
    * (etc)

## Proposed Architecture

* SK Kernel Service – ASP.NET Core Service with REST API
* SK Skills:
  * PM Skill – generates pot, word docs, describing app,
  * Designer Skill – mockups?
  * Architect Skill – proposes overall arch
  * DevLead Skill – proposes task breakdown
  * CoderSkill – builds code modules for each task
  * ReviewerSkill – improves code modules
  * TestSkill – writes tests
  * Etc
* Web app: prompt front end and wizard style editor of app
* Build service sandboxes – using branches and actions/pipelines 1st draft; Alternate – ephemeral build containers
* Logging service streaming back to azure logs analytics, app insights, and teams channel
* Deployment service – actions/pipelines driven
* Azure Dev Skill – lean into azure integrations – crawl the azure estate to inventory a tenant’s existing resources to memory and help inform new code. Eg: you have a large azure sql estate? Ok, most likely you want to wire your new app to one of those dbs, etc….

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit <https://cla.opensource.microsoft.com>.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

# Legal Notices

Microsoft and any contributors grant you a license to the Microsoft documentation and other content
in this repository under the [Creative Commons Attribution 4.0 International Public License](https://creativecommons.org/licenses/by/4.0/legalcode),
see the [LICENSE](LICENSE) file, and grant you a license to any code in the repository under the [MIT License](https://opensource.org/licenses/MIT), see the
[LICENSE-CODE](LICENSE-CODE) file.

Microsoft, Windows, Microsoft Azure and/or other Microsoft products and services referenced in the documentation
may be either trademarks or registered trademarks of Microsoft in the United States and/or other countries.
The licenses for this project do not grant you rights to use any Microsoft names, logos, or trademarks.
Microsoft's general trademark guidelines can be found at <http://go.microsoft.com/fwlink/?LinkID=254653>.

Privacy information can be found at <https://privacy.microsoft.com/en-us/>

Microsoft and any contributors reserve all other rights, whether under their respective copyrights, patents,
or trademarks, whether by implication, estoppel or otherwise.
