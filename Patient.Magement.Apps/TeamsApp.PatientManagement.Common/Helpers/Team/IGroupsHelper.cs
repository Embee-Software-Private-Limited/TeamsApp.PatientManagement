using TeamsApp.PatientManagement.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TeamsApp.PatientManagement.Common.Helper
{
    public interface IGroupsHelper
    {
        Task<IList<TeamChannelModel>> GetTeamChannelsAsync(string teamId);
        Task<bool> InstallIfNotAlreadyInTeam(string teamId, string teamsAppId);
        Task<IList<GroupModel>> SearchAsync(string query);
    }
}