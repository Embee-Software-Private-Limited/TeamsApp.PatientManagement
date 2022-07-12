

namespace Bansal.TeamsApp.PatientManagement.Bot.Authentication
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// This class is an authorization policy requirement.
    /// It specifies that an id token must contain Upn claim.
    /// </summary>
    public class MustBeValidUpnRequirement : IAuthorizationRequirement
    {
    }
}
