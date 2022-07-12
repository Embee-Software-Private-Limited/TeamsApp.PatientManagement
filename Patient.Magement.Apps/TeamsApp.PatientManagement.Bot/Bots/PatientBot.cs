using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using TeamsApp.PatientManagement.Common.Resources;
using TeamsApp.PatientManagement.Common.Services.AdaptiveCard;
using TeamsApp.PatientManagement.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using TeamsApp.PatientManagement.Common.Helper;
using TeamsApp.PatientManagement.Common.Models.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;

namespace TeamsApp.PatientManagement.Bot.Bots
{
    public sealed class PatientBot : TeamsActivityHandler
    {
        /// <summary>
        /// Instance to send logs to the Application Insights service.
        /// </summary>
        private readonly ILogger<PatientBot> logger;

        /// <summary>
        /// The current culture's string localizer.
        /// </summary>
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// Instance of Application Insights Telemetry client.
        /// </summary>
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Provides helper methods for bot related activities.
        /// </summary>
        private readonly IAppLifecycleHandler appLifecycleHandler;

        private readonly IAdaptiveCardService adaptiveCardService;
        private readonly UserState userState;
        private readonly IStatePropertyAccessor<string> userConfigProperty;

        private readonly IUserHelper _userHelper;
        /// <summary>
        /// Instance of memory cache to cache reportees for managers.
        /// </summary>
        private readonly IMemoryCache memoryCache;
        /// <summary>
        /// Initializes a new instance of the <see cref="DexterActivityHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="localizer">The current culture's string localizer.</param>
        /// <param name="telemetryClient">The Application Insights telemetry client. </param>
        /// <param name="appLifecycleHandler">Provides helper methods for bot related activities.</param>
        public PatientBot(
            ILogger<PatientBot> logger,
            IStringLocalizer<Strings> localizer,
            TelemetryClient telemetryClient,
            IAppLifecycleHandler appLifecycleHandler,
            IAdaptiveCardService adaptiveCardService,
            UserState userState,
            IUserHelper userHelper,
            IMemoryCache memoryCache
            )

        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.appLifecycleHandler = appLifecycleHandler ?? throw new ArgumentNullException(nameof(appLifecycleHandler));
            this.adaptiveCardService = adaptiveCardService ?? throw new ArgumentNullException(nameof(adaptiveCardService));

            //this.connectionName = configuration["ConnectionName"] ?? throw new NullReferenceException("ConnectionName");
            //this.siteUrl = configuration["SiteUrl"] ?? throw new NullReferenceException("SiteUrl");
            this.userState = userState ?? throw new NullReferenceException(nameof(userState));
            this.userConfigProperty = userState.CreateProperty<string>("UserConfiguration");
            this._userHelper = userHelper ?? throw new ArgumentNullException(nameof(userHelper));
            
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // After the turn is complete, persist any UserState changes.
            await userState.SaveChangesAsync(turnContext);
        }
        /// <summary>
        /// Invoked when members other than this bot (like a user) are removed from the conversation.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
                this.RecordEvent(nameof(this.OnConversationUpdateActivityAsync), turnContext);

                var activity = turnContext.Activity;
                this.logger.LogInformation($"conversationType: {activity.Conversation.ConversationType}, membersAdded: {activity.MembersAdded?.Count}, membersRemoved: {activity.MembersRemoved?.Count}");

                switch (activity.Conversation.ConversationType)
                {
                    case ConversationTypes.Channel:
                        var teamsChannelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
                        //App installed in team
                        if (activity.MembersAdded != null && activity.MembersAdded.Any(member => member.Id == activity.Recipient.Id))
                        {
                            await this.appLifecycleHandler.OnBotInstalledInTeamsAsync(turnContext, teamsChannelData);
                        }
                        //App unistalled in team
                        else if (activity.MembersRemoved != null && activity.MembersRemoved.Any(member => member.Id == activity.Recipient.Id))
                        {
                            //await this.appLifecycleHandler.OnBotRemovedInTeamsAsync(turnContext, AppName, teamsChannelData);
                        }
                        break;
                    default: break;
                }

            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occurred while bot conversation update event.");
                throw;
            }
            
        }

        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(
            ITurnContext<IInvokeActivity> turnContext,
            TaskModuleRequest taskModuleRequest,
            CancellationToken cancellationToken)
        {
            try
            {
                return this.appLifecycleHandler.OnFetchAsync(turnContext, taskModuleRequest);
                
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error fetching task module : {ex.Message}", SeverityLevel.Error);
                return default;
            }
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(
            ITurnContext<IInvokeActivity> turnContext,
            TaskModuleRequest taskModuleRequest,
            CancellationToken cancellationToken)
        {
            try
            {
                return await this.appLifecycleHandler.OnSubmitAsync(turnContext, taskModuleRequest);
                
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error submitting the task module : {ex.Message}", SeverityLevel.Error);
                return default;
            }
        }

        /// <summary>
        /// Records event data to Application Insights telemetry client.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// 
        private void RecordEvent(string eventName, ITurnContext turnContext)
        {
            var teamsChannelData = turnContext.Activity.GetChannelData<TeamsChannelData>();

            this.telemetryClient.TrackEvent(eventName, new Dictionary<string, string>
            {
                { "userId", turnContext.Activity.From.AadObjectId },
                { "tenantId", turnContext.Activity.Conversation.TenantId },
                { "teamId", teamsChannelData?.Team?.Id },
                { "channelId", teamsChannelData?.Channel?.Id },
            });
        }
    }

}
