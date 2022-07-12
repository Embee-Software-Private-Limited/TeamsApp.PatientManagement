namespace TeamsApp.PatientManagement.Common
{
    public static class Constants
    {
        /// <summary>
        /// get the group read all scope.
        /// </summary>
        public const string ScopeGroupReadAll = "Group.Read.All";

        /// <summary>
        /// AppCatalog Read All scope.
        /// </summary>
        public const string ScopeAppCatalogReadAll = "AppCatalog.Read.All";

        /// <summary>
        /// get the user read scope.
        /// </summary>
        public const string ScopeUserRead = "User.Read";

        /// <summary>
        /// scope claim type.
        /// </summary>
        public const string ClaimTypeScp = "scp";

        /// <summary>
        /// authorization scheme.
        /// </summary>
        public const string BearerAuthorizationScheme = "Bearer";

        /// <summary>
        /// claim type user id.
        /// </summary>
        public const string ClaimTypeUserId = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        /// <summary>
        /// claim type tenant id.
        /// </summary>
        public const string ClaimTypeTenantId = "http://schemas.microsoft.com/identity/claims/tenantid";

        /// <summary>
        /// blob container name.
        /// </summary>
        public const string BlobContainerName = "exportdatablobs";

        /// <summary>
        /// get the group type Hidden Membership.
        /// </summary>
        public const string HiddenMembership = "HiddenMembership";

        /// <summary>
        /// get the header key for graph permission type.
        /// </summary>
        public const string PermissionTypeKey = "x-api-permission";

        /// <summary>
        /// get the default graph scope.
        /// </summary>
        public const string ScopeDefault = "https://graph.microsoft.com/.default";

        /// <summary>
        /// get the OData next page link.
        /// </summary>
        public const string ODataNextPageLink = "@odata.nextLink";

        public const string GraphAPIBaseUrl = "https://graph.microsoft.com";



    }
    /// <summary>
    /// constants.
    /// </summary>
    public static class CommonConstants
    {
       

       
    }
    public static class BotCommandConstants
    {
        public const string ViewPatientDetails = "VIEW-PATIENT-DETAILS";
        public const string ViewPatientDetailsPageUrl = "/patients/details";
        public const string ViewVitalsDetails = "VIEW-VITALS-DETAILS";
        public const string ViewVitalsDetailsPageUrl = "/vitals/view";
        public const string ViewIntakeOutputDetails = "VIEW-INTAKEOUTPUT-DETAILS";
        public const string ViewIntakeOutputDetailsPageUrl = "/intakeoutput/view";
        public const string ViewDoctorNotesDetails = "VIEW-DOCTORNOTES-DETAILS";
        public const string ViewDoctorNotesDetailsPageUrl = "/doctornotes/view";
        public const string ViewPrescriptionDetails = "VIEW-PRESCRIPTION-DETAILS";
        public const string ViewPrescriptionDetailsPageUrl = "/prescription/view";
        public const string ViewLabReportDetails = "VIEW-LABREPORT-DETAILS";
        public const string ViewLabReportDetailsPageUrl = "/labreport/view";
        public const string ViewMedicalHistoryDetails = "VIEW-MEDICALHISTORY-DETAILS";
        public const string ViewMedicalHistoryDetailsPageUrl = "/medicalhistory/view";
        public const string FetchActionType = "task/fetch";
    }
}
