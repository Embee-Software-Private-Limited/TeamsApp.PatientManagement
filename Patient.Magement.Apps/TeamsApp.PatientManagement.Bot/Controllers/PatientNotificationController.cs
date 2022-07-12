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
using TeamsApp.PatientManagement.Common.Models.Ether;
using TeamsApp.PatientManagement.Common.Providers;
using TeamsApp.PatientManagement.Common.Helper;
using System.Linq;
using TeamsApp.PatientManagement.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;

namespace TeamsApp.PatientManagement.Bot.Controllers
{
    [Route("api/v1.0/etherteamsapp")]
    [ApiController]
    [Authorize]
    public class PatientNotificationController : BaseController
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

        private readonly IOptions<EtherApiSettings> _etherAPISettings;

        private readonly IOptions<BotSettings> _botOptions;
        private readonly INotificationHelper _notificationHelper;
        private readonly IMemoryCache _memoryCache;
        public PatientNotificationController(
            ILogger<CommonController> logger,
            TelemetryClient telemetryClient,
            IOptions<EtherApiSettings> etherAPISettings,
            INotificationHelper notificationHelper,
            IMemoryCache memoryCache,
            IOptions<BotSettings> botOptions
            )
            : base(telemetryClient)
        {

            this._telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._etherAPISettings = etherAPISettings ?? throw new ArgumentNullException(nameof(etherAPISettings));
            this._notificationHelper = notificationHelper ?? throw new ArgumentNullException(nameof(notificationHelper));
            this._memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this._botOptions = botOptions ?? throw new ArgumentNullException(nameof(botOptions));
        }

        #region Create New Post in a Channel
        [HttpPost]
        [Route("CreateNewPostInChannel")]
        [AllowAnonymous]
        public async Task<IActionResult> PostMessageInChannel(EtherNewPatientPostModel dataModel)
        {
            var retValue = false;
            try
            {
                #region Check API Key
                if (dataModel.APIKey != this._botOptions.Value.APIKey)
                {
                    return this.Problem("UnAuthorized Request",statusCode:402, title:"Your request is unauthorized");
                }
                #endregion
                List<Task> tskList= new List<Task>();
                EtherPatientResponseModel patientDetails = null;

                EtherPatientAdmissionDetailsResponseModel admissionDetails = null;

                tskList.Add(Task.Run(async () => patientDetails = await this.GetPatientDetails(dataModel.UHID).ConfigureAwait(false)));
                tskList.Add(Task.Run(async () => admissionDetails = await this.GetPatientAdmissionDetailsInfo(dataModel.UHID, dataModel.AdmissionId).ConfigureAwait(false)));
                Task.WaitAll(tskList.ToArray());
                //patientDetails = await this.GetPatientDetails(dataModel.UHID).ConfigureAwait(false);

                //patientDetails = await this.GetPatientAdmissionDetails(dataModel.UHID, dataModel.UHID).ConfigureAwait(false);


                if (patientDetails != null && patientDetails.body.Any() && admissionDetails != null)
                {
                    retValue= await _notificationHelper.NotifyOnNewPatientAdmission(dataModel, patientDetails, admissionDetails);                    
                }
                return this.Ok(retValue);
            }
            catch (Exception ex)
            {
                this.RecordEvent("The HTTP Post call to CreateNewPostInChannel has failed.", RequestType.Failed);
                this._logger.LogError(ex, "Error occurred while CreateNewPostInChannel.");
                return this.Problem(ex.Message);
            }
        }
        #endregion

        #region Department
        [HttpGet("GetDepartments")]
      
        public async Task<ActionResult> GetDepartments()
        {            
            return this.Ok(await this.GetDeparmentList().ConfigureAwait(false));
        }
        private async Task<EtherDeparmentResponseModel> GetDeparmentList()
        {
            EtherDeparmentResponseModel retObject = null;
            try
            {
                var cacheKey = $"DepartmentDetails";
                //checks if cache entries exists
                if (!this._memoryCache.TryGetValue(cacheKey, out retObject))
                {
                    using (var httpClient = new HttpClient())
                    {
                        var endPointUrl = this._etherAPISettings.Value.EndPointsUrl;
                        var requestData = new
                        {
                            head = new
                            {
                                checksum = this._etherAPISettings.Value.Checksum
                            },
                            body = new
                            {
                                mid = this._etherAPISettings.Value.Mid,
                                method = "GetDepartments"
                            }
                        };
                        var json = JsonConvert.SerializeObject(requestData);
                        var data = new StringContent(json, Encoding.UTF8, "application/json");

                        using (var response = await httpClient.PostAsync(endPointUrl, data))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            retObject = JsonConvert.DeserializeObject<EtherDeparmentResponseModel>(apiResponse);

                            if (retObject != null && retObject.body.Any())
                            {
                                if (retObject.body.ToList()[0].Result == true)
                                {
                                    retObject.body = retObject.body.Skip(1);
                                }
                                else
                                {
                                    retObject.body = new List<EtherDeparmentModel>();
                                }
                            }
                        }
                    }
                    // Set cache duration
                    var cacheDurationInHour = TimeSpan.FromHours(this._botOptions.Value.DepartmentCacheDurationInMinutes);
                    cacheDurationInHour = cacheDurationInHour.Hours <= 0 ? TimeSpan.FromHours(this._botOptions.Value.DepartmentCacheDurationInMinutes) : cacheDurationInHour;
                    //setting cache entries
                    this._memoryCache.Set(cacheKey, retObject, cacheDurationInHour);

                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("GetRoster - The HTTP GET call to get department list.", RequestType.Failed);
                this._logger.LogError(ex, $"Error occurred while fetching department list.");

            }
            return retObject;
        }
        #endregion

        #region Patient Detials
        [HttpGet("GetPatient")]
        public async Task<ActionResult> GetPatient([FromQuery]string UHID)
        {
            return this.Ok(await this.GetPatientDetails(UHID).ConfigureAwait(false));
        }
        private async Task<EtherPatientResponseModel> GetPatientDetails(string UHID)
        {
            EtherPatientResponseModel retObject = null;
            try
            {
                var cacheKey = $"PatientDetails-{UHID}";
                //checks if cache entries exists
                if (!this._memoryCache.TryGetValue(cacheKey, out retObject))
                {
                    using (var httpClient = new HttpClient())
                    {
                        var endPointUrl = this._etherAPISettings.Value.EndPointsUrl;
                        var requestData = new
                        {
                            head = new
                            {
                                checksum = this._etherAPISettings.Value.Checksum
                            },
                            body = new
                            {
                                mid = this._etherAPISettings.Value.Mid,
                                method = "GetPatientDetails",
                                post_data = new
                                {
                                    UHID = UHID
                                }
                            }
                        };
                        var json = JsonConvert.SerializeObject(requestData);
                        var data = new StringContent(json, Encoding.UTF8, "application/json");

                        using (var response = await httpClient.PostAsync(endPointUrl, data))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            retObject = JsonConvert.DeserializeObject<EtherPatientResponseModel>(apiResponse);

                            if (retObject != null && retObject.body.Any())
                            {
                                if (retObject.body.ToList()[0].Result == true)
                                {
                                    retObject.body = retObject.body.ToList().Skip(1);
                                }
                                else
                                {
                                    retObject.body = new List<EtherPatientModel>();
                                }
                            }
                        }
                    }

                    // If cache duration is not specified then by default cache for 12 hours.
                    var cacheDurationInHour = TimeSpan.FromHours(this._botOptions.Value.PatientDetailsCacheDurationInMinutes);
                    cacheDurationInHour = cacheDurationInHour.Hours <= 0 ? TimeSpan.FromHours(this._botOptions.Value.PatientDetailsCacheDurationInMinutes) : cacheDurationInHour;

                    //setting cache entries
                    this._memoryCache.Set(cacheKey, retObject, cacheDurationInHour);
                }

               
            }
            catch (Exception ex)
            {
                this.RecordEvent("GetRoster - The HTTP GET call to get patient details.", RequestType.Failed);
                this._logger.LogError(ex, $"Error occurred while fetching patient details.");

            }
            return retObject;
        }
        #endregion

        #region Patient Admission Detials
        [HttpGet("GetPatientAdmissionDetails")]
        public async Task<ActionResult> GetPatientAdmissionDetails([FromQuery] string UHID, [FromQuery] string AdmissionId)
        {
            return this.Ok(await this.GetPatientAdmissionDetailsInfo(UHID, AdmissionId).ConfigureAwait(false));
        }
        private async Task<EtherPatientAdmissionDetailsResponseModel> GetPatientAdmissionDetailsInfo(string UHID, string admissionId)
        {
            EtherPatientAdmissionDetailsResponseModel retObject = null;
            try
            {
                var cacheKey = $"PatientAdmissionDetails-{admissionId}";
                //checks if cache entries exists
                if (!this._memoryCache.TryGetValue(cacheKey, out retObject))
                {
                    using (var httpClient = new HttpClient())
                    {
                        var endPointUrl = this._etherAPISettings.Value.EndPointsUrl;
                        var requestData = new
                        {
                            head = new
                            {
                                checksum = this._etherAPISettings.Value.Checksum
                            },
                            body = new
                            {
                                mid = this._etherAPISettings.Value.Mid,
                                method = "GetPatientAdmissionDetails",
                                post_data = new
                                {
                                    UHID = UHID,
                                    AdmissionId = admissionId
                                }
                            }
                        };
                        var json = JsonConvert.SerializeObject(requestData);
                        var data = new StringContent(json, Encoding.UTF8, "application/json");

                        using (var response = await httpClient.PostAsync(endPointUrl, data))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            retObject = JsonConvert.DeserializeObject<EtherPatientAdmissionDetailsResponseModel>(apiResponse);

                            if (retObject != null && retObject.body.Any())
                            {
                                if (retObject.body[0].Result == true)
                                {
                                    EtherPatientAdmissionDetailsModel item = new EtherPatientAdmissionDetailsModel();
                                    item = retObject.body.Skip(1).Take(1).FirstOrDefault();
                                    //Beddetails
                                    if (retObject.body.Count >= 3)
                                    {
                                        var bedDetailItem = retObject.body.ToList()[2];
                                        if (bedDetailItem != null)
                                        {
                                            item.BedDetails = bedDetailItem.BedDetails;
                                        }
                                    }
                                    //Beddetails
                                    if (retObject.body.Count >= 4)
                                    {
                                        var DiagnosisDetailsItem = retObject.body.ToList()[3];
                                        if (DiagnosisDetailsItem != null)
                                        {
                                            item.DiagnosisDetails = DiagnosisDetailsItem.DiagnosisDetails;
                                        }
                                    }

                                    retObject.body = new List<EtherPatientAdmissionDetailsModel>();
                                    retObject.body.Add(item);

                                }
                                else
                                {
                                    retObject.body = new List<EtherPatientAdmissionDetailsModel>();
                                }
                            }
                        }
                    }

                    // Set cache duration
                    var cacheDurationInHour = TimeSpan.FromHours(this._botOptions.Value.PatientDetailsCacheDurationInMinutes);
                    cacheDurationInHour = cacheDurationInHour.Hours <= 0 ? TimeSpan.FromHours(this._botOptions.Value.PatientDetailsCacheDurationInMinutes) : cacheDurationInHour;
                    //setting cache entries
                    this._memoryCache.Set(cacheKey, retObject, cacheDurationInHour);
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent("GetPatientAdmissionDetailsInfo - The HTTP GET call to get patient details.", RequestType.Failed);
                this._logger.LogError(ex, $"Error occurred while fetching patient details.");

            }
            return retObject;
        }
        #endregion

        #region Doctor Notes
        [HttpGet("GetDoctorNotes")]
        public async Task<ActionResult> GetDoctorNotes([FromQuery] string UHID, [FromQuery] string AdmissionId)
        {
            return this.Ok(await this.GetAllDoctorNotes(UHID, AdmissionId).ConfigureAwait(false));
        }

        [HttpPost("AddUpdateDoctorNotes")]
       
        public async Task<ActionResult> AddUpdateDoctorNotes(EtherPostDataDoctorNote data)
        {
            bool retValue = false;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var methodName = string.IsNullOrEmpty(data.DoctorNoteId) || data.DoctorNoteId=="0" ? "addDoctorNotes" : "UpdateDoctorNotes";
                    var endPointUrl = this._etherAPISettings.Value.EndPointsUrl;
                    var requestData = new
                    {
                        head = new
                        {
                            checksum = this._etherAPISettings.Value.Checksum
                        },
                        body = new
                        {
                            mid = this._etherAPISettings.Value.Mid,
                            method = methodName,
                            post_data = new
                            {
                                UHID = data.UHID,
                                AdmissionId = data.AdmissionId,
                                Name = data.Name,
                                Description = data.Description,
                                CreatedBy = data.CreatedBy,
                                CreatedByEmail = data.CreatedByEmail,
                                DoctorNoteDate = data.DoctorNoteDate.ToString("yyyy-MM-dd HH:mm:ss"),//data.DoctorNoteDate,
                                DoctorNoteId = string.IsNullOrEmpty(data.DoctorNoteId) || data.DoctorNoteId == "0" ? "" : data.DoctorNoteId
                            }
                        }
                    };
                    var json = JsonConvert.SerializeObject(requestData);
                    var datajson = new StringContent(json, Encoding.UTF8, "application/json");

                    using (var response = await httpClient.PostAsync(endPointUrl, datajson))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        var retObject = JsonConvert.DeserializeObject<EtherDoctorNotesResponseModel>(apiResponse);

                        if (retObject != null && retObject.body.Any())
                        {
                            if (retObject.body.ToList()[0].Result == true)
                            {
                                retValue = true;
                            }
                        }
                    }
                }
                if (data != null)
                {
                    AdaptiveCardActionModel obj = new AdaptiveCardActionModel();
                    obj.TeamId = data.TeamId;
                    obj.ChannelId = data.ChannelId;
                    obj.UHID = data.UHID;
                    obj.AdmissionId = data.AdmissionId;
                    var headerTitle = string.IsNullOrEmpty(data.DoctorNoteId) || data.DoctorNoteId == "0" ? "Doctor's note has been added" : "Doctor's note been updated";

                    List<string> sb = new List<string>();
                    if (data.DoctorNoteDate!=null)
                    {
                        sb.Add($"<p><b>Doctor Note Date</b> : {data.DoctorNoteDate.ToString("dd/MM/yyyy HH:mm")}</p>");
                        
                    }
                    if (!string.IsNullOrEmpty(data.Description))
                    {
                        sb.Add($"<b>Description</b> : {data.Description}");
                    }
                    
                    obj.ReplyMessage = $"{headerTitle} :{ String.Join(" ", sb)}";
                    try
                    {
                        await this._notificationHelper.ReplyToMessageInChannel(obj);
                    }
                    catch (Exception ex)
                    {
                        this.RecordEvent($"AddUpdateDoctorNotes - ReplyToMessageInChannel failed to post message.UHID -{data.UHID} AdmissionId - {data.AdmissionId}", RequestType.Failed);
                        this._logger.LogError(ex, $"Error occurred while ReplyToMessageInChannel failed to post message.UHID - {data.UHID} AdmissionId - {data.AdmissionId}");

                    }
                    var cacheKey = $"PatientDoctorNotes-{data.AdmissionId}";
                    if (this._memoryCache.Get(cacheKey) != null)
                    {
                        this._memoryCache.Remove(cacheKey);
                    }
                }
                return this.Ok(retValue);
            }
            catch (Exception ex)
            {
                this.RecordEvent($"AddUpdateDoctorNotes - The HTTP GET call to AddUpdateDoctorNotes.", RequestType.Failed);
                this._logger.LogError(ex, $"Error occurred while fetching AddUpdateDoctorNotes.");
                return this.Problem(ex.Message);
            }
        }
        private async Task<EtherDoctorNotesResponseModel> GetAllDoctorNotes(string UHID,string admissionId)
        {
            EtherDoctorNotesResponseModel retObject = null;
            try
            {
                var cacheKey = $"PatientDoctorNotes-{admissionId}";
                //checks if cache entries exists
                if (!this._memoryCache.TryGetValue(cacheKey, out retObject))
                {
                    using (var httpClient = new HttpClient())
                    {
                        var endPointUrl = this._etherAPISettings.Value.EndPointsUrl;
                        var requestData = new
                        {
                            head = new
                            {
                                checksum = this._etherAPISettings.Value.Checksum
                            },
                            body = new
                            {
                                mid = this._etherAPISettings.Value.Mid,
                                method = "GetAllDoctorNotes",
                                post_data = new
                                {
                                    UHID = UHID,
                                    AdmissionId = admissionId
                                }
                            }
                        };
                        var json = JsonConvert.SerializeObject(requestData);
                        var data = new StringContent(json, Encoding.UTF8, "application/json");
                        EtherDoctorNotesResponseModel dd = new EtherDoctorNotesResponseModel();

                        using (var response = await httpClient.PostAsync(endPointUrl, data))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();

                            retObject = JsonConvert.DeserializeObject<EtherDoctorNotesResponseModel>(apiResponse);
                            //var errModel = JsonConvert.DeserializeObject<EtherResponseError>(apiResponse);
                            if (retObject != null && retObject.body.Any())
                            {
                                if (retObject.body.ToList()[0].Result == true)
                                {
                                    for(var i=1;i< retObject.body.Count; i++)
                                    {
                                        retObject.body[i].ModifiedBy =String.IsNullOrEmpty(retObject.body[i].ModifiedBy) ? retObject.body[i].CreatedBy : retObject.body[i].ModifiedBy;
                                        retObject.body[i].ModifiedByEmail = String.IsNullOrEmpty(retObject.body[i].ModifiedByEmail) ? retObject.body[i].CreatedByEmail : retObject.body[i].ModifiedByEmail;
                                        retObject.body[i].DoctorNoteDate = retObject.body[i].DoctorNoteDate == null ? retObject.body[i].CreatedOn : retObject.body[i].DoctorNoteDate;
                                    }
                                    retObject.body = retObject.body.Skip(1).ToList();
                                }
                                else
                                {
                                    retObject.body = new List<EtherDoctorNotesModel>();
                                }
                            }
                            if (retObject != null && retObject.body != null && retObject.body.Any())
                            {
                                var groupedList = retObject.body
                                     .OrderByDescending(u => u.DoctorNoteDate)
                                    .GroupBy(u => ((DateTime)u.DoctorNoteDate).Date);
                                //.Select(grp => grp.ToList());

                                retObject.TimeLineDates = new List<DateTime>();
                                foreach (var grp in groupedList)
                                {
                                    retObject.TimeLineDates.Add(grp.Key);
                                }
                            }
                        }
                    }

                    var cacheDurationInHour = TimeSpan.FromHours(this._botOptions.Value.DoctorNotesCacheDurationInMinutes);
                    cacheDurationInHour = cacheDurationInHour.Hours <= 0 ? TimeSpan.FromHours(this._botOptions.Value.DoctorNotesCacheDurationInMinutes) : cacheDurationInHour;
                    //setting cache entries
                    this._memoryCache.Set(cacheKey, retObject, cacheDurationInHour);
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent($"GetAllDoctorNotes - The HTTP GET call to GetAllDoctorNotes. UHID:{UHID} ->  AdmissionId:{admissionId} ", RequestType.Failed);
                this._logger.LogError(ex, $"Error occurred while fetching GetAllDoctorNotes. UHID:{UHID} ->  AdmissionId:{admissionId} ");

            }
            return retObject;
        }
        #endregion

        #region Vital
        [HttpGet("GetVitals")]
        public async Task<ActionResult> GetVitals([FromQuery] string UHID, [FromQuery] string AdmissionId)
        {
            return this.Ok(await this.GetAllVitals(UHID, AdmissionId).ConfigureAwait(false));
        }
        private async Task<EtherVitalsResponseModel> GetAllVitals(string UHID, string admissionId)
        {
            EtherVitalsResponseModel retObject = null;
            try
            {
                var cacheKey = $"PatientVitals-{admissionId}";
                //checks if cache entries exists
                if (!this._memoryCache.TryGetValue(cacheKey, out retObject))
                {
                    using (var httpClient = new HttpClient())
                    {
                        var endPointUrl = this._etherAPISettings.Value.EndPointsUrl;
                        var requestData = new
                        {
                            head = new
                            {
                                checksum = this._etherAPISettings.Value.Checksum
                            },
                            body = new
                            {
                                mid = this._etherAPISettings.Value.Mid,
                                method = "GetAllVitals",
                                post_data = new
                                {
                                    UHID = UHID,
                                    AdmissionId = admissionId
                                }
                            }
                        };
                        var json = JsonConvert.SerializeObject(requestData);
                        var data = new StringContent(json, Encoding.UTF8, "application/json");

                        using (var response = await httpClient.PostAsync(endPointUrl, data))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            retObject = JsonConvert.DeserializeObject<EtherVitalsResponseModel>(apiResponse);

                            if (retObject != null && retObject.body.Any())
                            {
                                if (retObject.body.ToList()[0].Result == true)
                                {
                                    for (var i = 1; i < retObject.body.Count; i++)
                                    {
                                        retObject.body[i].ModifiedBy = String.IsNullOrEmpty(retObject.body[i].ModifiedBy) ? retObject.body[i].CreatedBy : retObject.body[i].ModifiedBy;
                                        retObject.body[i].ModifiedByEmail = String.IsNullOrEmpty(retObject.body[i].ModifiedByEmail) ? retObject.body[i].CreatedByEmail : retObject.body[i].ModifiedByEmail;
                                        retObject.body[i].ModifiedOn = retObject.body[i].ModifiedOn == null ? retObject.body[i].CreatedOn : retObject.body[i].ModifiedOn;
                                    }
                                    retObject.body = retObject.body.Skip(1).ToList();
                                }
                                else
                                {
                                    retObject.body = new List<EtherVitalsModel>();
                                }
                            }

                            if (retObject != null && retObject.body != null && retObject.body.Any())
                            {
                                var groupedList = retObject.body
                                     .OrderByDescending(u => u.ModifiedOn)
                                    .GroupBy(u => u.ModifiedOn.Date);
                                //.Select(grp => grp.ToList());

                                retObject.TimeLineDates = new List<DateTime>();
                                foreach (var grp in groupedList)
                                {
                                    retObject.TimeLineDates.Add(grp.Key);
                                }
                            }
                        }
                    }

                    var cacheDurationInHour = TimeSpan.FromHours(this._botOptions.Value.VitalDetailsCacheDurationInMinutes);
                    cacheDurationInHour = cacheDurationInHour.Hours <= 0 ? TimeSpan.FromHours(this._botOptions.Value.VitalDetailsCacheDurationInMinutes) : cacheDurationInHour;
                    //setting cache entries
                    this._memoryCache.Set(cacheKey, retObject, cacheDurationInHour);
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent($"GetAllVitals - The HTTP GET call to GetAllVitals. UHID:{UHID} ->  AdmissionId:{admissionId} ", RequestType.Failed);
                this._logger.LogError(ex, $"Error occurred while fetching GetAllVitals. UHID:{UHID} ->  AdmissionId:{admissionId} ");

            }
            return retObject;
        }

        [HttpPost("AddUpdateVitals")]
        public async Task<ActionResult> AddUpdateVitals(EtherPostDataVitals data)
        {
            bool retValue = false;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var methodName = string.IsNullOrEmpty(data.VitalDetails.VitalId) || data.VitalDetails.VitalId == "0" ? "AddVitals" : "UpdateVitals";
                    var endPointUrl = this._etherAPISettings.Value.EndPointsUrl;
                    var requestData = new
                    {
                        head = new
                        {
                            checksum = this._etherAPISettings.Value.Checksum
                        },
                        body = new
                        {
                            mid = this._etherAPISettings.Value.Mid,
                            method = methodName,
                            post_data = new
                            {
                                UHID = data.UHID,
                                AdmissionId = data.AdmissionId,
                                Name=data.Name,
                                DepartmentId= data.DepartmentId,
                                CreatedBy=data.CreatedBy,
                                CreatedByEmail = data.CreatedByEmail,
                                Temp = data.VitalDetails.Temp,
				                Weight= data.VitalDetails.Weight,
				                Height= data.VitalDetails.Height,
                                PulseRate = data.VitalDetails.PulseRate,
                                BPDiastolic = data.VitalDetails.BPDiastolic,
                                BPSystolic = data.VitalDetails.BPSystolic,
                                RR = data.VitalDetails.RR,
                                Spo2 = data.VitalDetails.Spo2,
                                description = data.VitalDetails.Description,
                                VitalId = string.IsNullOrEmpty(data.VitalDetails.VitalId) || data.VitalDetails.VitalId == "0" ? "" : data.VitalDetails.VitalId
                            }
                        }
                    };
                    
                    var json = JsonConvert.SerializeObject(requestData);
                    var datajson = new StringContent(json, Encoding.UTF8, "application/json");

                    using (var response = await httpClient.PostAsync(endPointUrl, datajson))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        var retObject = JsonConvert.DeserializeObject<EtherVitalsResponseModel>(apiResponse);

                        if (retObject != null && retObject.body.Any())
                        {
                            if (retObject.body.ToList()[0].Result == true)
                            {
                                retValue = true;
                            }
                        }
                    }
                }
                if (retValue==true && data != null)
                {
                    AdaptiveCardActionModel obj = new AdaptiveCardActionModel();
                    obj.TeamId = data.TeamId;
                    obj.ChannelId = data.ChannelId;
                    obj.UHID = data.UHID;
                    obj.AdmissionId = data.AdmissionId;
                    var headerTitle = string.IsNullOrEmpty(data.VitalDetails.VitalId) || data.VitalDetails.VitalId == "0" ? "Vitals has been added" : "Vitals has been updated";
                   
                    List<string> sb = new List<string>();
                    if (!string.IsNullOrEmpty(data.VitalDetails.Temp)){
                        sb.Add($"<b>Temp</b> : {data.VitalDetails.Temp}");
                    }
                    if (!string.IsNullOrEmpty(data.VitalDetails.Weight))
                    {
                        sb.Add($"<br/><b>Weight</b> : {data.VitalDetails.Weight}");
                    }
                    if (!string.IsNullOrEmpty(data.VitalDetails.Height))
                    {

                        sb.Add($"<br/><b>Height</b> : {data.VitalDetails.Height}");
                    }
                    if (!string.IsNullOrEmpty(data.VitalDetails.PulseRate))
                    {
                        sb.Add($"<br/><b>PulseRate</b> : {data.VitalDetails.PulseRate}");
                    }
                    if (!string.IsNullOrEmpty(data.VitalDetails.BPSystolic))
                    {
                        sb.Add($"<br/><b>BPSystolic</b> : {data.VitalDetails.BPSystolic}");
                    }
                    if (!string.IsNullOrEmpty(data.VitalDetails.BPDiastolic))
                    {
                        sb.Add($"<br/><b>BPDiastolic</b> : {data.VitalDetails.BPDiastolic}");
                    }                   
                    if (!string.IsNullOrEmpty(data.VitalDetails.RR))
                    {
                        sb.Add($"<br/><b>RR</b> : {data.VitalDetails.RR}");
                    }
                    if (!string.IsNullOrEmpty(data.VitalDetails.Spo2))
                    {
                        sb.Add($"<br/><b>Spo2</b> : {data.VitalDetails.Spo2}");
                    }
                    if (!string.IsNullOrEmpty(data.VitalDetails.Description))
                    {
                        sb.Add($"<br/><b>Description</b> : {data.VitalDetails.Description}");
                    }
                   
                    obj.ReplyMessage = $"{headerTitle} :<p>{ String.Join(" ", sb)}</p>";
                    try
                    {
                        await this._notificationHelper.ReplyToMessageInChannel(obj);
                    }
                    catch(Exception ex)
                    {
                        this.RecordEvent($"AddUpdateVitals - ReplyToMessageInChannel failed to post message.UHID -{data.UHID} AdmissionId - {data.AdmissionId}", RequestType.Failed);
                        this._logger.LogError(ex, $"Error occurred while ReplyToMessageInChannel failed to post message.UHID - {data.UHID} AdmissionId - {data.AdmissionId}");

                    }

                    var cacheKey = $"PatientVitals-{data.AdmissionId}";
                    if (this._memoryCache.Get(cacheKey)!=null)
                    {
                        this._memoryCache.Remove(cacheKey);
                    }
                }
                return this.Ok(retValue);
            }
            catch (Exception ex)
            {
                this.RecordEvent($"AddUpdateVitals - The HTTP GET call to AddUpdateVitals. AdmissionId - {data.AdmissionId}", RequestType.Failed);
                this._logger.LogError(ex, $"Error occurred while fetching AddUpdateVitals. AdmissionId - {data.AdmissionId}");
                return this.Problem(ex.Message);
            }
        }

        #endregion 

        #region IntakeOutput
        [HttpGet("GetIntakeAndOutput")]
        public async Task<ActionResult> GetIntakeAndOutput([FromQuery] string UHID, [FromQuery] string AdmissionId)
        {
            return this.Ok(await this.GetAllIntakeAndOutput(UHID, AdmissionId).ConfigureAwait(false));
        }
        private async Task<EtherIntakeOutputResponseModel> GetAllIntakeAndOutput(string UHID, string admissionId)
        {
            EtherIntakeOutputResponseModel retObject = null;
            try
            {
                var cacheKey = $"PatientIntakeAndOutput-{admissionId}";
                //checks if cache entries exists
                if (!this._memoryCache.TryGetValue(cacheKey, out retObject))
                {
                    using (var httpClient = new HttpClient())
                    {
                        var endPointUrl = this._etherAPISettings.Value.EndPointsUrl;
                        var requestData = new
                        {
                            head = new
                            {
                                checksum = this._etherAPISettings.Value.Checksum
                            },
                            body = new
                            {
                                mid = this._etherAPISettings.Value.Mid,
                                method = "GetIntakeAndOutput",
                                post_data = new
                                {
                                    UHID = UHID,
                                    AdmissionId = admissionId
                                }
                            }
                        };
                        var json = JsonConvert.SerializeObject(requestData);
                        var data = new StringContent(json, Encoding.UTF8, "application/json");

                        using (var response = await httpClient.PostAsync(endPointUrl, data))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            retObject = JsonConvert.DeserializeObject<EtherIntakeOutputResponseModel>(apiResponse);

                            if (retObject != null && retObject.body.Any())
                            {
                                if (retObject.body.ToList()[0].Result == true)
                                {

                                    for (var i = 1; i < retObject.body.Count; i++)
                                    {
                                        retObject.body[i].ModifiedBy = String.IsNullOrEmpty(retObject.body[i].ModifiedBy) ? retObject.body[i].CreatedBy : retObject.body[i].ModifiedBy;
                                        retObject.body[i].ModifiedByEmail = String.IsNullOrEmpty(retObject.body[i].ModifiedByEmail) ? retObject.body[i].CreatedByEmail : retObject.body[i].ModifiedByEmail;
                                        retObject.body[i].ModifiedOn = retObject.body[i].ModifiedOn == null ? retObject.body[i].CreatedOn : retObject.body[i].ModifiedOn;
                                        retObject.body[i].DateAdded = retObject.body[i].DateAdded == null ? retObject.body[i].CreatedOn : retObject.body[i].DateAdded;
                                    }
                                    retObject.body = retObject.body.Skip(1).ToList();
                                }
                                else
                                {
                                    retObject.body = new List<EtherIntakeOutputModel>();
                                }
                            }

                            if (retObject != null && retObject.body != null && retObject.body.Any())
                            {
                                var groupedList = retObject.body
                                     .OrderByDescending(u => u.DateAdded)
                                    .GroupBy(u => ((DateTime)u.DateAdded).Date);
                                //.Select(grp => grp.ToList());

                                retObject.TimeLineDates = new List<DateTime>();
                                foreach (var grp in groupedList)
                                {
                                    retObject.TimeLineDates.Add(grp.Key);
                                }
                            }
                        }
                    }
                    var cacheDurationInHour = TimeSpan.FromHours(this._botOptions.Value.IntakeOutputCacheDurationInMinutes);
                    cacheDurationInHour = cacheDurationInHour.Hours <= 0 ? TimeSpan.FromHours(this._botOptions.Value.IntakeOutputCacheDurationInMinutes) : cacheDurationInHour;
                    //setting cache entries
                    this._memoryCache.Set(cacheKey, retObject, cacheDurationInHour);
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent($"GetAllIntakeAndOutput - The HTTP GET call to GetAllIntakeAndOutput. UHID:{UHID} ->  AdmissionId:{admissionId} ", RequestType.Failed);
                this._logger.LogError(ex, $"Error occurred while fetching GetAllIntakeAndOutput. UHID:{UHID} ->  AdmissionId:{admissionId} ");

            }
            return retObject;
        }

        [HttpPost("AddUpdateIntakeAndOutput")]
        public async Task<ActionResult> AddUpdateIntakeOutput(EtherPostDataIntakeOut data)
        {
            bool retValue = false;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var methodName = string.IsNullOrEmpty(data.IntakeOutputDetails.Id) || data.IntakeOutputDetails.Id == "0" ? "AddIntakeAndOutput" : "UpdateIntakeAndOutput";
                    var endPointUrl = this._etherAPISettings.Value.EndPointsUrl;
                    var requestData = new
                    {
                        head = new
                        {
                            checksum = this._etherAPISettings.Value.Checksum
                        },
                        body = new
                        {
                            mid = this._etherAPISettings.Value.Mid,
                            method = methodName,
                            post_data = new
                            {
                                UHID = data.UHID,
                                AdmissionId = data.AdmissionId,
                               // Name = data.Name,
                               // DepartmentId = data.DepartmentId,
                                CreatedBy = data.CreatedBy,
                                CreatedByEmail = data.CreatedByEmail,
                                //OutputType = data.IntakeOutputDetails.OutputType,
                                fluid_in0 = data.IntakeOutputDetails.Fluid_in0,
                                fluid_out0 = data.IntakeOutputDetails.Fluid_out0,
                                fluid_bal = data.IntakeOutputDetails.Fluid_bal,
                                Description = data.IntakeOutputDetails.Description,
                                DateAdded = data.IntakeOutputDetails.DateAdded.ToString("yyyy-MM-dd HH:mm:ss"),//data.IntakeOutputDetails.DateAdded,
                                IntakeId = string.IsNullOrEmpty(data.IntakeOutputDetails.Id) || data.IntakeOutputDetails.Id == "0" ? "" : data.IntakeOutputDetails.Id
                            }
                        }
                    };

                    var json = JsonConvert.SerializeObject(requestData);
                    var datajson = new StringContent(json, Encoding.UTF8, "application/json");

                    using (var response = await httpClient.PostAsync(endPointUrl, datajson))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        var retObject = JsonConvert.DeserializeObject<EtherIntakeOutputResponseModel>(apiResponse);

                        if (retObject != null && retObject.body.Any())
                        {
                            if (retObject.body.ToList()[0].Result == true)
                            {
                                retValue = true;
                            }
                        }
                    }
                }
                if (retValue == true && data != null)
                {
                    AdaptiveCardActionModel obj = new AdaptiveCardActionModel();
                    obj.TeamId = data.TeamId;
                    obj.ChannelId = data.ChannelId;
                    obj.UHID = data.UHID;
                    obj.AdmissionId = data.AdmissionId;
                    var headerTitle = string.IsNullOrEmpty(data.IntakeOutputDetails.Id) || data.IntakeOutputDetails.Id == "0" ? "Intake & Output has been added" : "Intake & Output has been updated";

                    List<string> sb = new List<string>();
                    if (!string.IsNullOrEmpty(data.IntakeOutputDetails.Fluid_in0))
                    {
                        sb.Add($"Intake : {data.IntakeOutputDetails.Fluid_in0}");
                    }
                    if (!string.IsNullOrEmpty(data.IntakeOutputDetails.Fluid_out0))
                    {
                        sb.Add($"Output : {data.IntakeOutputDetails.Fluid_out0}");
                    }
                    if (!string.IsNullOrEmpty(data.IntakeOutputDetails.Fluid_bal))
                    {

                        sb.Add($"Balance : {data.IntakeOutputDetails.Fluid_bal}");
                    }
                    if (!string.IsNullOrEmpty(data.IntakeOutputDetails.Description))
                    {
                        sb.Add($"Description : {data.IntakeOutputDetails.Description}");
                    }
                    if (data.IntakeOutputDetails.DateAdded != null)
                    {
                        sb.Add($"<p><b>Date</b> : {data.IntakeOutputDetails.DateAdded.ToString("dd/MM/yyyy HH:mm")}</p>");
                    }
                    obj.ReplyMessage = $"{headerTitle} :<p>{ String.Join(", ", sb)}</p>";
                    try
                    {
                        await this._notificationHelper.ReplyToMessageInChannel(obj);
                    }
                    catch (Exception ex)
                    {
                        this.RecordEvent($"AddUpdateIntakeOutput - ReplyToMessageInChannel failed to post message.UHID -{data.UHID} AdmissionId - {data.AdmissionId}", RequestType.Failed);
                        this._logger.LogError(ex, $"Error occurred while ReplyToMessageInChannel failed to post message.UHID - {data.UHID} AdmissionId - {data.AdmissionId}");

                    }
                    var cacheKey = $"PatientIntakeAndOutput-{data.AdmissionId}";
                    if (this._memoryCache.Get(cacheKey) != null)
                    {
                        this._memoryCache.Remove(cacheKey);
                    }
                }
                return this.Ok(retValue);
            }
            catch (Exception ex)
            {
                this.RecordEvent($"AddUpdateVitals - The HTTP GET call to AddUpdateIntakeOutput. AdmissionId - {data.AdmissionId}", RequestType.Failed);
                this._logger.LogError(ex, $"Error occurred while fetching AddUpdateIntakeOutput. AdmissionId - {data.AdmissionId}");
                return this.Problem(ex.Message);
            }
        }
        #endregion

        #region Prescription
        [HttpGet("GetPrescriptions")]
        public async Task<ActionResult> GetPrescriptions([FromQuery] string UHID, [FromQuery] string AdmissionId)
        {
            return this.Ok(await this.GetAllPrescription(UHID, AdmissionId).ConfigureAwait(false));
        }
        private async Task<EtherPrescriptionResponseModel> GetAllPrescription(string UHID, string admissionId)
        {
            EtherPrescriptionResponseModel retObject = null;
            try
                {
                var cacheKey = $"PatientPrescription-{admissionId}";
                //checks if cache entries exists
                if (!this._memoryCache.TryGetValue(cacheKey, out retObject))
                {
                    using (var httpClient = new HttpClient())
                {
                    var endPointUrl = this._etherAPISettings.Value.EndPointsUrl;
                    var requestData = new
                    {
                        head = new
                        {
                            checksum = this._etherAPISettings.Value.Checksum
                        },
                        body = new
                        {
                            mid = this._etherAPISettings.Value.Mid,
                            method = "GetAllPrescription",
                            post_data = new
                            {
                                UHID = UHID,
                                AdmissionId = admissionId
                            }
                        }
                    };
                    var json = JsonConvert.SerializeObject(requestData);
                    var data = new StringContent(json, Encoding.UTF8, "application/json");

                    using (var response = await httpClient.PostAsync(endPointUrl, data))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        retObject= JsonConvert.DeserializeObject<EtherPrescriptionResponseModel>(apiResponse);
                        if (retObject != null && retObject.body!=null)
                        {
                            var bodyList = new List<EtherPrescriptionModelDetails>();
                            if (retObject.body._0.Result == true && retObject.body.pdetails.Any())
                            {
                                List<string> description = new List<string>();
                                EtherPrescriptionModelDetails item;
                                retObject.TimeLineDates = new List<string>();
                                for (var i = 0; i < retObject.body.pdetails.Count; i++)
                                {
                                    item = retObject.body.pdetails[i];
                                    item.PrescriptionId = Guid.NewGuid().ToString();
                                    retObject.TimeLineDates.Add(item.AddedOn);

                                    description = item.medicine_details;
                                    bodyList.Add(item);
                                }
                                retObject.bodyList = bodyList;
                            }
                            else
                            {
                                retObject.bodyList = new List<EtherPrescriptionModelDetails>();
                            }
                        }

                    }

                }

                    var cacheDurationInHour = TimeSpan.FromHours(this._botOptions.Value.PrescriptionCacheDurationInMinutes);
                    cacheDurationInHour = cacheDurationInHour.Hours <= 0 ? TimeSpan.FromHours(this._botOptions.Value.DoctorNotesCacheDurationInMinutes) : cacheDurationInHour;
                    //setting cache entries
                    this._memoryCache.Set(cacheKey, retObject, cacheDurationInHour);
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent($"GetAllDoctorNotes - The HTTP GET call to GetAllPrescription. UHID:{UHID} ->  AdmissionId:{admissionId} ", RequestType.Failed);
                this._logger.LogError(ex, $"Error occurred while fetching GetAllPrescription. UHID:{UHID} ->  AdmissionId:{admissionId} ");

            }
            return retObject;
        }
        #endregion

        #region LabReports
        [HttpGet("GetLabReports")]
        public async Task<ActionResult> GetLabReports([FromQuery] string UHID, [FromQuery] string AdmissionId)
        {
            return this.Ok(await this.GetAllLabReports(UHID, AdmissionId).ConfigureAwait(false));
        }
        private async Task<EtherLabReportsResponseModel> GetAllLabReports(string UHID, string admissionId)
        {
            EtherLabReportsResponseModel retObject = null;
            try
            {                
                var cacheKey = $"PatientLabReports-{admissionId}";
                //checks if cache entries exists
                if (!this._memoryCache.TryGetValue(cacheKey, out retObject))
                {
                    using (var httpClient = new HttpClient())
                    {
                        var endPointUrl = this._etherAPISettings.Value.EndPointsUrl;
                        var requestData = new
                        {
                            head = new
                            {
                                checksum = this._etherAPISettings.Value.Checksum
                            },
                            body = new
                            {
                                mid = this._etherAPISettings.Value.Mid,
                                method = "GetAllLabReports",
                                post_data = new
                                {
                                    UHID = UHID,
                                    AdmissionId = admissionId
                                }
                            }
                        };
                        var json = JsonConvert.SerializeObject(requestData);
                        var data = new StringContent(json, Encoding.UTF8, "application/json");

                        using (var response = await httpClient.PostAsync(endPointUrl, data))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            retObject = JsonConvert.DeserializeObject<EtherLabReportsResponseModel>(apiResponse);

                            if (retObject != null && retObject.body.Any())
                            {
                                if (retObject.body.ToList()[0].Result == true)
                                {
                                    retObject.body = retObject.body.ToList().Skip(1);
                                }
                                else
                                {
                                    retObject.body = new List<EtherLabReportsModel>();
                                }
                            }

                            if (retObject != null && retObject.body != null && retObject.body.Any())
                            {
                                var groupedList = retObject.body
                                     .OrderByDescending(u => u.ModifiedOn)
                                    .GroupBy(u => u.ModifiedOn.Date);
                                //.Select(grp => grp.ToList());

                                retObject.TimeLineDates = new List<DateTime>();
                                foreach (var grp in groupedList)
                                {
                                    retObject.TimeLineDates.Add(grp.Key);
                                }
                            }
                        }
                    }
                    var cacheDurationInHour = TimeSpan.FromHours(this._botOptions.Value.LabReportsCacheDurationInMinutes);
                    cacheDurationInHour = cacheDurationInHour.Hours <= 0 ? TimeSpan.FromHours(this._botOptions.Value.LabReportsCacheDurationInMinutes) : cacheDurationInHour;
                    //setting cache entries
                    this._memoryCache.Set(cacheKey, retObject, cacheDurationInHour);
                }
            }
            catch (Exception ex)
            {
                this.RecordEvent($"GetAllLabReports - The HTTP GET call to GetAllLabReports. UHID:{UHID} ->  AdmissionId:{admissionId} ", RequestType.Failed);
                this._logger.LogError(ex, $"Error occurred while fetching GetAllLabReports. UHID:{UHID} ->  AdmissionId:{admissionId} ");

            }
            return retObject;
        }
        #endregion

        #region MedicalHistory
        [HttpGet("GetMedicalHistory")]
        public async Task<ActionResult> GetMedicalHistory([FromQuery] string UHID, [FromQuery] string AdmissionId)
        {
            return this.Ok(await this.GetAllMedicalHistory(UHID, AdmissionId).ConfigureAwait(false));
        }
       private async Task<EtherMedicalHistoryResponseModel> GetAllMedicalHistory(string UHID, string admissionId)
        {
            EtherMedicalHistoryResponseModel retObject = null;
            try
            {
                var cacheKey = $"PatientMedicalHistory-{admissionId}";
                //checks if cache entries exists
                //if (!this._memoryCache.TryGetValue(cacheKey, out retObject))
                //{
                    using (var httpClient = new HttpClient())
                    {
                        var endPointUrl = this._etherAPISettings.Value.EndPointsUrl;
                        var requestData = new
                        {
                            head = new
                            {
                                checksum = this._etherAPISettings.Value.Checksum
                            },
                            body = new
                            {
                                mid = this._etherAPISettings.Value.Mid,
                                method = "GetAllMedicalHistory",
                                post_data = new
                                {
                                    UHID = UHID,
                                    AdmissionId = admissionId
                                }
                            }
                        };
                        var json = JsonConvert.SerializeObject(requestData);
                        var data = new StringContent(json, Encoding.UTF8, "application/json");
                       
                        using (var response = await httpClient.PostAsync(endPointUrl, data))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            retObject = JsonConvert.DeserializeObject<EtherMedicalHistoryResponseModel>(apiResponse);
                            if (retObject != null && retObject.body.Any())
                            {
                                if (retObject.body.ToList()[0].Result == true)
                                {
                                    for (var i = 1; i < retObject.body.Count; i++)
                                    {
                                        retObject.body[i].ModifiedBy = String.IsNullOrEmpty(retObject.body[i].ModifiedBy) ? retObject.body[i].CreatedBy : retObject.body[i].ModifiedBy;
                                        retObject.body[i].ModifiedByEmail = String.IsNullOrEmpty(retObject.body[i].ModifiedByEmail) ? retObject.body[i].CreatedByEmail : retObject.body[i].ModifiedByEmail;
                                        retObject.body[i].ModifiedOn = retObject.body[i].ModifiedOn == null ? retObject.body[i].CreatedOn : retObject.body[i].ModifiedOn;
                                    }
                                    retObject.body = retObject.body.Skip(1).ToList();
                                }
                                else
                                {
                                    retObject.body = new List<EtherMedicalHistoryModel>();
                                }
                            }
                            if (retObject != null && retObject.body != null && retObject.body.Any())
                            {
                                var groupedList = retObject.body
                                     .OrderByDescending(u => u.ModifiedOn)
                                    .GroupBy(u => u.ModifiedOn.Date);
                                //.Select(grp => grp.ToList());

                                retObject.TimeLineDates = new List<DateTime>();
                                foreach (var grp in groupedList)
                                {
                                    retObject.TimeLineDates.Add(grp.Key);
                                }
                            }
                        }
                    }

                    //var cacheDurationInHour = TimeSpan.FromHours(this._botOptions.Value.DoctorNotesCacheDurationInMinutes);
                    //cacheDurationInHour = cacheDurationInHour.Hours <= 0 ? TimeSpan.FromHours(this._botOptions.Value.DoctorNotesCacheDurationInMinutes) : cacheDurationInHour;
                    ////setting cache entries
                    //this._memoryCache.Set(cacheKey, retObject, cacheDurationInHour);
                //}
            }
            catch (Exception ex)
            {
                this.RecordEvent($"GetAllDoctorNotes - The HTTP GET call to GetAllMedicalHistory. UHID:{UHID} ->  AdmissionId:{admissionId} ", RequestType.Failed);
                this._logger.LogError(ex, $"Error occurred while fetching GetAllMedicalHistory. UHID:{UHID} ->  AdmissionId:{admissionId} ");

            }
            return retObject;
        }
        #endregion


        //#region Adaptive Card
        //[HttpPost]
        //[Route("RemoveAdaptiveCard")]
        //[AllowAnonymous]
        //public async Task<IActionResult> RemoveAdaptiveCard([FromBody] UpdatePatientCardModel data)
        //{
        //    bool retValue = false;
        //    try
        //    {

        //        retValue = await _notificationHelper.RemoveAdaptiveCard(data.ServiceUrl, data.channelId, data.ActivityId,data.AdmissionId,data.PatientId,data.teamId,data.TenantId,data.UHID);

        //        return this.Ok(retValue);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.RecordEvent("The HTTP Post call to RemoveAdaptiveCard has failed.", RequestType.Failed);
        //        this._logger.LogError(ex, "Error occurred while RemoveAdaptiveCard.");
        //        return this.Problem(ex.Message);
        //    }
        //}
        //#endregion

        //public class UpdatePatientCardModel
        //{
        //    public string PatientId { get; set; }
        //    public string UHID { get; set; }
        //    public string AdmissionId { get; set; }
        //    public string Gender { get; set; }
        //    public string AdmissionNo { get; set; }
        //    public string PatientName { get; set; }
        //    public string DOA { get; set; }
        //    public string channelId { get; set; }
        //    public string teamId { get; set; }
        //    public string departmentId { get; set; }
        //    public string ServiceUrl { get; set; }
        //    public string ActivityId { get; set; }
        //    public string TenantId { get; set; }
        //}
        //#region Adaptive Card
        //[HttpPost]
        //[Route("UpdateAdaptiveCard")]
        //[AllowAnonymous]
        //public async Task<IActionResult> UpdateAdaptiveCard(
        //    [FromBody] UpdatePatientCardModel data
        //    )
        //{
        //    bool retValue = false;
        //    try
        //    {
        //        retValue = await _notificationHelper.NotifyOnNewPatientAdmissionUpdateCard(data.PatientId, 
        //            data.UHID, 
        //            data.AdmissionId,
        //            data.PatientName,
        //            data.Gender,
        //            data.AdmissionNo,
        //            data.DOA,
        //            data.channelId,
        //            data.teamId,
        //            data.departmentId,
        //            data.ServiceUrl,
        //            data.ActivityId,
        //            data.TenantId);

        //        return this.Ok(retValue);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.RecordEvent("The HTTP Post call to UpdateAdaptiveCard has failed.", RequestType.Failed);
        //        this._logger.LogError(ex, "Error occurred while UpdateAdaptiveCard.");
        //        return this.Problem(ex.Message);
        //    }
        //}
        //#endregion

    }
}