using TeamsApp.PatientManagement.Common.Models.Ether;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace TeamsApp.PatientManagement.Common.Helper
{
    public interface INotificationHelper
    {
        Task<bool> NotifyOnNewPatientAdmission(EtherNewPatientPostModel dataModel, EtherPatientResponseModel patientDetails, EtherPatientAdmissionDetailsResponseModel admissionDetails);
        Task<bool> NotifyOnNewPatientAdmissionUpdateCard(string PatientId, string UHID, string AdmissionId, string PatientName, string Gender, string AdmissionNo, string DOA, string channelId, string teamId, string departmentId, string serviceUrl, string activityId, string TenantId);
        Task<bool> RemoveAdaptiveCard(string serviceUrl, string conversationId, string activityId, string admissionId, string patientId, string teamId, string tenantId, string uhid);
        Task<bool> ReplyToMessageInChannel(AdaptiveCardActionModel cardActionDataModel);
    }
}