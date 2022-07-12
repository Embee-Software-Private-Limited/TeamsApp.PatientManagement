using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeamsApp.PatientManagement.Common
{
    public class AdaptiveCardActionModel
    {
        /// <summary>
        /// Gets or sets Ms Teams card action type.
        /// </summary>
        [JsonProperty("msteams")]
        public CardAction MsteamsCardAction { get; set; }

        /// <summary>
        /// Gets or sets commands from which task module is invoked.
        /// </summary>
        [JsonProperty("command")]
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets TicketId from TicketDetail.
        /// </summary>
        [JsonProperty("postedValues")]
        public string PostedValues { get; set; }

        /// <summary>
        /// Gets or sets card id.
        /// </summary>
        [JsonProperty("teamId")]
        public string TeamId { get; set; }

       
        [JsonProperty("channelId")]
        public string ChannelId { get; set; }

       
        [JsonProperty("patientId")]
        public string PatientId { get; set; }

        [JsonProperty("PatientName")]
        public string PatientName{ get; set; }

        [JsonProperty("UHID")]
        public string UHID { get; set; }

        [JsonProperty("admissionId")]
        public string AdmissionId { get; set; }

        [JsonProperty("departmentId")]
        public string DepartmentId { get; set; }
        /// <summary>
        /// Gets or sets the activity associated with this turn.
        /// </summary>
        [JsonProperty("activityId")]
        public string ActivityId { get; set; }

        [JsonProperty("replyMessage")]
        public string ReplyMessage { get; set; }
    }
}
