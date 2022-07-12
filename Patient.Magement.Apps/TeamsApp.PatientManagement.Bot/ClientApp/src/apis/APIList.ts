import axios from './axiosJWTDecorator';
import {getUrl,APIEndPoints} from './Endpoint'


////////////////////// User Profile ////////////////////

export const getMyProfileAPIEndpoint = async () => {
    console.log('In api',  getUrl(APIEndPoints.getMyProfile))
    return await axios.get(getUrl(APIEndPoints.getMyProfile));
}

///////////////////////  SSO ///////////////////

export const getAuthenticationConsentMetadata = async (windowLocationOriginDomain: string, login_hint: string): Promise<any> => {
    console.log('In api',  getUrl(APIEndPoints.authenticationMetadata)+`?windowLocationOriginDomain=${windowLocationOriginDomain}&loginhint=${login_hint}`)
    return await axios.get(getUrl(APIEndPoints.authenticationMetadata)+`?windowLocationOriginDomain=${windowLocationOriginDomain}&loginhint=${login_hint}`, undefined, false);
}
////////////////////// Department ////////////////////
export const getDepartmentsAPIEndpoint = async () => {
    console.log('In api',  getUrl(APIEndPoints.getDepartments))
    return await axios.get(getUrl(APIEndPoints.getDepartments));
}
////////////////////// Department By  Id ////////////////////
export const getDepartmentByIdAPIEndpoint = async (departmentId:string) => {
    console.log('In api',  getUrl(APIEndPoints.getDepartmentById))
    return await axios.get(getUrl(APIEndPoints.getDepartmentById)+'?departmentId='+ departmentId);
}

export const postSaveDepartmentAPIEndpoint = async (department:any) => {
    console.log('In api', getUrl(APIEndPoints.saveDepartment))
    return await axios.post(getUrl(APIEndPoints.saveDepartment), department);
}

////////////////////// Get Team Groups////////////////////
export const getGroupsAPIEndpoint = async (query:string) => {
    console.log('In api',  getUrl(APIEndPoints.getGroups)+`?query=${query}`)
    return await axios.get(getUrl(APIEndPoints.getGroups)+`?query=${query}`);
}
////////////////////// Get Team Channels////////////////////
export const getChannelsAPIEndpoint = async (teamId:string) => {
    console.log('In api',  getUrl(APIEndPoints.getChannels)+`?teamId=${teamId}`)
    return await axios.get(getUrl(APIEndPoints.getChannels)+`?teamId=${teamId}`,false);
}


////////////////////// Department From Ether////////////////////
export const getDepartmentsFromEtherAPIEndpoint = async () => {
    console.log('In api',  getUrl(APIEndPoints.getDepartmentsFromEther))
    return await axios.get(getUrl(APIEndPoints.getDepartmentsFromEther),true,true,"");
}
//////////////////////Patients Details  Id ////////////////////
export const getPatientsDetailsFromEtherAPIEndpoint = async (UHID:string,accessToken:any) => {
    console.log('In api',  getUrl(APIEndPoints.getPatientFromEther))
    return await axios.get(getUrl(APIEndPoints.getPatientFromEther)+'?UHID='+ UHID,true,true,accessToken);
}
//////////////////////Patients Admission Details  ////////////////////
export const getPatientAdmissionDetailsFromEtherAPIEndpoint = async (UHID:string,AdmissionId:string,accessToken:any) => {
    console.log('In api',  getUrl(APIEndPoints.getPatientAdmissionDetailsFromEther))
    return await axios.get(getUrl(APIEndPoints.getPatientAdmissionDetailsFromEther)+'?UHID='+ UHID+'&AdmissionId='+AdmissionId,true,true,accessToken);
}

//////////////////////Vitals////////////////////
export const getVitalsFromEtherAPIEndpoint = async (UHID:string,AdmissionId:string,accessToken:any) => {
    console.log('In api',  getUrl(APIEndPoints.getVitalsFromEther))
    return await axios.get(getUrl(APIEndPoints.getVitalsFromEther)+'?UHID='+ UHID+'&AdmissionId='+AdmissionId,true,true,accessToken);
}
export const postAddUpdateVitalsAPIEndpoint = async (data:any,accessToken:any) => {
    console.log('In api', getUrl(APIEndPoints.postAddUpdateVitals))
    return await axios.post(getUrl(APIEndPoints.postAddUpdateVitals), data,true,true,accessToken);
}
//////////////////////Doctor Notes////////////////////
export const getDoctorNotesFromEtherAPIEndpoint = async (UHID:string,AdmissionId:string,accessToken:any) => {
    console.log('In api',  getUrl(APIEndPoints.getDoctorNotesFromEther))
    return await axios.get(getUrl(APIEndPoints.getDoctorNotesFromEther)+'?UHID='+ UHID+'&AdmissionId='+AdmissionId,true,true,accessToken);
}
export const postAddUpdateDoctorNotesAPIEndpoint = async (data:any,accessToken:any) => {
    console.log('In api', getUrl(APIEndPoints.postAddUpdateDoctorNotes))
    return await axios.post(getUrl(APIEndPoints.postAddUpdateDoctorNotes), data,true,true,accessToken);
}

//////////////////////Intake Output////////////////////
export const getIntakeAndOutputFromEtherAPIEndpoint = async (UHID:string,AdmissionId:string,accessToken:any) => {
    console.log('In api',  getUrl(APIEndPoints.getIntakeAndOutputFromEther))
    return await axios.get(getUrl(APIEndPoints.getIntakeAndOutputFromEther)+'?UHID='+ UHID+'&AdmissionId='+AdmissionId,true,true,accessToken);
}
export const postAddUpdateIntakeAndOutputAPIEndpoint = async (data:any,accessToken:any) => {
    console.log('In api', getUrl(APIEndPoints.postAddUpdateIntakeAndOutput))
    return await axios.post(getUrl(APIEndPoints.postAddUpdateIntakeAndOutput), data,true,true,accessToken);
}
//////////////////////Prescription////////////////////
export const getPrescriptionsFromEtherAPIEndpoint = async (UHID:string,AdmissionId:string,accessToken:any) => {
    console.log('In api',  getUrl(APIEndPoints.getPrescriptionsFromEther))
    return await axios.get(getUrl(APIEndPoints.getPrescriptionsFromEther)+'?UHID='+ UHID+'&AdmissionId='+AdmissionId,true,true,accessToken);
}

//////////////////////Lab Reports////////////////////
export const getLabReportsFromEtherAPIEndpoint = async (UHID:string,AdmissionId:string,accessToken:any) => {
    console.log('In api',  getUrl(APIEndPoints.getLabReportsFromEther))
    return await axios.get(getUrl(APIEndPoints.getLabReportsFromEther)+'?UHID='+ UHID+'&AdmissionId='+AdmissionId,true,true,accessToken);
}
//////////////////////Medical History////////////////////
export const getMedicalHistoryFromEtherAPIEndpoint = async (UHID:string,AdmissionId:string,accessToken:any) => {
    console.log('In api',  getUrl(APIEndPoints.getMedicalHistoryFromEther))
    return await axios.get(getUrl(APIEndPoints.getMedicalHistoryFromEther)+'?UHID='+ UHID+'&AdmissionId='+AdmissionId,true,true,accessToken);
}


