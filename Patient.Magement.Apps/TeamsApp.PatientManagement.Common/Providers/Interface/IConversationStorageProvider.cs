using TeamsApp.PatientManagement.Common.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TeamsApp.PatientManagement.Common.Providers
{
    public interface IConversationStorageProvider
    {
        Task<bool> AddEntityAsync(ConversationEntity entity);
        Task<bool> DeleteEntityAsync(ConversationEntity entity);
        Task<IList<ConversationEntity>> GetTeamAsync(string teamId);
        Task<IList<ConversationEntity>> GetTeamGroupIdAsync(string teamGroupdId);
        Task<IList<ConversationEntity>> GetUserAsync(string userId);
    }
}