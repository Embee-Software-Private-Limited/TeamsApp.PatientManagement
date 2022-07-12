// <copyright file="UsersService.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using TeamsApp.PatientManagement.Common.Models.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using System.Threading.Tasks;

namespace TeamsApp.PatientManagement.Bot.Services.MicrosoftGraph
{
    public interface IMessageService
    {
        Task<ChatMessage> SendMessageAsync(string teamId, string channelId, string activityId, string content);
    }
}