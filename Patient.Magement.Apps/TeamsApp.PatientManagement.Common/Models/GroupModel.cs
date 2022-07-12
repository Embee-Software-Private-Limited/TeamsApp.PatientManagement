namespace TeamsApp.PatientManagement.Common.Models
{
    public class UserProfileModel
    {
        /// <summary>
        /// Gets or sets the user object Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets DisplayName name of user.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets email of user.
        /// </summary>
        public string Mail { get; set; }

        /// <summary>
        /// Gets or sets GivenName of user.
        /// </summary>
        public string GivenName { get; set; }

        /// <summary>
        /// Gets or sets UserPrincipalName of user.
        /// </summary>
        public string UserPrincipalName { get; set; }

        /// <summary>
        /// Gets or sets Photo of user.
        /// </summary>
        public string Photo { get; set; }
    }
}
