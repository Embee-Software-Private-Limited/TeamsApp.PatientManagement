using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using TeamsApp.PatientManagement.Common.Models;
using TeamsApp.PatientManagement.Common.Models.Ether;

namespace TeamsApp.PatientManagement.Common.Services.AdaptiveCard
{
    public interface IAdaptiveCardService
    {
        Attachment GetNewPatientDetailCard(EtherPatientResponseModel patientDetails, string channelId,string teamId, string departmentId, EtherPatientAdmissionDetailsResponseModel admissionDetails);
        Attachment GetUpdatePatientDetailCard_Update(string PatientId, string UHID, string AdmissionId, string PatientName, string Gender, string AdmissionNo, string DOA, string channelId, string teamId, string departmentId);
    
    }
}