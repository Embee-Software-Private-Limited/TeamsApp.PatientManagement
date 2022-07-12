using TeamsApp.PatientManagement.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TeamsApp.PatientManagement.Common.Helper
{
    public interface IUserHelper
    {
        Task<UserProfileModel> GetMyProfileAsync();
        Task<UserProfileModel> GetUserProfileAsync(string userId);
        Task<IEnumerable<UserProfileModel>> GetUsersAsync(string filter = null);
    }
}