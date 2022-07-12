import React from 'react';
import { Container, Row} from 'react-bootstrap';
import {
    Text,
    Header,
    Loader,
    Segment
} from '@fluentui/react-northstar';

import {
    PersonCircle24Regular,
    ChevronRight24Regular,
    HeartPulse24Regular,
    Pill24Regular,
    Notepad24Regular,
    ArrowSort24Regular,
    Stethoscope24Regular,
    Beaker24Regular
} from "@fluentui/react-icons";
import * as microsoftTeams from "@microsoft/teams-js";
import { getPatientAdmissionDetailsFromEtherAPIEndpoint, getPatientsDetailsFromEtherAPIEndpoint } from '../../apis/APIList';
import Moment from 'react-moment';

interface IProps {
    history?:any,
    location?:any
}

interface IState {
    loading:boolean,
    UHID?:any
    AdmissionId?:any,
    DepartmentId?:any,
    PatientDetails:{
        PatientId:string,
        UHID:string,
        Salutation:string,
        PatientName:string,
        RegistrationAge:string,
        RegistrationDate: any,
        AdmissionDate: any,
    },
   
    AccessToken?:any
    
}

class PatientDetails extends React.Component<IProps, IState> {

    constructor(props: IProps) {
        super(props);
        this.state = {
            loading: true,
            AccessToken:"",
            PatientDetails:{
                PatientId:"",
                UHID:"",
                Salutation:"",
                PatientName:"",
                RegistrationAge:"",
                RegistrationDate: "",
                AdmissionDate: ""
            },
            

        };

    }
    componentDidMount() {
        microsoftTeams.initialize();
        microsoftTeams.getContext(context => {

            //get access token
            const authTokenRequest = {
                successCallback: (token: string) => {
                    //alert(token);
                  this.setState(
                    {AccessToken:token},
                    ()=>{this.onPageLoad(context)}
                    
                    )
                },
                failureCallback: (error: string) => {
                    console.error("Error from getAuthToken: ", error);
                    //alert(error);
                    window.location.href = `/signin?locale=en-US`;
                },
                resources: []
            };
            microsoftTeams.authentication.getAuthToken(authTokenRequest);

            //this.onPageLoad(context);
        });
    }
    onPageLoad(context:any){
        const queryString=window.location.search;
            //console.log(context);
            if(queryString!=""){
                const urlParams = new URLSearchParams(queryString);
                const UHID = urlParams.get('UHID');
                const AdmissionId = urlParams.get('AdmissionId');
                const DepartmentId=urlParams.get('DepartmentId');
              
                if(UHID!==null && UHID!=="" && AdmissionId!==null && AdmissionId!==""  && DepartmentId!==null && DepartmentId!==""){                   
                this.setState({
                        UHID:UHID,
                        AdmissionId:AdmissionId,
                        DepartmentId:
                        DepartmentId
                    },()=>{ this.getPatientDetails(UHID);});
                    }               
            }
            else{
                this.setState({
                    UHID:this.props.location.state.UHID,
                    AdmissionId:this.props.location.state.AdmissionId,
                    DepartmentId:this.props.location.state.DepartmentId
                },
                ()=>{
                    this.getPatientDetails(this.state.UHID);
                    
                }
                );            
            }
    }
    getPatientDetails(UHID:any){
        getPatientsDetailsFromEtherAPIEndpoint(UHID,this.state.AccessToken).then((res) => {
            //console.log(res.data );
            if(res.data.status==="success" && res.data.body.length>0){
                const responseData=res.data.body[0];
                this.setState({ PatientDetails: responseData }, () => { this.getPatientAdmissionDetails(this.state.UHID, this.state.AdmissionId);});
                
            }
           
        });
    }
    getPatientAdmissionDetails(UHID: any, AdmissionId: any) {
        getPatientAdmissionDetailsFromEtherAPIEndpoint(UHID, AdmissionId, this.state.AccessToken).then((res) => {
            console.log(res);
            if (res.data.status === "success" && res.data.body.length > 0) {
                var pdetails = this.state.PatientDetails;
                pdetails.AdmissionDate = res.data.body[0].AdmissionDate;
                this.setState({ PatientDetails: pdetails });
            }
            this.setState({ loading: false });
        });
    }
    pushToLocationHistory(url:any){       
        this.props.history.push({ 
                pathname: url, 
                state: { UHID: this.state.UHID, 
                    AdmissionId: this.state.AdmissionId ,
                    DepartmentId:this.state.DepartmentId,
                    PatientDetails:this.state.PatientDetails,
                    AccessToken:this.state.AccessToken
                } 
            });
    }
    render() {

        if(!this.state.loading){
        var Salutation=this.state.PatientDetails.Salutation?this.state.PatientDetails.Salutation:"";
        var PatientName=this.state.PatientDetails.PatientName?this.state.PatientDetails.PatientName:"";
        return (
            <div>
                <Segment>
                    <div className='hero_sec py-4 px-3'>
                        <div className='d-flex justify-content-between mb-3'>
                            <div className='d-flex align-items-center'><PersonCircle24Regular color="#fff" /><Header className='mb-0 ms-1 text-light' as="h4" content={Salutation +" "+PatientName} /></div>
                            <div><a href='javascript:void(0)' onClick={()=>this.pushToLocationHistory('/patients/view')}><ChevronRight24Regular color="#fff" /></a></div>
                        </div>
                        <Row>
                            <div className='col-md-4 col-4'>
                                <div className='d-flex'>
                                    <div>
                                        <Text className='d-block text-light' color="grey" content="Age" size="small" timestamp />
                                        <Text className='d-block text-light' content={this.state.PatientDetails.RegistrationAge} size="medium" weight="regular" />
                                    </div>
                                </div>
                            </div>
                            <div className='col-md-4 col-4'>
                                <div className='d-flex'>
                                    <div>
                                        <Text className='d-block text-light' color="grey" content="UHID" size="small" timestamp />
                                        <Text className='d-block text-light' content={this.state.PatientDetails.UHID} size="medium" weight="regular" />
                                    </div>
                                </div>
                            </div>
                            <div className='col-md-4 col-4'>
                                <div className='d-flex'>
                                    <div>
                                        <Text className='d-block text-light' color="grey" content="DOA" size="small" timestamp />
                                        <Text className='d-block text-light' content={this.state.PatientDetails.AdmissionDate ? <Moment format='MMM Do YYYY'>{this.state.PatientDetails.AdmissionDate}</Moment>:""} size="medium" weight="regular" />
                                    </div>
                                </div>
                            </div>
                        </Row>
                    </div>
                    <Container fluid>
                        <Row>
                            <div className="col-md-4 col-12">
                            <a href='javascript:void(0)' onClick={()=>this.pushToLocationHistory('/vitals/view')}>
                                <div className='d-flex justify-content-between py-3 px-2 border-bottom'>
                                    <div className='d-flex align-items-center'><HeartPulse24Regular color="#6264A7" /><Text className='mb-0 ms-3' content="Vitals" size="large" /></div>
                                    <div className='d-flex align-items-center'><ChevronRight24Regular color="#6264A7" /></div>
                                </div>
                                </a>
                            </div>
                            <div className="col-md-4 col-12">
                            <a href='javascript:void(0)' onClick={()=>this.pushToLocationHistory('/medicalhistory/view')}>
                                <div className='d-flex justify-content-between py-3 px-2 border-bottom'>
                                    <div className='d-flex align-items-center'><Pill24Regular color="#6264A7" /><Text className='mb-0 ms-3' content="Medical History" size="large" /></div>
                                    <div><ChevronRight24Regular color="#6264A7" /></div>
                                </div>
                                </a>
                            </div>
                            <div className="col-md-4 col-12">
                            <a href='javascript:void(0)' onClick={()=>this.pushToLocationHistory('/intakeoutput/view')}>
                                <div className='d-flex justify-content-between py-3 px-2 border-bottom'>
                                    <div className='d-flex align-items-center'><ArrowSort24Regular color="#6264A7" /><Text className='mb-0 ms-3' content="Patient Intake & Output" size="large" /></div>
                                    <div><ChevronRight24Regular color="#6264A7" /></div>
                                </div>
                                </a>
                            </div>
                            <div className="col-md-4 col-12">
                            <a href='javascript:void(0)' onClick={()=>this.pushToLocationHistory('/prescription/view')}>
                                <div className='d-flex justify-content-between py-3 px-2 border-bottom'>
                                    <div className='d-flex align-items-center'><Notepad24Regular color="#6264A7" /><Text className='mb-0 ms-3' content="Prescription" size="large" /></div>
                                    <div><ChevronRight24Regular color="#6264A7" /></div>
                                </div>
                                </a>
                            </div>
                            <div className="col-md-4 col-12">
                            <a href='javascript:void(0)' onClick={()=>this.pushToLocationHistory('/doctornotes/view')}>
                                <div className='d-flex justify-content-between py-3 px-2 border-bottom'>
                                    <div className='d-flex align-items-center'><Stethoscope24Regular color="#6264A7" /><Text className='mb-0 ms-3' content="Doctor's Notes" size="large" /></div>
                                    <div><ChevronRight24Regular color="#6264A7" /></div>
                                </div>
                                </a>
                            </div>
                            <div className="col-md-4 col-12">
                            <a href='javascript:void(0)' onClick={()=>this.pushToLocationHistory('/labreport/view')}>
                                <div className='d-flex justify-content-between py-3 px-2 border-bottom'>
                                    <div className='d-flex align-items-center'><Beaker24Regular color="#6264A7" /><Text className='mb-0 ms-3' content="Lab Reports" size="large" /></div>
                                    <div><ChevronRight24Regular color="#6264A7" /></div>
                                </div>
                                </a>
                            </div>
                        </Row>
                    </Container>
                </Segment>
            </div>
        );
        }
    else{
        return <Segment><Loader></Loader></Segment>
    }
    }
    
}
export default PatientDetails;



