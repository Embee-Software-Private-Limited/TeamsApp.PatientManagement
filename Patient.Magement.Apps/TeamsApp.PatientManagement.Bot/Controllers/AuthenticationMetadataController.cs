﻿
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamsApp.PatientManagement.Common.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeamsApp.PatientManagement.Bot.Controllers
{
    /// <summary>
    /// Controller for the authentication sign in data.
    /// </summary>
    [Route("api/v1.0")]
    public class AuthenticationMetadataController : ControllerBase
    {
        /// <summary>
        /// Logs errors and information.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Represents a set of key/value application configuration properties for Azure.
        /// </summary>
        private readonly IOptions<AzureSettings> azureOptions;

        /// <summary>
        /// Represents a set of key/value application configuration properties for bot.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationMetadataController"/> class.
        /// </summary>
        /// <param name="logger">The ILogger object which logs errors and information.</param>
        /// <param name="azureOptions">A set of key/value application configuration properties for Azure.</param>
        /// <param name="botOptions">A set of key/value application configuration properties for bot.</param>
        public AuthenticationMetadataController(
            ILogger<AuthenticationMetadataController> logger, IOptions<AzureSettings> azureOptions, IOptions<BotSettings> botOptions)
        {
            this.azureOptions = azureOptions ?? throw new ArgumentNullException(nameof(azureOptions));
            this.botOptions = botOptions ?? throw new ArgumentNullException(nameof(botOptions));
            this.logger = logger;
        }

        /// <summary>
        /// Get authentication consent URL.
        /// </summary>
        /// <param name="windowLocationOriginDomain">Window location origin domain.</param>
        /// <param name="loginHint">User Principal Name value.</param>
        /// <returns>Consent URL.</returns>
        [HttpGet("consentUrl")]
        public string GetConsentUrl(
            [FromQuery] string windowLocationOriginDomain,
            [FromQuery] string loginHint)
        {
            try
            {
                var consentUrlComponentDictionary = new Dictionary<string, string>
                {
                    ["redirect_uri"] = $"https://{HttpUtility.UrlDecode(windowLocationOriginDomain)}/signin-simple-end",
                    ["client_id"] = this.azureOptions.Value.ClientId,
                    ["response_type"] = "id_token",
                    ["response_mode"] = "fragment",
                    ["scope"] = "https://graph.microsoft.com/.default",
                    ["nonce"] = Guid.NewGuid().ToString(),
                    ["state"] = Guid.NewGuid().ToString(),
                    ["login_hint"] = loginHint,
                };

                var consentUrlComponentList = consentUrlComponentDictionary
                    .Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value)}")
                    .ToList();

                var consentUrlPrefix = $"https://login.microsoftonline.com/{this.azureOptions.Value.TenantId}/oauth2/v2.0/authorize?";
                var consentUrlString = consentUrlPrefix + string.Join('&', consentUrlComponentList);

                return consentUrlString;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error occurred while generating consent URL.");
                throw;
            }
        }
    }
}
