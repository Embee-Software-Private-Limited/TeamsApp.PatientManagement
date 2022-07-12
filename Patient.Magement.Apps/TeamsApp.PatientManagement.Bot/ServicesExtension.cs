
namespace TeamsApp.PatientManagement.Bot
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Identity.Client;
    using Microsoft.Identity.Web;
    using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
    using Microsoft.IdentityModel.Tokens;
    using TeamsApp.PatientManagement.Bot.Authentication;
    using TeamsApp.PatientManagement.Bot.Bots;
    using TeamsApp.PatientManagement.Common.Helper;
    using TeamsApp.PatientManagement.Bot.Services.MicrosoftGraph;
    using TeamsApp.PatientManagement.Common.Extensions;
    using TeamsApp.PatientManagement.Common.Models.Configuration;
    using TeamsApp.PatientManagement.Common.Providers;
    using TeamsApp.PatientManagement.Common.Services.AdaptiveCard;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using TeamsApp.PatientManagement.Common;
    using TeamsApp.PatientManagement.Common.Models;
    using Microsoft.Bot.Connector;

    /// <summary>
    /// Class to extend ServiceCollection.
    /// </summary>
    public static class ServicesExtension
    {
        /// <summary>
        /// Adds application configuration settings to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void RegisterConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BotSettings>(options =>
            {
                options.AppBaseUri = configuration.GetValue<string>("App:AppBaseUri");
                options.APIKey = configuration.GetValue<string>("App:APIKey");
                options.ManifestId = configuration.GetValue<string>("App:ManifestId");
                options.MicrosoftAppId = configuration.GetValue<string>("MicrosoftAppId");
                options.MicrosoftAppPassword = configuration.GetValue<string>("MicrosoftAppPassword");
                options.CardCacheDurationInHour = configuration.GetValue<int>("App:CardCacheDurationInHour");
                options.PatientDetailsCacheDurationInMinutes = configuration.GetValue<int>("App:PatientDetailsCacheDurationInMinutes");
                options.VitalDetailsCacheDurationInMinutes = configuration.GetValue<int>("App:VitalDetailsCacheDurationInMinutes");
                options.LabReportsCacheDurationInMinutes = configuration.GetValue<int>("App:LabReportsCacheDurationInMinutes");
                options.IntakeOutputCacheDurationInMinutes = configuration.GetValue<int>("App:IntakeOutputCacheDurationInMinutes");
                options.PrescriptionCacheDurationInMinutes = configuration.GetValue<int>("App:PrescriptionCacheDurationInMinutes");
                options.DoctorNotesCacheDurationInMinutes = configuration.GetValue<int>("App:DoctorNotesCacheDurationInMinutes");
                options.DepartmentCacheDurationInMinutes = configuration.GetValue<int>("App:DepartmentCacheDurationInMinutes");               
                options.TenantId = configuration.GetValue<string>("MicrosoftAppTenantId");
            });

            services.Configure<AzureSettings>(options =>
            {
                options.TenantId = configuration.GetValue<string>("AzureAd:TenantId");
                options.ClientId = configuration.GetValue<string>("AzureAd:ClientId");
                options.ApplicationIdURI = configuration.GetValue<string>("AzureAd:ApplicationIdURI");
                options.ValidIssuers = configuration.GetValue<string>("AzureAd:ValidIssuers");
                options.Instance = configuration.GetValue<string>("AzureAd:Instance");
                options.GraphScope = configuration.GetValue<string>("AzureAd:GraphScope");
            });

            services.Configure<AzureStorageSettings>(settings =>            {
                settings.StorageConnectionString = configuration.GetValue<string>("StorageConnectionString");
            });
            services.Configure<EtherApiSettings>(settings => {
                settings.EndPointsUrl = configuration.GetValue<string>("EtherApiSettings:EndPointsUrl");
                settings.Checksum = configuration.GetValue<string>("EtherApiSettings:Checksum");
                settings.Mid = configuration.GetValue<string>("EtherApiSettings:Mid");
            });
            services.Configure<TokenOptions>(options =>
            {
                options.SecurityKey = configuration.GetValue<string>("Token:SecurityKey");
            });

        }
        public static void RegisterQnAMakerSettings(this IServiceCollection services, IConfiguration configuration)
        {
            
            services.AddSingleton<IConversationStorageProvider>((provider) => new ConversationStorageProvider(provider.GetRequiredService<IOptionsMonitor<AzureStorageSettings>>()));
            services.AddSingleton<IDepartmentStorageProvider>((provider) => new DepartmentStorageProvider(provider.GetRequiredService<IOptionsMonitor<AzureStorageSettings>>()));
            services.AddSingleton<ICardNotificationsStorageProvider>((provider) => new CardNotificationsStorageProvider(provider.GetRequiredService<IOptionsMonitor<AzureStorageSettings>>()));
        }
        /// <summary>
        /// Registers helpers for DB operations.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        public static void RegisterHelpers(this IServiceCollection services)
        {
            services.AddTransient<IAppLifecycleHandler, AppLifecycleHandler>();
            services.AddTransient<IUserHelper, UserHelper>();
            services.AddTransient<INotificationHelper, NotificationHelper>();
            services.AddTransient<IGroupsHelper, GroupsHelper>();
            services.AddTransient<ITokenHelper, TokenHelper>();
            
        }



        /// <summary>
        /// Registers services such as MS Graph, token acquisition etc.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        public static void RegisterGraphServices(this IServiceCollection services)
        {
            //// Add microsoft graph services.
            services.AddScoped<IAuthenticationProvider, GraphTokenProvider>();
            services.AddScoped<IGraphServiceClient, GraphServiceClient>();
            services.AddScoped<IGraphServiceFactory, GraphServiceFactory>();
            services.AddScoped<IUsersService>(sp => sp.GetRequiredService<IGraphServiceFactory>().GetUsersService());
            services.AddScoped<IGroupsService>(sp => sp.GetRequiredService<IGraphServiceFactory>().GetGroupsService());
            services.AddScoped<IMessageService>(sp => sp.GetRequiredService<IGraphServiceFactory>().GetMessageService());

            
        }

        /// <summary>
        /// Adds services to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IAdaptiveCardService, AdaptiveCardService>();
        }

        /// <summary>
        /// Adds credential providers for authentication.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void RegisterCredentialProviders(this IServiceCollection services, IConfiguration configuration)
        {
            ICredentialProvider credentialProvider = new SimpleCredentialProvider(
                appId: configuration.GetValue<string>("MicrosoftAppId"),
                password: configuration.GetValue<string>("MicrosoftAppPassword"));

            services
                .AddSingleton(credentialProvider);

            services.AddSingleton(new OAuthClient((new MicrosoftAppCredentials(configuration.GetValue<string>("MicrosoftAppId"), configuration.GetValue<string>("MicrosoftAppPassword")))));
        }

        public static void RegisterAuthenticationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            services.AddProtectedWebApi(configuration)
                .AddProtectedWebApiCallsProtectedWebApi(configuration)
                .AddInMemoryTokenCaches();

            // This works specifically for single tenant application.
            var azureSettings = new AzureSettings();
            configuration.Bind("AzureAd", azureSettings);
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
               
                options.Authority = $"{azureSettings.Instance}/{azureSettings.TenantId}/v2.0";
                options.SaveToken = true;
                options.TokenValidationParameters.ValidAudiences = new List<string> { azureSettings.ClientId, azureSettings.ApplicationIdURI.ToUpperInvariant() };
                options.TokenValidationParameters.AudienceValidator = AudienceValidator;
                options.TokenValidationParameters.ValidIssuers = (azureSettings.ValidIssuers?
                    .Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)?
                    .Select(p => p.Trim())).Select(validIssuer => validIssuer.Replace("TENANT_ID", azureSettings.TenantId, StringComparison.OrdinalIgnoreCase));
            });
        }

        /// <summary>
        /// Add confidential credential provider to access API.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void RegisterConfidentialCredentialProvider(this IServiceCollection services, IConfiguration configuration)
        {
            configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            IConfidentialClientApplication confidentialClientApp = ConfidentialClientApplicationBuilder.Create(configuration["MicrosoftAppId"])
                .WithClientSecret(configuration["MicrosoftAppPassword"])
                .Build();
            services.AddSingleton<IConfidentialClientApplication>(confidentialClientApp);
        }

        /// <summary>
        /// Adds localization settings to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void RegisterLocalizationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddLocalization();
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var defaultCulture = CultureInfo.GetCultureInfo(configuration.GetValue<string>("i18n:DefaultCulture"));
                var supportedCultures = configuration.GetValue<string>("i18n:SupportedCultures").Split(',')
                    .Select(culture => CultureInfo.GetCultureInfo(culture))
                    .ToList();

                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new AppLocalizationCultureProvider(),
                };
            });
        }

        private static bool AudienceValidator(
            IEnumerable<string> tokenAudiences,
            SecurityToken securityToken,
            TokenValidationParameters validationParameters)
        {
            if (tokenAudiences.IsNullOrEmpty())
            {                
                throw new ApplicationException("No audience defined in token!");
            }

            var validAudiences = validationParameters.ValidAudiences;
            if (validAudiences.IsNullOrEmpty())
            {
                throw new ApplicationException("No valid audiences defined in validationParameters!");
            }

            return tokenAudiences.Intersect(tokenAudiences).Any();
        }
    }
}