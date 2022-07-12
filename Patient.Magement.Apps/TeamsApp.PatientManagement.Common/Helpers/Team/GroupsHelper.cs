
namespace TeamsApp.PatientManagement.Common.Helper
{
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using TeamsApp.PatientManagement.Bot.Services.MicrosoftGraph;
    using TeamsApp.PatientManagement.Common.Models;
    using TeamsApp.PatientManagement.Common.Models.Configuration;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides helper methods for fetching reportees.
    /// </summary>
    public class GroupsHelper : IGroupsHelper
    {
        /// <summary>
        /// Holds the instance of Graph service to access logged in user's reportees and manager.
        /// </summary>
        private readonly IGroupsService groupGraphService;

        /// <summary>
        /// Instance of memory cache to cache reportees for managers.
        /// </summary>
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// A set of key/value application configuration properties.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserHelper"/> class.
        /// </summary>
        /// <param name="userGraphService">The instance of user Graph service to access logged in user's reportees and manager.</param>
        /// <param name="memoryCache">Instance of memory cache to cache reportees for managers.</param>
        /// <param name="botOptions">A set of key/value application configuration properties.</param>
        public GroupsHelper(IGroupsService groupGraphService, IMemoryCache memoryCache, IOptions<BotSettings> botOptions)
        {
            this.botOptions = botOptions;
            this.groupGraphService = groupGraphService;
            this.memoryCache = memoryCache;
        }
        public async Task<IList<GroupModel>> SearchAsync(string query)
        {
            IList<GroupModel> grpList = new List<GroupModel>();
            var result = await this.groupGraphService.SearchAsync(query);

            if (result != null && result.Any())
            {
                foreach (var grp in result)
                {
                    grpList.Add(new GroupModel
                    {
                        Id = grp.Id,
                        DisplayName = grp.DisplayName,
                        Mail = grp.Mail
                    });
                }
            }
            return grpList;
        }

        public async Task<IList<TeamChannelModel>> GetTeamChannelsAsync(string teamId)
        {
            IList<TeamChannelModel> lst = new List<TeamChannelModel>();
            var result = await this.groupGraphService.GetTeamChannelsAsync(teamId);

            if (result != null && result.Any())
            {
                foreach (var channel in result)
                {
                    lst.Add(new TeamChannelModel
                    {
                        Id = channel.Id,
                        DisplayName = channel.DisplayName,
                        MembershipType = channel.MembershipType?.ToString(),
                        TeamGroupId = teamId
                    });
                }
            }
            return lst;
        }

        public async Task<bool> InstallIfNotAlreadyInTeam(string teamId, string teamsAppId)
        {
          return await this.groupGraphService.InstallIfNotAlreadyInTeam(teamId, teamsAppId);
        }
    }
}