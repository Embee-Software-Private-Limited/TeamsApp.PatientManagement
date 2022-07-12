import React from 'react';
import { Container, Row } from 'react-bootstrap';
import { ReactComponent as Jotaro } from "../../images/addVitalbg.svg";
import {
    Text,
    Segment,
    Loader
} from '@fluentui/react-northstar';

import {
    Clock20Regular,
    ChevronLeft24Regular,
    Chat20Regular,
    Call20Regular
} from "@fluentui/react-icons";
import Moment from 'react-moment';
import * as microsoftTeams from "@microsoft/teams-js";
import { getMedicalHistoryFromEtherAPIEndpoint, getPatientsDetailsFromEtherAPIEndpoint } from '../../apis/APIList';
import moment from 'moment';
import { Persona,PersonaSize } from '@fluentui/react';
import PatientPrimaryDetails from '../Interfaces/patientPrimaryDetails';


interface IProps {
    history?:any,
    location?:any
}

interface IState {
    medicalHistoryList: any;
    timeLineDatesList: any;
    loading:boolean,
    PatientDetails?:any,
    patientPrimaryDetails: any,
    UHID?:any
    AdmissionId?:any,
    DepartmentId?:any,
    LoggedInUserEmail:any,
    LoggedInUserUPN:any,
    AccessToken?:any
}

class ViewMedicalHistory extends React.Component<IProps, IState> {

    constructor(props: IProps) {
        super(props);
        this.state = {
            loading: true,
            medicalHistoryList:[],
            timeLineDatesList: [],
            LoggedInUserEmail:"",
            LoggedInUserUPN:"",
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
                },
                resources: []
            };
            microsoftTeams.authentication.getAuthToken(authTokenRequest);     
        }
        //this.onPageLoad(context);
        });

    }
    onPageLoad(context:any){
        const queryString=window.location.search;
            if(queryString!==""){
                const urlParams = new URLSearchParams(queryString);
                const UHID = urlParams.get('UHID');
                const AdmissionId = urlParams.get('AdmissionId');
                const DepartmentId=urlParams.get('DepartmentId');
                if(UHID!==null && UHID!=="" && AdmissionId!==null && AdmissionId!==""  && DepartmentId!==null && DepartmentId!==""){
                     
                    this.setState({
                        UHID:UHID,
                        AdmissionId:AdmissionId,
                        DepartmentId:DepartmentId,
                        LoggedInUserEmail:context.userPrincipalName,
                        LoggedInUserUPN:context.userPrincipalName
                    });
                    this.loadMedicalHistory(UHID,AdmissionId);
                    this.getPatientDetails(UHID);
                }               
            }
            else{

                this.setState({
                    UHID:this.props.location.state.UHID,
                    AdmissionId:this.props.location.state.AdmissionId,
                    DepartmentId:this.props.location.state.DepartmentId,
                    LoggedInUserEmail:context.userPrincipalName,
                    LoggedInUserUPN:context.userPrincipalName,
                    PatientDetails:this.props.location.state.PatientDetails,
                    patientPrimaryDetails:this.props.location.state.PatientDetails,
                },
                ()=>{
                    this.loadMedicalHistory(this.state.UHID,this.state.AdmissionId);
                    //this.getPatientDetails(this.state.UHID);
                }
                );
            }
    }
    loadMedicalHistory = (UHID:any,AdmissionId:any) => {
        
        this.setState({loading:true});
        getMedicalHistoryFromEtherAPIEndpoint(UHID,AdmissionId,this.state.AccessToken).then((res) => {
        //console.log(res.data );
        this.setState({loading:false});
          if (res.data.status=="success") {
            this.setState({ medicalHistoryList: res.data.body, timeLineDatesList:res.data.timeLineDates });
          }
        });
      };
      getPatientDetails(UHID:any){
        getPatientsDetailsFromEtherAPIEndpoint(UHID,this.state.AccessToken).then((res) => {
            if(res.data.status==="success" && res.data.body.length>0){
                const responseData=res.data.body[0];
                this.setState({ PatientDetails: responseData, patientPrimaryDetails: responseData}); 
            }
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
     //In no record exists then render this content
     NoRecordFoundContent(){
        return (
                <Container fluid>
                    <div className='d-flex justify-content-between align-items-center my-3'>
                        <div><a href='javascript:void(0)' onClick={()=>this.pushToLocationHistory('/patients/details')}><ChevronLeft24Regular /></a></div>
                        <div className='d-flex'><Text className='ms-2' content="Medical History" size="medium" weight="semibold" /></div>
                        <div></div>
                    </div>
                    <div className='mx-3'>
                        <PatientPrimaryDetails patientPrimaryDetails={this.state.patientPrimaryDetails}/>
                    </div>
                    <div className='mt-5 text-center'>
                        <div><Jotaro  height={150}/></div>
                        <Text className='d-block mb-2' content="No Medical History added" size="large" weight="bold" />
                        <Text className='d-block' content="Medical History of the patient show up here" size="medium" weight="regular" />
                       
                    </div>
                </Container>
               
        )
     }
     onChatClick(item:any){
        var user1=this.state.LoggedInUserUPN;
        var user2=item.ModifiedByEmail;
        var patientName=this.state.PatientDetails?this.state.PatientDetails.PatientName:"";
        var message="Discussion regarding the patient, Patient Name : "+patientName+", UHID : "+this.state.UHID;        
        var topic="Patient Medical History";
        var link="https://teams.microsoft.com/l/chat/0/0?users="+user1+","+user2+"&topicName="+topic+"&message="+message;
        this.executeDeepLink(link);
    }
    onCallClick(item:any){
        var user1=this.state.LoggedInUserUPN;
        var user2=item.ModifiedByEmail;
        var link="https://teams.microsoft.com/l/call/0/0?users="+user2;
        this.executeDeepLink(link);
    }
    executeDeepLink(deepLink:any){
        microsoftTeams.executeDeepLink(deepLink);
    }
     formateTimeLineDate=(date:any)=>{
        var today = moment().endOf('day').format('DD MMMM YYYY');
        var mydate = moment(date).format('DD MMMM YYYY');
        var yesterday = new Date();
        var yesterdayText=moment(yesterday.setDate((new Date()).getDate()-1)).format('DD MMMM YYYY');
     

        if (mydate==today) return 'Today'
        if (mydate==yesterdayText) return 'Yesterday'
        return moment(date).format('DD MMMM YYYY');        
         
     }
     BuildItemDetailDateWise = (date: any) => {
        return (
        <div className="tl-item active">
        <div className="tl-dot"></div>
        <div className="d-flex flex-column w-100">
            <div><span className="badge bg-gadient-green mb-1">{this.formateTimeLineDate(date)}</span></div>            
            {this.state.medicalHistoryList.filter((x:any)=>moment(x.ModifiedOn).format('L')===moment(date).format('L')).map((item: any) => this.BuildItemDetail(item))}
            </div>
        </div>
        )
     };

     BuildItemDetail = (item: any) => {
        return (
            
                
                <div className="card w-100">
                    <div className="accordion accordion-flush" id={"accordionOne-"+item.DoctorNoteId}>
                        <div className="accordion-item">
                            <h2 className="accordion-header" id={"headingOne-"+item.DoctorNoteId}>
                                <div className='d-flex justify-content-between align-items-center accordion-button collapsed' data-bs-toggle="collapse" data-bs-target={"#collapseOne-"+item.DoctorNoteId} aria-expanded="false" aria-controls={"collapseOne-"+item.DoctorNoteId} >
                                    <div className='d-flex align-items-center'><Clock20Regular color="" /><Text className='ms-1' content={<Moment format='h:mm:ss a'>{item.ModifiedOn}</Moment>} size="medium" weight="semibold" /></div>                                    
                                </div>
                            </h2>
                            <div id={"collapseOne-"+item.DoctorNoteId} className="accordion-collapse collapse" aria-labelledby={"headingOne-"+item.DoctorNoteId} data-bs-parent={"#accordionOne-"+item.DoctorNoteId}>
                                <div className="accordion-body">
                                    <Row>
                                        <div className='col-12'>
                                            <div>
                                                <Text className='d-block' content={item.Description} size="medium" />
                                            </div>
                                        </div>
                                    </Row>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div className="card-body">
                        <div className='d-flex justify-content-between align-items-center'>
                            <div className='mw-50'><Persona text={item.ModifiedBy} size={PersonaSize.size24} /></div>

                            <div>
                                <a href="javascript:void(0)" onClick={()=>this.onChatClick(item)} ><Chat20Regular color='#6264a7' /></a> 
                                <a href="javascript:void(0)"  onClick={()=>this.onCallClick(item)} ><Call20Regular color='#6264a7' className='ms-2'  /></a>
                               
                            </div>
                        </div>
                    </div>
                </div>                                
           
        );
      };
     //In  record exists then render this content
    RecordFoundContent(){
        return (
            <Container fluid>
                    <div className='d-flex justify-content-between align-items-center my-3'>
                    <div><a href='javascript:void(0)' onClick={()=>this.pushToLocationHistory('/patients/details')}><ChevronLeft24Regular /></a></div>
                        <div className='d-flex'><Text className='ms-2' content="Medical History" size="medium" weight="semibold" /></div>
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
            const isRecordExists = this.state.medicalHistoryList.length > 0 ? true : false;;
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
export default ViewMedicalHistory;



