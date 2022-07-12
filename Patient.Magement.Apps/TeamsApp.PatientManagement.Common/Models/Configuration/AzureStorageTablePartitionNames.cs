using System;
using System.Collections.Generic;
using System.Text;

namespace TeamsApp.PatientManagement.Common.Models.Configuration
{
    public static class AzureStorageTablePartitionNames
    {
        public static readonly string TeamConversationPartitionKey = "TeamConversation";

        public static readonly string UserConversationPartitionKey = "UserConversation";
    }
}
