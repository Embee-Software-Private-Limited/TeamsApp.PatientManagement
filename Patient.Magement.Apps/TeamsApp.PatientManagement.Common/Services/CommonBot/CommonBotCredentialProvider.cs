
namespace TeamsApp.PatientManagement.Common.CommonBot
{
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// A common bot credential provider.
    /// </summary>
    public class CommonBotCredentialProvider : SimpleCredentialProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommonBotCredentialProvider"/> class.
        /// </summary>
        /// <param name="botOptions">The bot options.</param>
        public CommonBotCredentialProvider(IOptions<BotOptions> botOptions)
            : base(appId: botOptions?.Value?.MicrosoftAppId, password: botOptions?.Value?.MicrosoftAppPassword)
        {
        }
    }
}
