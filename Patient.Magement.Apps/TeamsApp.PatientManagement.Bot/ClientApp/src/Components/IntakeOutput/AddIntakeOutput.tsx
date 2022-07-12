import React from 'react';
import { Container, Row } from 'react-bootstrap';
import moment from 'moment';
import {
    Text,
    FormInput,
    Button,
    Datepicker,
    FormDropdown,
    Flex,
    Loader,
    Segment
} from '@fluentui/react-northstar';
import { getPatientsDetailsFromEtherAPIEndpoint } from '../../apis/APIList';
import PatientPrimaryDetails from '../Interfaces/patientPrimaryDetails';

import {
    ChevronLeft24Regular
} from "@fluentui/react-icons";
import * as microsoftTeams from "@microsoft/teams-js";
import { MockLoginUsersDetails } from '../../apis/AppConfiguration';
import { postAddUpdateIntakeAndOutputAPIEndpoint } from '../../apis/APIList';


const HoursList = ['00','01','02','03','04','05','06','07', '08', '09', '10', '11','12','13','14','15','16','17','18','19','20','21','22','23'];
const MinutesList = ['00', '01', '02', '03', '04', '05', '06', '07', '08', '09', '10', '11', '12', '13', '14', '15', '16', '17', '18', '19', '20', '21', '22', '23', '24', '25', '26', '27', '28', '29', '30', '31', '32', '33', '34', '35', '36', '37', '38', '39', '40', '41', '42', '43', '44', '45', '46', '47', '48', '49', '50', '51', '52', '53', '54', '55', '56', '57', '58', '59']
interface IProps {
    location?: any,
    history?: any
}

interface IState {
    UHID?: any
    AdmissionId?: any,
    loading: any,
    channelId?: any,
    groupId?: any,
    PatientDetails?: any,
    patientPrimaryDetails: any,
    DepartmentId?: any,
    submitLoading: any,
    errorMessage: any,
    IntakeOutPutDetails?: any,
    SubmitButtonText: any,
    PageHeadding: any,
    Date:string,
    Hour:string,
    Minute:string,
    DefaultDate?:any,
    disableSubmit?:boolean,
    AccessToken?:any
}

class AddIntakeOutput extends React.Component<IProps, IState> {
    constructor(props: IProps) {
        super(props);
        this.state = {
            loading: true,
            submitLoading: false,
            errorMessage: '',
            SubmitButtonText: "Add Intake/Output",
            PageHeadding: "Add Intake/Output",
            Date:'',
            Hour:'',
            Minute:'',
            patientPrimaryDetails: {},
            disableSubmit:true
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
        var details: any = {};
            if (this.props.location.state.IntakeOutPutDetails != null) {
                details = this.props.location.state.IntakeOutPutDetails;
                //console.log('add intake output details from props:',details);
                this.setState({ SubmitButtonText: "Update Intake/Output", PageHeadding: "Edit Intake/Output" });
            }
            else {
                details = {
                    Id: "",
                    Description: "",
                    OutputType: "",
                    fluid_in0: "",
                    fluid_out0: "",
                    fluid_bal: "",
                    DateAdded: new Date()
                }
            }

            this.setState({
                UHID: this.props.location.state.UHID,
                AdmissionId: this.props.location.state.AdmissionId,
                DepartmentId: this.props.location.state.DepartmentId,
                PatientDetails: this.props.location.state.PatientDetails,
                patientPrimaryDetails: this.props.location.state.PatientDetails,
                channelId: context.channelId,
                groupId: context.groupId,
                IntakeOutPutDetails: details,
                Date:moment(details.DateAdded).format('YYYY-MM-DD'),
                Hour:moment(details.DateAdded).format('HH'),
                Minute:moment(details.DateAdded).format('mm'),
                DefaultDate:new Date(details.DateAdded),  
                loading: false
            }, () => { 
                //this.getPatientDetails(this.state.UHID) 
            } );
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
    onBackClick(url: any) {
        this.pushToLocationHistory(url);
    }
    submitHandler = () => {
        this.setState({ errorMessage: '' });
        //Check Mandatory field
        //Check BMI
        if (this.state.IntakeOutPutDetails.fluid_in0 === "" || this.state.IntakeOutPutDetails.fluid_out0 ==="" || this.state.IntakeOutPutDetails.fluid_bal==="" || this.state.IntakeOutPutDetails.Description === "") {
            this.setState({ errorMessage: 'Please enter intake' });
            return;
        }

       
            this.addIntakeOutputApiCall()
        
        
        
    };

    addIntakeOutputApiCall = () => {
        const formatedDate = this.state.Date + 'T' + this.state.Hour + ':' + this.state.Minute + ':00';
        //console.log(formatedDate);
        this.setState({errorMessage:""});
        microsoftTeams.getContext(context => {
            var IntakeOutPutDetailsObj = this.state.IntakeOutPutDetails;
            IntakeOutPutDetailsObj.DateAdded = formatedDate;

            this.setState({ submitLoading: true, IntakeOutPutDetails: IntakeOutPutDetailsObj});
            const dataObjects = {
                TeamId: this.state.groupId,
                ChannelId: this.state.channelId,
                UHID: this.state.UHID,
                AdmissionId: this.state.AdmissionId,
                DepartmentId: this.state.DepartmentId,
                CreatedBy: context.userPrincipalName,
                CreatedByEmail: MockLoginUsersDetails.MockUserEmail ? MockLoginUsersDetails.MockUserEmail : context.userPrincipalName,
                CreatedOn: formatedDate,
                Name: context.userPrincipalName,
                IntakeOutPutDetails: this.state.IntakeOutPutDetails
            };
            //console.log(dataObjects);
            postAddUpdateIntakeAndOutputAPIEndpoint(dataObjects,this.state.AccessToken).then((res: any) => {
               // console.log("save intake/output", res.data);
                this.setState({ submitLoading: false });
                if (res.data === true) {
                    this.pushToLocationHistory('/intakeoutput/view');
                }
                else {
                    this.setState({errorMessage:"Sorry unable to save your data."});
                }
                //this.pushToLocationHistory('/vitals/view');
            });
        });
    }

    handleFormFieldChanged(event: any, fieldName: any) {
        // Extract the current value of the customer from state
        var details = this.state.IntakeOutPutDetails;
        details.DateAdded=new Date();
        // Extract the value of the input element represented by `target`
        var modifiedValue = event.target.value;
        switch (fieldName) {
            case "fluid_in0":
                details.fluid_in0 = modifiedValue;
                break;
            case "fluid_out0":
                details.fluid_out0 = modifiedValue;
                break;
            case "fluid_bal":
                details.fluid_bal = modifiedValue;
                break;
            case "Description":
                details.Description = modifiedValue;
                break;
        }
        this.setState({ IntakeOutPutDetails: details });
    }
    ErrorMessageContent() {
        if (this.state.errorMessage.length > 0) {
            return (
                <div className='py-3 d-flex justify-content-end'  >
                    <p className="error-message-save">{this.state.errorMessage}</p>
                </div>
            );
        }
    }
    render() {
        if (!this.state.loading) {
            return (
                <div>
                    <Segment>
                        <Container fluid>
                            <div className='d-flex justify-content-between align-items-center my-3'>
                                <div><a href='javascript:void(0);' onClick={() => this.onBackClick('/intakeoutput/view')}><ChevronLeft24Regular /></a></div>

                                <div className='d-flex'><Text className='ms-2' content={this.state.PageHeadding} size="medium" weight="semibold" /></div>
                                <div></div>
                            </div>
                            <PatientPrimaryDetails patientPrimaryDetails={this.state.patientPrimaryDetails}/>
                            <div>
                                <Row className='mt-3'>
                                    <div className="col-md-12 col-12">
                                        <div className='mb-2'>
                                            <Text className='mb-1 p-0 d-block'  content="Intake" />
                                            <FormInput fluid name="fluid_in0" value={this.state.IntakeOutPutDetails.fluid_in0} onChange={(event) => this.handleFormFieldChanged(event, "fluid_in0")} />
                                        </div>
                                    </div>
                                    <div className="col-md-12 col-12">
                                        <div className='mb-2'>
                                            <Text className='mb-1 p-0 d-block' content="Output" />
                                            <FormInput fluid name="fluid_out0" value={this.state.IntakeOutPutDetails.fluid_out0} onChange={(event) => this.handleFormFieldChanged(event, "fluid_out0")} />
                                        </div>
                                    </div>
                                    <div className="col-md-12 col-12">
                                        <div className='mb-2'>
                                            <Text className='mb-1 p-0 d-block'  content="Balance" />
                                            <FormInput fluid name="fluid_bal"  value={this.state.IntakeOutPutDetails.fluid_bal} onChange={(event) => this.handleFormFieldChanged(event, "fluid_bal")} />
                                        </div>
                                    </div>
                                    <div className="col-md-12 col-12">
                                        <div className='mb-2'>
                                            <Text className='mb-1 p-0 d-block'  content="Description" />
                                            <FormInput fluid name="Description" value={this.state.IntakeOutPutDetails.Description} onChange={(event) => this.handleFormFieldChanged(event, "Description")} />
                                        </div>
                                    </div>                                    
                                </Row>
                            </div>
                            <div className='mb-2'>
                                <Row>
                                    <div className='col-md-4 mb-4 '>
                                    <Row>
                                        <div className='col-6 mb-2'>
                                                    <Text className='mb-1 p-0 d-block' color="grey" content="Date" size="small" timestamp />
                                                    <Datepicker
                                                        allowManualInput={false}
                                                        className='pikdate mb-2'
                                                        inputOnly                                                        
                                                        defaultSelectedDate={this.state.DefaultDate?this.state.DefaultDate:new Date()}  
                                                        maxDate={new Date()}
                                                        onDateChange={(e:any, data:any)=>this.setState({Date:moment(data.value).format('YYYY-MM-DD')})}
                                                    />
                                                </div>

                                                <div className='col-3'>
                                                    <Text className='mb-1 p-0 d-block' color="grey" content="Hour" size="small" timestamp />
                                                    <FormDropdown
                                                    className='overflow_off'
                                                        items={HoursList}
                                                        placeholder={this.state.Hour}
                                                        checkable
                                                        fluid
                                                        onChange={(e:any,data:any)=> this.setState({Hour:data.value})}
                                                    />
                                                </div>

                                                <div className='col-3'>
                                                    <Text className='mb-1 p-0 d-block' color="grey" content="Minute" size="small" timestamp />
                                                    <FormDropdown
                                                    className='overflow_off'
                                                        items={MinutesList}
                                                        placeholder={this.state.Minute}
                                                        checkable
                                                        fluid
                                                        onChange={(e:any,data:any)=> this.setState({Minute:data.value})}
                                                    />
                                                </div>
                                        </Row>
                                    </div>
                                </Row>
                            </div>
                            {this.ErrorMessageContent()}
                            <div className='py-3 d-flex justify-content-end'>
                                <Button content="Back" secondary onClick={() => this.onBackClick('/intakeoutput/view')} />
                                <Button className='ms-2' disabled={this.state.submitLoading} content={this.state.submitLoading ? <Loader label="Submitting Data.." labelPosition='end'></Loader> : this.state.SubmitButtonText} primary onClick={() => this.submitHandler()} />

                            </div>
                        </Container>
                    </Segment>
                </div>
            );
        }
        else {
            return (
                <Segment>
                    <Loader ></Loader>
                </Segment>
            );
        }
    }
}

export default AddIntakeOutput;