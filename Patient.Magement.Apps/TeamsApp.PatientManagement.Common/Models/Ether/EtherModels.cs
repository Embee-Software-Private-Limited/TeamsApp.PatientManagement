using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeamsApp.PatientManagement.Common.Models.Ether
{
    public class EtherResponseError
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("number")]
        public string Number { get; set; }
        [JsonProperty("head")]
        public EtherResponseHead Head { get; set; }

        public IEnumerable<EtherResponseErrorBody> body { get; set; }

    }
    public class EtherResponseErrorBody
    {
        [JsonProperty("Status")]
        public string Status { get; set; }
        [JsonProperty("Message")]
        public string Message { get; set; }
    }

    public class EtherResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("number")]
        public string Number { get; set; }
        [JsonProperty("head")]
        public EtherResponseHead Head { get; set; }
    }
    public class EtherResponseHead
    {
        [JsonProperty("requestTimeStamp")]
        public DateTime RequestTimeStamp { get; set; }
        [JsonProperty("checksum")]
        public string Checksum { get; set; }
    }

    #region Department
    public class EtherDeparmentResponseModel: EtherResponse
    {
        public IEnumerable<EtherDeparmentModel> body { get; set; }
        public List<DateTime> TimeLineDates { get; set; }
    }

    public class EtherDeparmentModel
    {
        [JsonProperty("DepartmentId")]
        public string DepartmentId { get; set; }
        [JsonProperty("DepartmentName")]
        public string DepartmentName { get; set; }

        [JsonProperty("Result")]
        public bool Result { get; set; }
        
    }

    #endregion
    #region Patient
    public class EtherPatientResponseModel : EtherResponse
    {
        public IEnumerable<EtherPatientModel> body { get; set; }
    }

    public class EtherPatientModel
    {
        [JsonProperty("PatientId")]
        public string PatientId { get; set; }

        [JsonProperty("UHID")]
        public string UHID { get; set; }

        [JsonProperty("Salutation")]
        public string Salutation { get; set; }

        [JsonProperty("PatientName")]
        public string PatientName { get; set; }

        [JsonProperty("Sex")]
        public string Sex { get; set; }

        [JsonProperty("BloodGroup")]
        public string BloodGroup { get; set; }

        [JsonProperty("DateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [JsonProperty("RegistrationAge")]
        public string RegistrationAge { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("PhoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("RegistrationDate")]
        public DateTime RegistrationDate { get; set; }

        [JsonProperty("AdmissionId")]
        public string AdmissionId { get; set; }

        [JsonProperty("Result")]
        public bool Result { get; set; }
    }



    #endregion

    #region PatientAdmissionDetails
    public class EtherPatientAdmissionDetailsResponseModel : EtherResponse
    {
        public IList<EtherPatientAdmissionDetailsModel> body { get; set; }

    }

    public class EtherPatientAdmissionDetailsModel : EtherResponseBodyCommon
    {
        [JsonProperty("PatientId")]
        public string PatientId { get; set; }

        [JsonProperty("ReferedBy")]
        public string ReferedBy { get; set; }

        [JsonProperty("RefererNote")]
        public string RefererNote { get; set; }

        [JsonProperty("AdmissionReason")]
        public string AdmissionReason { get; set; }

        [JsonProperty("AdmissionDate")]
        public DateTime AdmissionDate { get; set; }

        [JsonProperty("DepartmentId")]
        public string DepartmentId { get; set; }

        [JsonProperty("DepartmentName")]
        public string DepartmentName { get; set; }

        [JsonProperty("DischargeDate")]
        public DateTime? DischargeDate { get; set; }

        [JsonProperty("DischargeReason")]
        public string DischargeReason { get; set; }

        [JsonProperty("DischargeInfo")]
        public string DischargeInfo { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("BedDetails")]
        public BedDetails BedDetails { get; set; }

        [JsonProperty("DiagnosisDetails")]
        public DiagnosisDetails DiagnosisDetails { get; set; }

        [JsonProperty("Admnumber")]
        public string Admnumber { get; set; }
        
    }

    public class BedDetails
    {
        [JsonProperty("BedNumber")]
        public string BedNumber { get; set; }
        [JsonProperty("Status")]
        public string Status { get; set; }
        [JsonProperty("TransferNote")]
        public string TransferNote { get; set; }
        [JsonProperty("BedAssignDate")]
        public DateTime? BedAssignDate { get; set; }

    }
    public class DiagnosisDetails
    {
        [JsonProperty("Disease")]
        public string Disease { get; set; }
        [JsonProperty("InformationonFinalDiagnosis")]
        public string InformationonFinalDiagnosis { get; set; }
        [JsonProperty("Date")]
        public DateTime Date { get; set; }

    }
    #endregion

    #region Doctor Notes


    public class EtherDoctorNotesResponseModel : EtherResponse
    {
        public IList<EtherDoctorNotesModel> body { get; set; }

        public List<DateTime> TimeLineDates { get; set; }
    }

    public class EtherDoctorNotesModel : EtherResponseBodyCommon
    {
        [JsonProperty("DoctorNoteId")]
        public string DoctorNoteId { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }       

        [JsonProperty("Description")]
        public string Description { get; set; }



        private DateTime? docDate;
        [JsonProperty("DoctorNoteDate")]
        public DateTime? DoctorNoteDate
        {
            get { return docDate.ToString() == "0000-00-00 00:00:00" ? null : docDate; }
            set { docDate = value; }
        }


    }

    public class EtherPostDataDoctorNote : EtherPostDataCommon
    {
        [JsonProperty("DoctorNoteId")]
        public string DoctorNoteId { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("DoctorNoteDate")]
        public DateTime DoctorNoteDate { get; set; }
    }
    #endregion

    #region Vitals
    public class EtherVitalsResponseModel : EtherResponse
    {
        public IList<EtherVitalsModel> body { get; set; }
        public List<DateTime> TimeLineDates { get; set; }
    }

    public class EtherVitalsModel : EtherResponseBodyCommon
    {
        [JsonProperty("VitalId")]
        public string VitalId { get; set; }

        [JsonProperty("DepartmentId")]
        public string DepartmentId { get; set; }

        [JsonProperty("DepartmentName")]
        public string DepartmentName { get; set; }

        [JsonProperty("Temp")]
        public string Temp { get; set; }

        [JsonProperty("Weight")]
        public string Weight { get; set; }

        [JsonProperty("Height")]
        public string Height { get; set; }

        [JsonProperty("PulseRate")]
        public string PulseRate { get; set; }

        [JsonProperty("BPDiastolic")]
        public string BPDiastolic { get; set; }

        [JsonProperty("BPSystolic")]
        public string BPSystolic { get; set; }

        [JsonProperty("RR")]
        public string RR { get; set; }
        [JsonProperty("Spo2")]
        public string Spo2 { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }
        
    }
    public class EtherVitalsModelPostData
    {
        [JsonProperty("VitalId")]
        public string VitalId { get; set; }

        [JsonProperty("DepartmentId")]
        public string DepartmentId { get; set; }

        [JsonProperty("DepartmentName")]
        public string DepartmentName { get; set; }

        [JsonProperty("Temp")]
        public string Temp { get; set; }

        [JsonProperty("Weight")]
        public string Weight { get; set; }

        [JsonProperty("Height")]
        public string Height { get; set; }

        [JsonProperty("PulseRate")]
        public string PulseRate { get; set; }

        [JsonProperty("BPDiastolic")]
        public string BPDiastolic { get; set; }

        [JsonProperty("BPSystolic")]
        public string BPSystolic { get; set; }

        [JsonProperty("RR")]
        public string RR { get; set; }

        [JsonProperty("Spo2")]
        public string Spo2 { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }
    }
    public class EtherPostDataVitals : EtherPostDataCommon
    {
        public EtherVitalsModelPostData VitalDetails { get; set; }
    }
    
    #endregion

    #region IntakeOutput
    public class EtherIntakeOutputResponseModel : EtherResponse
    {
        public IList<EtherIntakeOutputModel> body { get; set; }
        public List<DateTime> TimeLineDates { get; set; }
    }

    public class EtherIntakeOutputModel : EtherResponseBodyCommon
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("OutputType")]
        public string OutputType { get; set; }

        [JsonProperty("fluid_in0")]
        public string Fluid_in0 { get; set; }

        [JsonProperty("fluid_out0")]
        public string Fluid_out0 { get; set; }

        [JsonProperty("fluid_bal")]
        public string Fluid_bal { get; set; }

        [JsonProperty("DateAdded")]
        public DateTime? DateAdded { get; set; }

    }

    public class EtherIntakeOutModelPostData
    {
        [JsonProperty("Id")]
        public string Id { get; set; }
        [JsonProperty("IntakeId")]
        public string IntakeId { get; set; }
        

        [JsonProperty("OutputType")]
        public string OutputType { get; set; }

        [JsonProperty("fluid_in0")]
        public string Fluid_in0 { get; set; }

        [JsonProperty("fluid_out0")]
        public string Fluid_out0 { get; set; }

        [JsonProperty("fluid_bal")]
        public string Fluid_bal { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }
        [JsonProperty("DateAdded")]
        public DateTime DateAdded { get; set; }
        
    }
    public class EtherPostDataIntakeOut : EtherPostDataCommon
    {
        public EtherIntakeOutModelPostData IntakeOutputDetails { get; set; }
    }

    #endregion

    #region LabReports
    public class EtherLabReportsResponseModel : EtherResponse
    {
        public IEnumerable<EtherLabReportsModel> body { get; set; }
        public List<DateTime> TimeLineDates { get; set; }
    }
    public class EtherLabReportsModel : EtherResponseBodyCommon
    {
        [JsonProperty("LabRequestId")]
        public string LabRequestId { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }
        
        [JsonProperty("TestName")]
        public string TestName { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("TestId")]
        public string TestId { get; set; }

        [JsonProperty("CategoryId")]
        public string CategoryId { get; set; }

        [JsonProperty("CategoryName")]
        public string CategoryName { get; set; }

        [JsonProperty("DepartmentId")]
        public string DepartmentId { get; set; }

        [JsonProperty("DepartmentName")]
        public string DepartmentName { get; set; }

        [JsonProperty("Outcome")]
        public string Outcome { get; set; }

        [JsonProperty("Priority")]
        public string Priority { get; set; }

        [JsonProperty("SampleName")]
        public string SampleName { get; set; }

        [JsonProperty("DoneDate")]
        public DateTime? DoneDate { get; set; }

        [JsonProperty("ReportFileUrl")]
        public EtherLabReportsDocumentModel ReportFileUrl { get; set; }

    }
    public class EtherLabReportsDocumentModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("date_entered")]
        public string DateEntered { get; set; }
    }
    #endregion

    #region Medical History
    public class EtherMedicalHistoryResponseModel : EtherResponse
    {
        public IList<EtherMedicalHistoryModel> body { get; set; }

        public List<DateTime> TimeLineDates { get; set; }
    }

    public class EtherMedicalHistoryModel : EtherResponseBodyCommon
    {
        [JsonProperty("MedicalHistoryId")]
        public string MedicalHistoryId { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

    }
    #endregion

    #region Prescriptions

    public class _0
    {
        public bool Result { get; set; }
    }

    public class EtherPrescriptionModel
    {
        [JsonProperty("0")]
        public _0 _0 { get; set; }
        public List<EtherPrescriptionModelDetails> pdetails { get; set; }
    }

    public class EtherPrescriptionModelDetails
    {
        [JsonProperty("PrescriptionId")]
        public string PrescriptionId { get; set; }

        [JsonProperty("AddedOn")]
        public string AddedOn { get; set; }

        [JsonProperty("medicine_details")]
        public List<string> medicine_details { get; set; }
    }

    public class EtherPrescriptionResponseModel: EtherResponse
    {
        public EtherPrescriptionModel body { get; set; }

        public List<EtherPrescriptionModelDetails> bodyList { get; set; }

        public List<string> TimeLineDates { get; set; }
    }
   
   
    #endregion
    public class EtherResponseBodyCommon
    {
        [JsonProperty("UHID")]
        public string UHID { get; set; }

        [JsonProperty("AdmissionId")]
        public string AdmissionId { get; set; }

        [JsonProperty("CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("CreatedByEmail")]
        public string CreatedByEmail { get; set; }

        [JsonProperty("ModifiedOn")]
        public DateTime ModifiedOn { get; set; }

        [JsonProperty("ModifiedBy")]
        public string ModifiedBy { get; set; }

        [JsonProperty("ModifiedByEmail")]
        public string ModifiedByEmail { get; set; }

        [JsonProperty("Result")]
        public bool Result { get; set; }

    }

    public class EtherNewPatientPostModel
    {
        [JsonProperty("admissionId")]
        public string AdmissionId { get; set; }
        [JsonProperty("UHID")]
        public string UHID { get; set; }
        [JsonProperty("departmentId")]
        public string DepartmentId { get; set; }

        [JsonProperty("apiKey")]
        public string APIKey { get; set; }
    }

    public class EtherPostDataCommon
    {
        [JsonProperty("UHID")]
        public string UHID { get; set; }

        [JsonProperty("AdmissionId")]
        public string AdmissionId { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("CreatedByEmail")]
        public string CreatedByEmail { get; set; }

        [JsonProperty("TeamId")]
        public string TeamId { get; set; }

        [JsonProperty("ChannelId")]
        public string ChannelId { get; set; }

        [JsonProperty("DepartmentId")]
        public string DepartmentId { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

    }
}
