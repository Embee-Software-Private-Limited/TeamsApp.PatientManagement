
namespace TeamsApp.PatientManagement.Common.Models.Configuration
{

    /// <summary>
    /// A class which helps to provide Bot settings for application.
    /// </summary>
    public class BotSettings
    {
        public string MicrosoftAppId { get; set; }

        /// <summary>
        /// Gets or sets the Microsoft app password for the bot.
        /// </summary>
        public string MicrosoftAppPassword { get; set; }

        /// <summary>
        /// Gets or sets application base Uri which helps in generating customer token.
        /// </summary>
        public string AppBaseUri { get; set; }

        /// <summary>
        /// Gets or sets application manifest id.
        /// </summary>
        public string ManifestId { get; set; }

        /// <summary>
        /// Gets or sets cache duration for card payload.
        /// </summary>
        public int CardCacheDurationInHour { get; set; }

        public int PatientDetailsCacheDurationInMinutes { get; set; }
        public int VitalDetailsCacheDurationInMinutes { get; set; }
        public int LabReportsCacheDurationInMinutes { get; set; }
        public int IntakeOutputCacheDurationInMinutes { get; set; }
        public int PrescriptionCacheDurationInMinutes { get; set; }
        public int DoctorNotesCacheDurationInMinutes { get; set; }
        public int DepartmentCacheDurationInMinutes { get; set; }      
        public string APIKey { get; set; }
        public string TenantId { get; set; }

    }
}