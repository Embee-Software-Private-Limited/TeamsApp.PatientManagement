using Microsoft.Azure.Cosmos.Table;
using System;

namespace TeamsApp.PatientManagement.Common.Models.Entities
{
    public class ConversationEntity : TableEntity
    {
        
        public string ConversationId { get; set; }

        public string ActivityId { get; set; }

        public string ServiceUrl { get; set; }

        public string TenantId { get; set; }

        public DateTime BotInstalledOn { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserPrincipalName { get; set; }
        public string TeamId { get; set; }
        public string TeamAadGroupId { get; set; }
        public string TeamName { get; set; }
        public DateTime? BotRemovedOn { get; set; }
    }
}
