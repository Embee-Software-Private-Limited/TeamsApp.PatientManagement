// <copyright file="TokenHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ApplicationInsights;
using TeamsApp.PatientManagement.Common.Models;
using TeamsApp.PatientManagement.Common.Models.Configuration;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
namespace TeamsApp.PatientManagement.Common
{
    /// <summary>
    /// Helper class for JWT token generation and validation.
    /// </summary>
    public class TokenHelper : ITokenHelper
    {

        /// <summary>
        /// Used to retrieve user Active Directory access token from Bot Framework.
        /// </summary>
        private readonly OAuthClient oAuthClient;

        /// <summary>
        /// Security key for generating and validating token.
        /// </summary>
        private readonly string securityKey;

        /// <summary>
        /// Application base URL.
        /// </summary>
        private readonly string appBaseUri;

        IOptionsMonitor<BotSettings> _botSettings;
       

        /// <summary>
        /// Telemetry client to log event and errors.
        /// </summary>
        private readonly TelemetryClient telemetryClient;

      
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenHelper"/> class.
        /// </summary>
        /// <param name="remoteSupportActivityHandlerOptions">A set of key/value application configuration properties for Remote Support bot.</param>
        /// <param name="tokenOptions">A set of key/value application configuration properties for token.</param>
        public TokenHelper(
            IOptionsMonitor<BotSettings> botSettings,
            IOptionsMonitor<TokenOptions> tokenOptions,OAuthClient oAuthClient,TelemetryClient telemetryClient)
        {
            tokenOptions = tokenOptions ?? throw new ArgumentNullException(nameof(tokenOptions));
            this._botSettings = botSettings ?? throw new ArgumentNullException(nameof(botSettings));
            this.securityKey = tokenOptions.CurrentValue.SecurityKey;
            this.appBaseUri = _botSettings.CurrentValue.AppBaseUri;
            this.oAuthClient = oAuthClient;
            
            this.telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Generate JWT token used by client application to authenticate HTTP calls with API.
        /// </summary>
        /// <param name="applicationBasePath">Service URL from bot.</param>
        /// <param name="fromId">Unique Id from activity.</param>
        /// <param name="jwtExpiryMinutes">Expiry of token.</param>
        /// <returns>JWT token.</returns>
        public string GenerateAPIAuthToken(string applicationBasePath, string fromId, int jwtExpiryMinutes)
        {
            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(this.securityKey));
            SigningCredentials signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(
                    new List<Claim>()
                    {
                        new Claim("applicationBasePath", applicationBasePath),
                        new Claim("fromId", fromId),
                    }, "Custom"),
                NotBefore = DateTime.UtcNow,
                SigningCredentials = signingCredentials,
                Issuer = this.appBaseUri,
                Audience = this.appBaseUri,
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(jwtExpiryMinutes),
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(securityTokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
