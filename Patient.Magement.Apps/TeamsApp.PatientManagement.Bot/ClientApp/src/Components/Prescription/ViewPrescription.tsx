import React from 'react';
import { Container, Row } from 'react-bootstrap';
import { ReactComponent as Jotaro } from "../../images/addVitalbg.svg";
import {
    Text,
    Segment,
    Loader, 
    Header
} from '@fluentui/react-northstar';

import {
    Clock20Regular,
    ChevronLeft24Regular,
    AddRegular,
    Chat20Regular,
    Call20Regular,
    PersonCircle24Regular
} from "@fluentui/react-icons";
import { Persona, PersonaSize } from '@fluentui/react';
import Moment from 'react-moment';
import * as microsoftTeams from "@microsoft/teams-js";
import { getPatientsDetailsFromEtherAPIEndpoint, getPrescriptionsFromEtherAPIEndpoint } from '../../apis/APIList';
import moment from 'moment';
import PatientPrimaryDetails from '../Interfaces/patientPrimaryDetails';


interface IProps {
    history?: any,
    location?: any
}

interface IState {
    prescriptionList: any;
    timeLineDatesList: any;
    loading: boolean,
    PatientDetails?: any,
    patientPrimaryDetails: any,
    UHID?: any
    AdmissionId?: any,
    DepartmentId?: any,
    LoggedInUserEmail: any,
    LoggedInUserUPN: any,
    AccessToken?:any
}

class ViewPrescription extends React.Component<IProps, IState> {

    constructor(props: IProps) {
        super(props);
        this.state = {
            loading: false,
            prescriptionList: [],
            timeLineDatesList: [],
            LoggedInUserEmail: "",
            LoggedInUserUPN: "",
            patientPrimaryDetails: {}
        };

    }
    componentDidMount() {
        microsoftTeams.initialize();
        microsoftTeams.getContext(context => {
            if(this.props.location.state!==undefined && this.props.location.state.AccessToken){
                console.log("Access from location state");
                this.setState(
                    {AccessToken:this.props.location.state.AccessToken},
                    ()=>{this.onPageLoad(context)}
                    
                    )
            }
            else{
                console.log("Get new token");
            const authTokenRequest = {
                successCallback: (token: string) => {
                  this.setState(
                    {AccessToken:token},
                    ()=>{this.onPageLoad(context)}
                    
                    )
                },
                failureCallback: (error: string) => {
                    console.error("Error from getAuthToken: ", error);
                    window.location.href = `/signin?locale=en-US`;
                    //alert(error);
                },
                resources: []
            };
            microsoftTeams.authentication.getAuthToken(authTokenRequest);  
            
        }
        //this.onPageLoad(context);  
        });

    }
    onPageLoad(context:any){
        const queryString = window.location.search;
            if (queryString !== "") {
                const urlParams = new URLSearchParams(queryString);
                const UHID = urlParams.get('UHID');
                const AdmissionId = urlParams.get('AdmissionId');
                const DepartmentId = urlParams.get('DepartmentId');
                if (UHID !== null && UHID !== "" && AdmissionId !== null && AdmissionId !== "" && DepartmentId !== null && DepartmentId !== "") {

                    this.setState({
                        UHID: UHID,
                        AdmissionId: AdmissionId,
                        DepartmentId: DepartmentId,
                        LoggedInUserEmail: context.userPrincipalName,
                        LoggedInUserUPN: context.userPrincipalName
                    });
                    this.loadPrescription(UHID, AdmissionId);
                    this.getPatientDetails(UHID);
                }
            }
            else {
 
                this.setState({
                    UHID: this.props.location.state.UHID,
                    AdmissionId: this.props.location.state.AdmissionId,
                    DepartmentId: this.props.location.state.DepartmentId,
                    LoggedInUserEmail: context.userPrincipalName,
                    LoggedInUserUPN: context.userPrincipalName,
                    PatientDetails:this.props.location.state.PatientDetails,
                    patientPrimaryDetails:this.props.location.state.PatientDetails
                },
                    () => {
                        this.loadPrescription(this.state.UHID, this.state.AdmissionId);
                        //this.getPatientDetails(this.state.UHID);
                    }
                );
            }
    }
    loadPrescription = (UHID: any, AdmissionId: any) => {
        this.setState({ loading: true });        
        getPrescriptionsFromEtherAPIEndpoint(UHID, AdmissionId,this.state.AccessToken).then((res) => {
            //console.log(res.data);
            this.setState({ loading: false });
            if (res.data.status == "success") {
                this.setState({ prescriptionList: res.data.bodyList, timeLineDatesList: res.data.timeLineDates });
            }
        });
    };
    getPatientDetails(UHID: any) {
        getPatientsDetailsFromEtherAPIEndpoint(UHID,this.state.AccessToken).then((res) => {
            if (res.data.status === "success" && res.data.body.length > 0) {
                const responseData = res.data.body[0];
                this.setState({ PatientDetails: responseData, patientPrimaryDetails: responseData });
            }
        });
    }
    pushToLocationHistory(url: any) {
        this.props.history.push({
            pathname: url,
            state: {
                UHID: this.state.UHID,
                AdmissionId: this.state.AdmissionId,
                DepartmentId: this.state.DepartmentId,
                PatientDetails: this.state.PatientDetails,
                AccessToken:this.state.AccessToken
            }
        });
    }
    //In no record exists then render this content
    NoRecordFoundContent() {
        return (
            <Container fluid>
                <div className='d-flex justify-content-between align-items-center my-3'>
                    <div><a href='javascript:void(0)' onClick={() => this.pushToLocationHistory('/patients/details')}><ChevronLeft24Regular /></a></div>
                    <div className='d-flex'><Text className='ms-2' content="Prescriptions" size="medium" weight="semibold" /></div>
                    <div></div>
                </div>
                <div className='mx-3'>
                    <PatientPrimaryDetails patientPrimaryDetails={this.state.patientPrimaryDetails}/>
                </div>
                <div className='mt-5 text-center'>
                    <div><Jotaro  height={150}/></div>
                    <Text className='d-block mb-2' content="No Prescription added" size="large" weight="bold" />
                    <Text className='d-block' content="Prescription of the patient show up here" size="medium" weight="regular" />

                </div>
            </Container>

        )
    }
    
    
    BuildItemDetailDateWise = (date: any) => {
        return (
            <div className="tl-item active">
                <div className="tl-dot"></div>
                <div className="d-flex flex-column w-100">
                    <div><span className="badge bg-gadient-green mb-1">{date}</span></div>
                    {this.state.prescriptionList.filter((x: any) => x.AddedOn === date).map((item: any) => this.BuildItemDetailPrescription(item))}
                </div>
            </div>
        )
    };
    BuildItemDetailPrescription = (item: any) => {
        var rowId=item.PrescriptionId +"_"+item.AddedOn;
        return (
            item.medicine_details.map((prescriptionItem:any)=>this.BuildItemDetail(prescriptionItem,rowId))
        )
    };
    BuildItemDetail = (item: any,rowId:any) => {  
        console.log(item);
        var splitItem=item.split('|');
        return (              
            <div className="card w-100">
                <div className="accordion accordion-flush" id={"accordionOne-" + rowId}>
                    <div className="accordion-item">
                        <h2 className="accordion-header" id={"headingOne-" + rowId}>
                            <div className='d-flex justify-content-between align-items-center accordion-button collapsed' data-bs-toggle="collapse" data-bs-target={"#collapseOne-" + rowId} aria-expanded="false" aria-controls={"collapseOne-" + rowId} >
                                <div className='d-flex align-items-center'><Text className='ms-1' content="Prescription" size="medium" weight="semibold" /></div>
                            </div>
                        </h2>
                        <div id={"collapseOne-" + rowId} className="accordion-collapse collapse" aria-labelledby={"headingOne-" + rowId} data-bs-parent={"#accordionOne-" + rowId}>
                            <div className="accordion-body">
                                
                                <Row>
                                    <div className='col-md-12 col-12'>
                                        <div className='d-flex'>
                                            <div>
                                             {splitItem.map((dtl:any) =><Text className='d-block' content={dtl} size="medium"  />)}
                                            </div>
                                        </div>
                                    </div>
                                    
                                </Row>
                                
                            </div>
                        </div>
                    </div>
                </div>
            </div>

        );
    };
    //In  record exists then render this content
    RecordFoundContent() {
        return (
            <Container fluid>
                <div className='d-flex justify-content-between align-items-center my-3'>
                    <div><a href='javascript:void(0)' onClick={() => this.pushToLocationHistory('/patients/details')}><ChevronLeft24Regular /></a></div>
                    <div className='d-flex'><Text className='ms-2' content="Prescriptions" size="medium" weight="semibold" /></div>
                    <div></div>
                </div>
                <PatientPrimaryDetails patientPrimaryDetails={this.state.patientPrimaryDetails}/>
                <div className="timeline block my-4">

                    {this.state.timeLineDatesList.map((date: any) => this.BuildItemDetailDateWise(date))}


                    <div className="tl-item">
                        <div className="tl-dot"></div>
                        <div className="d-flex flex-column w-100">
                            <div><span className="badge bg-gadient-grey mb-1">No further history</span></div>

                        </div>
                    </div>
                </div>
            </Container>
        )
    }
    render() {
        if (!this.state.loading) {
            const isRecordExists = this.state.prescriptionList.length > 0 ? true : false;;
            let renderContent;
            if (isRecordExists) {
                renderContent = this.RecordFoundContent();
            } else {
                renderContent = this.NoRecordFoundContent();
            }
            return (
                <div>
                    {renderContent}
                </div>
            );
        }
        else {
            return <Segment><Loader></Loader></Segment>
        }
    }
}
export default ViewPrescription;





