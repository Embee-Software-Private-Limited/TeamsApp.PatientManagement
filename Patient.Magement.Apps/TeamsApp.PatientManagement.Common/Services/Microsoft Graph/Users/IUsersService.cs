
namespace TeamsApp.PatientManagement.Bot.Services.MicrosoftGraph
{
    using Microsoft.Graph;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Get the User data.
    /// </summary>
    public interface IUsersService
    {
        /// <summary>
        /// get the list of users by group of userids.
        /// </summary>
        /// <param name="userIdsByGroups">list of grouped user ids.</param>
        /// <returns>list of users.</returns>
        //Task<IEnumerable<User>> GetBatchByUserIds(IEnumerable<IEnumerable<string>> userIdsByGroups);

        /// <summary>
        /// get the stream of users.
        /// </summary>
        /// <param name="filter">the filter condition.</param>
        /// <returns>stream of users.</returns>
        Task<IEnumerable<User>> GetUsersAsync(string filter = null);

        /// <summary>
        /// get user by id.
        /// </summary>
        /// <param name="userId">the user id.</param>
        /// <returns>user data.</returns>
        Task<User> GetUserAsync(string userId);

        /// <summary>
        /// Gets all the users in the tenant. Doesn't include 'Guest' users.
        ///
        /// Note: If delta link is passed, the API returns delta changes only.
        /// </summary>
        /// <param name="deltaLink">Delta link.</param>
        /// <returns>List of users and delta link.</returns>
        Task<(IEnumerable<User>, string)> GetAllUsersAsync(string deltaLink = null);

        /// <summary>
        /// Checks if the user has teams license.
        /// </summary>
        /// <param name="userId">User's AAD id.</param>
        /// <returns>true if the user has teams license, false otherwise.</returns>
        Task<bool> HasTeamsLicenseAsync(string userId);

        /// <summary>
        /// Send mail to user.
        /// </summary>
        /// <param name="userId">User's AAD Id.</param>
        /// <param name="subject">Subject of the mail.</param>
        /// <param name="toMailAddress">Recipient Email address.</param>
        /// <param name="bodyContent">HTML body content of the mail.</param>
        /// <param name="saveToSentItems">Whether to save the email to sent items (true or false).</param>
        /// <returns>true is mail sent is successfull other wise false.</returns>
        Task<bool> SendMailToUserAsync(string userId, string subject, string toMailAddress, string bodyContent, bool saveToSentItems);
        Task<User> GetMyProfileAsync();

        Task<Dictionary<Guid, User>> GetUsersAsync(IEnumerable<string> userObjectIds);
        Task<string> GetUserProfilePhotoAsync(string userId);
        Task<string> GetMyProfilePhotoAsync();
    }
}