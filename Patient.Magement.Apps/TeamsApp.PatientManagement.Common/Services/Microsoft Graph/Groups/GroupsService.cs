
namespace TeamsApp.PatientManagement.Bot.Services.MicrosoftGraph
{
    using Microsoft.Graph;
    using TeamsApp.PatientManagement.Common.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CommonConstants = TeamsApp.PatientManagement.Common;
    /// <summary>
    /// Groups Service.
    /// </summary>
    internal class GroupsService : IGroupsService
    {
        private readonly IGraphServiceClient graphServiceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupsService"/> class.
        /// </summary>
        /// <param name="graphServiceClient">graph service client.</param>
        internal GroupsService(IGraphServiceClient graphServiceClient)
        {
            this.graphServiceClient = graphServiceClient ?? throw new ArgumentNullException(nameof(graphServiceClient));
        }

        private int MaxResultCount { get; set; } = 25;

        private int MaxRetry { get; set; } = 2;

        /// <summary>
        /// get groups by ids.
        /// </summary>
        /// <param name="groupIds">list of group ids.</param>
        /// <returns>list of groups.</returns>
        public async IAsyncEnumerable<Group> GetByIdsAsync(IEnumerable<string> groupIds)
        {
            foreach (var id in groupIds)
            {
                var group = await this.graphServiceClient
                                .Groups[id]
                                .Request()
                                .WithMaxRetry(this.MaxRetry)
                                .Select(gr => new { gr.Id, gr.Mail, gr.DisplayName, gr.Visibility, })
                                .Header(Common.Constants.PermissionTypeKey, GraphPermissionType.Delegate.ToString())
                                .GetAsync();
                yield return group;
            }
        }

        /// <summary>
        /// check if list has hidden membership group.
        /// </summary>
        /// <param name="groupIds">list of group ids.</param>
        /// <returns>boolean.</returns>
        public async Task<bool> ContainsHiddenMembershipAsync(IEnumerable<string> groupIds)
        {
            var groups = this.GetByIdsAsync(groupIds);
            await foreach (var group in groups)
            {
                if (group.IsHiddenMembership())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Search M365 groups,distribution groups, security groups based on query.
        /// </summary>
        /// <param name="query">query param.</param>
        /// <returns>list of group.</returns>
        public async Task<IList<Group>> SearchAsync(string query)
        {
            query = Uri.EscapeDataString(query);
            var groupList = await this.SearchM365GroupsAsync(query, this.MaxResultCount);
           // groupList.AddRange(await this.AddDistributionGroupAsync(query, this.MaxResultCount - groupList.Count()));
           // groupList.AddRange(await this.AddSecurityGroupAsync(query, this.MaxResultCount - groupList.Count()));
            return groupList;
        }

        /// <summary>
        /// Search M365 groups, distribution groups, security groups based on query and visibilty.
        /// </summary>
        /// <param name="query">query param.</param>
        /// <param name="resultCount">page size.</param>
        /// <param name="includeHiddenMembership">boolean to filter hidden membership.</param>
        /// <returns>list of group.</returns>
        private async Task<List<Group>> SearchM365GroupsAsync(string query, int resultCount, bool includeHiddenMembership = false)
        {
            //string filterQuery = $"groupTypes/any(c:c+eq+'Unified') and mailEnabled eq true and (startsWith(mail,'{query}') or startsWith(displayName,'{query}'))";
            //string filterQuery = $"groupTypes/any(c:c+eq+'Unified') and mailEnabled eq true)";
            string filterQuery = "";
            if (!string.IsNullOrEmpty(query))
            {
                filterQuery = $"startswith(displayName, '{query}')";
            }
            var groupsPaged = await this.SearchAsync(filterQuery, resultCount);
            if (includeHiddenMembership)
            {
                return groupsPaged.CurrentPage.ToList();
            }

            var groupList = groupsPaged.CurrentPage.
                                        Where(group => !group.IsHiddenMembership()).
                                        ToList();
            while (groupsPaged.NextPageRequest != null && groupList.Count() < resultCount)
            {
                groupsPaged = await groupsPaged.NextPageRequest.GetAsync();
                groupList.AddRange(groupsPaged.CurrentPage.
                          Where(group => !group.IsHiddenMembership()));
            }

            return groupList.Take(resultCount).ToList();
        }

        

        
        private async Task<IGraphServiceGroupsCollectionPage> SearchAsync(string filterQuery, int resultCount)
        {
            return await this.graphServiceClient
                                   .Groups
                                   .Request()
                                   .WithMaxRetry(this.MaxRetry)
                                   .Filter(filterQuery)
                                   .Select(group => new
                                   {
                                       group.Id,
                                       group.Mail,
                                       group.DisplayName,
                                       group.Visibility,
                                       group.GroupTypes,
                                   })
                                   .OrderBy("displayName")
                                   .Top(resultCount)
                                   .Header(Common.Constants.PermissionTypeKey, GraphPermissionType.Application.ToString())
                                   .GetAsync();
        }



        public async Task<IList<Channel>> GetTeamChannelsAsync(string teamId)
        {
            return await this.GetTeamChannelsAsync(teamId, this.MaxResultCount);
            
        }
        private async Task<IList<Channel>> GetTeamChannelsAsync(string teamId, int resultCount)
        {
            var channelPaged =await this.graphServiceClient
                                   .Teams[teamId]
                                   .Channels
                                   .Request()
                                   .WithMaxRetry(this.MaxRetry)
                                   .Select(chn => new
                                   {
                                       chn.Id,
                                       chn.DisplayName,
                                       chn.MembershipType
                                   })
                                   //.Top(resultCount)
                                   .Header(Common.Constants.PermissionTypeKey, GraphPermissionType.Application.ToString())
                                   .GetAsync();


            var channelList = channelPaged.CurrentPage.
                                       ToList();
            while (channelPaged.NextPageRequest != null && channelList.Count < resultCount)
            {
                channelPaged = await channelPaged.NextPageRequest.GetAsync();
                channelList.AddRange(channelPaged.CurrentPage);
            }

            return channelList.Take(resultCount).ToList();
        }

        public async Task<bool> InstallIfNotAlreadyInTeam(string teamId, string teamsAppId)
        {
            bool result = false;
            try
            {

                var appInstallation = await this.graphServiceClient
                    .Teams[teamId]
                    .InstalledApps[teamsAppId]
                    .Request()
                    .Header(CommonConstants.Constants.PermissionTypeKey, GraphPermissionType.Application.ToString())
                    .GetAsync();

                if (appInstallation != null) return true;
            }

            catch(Microsoft.Graph.ServiceException ex)
            {
                if (ex.StatusCode.ToString() == "NotFound")
                {

                }
            }

            


            var teamsAppInstallation = new TeamsAppInstallation
            {
                AdditionalData = new Dictionary<string, object>()
                {
                    {"teamsApp@odata.bind", $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{teamsAppId}"}
                }
            };
            var freshInstallation = await graphServiceClient
                .Teams[teamId]
                .InstalledApps
                .Request()
                .Header(CommonConstants.Constants.PermissionTypeKey, GraphPermissionType.Application.ToString())
                .AddAsync(teamsAppInstallation);
            result = true;
            return result;

        }
    }
}
