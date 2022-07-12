using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamsApp.PatientManagement.Bot.Models;
using TeamsApp.PatientManagement.Common.Models;
using TeamsApp.PatientManagement.Common.Models.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TeamsApp.PatientManagement.Common.Helper;
using TeamsApp.PatientManagement.Common.Providers;
using TeamsApp.PatientManagement.Common.Models.Entities;
using TeamsApp.PatientManagement.Common.Models.Ether;
using Microsoft.Extensions.Configuration;

namespace TeamsApp.PatientManagement.Bot.Controllers
{
    [Route("api/v1.0/common")]
    [ApiController]
    [Authorize]
    public class CommonController : BaseController
    {

        /// <summary>
        /// Logs errors and information.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Telemetry client to log event and errors.
        /// </summary>
        ///
        private readonly TelemetryClient _telemetryClient;

        private readonly IUserHelper _userHelper;
        private readonly IGroupsHelper _groupsHelper;

        private readonly IDepartmentStorageProvider _departmentStorageProvider;
        private readonly IConfiguration _configuration;

        public CommonController(
            ILogger<CommonController> logger,
            TelemetryClient telemetryClient,
            IUserHelper userHelper,
             IGroupsHelper groupsHelper,
             IDepartmentStorageProvider departmentStorageProvider,
             IConfiguration configuration
            )
            : base(telemetryClient)
        {

            this._telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._userHelper = userHelper ?? throw new ArgumentNullException(nameof(userHelper));
            this._groupsHelper = groupsHelper ?? throw new ArgumentNullException(nameof(groupsHelper));
            this._departmentStorageProvider = departmentStorageProvider ?? throw new ArgumentNullException(nameof(departmentStorageProvider));
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        [HttpGet("GetMyProfile")]
        public async Task<ActionResult> GetMyProfile()
        {
            try
            {
                var user = await this._userHelper.GetMyProfileAsync();
                return this.Ok(user);
            }
            catch (Exception ex)
            {
                this.RecordEvent("GetMyProfile - The HTTP GET call to get login user profile.", RequestType.Failed);
                this._logger.LogError(ex, "Error occurred while fetching user profile.");
                return this.Problem(ex.Message);
            }
        }

        [HttpGet("GetGroups")]
        public async Task<ActionResult> GetGroupsAsync([FromQuery] string query)
        {
            try                
            {
                if (query == null)
                {
                    query = "";
                }
                var result = await this._groupsHelper.SearchAsync(query);
                return this.Ok(result);
            }
            catch (Exception ex)
            {
                this.RecordEvent($"GetGroupsAsync - The HTTP GET call to GetGroupsAsync - {query}.", RequestType.Failed);
                this._logger.LogError(ex, $"Error occurred while fetching GetGroupsAsync - {query}.");
                return this.Problem(ex.Message);
            }
        }

        [HttpGet("GetChannels")]
        public async Task<ActionResult> GetTeamChannelsAsync([FromQuery] string teamId)
        {
            try
            {
                var result = await this._groupsHelper.GetTeamChannelsAsync(teamId);
                return this.Ok(result);
            }
            catch (Exception ex)
            {
                this.RecordEvent($"GetTeamChannelsAsync - The HTTP GET call to GetTeamChannelsAsync - {teamId}.", RequestType.Failed);
                this._logger.LogError(ex, $"Error occurred while fetching GetTeamChannelsAsync - {teamId}.");
                return this.Problem(ex.Message);
            }
        }

        [HttpGet("GetDepartments")]
        public async Task<ActionResult> GetAllDepartments()
        {
            try
            {
                var result = await this._departmentStorageProvider.GetAllAsync();
                return this.Ok(result);
            }
            catch (Exception ex)
            {
                this.RecordEvent($"GetAllDepartments - The HTTP GET call to GetAllDepartments.", RequestType.Failed);
                this._logger.LogError(ex, $"Error occurred while fetching GetAllDepartments.");
                return this.Problem(ex.Message);
            }
        }

        [HttpGet("GetDepartmentById")]
        public async Task<ActionResult> GetDepartmentById([FromQuery] string departmentId)
        {
            try
            {
                var result = await this._departmentStorageProvider.GetAsync(departmentId);
                return this.Ok(result);
            }
            catch (Exception ex)
            {
                this.RecordEvent($"GetAllDepartments - The HTTP GET call to GetAllDepartments.", RequestType.Failed);
                this._logger.LogError(ex, $"Error occurred while fetching GetAllDepartments.");
                return this.Problem(ex.Message);
            }
        }

        [HttpPost("SaveDepartment")]
        public async Task<ActionResult> SaveDepartment(DepartmentEntity data)
        {
            try
            {
                var result = await this._departmentStorageProvider.AddEntityAsync(data);

                #region Install Add to Team
                if (result)
                {
                    var appId = "";
                    try
                    {
                        appId= _configuration.GetValue<string>("App:PatientManagementAppPackageId");
                        if (appId != "")
                        {
                            await this._groupsHelper.InstallIfNotAlreadyInTeam(data.TeamId, appId);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.RecordEvent($"PatientManagementAppPackageId - The HTTP GET call to PatientManagementAppPackageId. {appId}", RequestType.Failed);
                        this._logger.LogError(ex, $"Error occurred while fetching PatientManagementAppPackageId.App ID : {appId}");
                    }
                }
                #endregion
                return this.Ok(result);
            }
            catch (Exception ex)
            {
                this.RecordEvent($"GetAllDepartments - The HTTP GET call to GetAllDepartments.", RequestType.Failed);
                this._logger.LogError(ex, $"Error occurred while fetching GetAllDepartments.");
                return this.Problem(ex.Message);
            }
        }

        

    }
}
