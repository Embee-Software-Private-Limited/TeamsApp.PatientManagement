using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using System.Threading.Tasks;

namespace TeamsApp.PatientManagement.Bot.Bots
{
    public interface IAppLifecycleHandler
    {
        Task OnBotInstalledInTeamsAsync(ITurnContext turnContext, TeamsChannelData teamsChannelData);
        Task OnBotRemovedInTeamsAsync(ITurnContext turnContext, TeamsChannelData teamsChannelData);
        Task<TaskModuleResponse> OnFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest);
        Task<TaskModuleResponse> OnSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest);
    }
}