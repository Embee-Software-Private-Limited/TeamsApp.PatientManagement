using TeamsApp.PatientManagement.Common.Models.Entities;
using System.Threading.Tasks;

namespace TeamsApp.PatientManagement.Common.Providers
{
    public interface ICardNotificationsStorageProvider
    {
        Task<bool> AddEntityAsync(CardNotificationsEntity entity);
        Task<bool> DeleteEntityAsync(CardNotificationsEntity entity);
        Task<CardNotificationsEntity> GetAsync(string activityId);
        Task<CardNotificationsEntity> GetByPatientAdmissionAndChannelIdAsync(string UHID, string admissionId, string channelId);
    }
}