
import {getAppBaseUrl} from './AppConfiguration'

export const APIEndPoints= {
    getMyProfile: 'common/GetMyProfile',
    authenticationMetadata:'consentUrl',
    getGroups: 'common/GetGroups',
    getChannels: 'common/GetChannels',
    getDepartments: 'common/GetDepartments',
    getDepartmentById: 'common/GetDepartmentById',
    saveDepartment: 'common/SaveDepartment',
    getDepartmentsFromEther: 'etherteamsapp/GetDepartments',
    getPatientFromEther: 'etherteamsapp/GetPatient',
    getPatientAdmissionDetailsFromEther: 'etherteamsapp/GetPatientAdmissionDetails',
    getDoctorNotesFromEther: 'etherteamsapp/GetDoctorNotes',
    postAddUpdateDoctorNotes: 'etherteamsapp/AddUpdateDoctorNotes',
    getVitalsFromEther: 'etherteamsapp/GetVitals',
    postAddUpdateVitals: 'etherteamsapp/AddUpdateVitals',
    getIntakeAndOutputFromEther: 'etherteamsapp/GetIntakeAndOutput',
    postAddUpdateIntakeAndOutput: 'etherteamsapp/AddUpdateIntakeAndOutput',
    getPrescriptionsFromEther: 'etherteamsapp/GetPrescriptions',
    getLabReportsFromEther: 'etherteamsapp/GetLabReports',
    getMedicalHistoryFromEther: 'etherteamsapp/GetMedicalHistory'
}
export const getUrl = (key: any) => {    
    //return base_URL + key;
    return getAppBaseUrl()+'/api/v1.0/'+ key;
} 
