
using TeamsApp.PatientManagement.Bot.Services.MicrosoftGraph;
using TeamsApp.PatientManagement.Common.Models.Configuration;
using TeamsApp.PatientManagement.Common.Models.Entities;
using TeamsApp.PatientManagement.Common.Models.Ether;
using TeamsApp.PatientManagement.Common.Providers;
using TeamsApp.PatientManagement.Common.Services.AdaptiveCard;
using Microsoft.ApplicationInsights;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace TeamsApp.PatientManagement.Common.Helper
{
    public class NotificationHelper : INotificationHelper
    {
        private readonly ILogger<NotificationHelper> _logger;

        /// <summary>
        /// Telemetry client to log event and errors.
        /// </summary>
        ///
        private readonly TelemetryClient _telemetryClient;

        private readonly IOptions<BotSettings> _botOptions;

        private readonly IAdaptiveCardService _adaptiveCardService;

        private readonly IConversationStorageProvider _conversationStorageProvider;
        private readonly IDepartmentStorageProvider _departmentStorageProvider;
        private readonly ICardNotificationsStorageProvider _cardNotificationsStorageProvider;
        private readonly IMessageService _messageService;
        public NotificationHelper(
            ILogger<NotificationHelper> logger,
            TelemetryClient telemetryClient,
            IOptions<BotSettings> botOptions,
            IAdaptiveCardService adaptiveCardService,
            IConversationStorageProvider conversationStorageProvider,
            IDepartmentStorageProvider departmentStorageProvider,
            ICardNotificationsStorageProvider cardNotificationsStorageProvider,
            IMessageService messageService)
        {
            this._telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this._logger = logger;
            this._botOptions = botOptions ?? throw new ArgumentNullException(nameof(botOptions));
            this._adaptiveCardService = adaptiveCardService ?? throw new ArgumentNullException(nameof(adaptiveCardService));
            this._conversationStorageProvider = conversationStorageProvider ?? throw new ArgumentNullException(nameof(conversationStorageProvider));
            this._departmentStorageProvider = departmentStorageProvider ?? throw new ArgumentNullException(nameof(departmentStorageProvider));
            this._cardNotificationsStorageProvider = cardNotificationsStorageProvider ?? throw new ArgumentNullException(nameof(cardNotificationsStorageProvider));
            this._messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        }
        private ConnectorClient CreateBotConnectionClient(string serviceUrl)
        {
            Uri url = new Uri(serviceUrl);
            string appId = this._botOptions.Value.MicrosoftAppId;
            string appPwd = this._botOptions.Value.MicrosoftAppPassword;
            return new ConnectorClient(url, appId, appPwd);
        }


        #region Channel Scope

        private async Task<ResourceResponse> SendNewNotificationInChannelScopeAsync(string teamChannelId, string serviceUrl, Attachment cardToSend, string replyToId)
        {
            ResourceResponse responseModel = null;
            try
            {
                if (!string.IsNullOrEmpty(teamChannelId) && !string.IsNullOrEmpty(serviceUrl))
                {
                    using (ConnectorClient connectorClient = CreateBotConnectionClient(serviceUrl))
                    {

                        var activity = new Activity()
                        {
                            ReplyToId = replyToId,
                            Type = ActivityTypes.Message,
                            ChannelData = new TeamsChannelData
                            {
                                Channel = new ChannelInfo(teamChannelId),

                            },
                            Conversation = new ConversationAccount()
                            {
                                Id = teamChannelId,
                                IsGroup = true,

                            },
                            Attachments = new List<Attachment>()
                                 {
                                     cardToSend
                                 }

                        };

                        if (!string.IsNullOrEmpty(replyToId))
                        {
                            responseModel = await connectorClient.Conversations.ReplyToActivityAsync(activity);
                        }
                        else
                        {
                            responseModel = await connectorClient.Conversations.SendToConversationAsync(activity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error in executing SendNewNotificationInChannelScopeAsync- Channel id -> {teamChannelId}");
                return null;
            }

            return responseModel;
        }
        private async Task<ResourceResponse> RelyToMessageInChannelScopeUsingGraphAPIAsync(string teamId, string teamChannelId, string message, string replyToId)
        {
            ResourceResponse responseModel = null;
            try
            {

                var messageReply = await this._messageService.SendMessageAsync(teamId, teamChannelId, replyToId, message);
                responseModel = new ResourceResponse();
                responseModel.Id = messageReply.Id;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error in executing RelyToMessageInChannelScopeUsingGraphAPIAsync- Team Id ->{teamId} : Channel id -> {teamChannelId} : Reply to id -> {replyToId}");
                return null;
            }

            return responseModel;
        }

        private async Task<ResourceResponse> SendNotificationInChannelMentionUserScopeAsync(string teamId, string teamChannelId, string serviceUrl, Attachment cardToSend, List<string> userEmails)
        {
            ResourceResponse responseModel = null;
            try
            {
                if (!string.IsNullOrEmpty(teamChannelId) && !string.IsNullOrEmpty(serviceUrl))
                {
                    using (ConnectorClient connectorClient = CreateBotConnectionClient(serviceUrl))
                    {

                        var activity = new Activity()
                        {
                            Type = ActivityTypes.Message,
                            ChannelData = new TeamsChannelData
                            {
                                Channel = new ChannelInfo(teamChannelId),


                            },
                            Conversation = new ConversationAccount()
                            {
                                Id = teamChannelId,
                                IsGroup = true,

                            },
                            Attachments = new List<Attachment>()
                                 {
                                     cardToSend
                                 }

                        };



                        var teamMembers = await connectorClient.Conversations.GetConversationMembersAsync(teamId, default);

                        if (userEmails != null && userEmails.Count > 0)
                        {
                            IList<Entity> mentionedEntities = new List<Entity>();
                            foreach (var email in userEmails)
                            {
                                var userToMention = teamMembers
                                .Select(channelAccount => JObject.FromObject(channelAccount).ToObject<TeamsChannelAccount>())
                                .First(user => user.Email.ToLower() == email.ToLower());
                                var mention = new Mention
                                {
                                    Mentioned = userToMention,
                                    Text = $"<at>{userToMention.Name}</at>",
                                };
                                mentionedEntities.Add(mention);

                            }
                            activity.Entities = mentionedEntities;
                        }

                        responseModel = await connectorClient.Conversations.SendToConversationAsync(activity);

                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error in executing SendNewNotificationInChannelScopeAsync- Channel id -> {teamChannelId}");
                return null;
            }

            return responseModel;
        }

        private async Task<ResourceResponse> SendMessageNotificationInChannelScopeAsync(string teamChannelId, string serviceUrl, string messsage, string replyToId)
        {
            ResourceResponse responseModel = null;
            try
            {
                if (!string.IsNullOrEmpty(teamChannelId) && !string.IsNullOrEmpty(serviceUrl))
                {
                    using (ConnectorClient connectorClient = CreateBotConnectionClient(serviceUrl))
                    {

                        var activity = new Activity()
                        {
                            ReplyToId = replyToId,
                            Type = ActivityTypes.Message,
                            ChannelData = new TeamsChannelData
                            {
                                Channel = new ChannelInfo(teamChannelId),

                            },
                            Conversation = new ConversationAccount()
                            {
                                Id = teamChannelId,
                                IsGroup = true,

                            },
                            Text = messsage

                        };

                        if (!string.IsNullOrEmpty(replyToId))
                        {
                            Activity reply = activity.CreateReply(messsage);
                            //responseModel = await connectorClient.Conversations.ReplyToActivityAsync(activity);
                            responseModel = await connectorClient.Conversations.ReplyToActivityAsync(teamChannelId, replyToId, reply, cancellationToken: default);
                        }
                        else
                        {
                            responseModel = await connectorClient.Conversations.SendToConversationAsync(activity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error in executing SendNewNotificationInChannelScopeAsync- Channel id -> {teamChannelId}");
                return null;
            }

            return responseModel;
        }
        #endregion


        #region Channel Notification

        public async Task<bool> NotifyOnNewPatientAdmission(EtherNewPatientPostModel dataModel, EtherPatientResponseModel patientDetails, EtherPatientAdmissionDetailsResponseModel admissionDetails)
        {
            IList<ConversationEntity> conversationEntity = null;
            ResourceResponse resourceResponse = null;
            Attachment cardAttachment = null;
            bool isNotified = false;
            try
            {
                this._logger.LogInformation($"Getting deparment from deparment entity deparment id = {dataModel.DepartmentId}");

                var department = await this._departmentStorageProvider.GetAsync(dataModel.DepartmentId).ConfigureAwait(false);
                if (department != null)
                {
                    var patientDetailItem = patientDetails.body.FirstOrDefault();
                    patientDetailItem.AdmissionId = dataModel.AdmissionId;

                    this._logger.LogInformation($"Getting team conversation from conversation entity department id = {dataModel.DepartmentId}");
                    conversationEntity = await this._conversationStorageProvider.GetTeamGroupIdAsync(department.TeamId).ConfigureAwait(false);

                    // var cardNotificationEntity=await this._cardNotificationsStorageProvider.GetByPatientAdmissionAndChannelIdAsync(patientDetailItem.UHID, patientDetailItem.PatientId, department.ChannelId).ConfigureAwait(false);

                    if (conversationEntity != null && conversationEntity.Count > 0)
                    {
                        this._logger.LogInformation($"Getting adaptive card details new patient department id = {dataModel.DepartmentId}");

                        cardAttachment = this._adaptiveCardService.GetNewPatientDetailCard(patientDetails, department.ChannelId, department.TeamId, department.DepartmentId, admissionDetails);
                        //cardAttachment = this._adaptiveCardService.TestCard();
                        if (conversationEntity.FirstOrDefault() != null && !string.IsNullOrEmpty(department.ChannelId) && cardAttachment != null)
                        {
                            this._logger.LogInformation($"Sending notification on new patient  UHID  -> {patientDetailItem.UHID} : to channel -> {department.ChannelId}");

                            resourceResponse = await this.SendNewNotificationInChannelScopeAsync(teamChannelId: department.ChannelId, conversationEntity.FirstOrDefault().ServiceUrl, cardToSend: cardAttachment, null);
                            if (resourceResponse != null)
                            {
                                //Save Card notification
                                CardNotificationsEntity notification = new CardNotificationsEntity
                                {
                                    ActivityId = resourceResponse.Id,
                                    ChannelId = department.ChannelId,
                                    AdmissionId = dataModel.AdmissionId,
                                    ConversationId = department.ChannelId,
                                    PatientId = patientDetailItem.PatientId,
                                    ServiceUrl = conversationEntity[0].ServiceUrl,
                                    TeamId = conversationEntity[0].TeamAadGroupId,
                                    TenantId = conversationEntity[0].TenantId,
                                    UHID = patientDetailItem.UHID,
                                    ReplyToId = resourceResponse.Id
                                };

                                await this._cardNotificationsStorageProvider.AddEntityAsync(notification).ConfigureAwait(false);

                                isNotified = true;
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error occurred while executing NotifyOnNewPatientAdmission.");
            }
            return isNotified;

        }

        public async Task<bool> ReplyToMessageInChannel(AdaptiveCardActionModel cardActionDataModel)
        {
            ResourceResponse resourceResponse = null;
            bool isNotified = false;
            try
            {

                if (!string.IsNullOrEmpty(cardActionDataModel.TeamId) && !string.IsNullOrEmpty(cardActionDataModel.ChannelId))
                {
                    this._logger.LogInformation($"Getting team conversation from conversation entity department id = {cardActionDataModel.TeamId}");
                    //conversationEntity = await this._conversationStorageProvider.GetTeamGroupIdAsync(cardActionDataModel.TeamId).ConfigureAwait(false);

                    var cardNotificationEntity = await this._cardNotificationsStorageProvider.GetByPatientAdmissionAndChannelIdAsync(cardActionDataModel.UHID, cardActionDataModel.AdmissionId, cardActionDataModel.ChannelId).ConfigureAwait(false);


                    if (cardNotificationEntity != null)
                    {

                        resourceResponse = await this.RelyToMessageInChannelScopeUsingGraphAPIAsync(cardActionDataModel.TeamId, cardActionDataModel.ChannelId, cardActionDataModel.ReplyMessage, cardNotificationEntity.ActivityId);
                        //resourceResponse = await this.SendMessageNotificationInChannelScopeAsync(cardActionDataModel.ChannelId, cardNotificationEntity.ServiceUrl, cardActionDataModel.ReplyMessage, cardNotificationEntity.ActivityId);

                    }

                }


            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error occurred while executing ReplyToMessageInChannel.");
            }
            return isNotified;

        }
        #endregion

        public async Task<bool> RemoveAdaptiveCard(string serviceUrl, string conversationId, string activityId,string admissionId,string patientId,string teamId,string tenantId,string uhid)
        {
            bool responseModel = false;
            try
            {
                using (ConnectorClient connectorClient = CreateBotConnectionClient(serviceUrl))
                {

                    await connectorClient.Conversations.DeleteActivityAsync(conversationId, activityId, cancellationToken: default);

                    //Save Card notification
                    CardNotificationsEntity notification = new CardNotificationsEntity
                    {
                        RowKey= activityId,
                        ActivityId = activityId,
                        ChannelId = conversationId,
                        AdmissionId= admissionId,
                        ConversationId= conversationId,
                        PatientId= patientId,
                        ServiceUrl= serviceUrl,
                        TeamId=teamId,
                        TenantId=tenantId,
                        UHID=uhid,
                        CardDeleted =true
                    };

                    await this._cardNotificationsStorageProvider.AddEntityAsync(notification).ConfigureAwait(false);
                    responseModel = true;
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error in executing RemoveAdaptiveCard- conversation id -> {conversationId} : activity id : {activityId}");
                return responseModel;
            }

            return responseModel;
        }

        public async Task<bool> NotifyOnNewPatientAdmissionUpdateCard(string PatientId, string UHID, string AdmissionId, string PatientName, string Gender, string AdmissionNo, string DOA, string channelId, string teamId, string departmentId, string serviceUrl, string activityId,string TenantId)
        {
            ResourceResponse resourceResponse = null;
            Attachment cardAttachment = null;
            bool isNotified = false;
            try
            {
                this._logger.LogInformation($"Getting adaptive card details new patient department id = {departmentId}");

                cardAttachment = this._adaptiveCardService.GetUpdatePatientDetailCard_Update(PatientId, UHID, AdmissionId, PatientName, Gender, AdmissionNo, DOA, channelId, teamId, departmentId);
                this._logger.LogInformation($"Sending notification on new patient  UHID  -> {UHID} : to channel -> {channelId}");

                resourceResponse = await this.SendUpdateAdaptiveCardNotification(serviceUrl, channelId, activityId, cardAttachment);
                if (resourceResponse != null)
                {
                    //Save Card notification
                    CardNotificationsEntity notification = new CardNotificationsEntity
                    {
                        ActivityId = resourceResponse.Id,
                        ChannelId = channelId,
                        AdmissionId = AdmissionId,
                        ConversationId = channelId,
                        PatientId = PatientId,
                        ServiceUrl = serviceUrl,
                        TeamId = teamId,// conversationEntity[0].TeamAadGroupId,
                        TenantId = TenantId,
                        UHID = UHID,
                        ReplyToId = resourceResponse.Id,
                        CardUpdated=true,
                        ParentActivityId = activityId
                    };

                    await this._cardNotificationsStorageProvider.AddEntityAsync(notification).ConfigureAwait(false);

                    isNotified = true;
                }


            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error occurred while executing NotifyOnNewPatientAdmission.");
            }
            return isNotified;

        }

        private async Task<ResourceResponse> SendUpdateAdaptiveCardNotification(string serviceUrl, string conversationId, string activityId, Attachment cardToSend)
        {
            ResourceResponse responseModel = null;
            try
            {
                using (ConnectorClient connectorClient = CreateBotConnectionClient(serviceUrl))
                {
                    var activity = new Activity()
                    {
                        ReplyToId = activityId,
                        Type = ActivityTypes.Message,
                        ChannelData = new TeamsChannelData
                        {
                            Channel = new ChannelInfo(conversationId),

                        },
                        Conversation = new ConversationAccount()
                        {
                            Id = conversationId,
                            IsGroup = true,

                        },
                        Attachments = new List<Attachment>()
                                 {
                                     cardToSend
                                 }

                    };
                    responseModel= await connectorClient.Conversations.UpdateActivityAsync(conversationId, activityId, activity, cancellationToken: default);
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error in executing UpdateAdaptiveCard- conversation id -> {conversationId} : activity id : {activityId}");
                return null;
            }

            return responseModel;
        }
    }

}
