import React from 'react';
import { Container, Row } from 'react-bootstrap';
import {Text, Loader,Segment, Header,Button } from '@fluentui/react-northstar';
import {CalendarAdd20Regular, Clock20Regular, PersonCircle24Regular} from "@fluentui/react-icons";
import { getPatientAdmissionDetailsFromEtherAPIEndpoint } from '../../apis/APIList';
import * as microsoftTeams from "@microsoft/teams-js";
import Moment from 'react-moment';

interface IProps {
    location?:any,
    history?:any
}
interface IState {
    UHID?:any
    AdmissionId?:any,
    loading:any,
    channelId?:any,
    groupId?:any,
    PatientDetails:any,
    DepartmentId?:any,
    AdmissionList:any,
    AccessToken?:any
}

class PatientView extends React.Component<IProps, IState> {

    constructor(props: IProps) {
        super(props);
        this.state = {
            loading: true,
            PatientDetails:{},
            AdmissionList:[]
        };

    }
    componentDidMount() {
        microsoftTeams.initialize();
        microsoftTeams.getContext(context => {
            //console.log(context);
            this.setState({
                UHID:this.props.location.state.UHID,
                AdmissionId:this.props.location.state.AdmissionId,
                DepartmentId:this.props.location.state.DepartmentId,
                channelId:context.channelId,
                groupId:context.groupId,
                PatientDetails:this.props.location.state.PatientDetails,
                loading:false,
                AccessToken:this.props.location.state.AccessToken
            },
            ()=>{
                //State Call Back
                this.getPatientAdmissionDetails(this.state.UHID,this.state.AdmissionId);
            }
            );            
        });
    }
    getPatientAdmissionDetails(UHID:any,AdmissionId:any){
        getPatientAdmissionDetailsFromEtherAPIEndpoint(UHID,AdmissionId,this.state.AccessToken).then((res) => {
            console.log(res);
            if(res.data.status==="success" && res.data.body.length>0){
                this.setState({ AdmissionList: res.data.body}); 
            }
            this.setState({loading:false})
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
    onBackClick(url:any){
        this.pushToLocationHistory(url);
    }
    BuildItemDetail = (item: any) => {
        console.log(item);
        return (
            <div className="tl-item active" key={item}>
            <div className="tl-dot"></div>
            <div className="d-flex flex-column w-100">
                <div><span className="badge bg-gadient-green mb-1"><Moment format='MMM Do YYYY'>{this.state.PatientDetails.AdmissionDate}</Moment></span></div>
                <div className="card w-100 mb-2">
                    <div className="accordion accordion-flush" id={"accordionOne-"+item.AdmissionId}>
                        <div className="accordion-item">
                            <h2 className="accordion-header"  id={"headingOne-"+item.AdmissionId}>
                                <div className='d-flex justify-content-between accordion-button collapsed' data-bs-toggle="collapse" data-bs-target={"#collapseOne-"+item.AdmissionId} aria-expanded="false" aria-controls="collapseOne">
                                    <div className='d-flex align-items-center'><Clock20Regular color="" /><Text className='ms-1' content="9:30 AM" size="medium" weight="semibold" /></div>
                                </div>
                            </h2>
                            <div id={"collapseOne-"+item.AdmissionId} className="accordion-collapse collapse" aria-labelledby={"headingOne-"+item.AdmissionId} data-bs-parent={"#accordionOne-"+item.AdmissionId}>
                                <div className="accordion-body">
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Refered By:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={item.ReferedBy} size="medium" weight="regular" /></div>
                                    </Row>
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Referer Note:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={item.RefererNote} size="medium" weight="regular" /></div>
                                    </Row>
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Admission No.:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={item.Admnumber} size="medium" weight="regular" /></div>
                                    </Row>
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Admission Reason:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={item.AdmissionReason} size="medium" weight="regular" /></div>
                                    </Row>
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Admission Date:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={<Moment format='MMM Do YYYY hh:mm'>{this.state.PatientDetails.AdmissionDate}</Moment>} size="medium" weight="regular" /></div>
                                    </Row>
                                    
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Department Name:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={item.DepartmentName} size="medium" weight="regular" /></div>
                                    </Row>
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Discharge Date:" size="medium" weight="semibold" /></div>
                                            <div className='col'><Text content={this.state.PatientDetails.DischargeDate ? <Moment format='MMM Do YYYY hh:mm'>{this.state.PatientDetails.DischargeDate}</Moment> : ""} size="medium" weight="regular" /></div>
                                    </Row>
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Discharge Reason:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={item.DischargeReason} size="medium" weight="regular" /></div>
                                    </Row>
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Discharge Info:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={item.DischargeInfo} size="medium" weight="regular" /></div>
                                    </Row>
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Status:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={item.Status} size="medium" weight="regular" /></div>
                                    </Row>
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Created By:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={item.CreatedBy} size="medium" weight="regular" /></div>
                                    </Row>
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Created By Email:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={item.CreatedByEmail} size="medium" weight="regular" /></div>
                                    </Row>
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Bed Number:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={item.BedDetails.BedNumber?item.BedDetails.BedNumber:""} size="medium" weight="regular" /></div>
                                    </Row>
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Status:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={item.BedDetails.Status?item.BedDetails.Status:""} size="medium" weight="regular" /></div>
                                    </Row>
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Bed Assign Date:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={item.BedDetails.BedAssignDate?<Moment format='MMM Do YYYY hh:mm'>{item.BedDetails.BedAssignDate}</Moment>:""} size="medium" weight="regular" /></div>
                                    </Row>
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="TransferNote:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={item.BedDetails.TransferNote?item.BedDetails.TransferNote:""} size="medium" weight="regular" /></div>
                                    </Row>
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Disease:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={item.DiagnosisDetails && item.DiagnosisDetails.Disease?item.DiagnosisDetails.Disease:""} size="medium" weight="regular" /></div>
                                    </Row>
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Final Diagnosis:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={item.DiagnosisDetails &&item.DiagnosisDetails.InformationonFinalDiagnosis?item.DiagnosisDetails.InformationonFinalDiagnosis:""} size="medium" weight="regular" /></div>
                                    </Row>
                                    <Row className='d-flex justify-content-between mb-2'>
                                        <div className='col-5 col-md-2'><Text content="Diagnosis Date:" size="medium" weight="semibold" /></div>
                                        <div className='col'><Text content={item.DiagnosisDetails && item.DiagnosisDetails.Date?<Moment format='MMM Do YYYY hh:mm'>{item.DiagnosisDetails.Date}</Moment>:""} size="medium" weight="regular" /></div>
                                        
                                    </Row>

                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>                               
           
        );
      };
    render() {
        if(!this.state.loading){
            var Salutation=this.state.PatientDetails.Salutation?this.state.PatientDetails.Salutation:"";
            var PatientName=this.state.PatientDetails.PatientName?this.state.PatientDetails.PatientName:"";
            
        return (
            <div>
            <Segment>
            <div className='hero_sec py-4 px-3'>
                    <div className='d-flex justify-content-between mb-3'>
                        <div className='d-flex align-items-center'><PersonCircle24Regular color="#fff" /><Header className='mb-0 ms-1 text-light' as="h4" content={Salutation +" "+PatientName}/></div>
                    </div>
                    <Row className='mb-2'>
                        <div className='col-md-4 col-4'>
                            <div className='d-flex'>
                                <div>
                                    <Text className='d-block text-light' color="grey" content="Age" timestamp />
                                    <Text className='d-block text-light' content={this.state.PatientDetails.RegistrationAge} size="medium" weight="semibold" />
                                </div>
                            </div>
                        </div>
                        <div className='col-md-4 col-4'>
                            <div className='d-flex'>
                                <div>
                                    <Text className='d-block text-light' color="grey" content="UHID"  timestamp />
                                    <Text className='d-block text-light' content={this.state.PatientDetails.UHID}  size="medium" weight="semibold" />
                                </div>
                            </div>
                        </div>
                        <div className='col-md-4 col-4'>
                            <div className='d-flex'>
                                <div>
                                    <Text className='d-block text-light' color="grey" content="DOA"  timestamp />
                                        <Text className='d-block text-light' content={this.state.PatientDetails.AdmissionDate ? <Moment format='MMM Do YYYY'>{this.state.PatientDetails.AdmissionDate}</Moment>:""} size="medium" weight="semibold" />
                                </div>
                            </div>
                        </div>
                    </Row>
                    <Row className='mb-2'>
                        <div className='col-md-4 col-4'>
                            <div className='d-flex'>
                                <div>
                                    <Text className='d-block text-light' color="grey" content="Sex"  timestamp />
                                    <Text className='d-block text-light' content={this.state.PatientDetails.Sex} size="medium" weight="semibold" />
                                </div>
                            </div>
                        </div>
                        <div className='col-md-4 col-4'>
                            <div className='d-flex'>
                                <div>
                                    <Text className='d-block text-light' color="grey" content="Blood Group"  timestamp />
                                    <Text className='d-block text-light' content={this.state.PatientDetails.BloodGroup} size="medium" weight="semibold" />
                                </div>
                            </div>
                        </div>
                        <div className='col-md-4 col-4'>
                            <div className='d-flex'>
                                <div>
                                    <Text className='d-block text-light' color="grey" content="DOB"  timestamp />
                                    <Text className='d-block text-light' content={<Moment format='MMM Do YYYY'>{this.state.PatientDetails.DateOfBirth}</Moment>}  size="medium" weight="semibold" />
                                </div>
                            </div>
                        </div>
                    </Row>
                    <Row>
                        <div className='col-md-4 col-8'>
                            <div className='d-flex'>
                                <div>
                                    <Text className='d-block text-light' color="grey" content="Email" timestamp />
                                    <Text className='d-block text-light' content={this.state.PatientDetails.Email?this.state.PatientDetails.Email:""} size="medium" weight="semibold" />
                                </div>
                            </div>
                        </div>
                        <div className='col-md-4 col-4'>
                            <div className='d-flex'>
                                <div>
                                    <Text className='d-block text-light' color="grey" content="Phone Number" timestamp />
                                    <Text className='d-block text-light' content={this.state.PatientDetails.PhoneNumber} size="medium" weight="semibold" />
                                </div>
                            </div>
                        </div>
                    </Row>
                   
                </div>
                <Container fluid className='my-3'>
                    <div className='d-flex justify-content-start align-items-center mb-3'>
                        <CalendarAdd20Regular />
                        <Text className='ms-2' color="grey" size='medium' content="Admission Details" />
                    </div>                    
                    <div className="timeline block my-4">                        
                    {this.state.AdmissionList.map((item: any) => this.BuildItemDetail(item))}
                        <div className="tl-item">
                            <div className="tl-dot"></div>
                            <div className="d-flex flex-column w-100">
                                <div><span className="badge bg-gadient-grey mb-1">No further history</span></div>
                             </div>
                        </div>
                    </div>
                    <div className='py-3 d-flex justify-content-end'>
                    <Button content="Back"  primary onClick={()=>this.onBackClick('/patients/details')} />
                        
                    </div>
                </Container>
                
                </Segment>
            </div>
           
        );
        }
        else{
            return(    
                <Segment>      
            <Loader ></Loader>
            </Segment>  
            );
        }
    }
}
export default PatientView;



