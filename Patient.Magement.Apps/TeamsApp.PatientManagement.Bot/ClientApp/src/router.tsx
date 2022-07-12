 
 import ErrorPage from './Components/ErrorPage/errorPage';
 import SignInPage from './Components/SignInPage/signInPage';
 import SignInSimpleStart from './Components/SignInPage/signInSimpleStart';
 import SignInSimpleEnd from './Components/SignInPage/signInSimpleEnd';
import { getAppBaseUrl } from "./apis/AppConfiguration";
import AddVitals from './Components/Vitals/AddVitals';
import ViewVitals from './Components/Vitals/ViewVitals';
import PatientDetails from './Components/Patient/PatientDetails';
import AddIntakeOutput from './Components/IntakeOutput/AddIntakeOutput';
import ViewIntakeOutput from './Components/IntakeOutput/ViewIntakeOutput';
import ViewPrescription from './Components/Prescription/ViewPrescription';
import ViewMedicalHistory from './Components/MedicalHistory/ViewMedicalHistory';
import AddDoctorNotes from './Components/DoctorNotes/AddDoctorNotes';
import ViewDoctorNotes from './Components/DoctorNotes/ViewDoctorNotes';
import ViewLabReport from './Components/LabReport/ViewLabReport';
import ListDepartment from './Components/Admin/ListDepartment';
import AddDepartment from './Components/Admin/AddDepartment';
import PatientView from './Components/Patient/PatientView';
import ViewLabReportPdf from './Components/LabReport/ViewLabReportPdf';

export const Routes=[
    {path:'/vitals/add', component:AddVitals},
    {path:'/vitals/view', component:ViewVitals},
    {path:'/patients/details', component:PatientDetails},
    {path:'/patients/view', component:PatientView},
    {path:'/intakeoutput/add', component:AddIntakeOutput},
    {path:'/intakeoutput/view', component:ViewIntakeOutput},
    {path:'/labreport/view', component:ViewLabReport},
    {path:'/labreport/viewPdf', component:ViewLabReportPdf},
    {path:'/medicalhistory/view', component:ViewMedicalHistory},
    {path:'/doctornotes/add', component:AddDoctorNotes},
    {path:'/doctornotes/view', component:ViewDoctorNotes},
    {path:'/prescription/view', component:ViewPrescription},
    {path:'/admin/department/list', component:ListDepartment},
    {path:'/admin/department/add', component:AddDepartment},
    {path:'/signin', component:SignInPage},
    {path:'/signin-simple-start', component:SignInSimpleStart},
    {path:'/signin-simple-end', component:SignInSimpleEnd},
    {path:'/errorpage', component:ErrorPage},
    {path:'/', exact:true, redirectTo:'/patients/details'}
]