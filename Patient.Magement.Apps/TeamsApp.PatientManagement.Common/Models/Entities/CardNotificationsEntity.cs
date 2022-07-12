
using Microsoft.Azure.Cosmos.Table;
using System;

namespace TeamsApp.PatientManagement.Common.Models.Entities
{
    public class CardNotificationsEntity: TableEntity
    {
        public const string EntityPartitionKey = "CardNotifications";

        /// <summary>
        /// Initializes a new instance of the <see cref="CardNotificationsEntity"/> class.
        /// </summary>
        public CardNotificationsEntity()
        {
            this.PartitionKey = EntityPartitionKey;
        }
        public string ActivityId
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }
       
        public string ConversationId { get; set; }
        public string ReplyToId { get; set; }
        public string ServiceUrl { get; set; }
        public string TenantId { get; set; }
        public string PatientId { get; set; }
        public string AdmissionId { get; set; }
        public string UHID { get; set; }       
        public string TeamId { get; set; }
        public string ChannelId { get; set; }
        public bool CardUpdated { get; set; }

        public string ParentActivityId { get; set; }
        public bool CardDeleted { get; set; }

    }
}
