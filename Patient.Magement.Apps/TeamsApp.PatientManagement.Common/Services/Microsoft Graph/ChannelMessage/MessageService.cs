// <copyright file="UsersService.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace TeamsApp.PatientManagement.Bot.Services.MicrosoftGraph
{
    using Microsoft.Graph;
    using TeamsApp.PatientManagement.Common.Extensions;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Identity.Client;
    using TeamsApp.PatientManagement.Common.Models.Configuration;
    using Microsoft.Extensions.Options;
    using System.Net.Http.Headers;

    /// <summary>
    /// Users service.
    /// </summary>
    internal class MessageService : IMessageService
    {


        /// <summary>
        /// MS Graph batch limit is 20
        /// https://docs.microsoft.com/en-us/graph/known-issues#json-batching.
        /// </summary>
        private const int BatchSplitCount = 20;

        private readonly IGraphServiceClient graphServiceClient;
        /// <summary>
        /// Initializes a new instance of the <see cref="UsersService"/> class.
        /// </summary>
        /// <param name="graphServiceClient">graph service client.</param>
        public MessageService(IGraphServiceClient graphServiceClient)
        {
            this.graphServiceClient = graphServiceClient ?? throw new ArgumentNullException(nameof(graphServiceClient));
        }


        public async Task<ChatMessage> SendMessageAsync(string teamId, string channelId, string activityId, string content)
        {
            var chatMessage = new ChatMessage
            {
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = content
                }
            };

            var graphResult = await this.graphServiceClient
                    .Teams[teamId]
                    .Channels[channelId]
                    .Messages[activityId]
                    .Replies
                    .Request()
                    .WithMaxRetry(GraphConstants.MaxRetry)
                    .Header(Common.Constants.PermissionTypeKey, GraphPermissionType.Delegate.ToString())
                    .AddAsync(chatMessage);
            return graphResult;
            
        }

      }
}