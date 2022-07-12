import React from 'react';
import { Container, Row } from 'react-bootstrap';
import {Text, FormInput, Button,Loader,Segment} from '@fluentui/react-northstar';
import {ChevronLeft24Regular} from "@fluentui/react-icons";
import { postAddUpdateVitalsAPIEndpoint } from '../../apis/APIList';
import * as microsoftTeams from "@microsoft/teams-js";
import { MockLoginUsersDetails } from '../../apis/AppConfiguration';
import PatientPrimaryDetails from '../Interfaces/patientPrimaryDetails';

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
    VitalDetails?:any,
    DepartmentId?:any,
    submitLoading:any,
    errorMessage:any,
    PatientDetails?:any,
    patientPrimaryDetails: any,
    SubmitButtonText:any,
    PageHeadding:any,
    AccessToken?:any
}

class AddVitals extends React.Component<IProps, IState> {

    constructor(props: IProps) {
        super(props);
        this.state = {
            loading: true,
            submitLoading:false,
            errorMessage:'',
            SubmitButtonText:"Add Vitals",
            PageHeadding:"Add Vitals",
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
        // console.log(context);
        var vitalDetails:any={};
        if(this.props.location.state.VitalDetails!=null){
            vitalDetails=this.props.location.state.VitalDetails;
            this.setState({SubmitButtonText:"Update Vitals",PageHeadding:"Edit Vitals"});
        }
        else{
            vitalDetails={
                VitalId:"",
                Temp:"",
                Weight:"",
                Height:"",
                PulseRate:"",
                BPDiastolic:"",
                BPSystolic:"",
                RR:"",
                Spo2:"",
                Description:""
            }
        }

        this.setState({
            UHID:this.props.location.state.UHID,
            AdmissionId:this.props.location.state.AdmissionId,
            DepartmentId:this.props.location.state.DepartmentId,
            PatientDetails:this.props.location.state.PatientDetails,
            patientPrimaryDetails:this.props.location.state.PatientDetails,
            channelId:context.channelId,
            groupId:context.groupId,
            VitalDetails:vitalDetails
            
        },
        ()=>{
           // this.getPatientDetails(this.state.UHID)
           this.setState({loading:false});
        }
        ); 
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
    submitHandler = () => {   
        this.setState({errorMessage:''});
        //Check Mandatory field
       
        //Check BMI
        if(this.state.VitalDetails.Temp===""){
            this.setState({errorMessage:'Please enter Temp'});
            return;
        }
        if(this.state.VitalDetails.PulseRate===""){
            this.setState({errorMessage:'Please enter Pulse Rate'});
            return;
        }
        if (this.state.VitalDetails.RR === "") {
            this.setState({ errorMessage: 'Please enter RR' });
            return;
        }

        this.setState({ submitLoading: true });
        microsoftTeams.getContext(context => { 
            this.setState({submitLoading:true});
            const dataObjects = {
                TeamId:this.state.groupId,
                ChannelId:this.state.channelId, 
                UHID:this.state.UHID,
                AdmissionId:this.state.AdmissionId,
                DepartmentId:this.state.DepartmentId,
                CreatedBy:context.userPrincipalName,
                CreatedByEmail:MockLoginUsersDetails.MockUserEmail?MockLoginUsersDetails.MockUserEmail:context.userPrincipalName,
                Name:context.userPrincipalName,
                VitalDetails:this.state.VitalDetails
            };    
            //console.log(dataObjects);    
            postAddUpdateVitalsAPIEndpoint(dataObjects,this.state.AccessToken).then((res: any) => {
            //console.log("save vitals", res.data);
                this.setState({submitLoading:false});
                if(res.data===true){
                    this.pushToLocationHistory('/vitals/view');
                }
                else{
                    this.setState({errorMessage:"Sorry unable to save your data."});
                }
            //this.pushToLocationHistory('/vitals/view');
            });
        });
    };
    handleFormFieldChanged(event:any,fieldName:any) {
        // Extract the current value of the customer from state
        var vital = this.state.VitalDetails;
     
        // Extract the value of the input element represented by `target`
        var modifiedValue = event.target.value;
        switch(fieldName){
            case "Temp":
                vital.Temp = modifiedValue;                 
            break;
            case "Weight":
                vital.Weight = modifiedValue;              
            break;
            case "Height":
                vital.Height = modifiedValue;               
            break;
            case "PulseRate":
                vital.PulseRate = modifiedValue;              
            break;
            case "BPDiastolic":
                vital.BPDiastolic = modifiedValue;             
            break;
            case "BPSystolic":
                vital.BPSystolic = modifiedValue;                 
            break;
            case "RR":
                vital.RR = modifiedValue;                  
            break;
            case "Spo2":
                vital.Spo2 = modifiedValue;               
            break;
            case "Description":
                vital.Description = modifiedValue;                
            break;
                       
        }   
        this.setState({VitalDetails: vital});        
    }
    ErrorMessageContent(){
        if(this.state.errorMessage.length>0){
            return(
                <div className='py-3 d-flex justify-content-end'  >
                    <p className="error-message-save">{this.state.errorMessage}</p>
                </div>
            );
        }        
    }
    BuildPatientPrimaryDetails = () => {
        if(this.state.patientPrimaryDetails){
        return (
            <PatientPrimaryDetails patientPrimaryDetails={this.state.patientPrimaryDetails}/>           
        );
        }
     }; 
    render() {
        if(!this.state.loading){
        return (
            <div>
                <Segment>
                <Container fluid>
                    <div className='d-flex justify-content-between align-items-center my-3'>
                    <div><a href='javascript:void(0);' onClick={()=>this.onBackClick('/vitals/view')}><ChevronLeft24Regular /></a></div>
                        <div className='d-flex'><Text className='ms-2' content={this.state.PageHeadding} size="medium" weight="semibold" /></div>
                        <div></div>
                    </div>
                    {this.BuildPatientPrimaryDetails()}
                    <Row className="mt-3">
                        <div className="col-md-4 col-6">
                            <div className='mb-2'>
                                <Text className='mb-1 p-0 d-block' color="grey" content="Temp" />
                                <FormInput fluid name="Temp"  value={this.state.VitalDetails.Temp} onChange={(event)=>this.handleFormFieldChanged(event,"Temp")}/>
                            </div> 
                        </div>
                        <div className="col-md-4 col-6"> 
                            <div className='mb-2'>
                                <Text className='mb-1 p-0 d-block' color="grey" content="Weight" size="small" timestamp />
                                <FormInput fluid name="" maxLength={10} value={this.state.VitalDetails.Weight} onChange={(event)=>this.handleFormFieldChanged(event,"Weight")}/>
                            </div>
                        </div>
                        <div className="col-md-4 col-6">
                            <div className='mb-2'>
                                <Text className='mb-1 p-0 d-block' color="grey" content="Height" size="small" timestamp />
                                <FormInput fluid name="" maxLength={10} value={this.state.VitalDetails.Height} onChange={(event)=>this.handleFormFieldChanged(event,"Height")}/>
                            </div>
                        </div>
                        <div className="col-md-4 col-6">
                            <div className='mb-2'>
                                <Text className='mb-1 p-0 d-block' color="grey" content="Pulse Rate" size="small" timestamp />
                                <FormInput fluid name="PulseRate" maxLength={10} value={this.state.VitalDetails.PulseRate} onChange={(event)=>this.handleFormFieldChanged(event,"PulseRate")}/>
                            </div>
                        </div>                        
                        <div className="col-md-4 col-6">
                            <div className='mb-2'>
                                <Text className='mb-1 p-0 d-block' color="grey" content="BP Systolic" size="small" timestamp />
                                <FormInput fluid name="BPSystolic"  maxLength={10} value={this.state.VitalDetails.BPSystolic} onChange={(event)=>this.handleFormFieldChanged(event,"BPSystolic")}/>
                            </div>
                            </div>
                            <div className="col-md-4 col-6">
                                <div className='mb-2'>
                                    <Text className='mb-1 p-0 d-block' color="grey" content="BP Diastolic" size="small" timestamp />
                                    <FormInput fluid name="BPDiastolic" maxLength={10} value={this.state.VitalDetails.BPDiastolic} onChange={(event) => this.handleFormFieldChanged(event, "BPDiastolic")} />
                                </div>
                            </div>
                        <div className="col-md-4 col-6">
                            <div className='mb-2'>
                                <Text className='mb-1 p-0 d-block' color="grey" content="RR" size="small" timestamp />
                                <FormInput fluid name="RR"  maxLength={10} value={this.state.VitalDetails.RR} onChange={(event)=>this.handleFormFieldChanged(event,"RR")}/>
                            </div>
                        </div>
                        <div className="col-md-4 col-6">
                            <div className='mb-2'>
                                <Text className='mb-1 p-0 d-block' color="grey" content="Spo2" size="small" timestamp />
                                <FormInput fluid name="Spo2" maxLength={10} value={this.state.VitalDetails.Spo2} onChange={(event)=>this.handleFormFieldChanged(event,"Spo2")}/>
                            </div>
                        </div>
                        <div className="col-md-12 col-12">
                            <div className='mb-2'>
                                <Text className='mb-1 p-0 d-block' color="grey" content="Description" size="small" timestamp />
                                <FormInput fluid aria-multiline name="Description"  value={this.state.VitalDetails.Description} onChange={(event)=>this.handleFormFieldChanged(event,"Description")}/>
                            </div>
                        </div>                        
                    </Row>
                   {this.ErrorMessageContent()}
                    <div className='py-3 d-flex justify-content-end'>
                        <Button content="Back" secondary onClick={()=>this.onBackClick('/vitals/view')} />
                        <Button className='ms-2' disabled={this.state.submitLoading} content={this.state.submitLoading?<Loader label="Submitting Data.." labelPosition='end'></Loader>:this.state.SubmitButtonText} primary onClick={()=>this.submitHandler()}/>

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
export default AddVitals;



