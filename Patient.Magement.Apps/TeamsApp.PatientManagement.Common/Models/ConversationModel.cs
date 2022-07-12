using System;
namespace TeamsApp.PatientManagement.Common.Models
{
    public partial class ConversationModel
    {
        /// <summary>
        /// Gets or sets the user object Id.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets User Friendly display name of user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets email of user.
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// Gets or sets Id of conversation between user and bot.
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets Id of conversation between user and bot.
        /// </summary>
        public string ActivityId { get; set; }

        /// <summary>
        /// Gets or sets TenantId of user.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets service URL.
        /// </summary>
        public string ServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets date on which bot was installed for user.
        /// </summary>
        public DateTime BotInstalledOn { get; set; }

        /// <summary>
        /// Gets or sets RecipientId of bot.
        /// </summary>
        public string RecipientId { get; set; }

        /// <summary>
        /// Gets or sets RecipientName of bot.
        /// </summary>
        public string RecipientName { get; set; }

        /// <summary>
        /// Gets or sets UserPrincipalName of bot.
        /// </summary>
        public string UserPrincipalName { get; set; }
    }
}
