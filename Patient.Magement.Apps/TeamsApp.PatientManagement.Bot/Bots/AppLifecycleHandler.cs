using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;
using TeamsApp.PatientManagement.Common;
using TeamsApp.PatientManagement.Common.Models;
using TeamsApp.PatientManagement.Common.Models.Entities;
using TeamsApp.PatientManagement.Common.Providers;
using TeamsApp.PatientManagement.Common.Services.AdaptiveCard;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using TeamsApp.PatientManagement.Common.Models.Configuration;
using Microsoft.Extensions.Options;
using TeamsApp.PatientManagement.Common.CommonBot;

namespace TeamsApp.PatientManagement.Bot.Bots
{
    public class AppLifecycleHandler : IAppLifecycleHandler
    {
        /// <summary>
        /// Represents the task module height.
        /// </summary>
        private const int TaskModuleHeight = 450;

        /// <summary>
        /// Represents the task module width.
        /// </summary>
        private const int TaskModuleWidth = 500;
        /// <summary>
        /// Instance to send logs to the Application Insights service.
        /// </summary>
        private readonly ILogger<AppLifecycleHandler> logger;

        /// <summary>
        /// Instance of adaptive card service to create and get adaptive cards.
        /// </summary>
        private readonly IAdaptiveCardService adaptiveCardService;

        private readonly IConversationStorageProvider conversationStorageProvider;
        private readonly IOptions<BotSettings> botOptions;

        private readonly ITokenHelper tokenHelper;
        /// <summary>
        /// Initializes a new instance of the <see cref="AppLifecycleHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="adaptiveCardService">Instance of adaptive card service to create and get adaptive cards.</param>
        /// <param name="repositoryAccessors">The instance of repository accessors.</param>
        public AppLifecycleHandler(
            ILogger<AppLifecycleHandler> logger,
            IAdaptiveCardService adaptiveCardService,
            IConversationStorageProvider conversationStorageProvider,
            IOptions<BotSettings> botOptions,
            ITokenHelper tokenHelper
            )
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.adaptiveCardService = adaptiveCardService ?? throw new ArgumentNullException(nameof(adaptiveCardService));
            this.conversationStorageProvider = conversationStorageProvider ?? throw new ArgumentNullException(nameof(conversationStorageProvider));
            this.botOptions = botOptions ?? throw new ArgumentNullException(nameof(botOptions));
            this.tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
        }

        #region Team Conversation
        public async Task OnBotRemovedInTeamsAsync(ITurnContext turnContext, TeamsChannelData teamsChannelData)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext), "Turncontext cannot be null");
            var activity = turnContext.Activity;
            await UpdateTeamsConversationOnBotUninstall(activity, teamsChannelData);

            this.logger.LogInformation($"Successfully uninstalled app for team {teamsChannelData.Team.Name}.");
        }
        public async Task OnBotInstalledInTeamsAsync(ITurnContext turnContext, TeamsChannelData teamsChannelData)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext), "Turncontext cannot be null");

            this.logger.LogInformation($"Bot added in channel scope for user {turnContext.Activity.From.AadObjectId}");

            var activity = turnContext.Activity;

            await InsertUpdateTeamsConversation(activity, teamsChannelData);

            this.logger.LogInformation($"Successfully installed app for user {activity.From.AadObjectId}.");
        }

        private async Task InsertUpdateTeamsConversation(IConversationUpdateActivity activity, TeamsChannelData teamsChannelData)
        {
            // Add or update user details when bot is installed.
            var existingRecord = await this.conversationStorageProvider.GetTeamAsync(teamsChannelData.Team.Id);
            bool operationStatus = false;
            if (existingRecord != null && existingRecord.Count > 0)
            {
                var userConversation = existingRecord[0];
                userConversation.ConversationId = activity.Conversation.Id;
                userConversation.ServiceUrl = activity.ServiceUrl;
                userConversation.BotInstalledOn = DateTime.UtcNow;
                userConversation.TeamId = teamsChannelData.Team.Id;
                userConversation.TeamName = teamsChannelData.Team.Name;
                userConversation.TeamAadGroupId = teamsChannelData.Team.AadGroupId;
                userConversation.ActivityId = activity.Id;
                userConversation.TenantId = activity.Conversation.TenantId;


                operationStatus = await this.conversationStorageProvider.AddEntityAsync(userConversation).ConfigureAwait(false);
                if (!operationStatus)
                {
                    this.logger.LogInformation($"Unable to update conversation data in table storage {teamsChannelData.Team.Id}");
                }
                this.logger.LogInformation($"Successfully updated conversation entity {teamsChannelData.Team.Id}.");
            }
            else
            {
                var conversationEntity = new ConversationEntity
                {
                    PartitionKey = AzureStorageTablePartitionNames.TeamConversationPartitionKey,
                    RowKey = teamsChannelData.Team.Id,
                    BotInstalledOn = DateTime.Now,
                    ConversationId = activity.Conversation.Id,
                    ServiceUrl = activity.ServiceUrl,
                    TeamId = teamsChannelData.Team.Id,
                    TeamAadGroupId = teamsChannelData.Team.AadGroupId,
                    TeamName = teamsChannelData.Team.Name,
                    ActivityId = activity.Id,
                    TenantId = activity.Conversation.TenantId
                };

                operationStatus = await this.conversationStorageProvider.AddEntityAsync(conversationEntity).ConfigureAwait(false);
                if (!operationStatus)
                {
                    this.logger.LogInformation($"Unable to add conversation data in table storage {activity.From.AadObjectId}");
                }
                this.logger.LogInformation($"Successfully added conversation entity {activity.From.AadObjectId}.");
            }
        }

        private async Task UpdateTeamsConversationOnBotUninstall(IConversationUpdateActivity activity, TeamsChannelData teamsChannelData)
        {
            //Add or update user details when bot is uninstalled.
            var existingRecord = await this.conversationStorageProvider.GetTeamAsync(teamsChannelData.Team.Id);

            if (existingRecord != null && existingRecord.Count > 0)
            {
                var userConversation = existingRecord[0];
                userConversation.ConversationId = activity.Conversation.Id;
                userConversation.ServiceUrl = activity.ServiceUrl;
                userConversation.BotInstalledOn = DateTime.UtcNow;
                userConversation.TeamId = teamsChannelData.Team.Id;
                userConversation.TeamName = teamsChannelData.Team.Name;
                userConversation.TeamAadGroupId = teamsChannelData.Team.AadGroupId;
                userConversation.ActivityId = activity.Id;
                userConversation.TenantId = activity.Conversation.TenantId;
                var operationStatus = await this.conversationStorageProvider.AddEntityAsync(userConversation).ConfigureAwait(false);
                if (!operationStatus)
                {
                    this.logger.LogInformation($"Unable to update conversation data in table storage {teamsChannelData.Team.Id}");
                }
                this.logger.LogInformation($"Successfully updated conversation entity {teamsChannelData.Team.Id}.");
            }
        }

        #endregion

        #region Task module
        private Task<TaskModuleResponse> GetTaskModuleResponseAsync(string taskModuleTitle, string taskModuleUrl, string queryParams = "")
        {
            return Task.FromResult(new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Url = queryParams != "" ? $"{taskModuleUrl}?theme={{theme}}&locale={{locale}}&{queryParams}" : $"{taskModuleUrl}?theme={{theme}}&locale={{locale}}",
                        Height = TaskModuleHeight,
                        Width = TaskModuleWidth,
                        Title = taskModuleTitle,
                        FallbackUrl = queryParams != "" ? $"{taskModuleUrl}?theme={{theme}}&locale={{locale}}&{queryParams}" : $"{taskModuleUrl}?theme={{theme}}&locale={{locale}}"
                    },
                },
            });
        }

        
        public Task<TaskModuleResponse> OnFetchAsync(ITurnContext<IInvokeActivity> turnContext,
            TaskModuleRequest taskModuleRequest)
        {
            try
            {
                
                var activity = (Activity)turnContext.Activity;
                var token = "";
                try
                {
                    token = this.tokenHelper.GenerateAPIAuthToken(applicationBasePath:this.botOptions.Value.AppBaseUri, fromId: activity.From.AadObjectId,60);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Error while GenerateAPIAuthToken.");
                }
                var postedValues = JsonConvert.DeserializeObject<AdaptiveCardActionModel>(JObject.Parse(taskModuleRequest?.Data?.ToString()).ToString());
                string command = postedValues.Command;
                var taskModuleTitle = "";
                var taskModuleUrl = "";
                var queryParams = "";
                queryParams = $"UHID={postedValues.UHID}&AdmissionId={postedValues.AdmissionId}&DepartmentId={postedValues.DepartmentId}&UserToken={token}";
                switch (command.ToUpperInvariant())
                {
                    case BotCommandConstants.ViewPatientDetails:
                        this.logger.LogInformation($"Invoking patient details task module for patiend id :{postedValues.PatientId}.");
                        taskModuleTitle = "Patient Details";
                        taskModuleUrl = $"{this.botOptions.Value.AppBaseUri}{BotCommandConstants.ViewPatientDetailsPageUrl}";
                        break;
                    case BotCommandConstants.ViewVitalsDetails:
                        this.logger.LogInformation($"Invoking vital details task module for patiend id :{postedValues.PatientId}.");
                        taskModuleTitle = "Vital Details";
                        taskModuleUrl = $"{this.botOptions.Value.AppBaseUri}{BotCommandConstants.ViewVitalsDetailsPageUrl}";
                        break;
                    case BotCommandConstants.ViewPrescriptionDetails:
                        this.logger.LogInformation($"Invoking prescription details task module for patiend id :{postedValues.PatientId}.");
                        taskModuleTitle = "Prescription Details";
                        taskModuleUrl = $"{this.botOptions.Value.AppBaseUri}{BotCommandConstants.ViewPrescriptionDetailsPageUrl}";
                        break;
                    case BotCommandConstants.ViewMedicalHistoryDetails:
                        this.logger.LogInformation($"Invoking medical history details task module for patiend id :{postedValues.PatientId}.");
                        taskModuleTitle = "Medical History Details";
                        taskModuleUrl = $"{this.botOptions.Value.AppBaseUri}{BotCommandConstants.ViewMedicalHistoryDetailsPageUrl}";
                        break;
                    case BotCommandConstants.ViewDoctorNotesDetails:
                        this.logger.LogInformation($"Invoking doctor notes details task module for patiend id :{postedValues.PatientId}.");
                        taskModuleTitle = "Doctor Notes Details";
                        taskModuleUrl = $"{this.botOptions.Value.AppBaseUri}{BotCommandConstants.ViewDoctorNotesDetailsPageUrl}";
                        break;
                    case BotCommandConstants.ViewIntakeOutputDetails:
                        this.logger.LogInformation($"Invoking intake output details task module for patiend id :{postedValues.PatientId}.");
                        taskModuleTitle = "Intake Output Details";
                        taskModuleUrl = $"{this.botOptions.Value.AppBaseUri}{BotCommandConstants.ViewIntakeOutputDetailsPageUrl}";
                        break;
                    case BotCommandConstants.ViewLabReportDetails:
                        this.logger.LogInformation($"Invoking lab report details task module for patiend id :{postedValues.PatientId}.");
                        taskModuleTitle = "Lab Report Details";
                        taskModuleUrl = $"{this.botOptions.Value.AppBaseUri}{BotCommandConstants.ViewLabReportDetailsPageUrl}";
                        break;

                }

                return GetTaskModuleResponseAsync(taskModuleTitle, taskModuleUrl, queryParams);

            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while fetch event is received from the user.");
                throw;
            }
        }


        public async Task<TaskModuleResponse> OnSubmitAsync(
            ITurnContext<IInvokeActivity> turnContext,
            TaskModuleRequest taskModuleRequest)
        {
            try
            {
                turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
                var activity = (Activity)turnContext.Activity;

                var postedValues = JsonConvert.DeserializeObject<AdaptiveCardActionModel>(((JObject)activity.Value).GetValue("data", StringComparison.OrdinalIgnoreCase)?.ToString());
                string command = postedValues.Command;
                switch (command.ToUpperInvariant())
                {
                    default:
                        this.logger.LogInformation($"Invalid command for task module fetch activity.Command is : {command} ");
                        await turnContext.SendActivityAsync("Invalid command for task module fetch activity.Command is : {command} ");
                        return null;
                }

            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while submit event is received from the user.");
                await turnContext.SendActivityAsync("Error while submit event is received from the user").ConfigureAwait(false);
                throw ex;
            }
        }
        #endregion

    }
}
