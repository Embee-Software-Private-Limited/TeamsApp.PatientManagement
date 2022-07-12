import React from 'react';
import { Container,Row } from 'react-bootstrap';
import { Text, Button, Loader, Datepicker, FormDropdown} from '@fluentui/react-northstar';
import { ChevronLeft24Regular} from '@fluentui/react-icons';
import moment from 'moment';
import * as microsoftTeams from "@microsoft/teams-js";
import { getPatientsDetailsFromEtherAPIEndpoint } from '../../apis/APIList';
import PatientPrimaryDetails from '../Interfaces/patientPrimaryDetails';
import { postAddUpdateDoctorNotesAPIEndpoint } from '../../apis/APIList';

//Editor
import { EditorState, convertToRaw, ContentState } from 'draft-js';
import { Editor } from "react-draft-wysiwyg";
import draftToHtml from 'draftjs-to-html';
// import htmlToDraft from 'html-to-draftjs';
import "../../../node_modules/react-draft-wysiwyg/dist/react-draft-wysiwyg.css";
import htmlToDraft from 'html-to-draftjs';
import { MockLoginUsersDetails } from '../../apis/AppConfiguration';

const HoursList = ['00','01','02','03','04','05','06','07', '08', '09', '10', '11','12','13','14','15','16','17','18','19','20','21','22','23'];
const MinutesList = ['00', '01', '02', '03', '04', '05', '06', '07', '08', '09', '10', '11', '12', '13', '14', '15', '16', '17', '18', '19', '20', '21', '22', '23', '24', '25', '26', '27', '28', '29', '30', '31', '32', '33', '34', '35', '36', '37', '38', '39', '40', '41', '42', '43', '44', '45', '46', '47', '48', '49', '50', '51', '52', '53', '54', '55', '56', '57', '58', '59']

interface IProps {
    location?:any,
    history?:any
}

interface IState {
    UHID?:any
    AdmissionId?:any,
    DepartmentId?:any,
    loading:any,
    channelId?:any,
    groupId?:any,
    editorState?:any,
    Description?:any,
    submitLoading:any,
    errorMessage:any,
    PatientDetails?:any,
    patientPrimaryDetails:any,
    SubmitButtonText:any,
    PageHeadding:any,
    DoctorNoteId?:any,
    Date:string,
    Hour:string,
    Minute:string,
    DefaultDate?:any,
    AccessToken?:any
}

class AddDoctorNotes extends React.Component<IProps, IState> {
    constructor(props: IProps) {
        super(props);
        this.state = {
            loading: false,
            editorState: EditorState.createEmpty(),
            submitLoading:false,
            errorMessage:'',
            SubmitButtonText:"Add Doctor's Note",
            PageHeadding:"Add Doctor's Note",
            DoctorNoteId:0,
            Date:moment().format('YYYY-MM-DD'),
            Hour:moment().format('HH'),
            Minute:moment().format('mm'),
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
        console.log('hi');
            var pHead="Add Doctor's Note";
            var butHead="Add Doctor's Note"
            if(this.props.location.state.DoctorNotesDetails!=null){
                pHead="Update Doctor's Note";
                butHead="Update Doctor's Note";
                console.log(this.props.location.state.DoctorNotesDetails.DoctorNoteDate);
                this.setState({Description:this.props.location.state.DoctorNotesDetails.Description,
                    DoctorNoteId:this.props.location.state.DoctorNotesDetails.DoctorNoteId,
                    Date:moment(this.props.location.state.DoctorNotesDetails.DoctorNoteDate).format('YYYY-MM-DD'),
                    Hour:moment(this.props.location.state.DoctorNotesDetails.DoctorNoteDate).format('HH'),
                    Minute:moment(this.props.location.state.DoctorNotesDetails.DoctorNoteDate).format('mm'),
                    DefaultDate:new Date(this.props.location.state.DoctorNotesDetails.DoctorNoteDate)                   
                }, ()=>this.setEditorState());
               
            }
            this.setState({
                UHID:this.props.location.state.UHID,
                AdmissionId:this.props.location.state.AdmissionId,
                DepartmentId:this.props.location.state.DepartmentId,
                channelId:context.channelId,
                groupId:context.groupId, 
                loading:false,
                SubmitButtonText:butHead,
                PageHeadding:pHead,
                DefaultDate:this.state.DefaultDate?this.state.DefaultDate:new Date(),
                PatientDetails: this.props.location.state.PatientDetails,
                patientPrimaryDetails: this.props.location.state.PatientDetails,
            }, () => {
                //this.getPatientDetails(this.state.UHID)
            });
    }
    setEditorState = () => {
        const blocksFromHtml = htmlToDraft(this.state.Description);
        const { contentBlocks, entityMap } = blocksFromHtml;
        const contentState = ContentState.createFromBlockArray(contentBlocks, entityMap);
        const editorState = EditorState.createWithContent(contentState);
        this.setState({
            editorState: editorState
        })
    }

    submitHandler = () => { 
        this.setState({errorMessage:''});  

        //Check BMI
        if(this.state.Description===""){
            this.setState({errorMessage:'Please enter Description'});
            return;
        }
        this.addDoctorNotesApiCall()
        
        
    };

    addDoctorNotesApiCall = () => {
        this.setState({errorMessage:""});
        const formatedDate = this.state.Date + 'T' + this.state.Hour + ':' + this.state.Minute + ':00';
        // console.log(formatedDate);
        this.setState({submitLoading:true});
        microsoftTeams.getContext(context => {  
            const doctorsNote =  draftToHtml(convertToRaw(this.state.editorState.getCurrentContent()));
        const dataObjects = {
            Description: doctorsNote,
            Name:context.userPrincipalName,
            DoctorNoteId:this.state.DoctorNoteId,
            TeamId:this.state.groupId,
            ChannelId:this.state.channelId,
            UHID:this.state.UHID,
            AdmissionId:this.state.AdmissionId,
            CreatedByEmail:MockLoginUsersDetails.MockUserEmail ? MockLoginUsersDetails.MockUserEmail : context.userPrincipalName,
            CreatedBy:context.userPrincipalName,
            // CreatedOn: formatedDate,
            DoctorNoteDate: formatedDate
        };    
       //console.log(dataObjects);    
        this.setState({submitLoading:true});
        postAddUpdateDoctorNotesAPIEndpoint(dataObjects,this.state.AccessToken).then((res: any) => {
            //console.log("save doctor notes", res.data);                
            this.setState({submitLoading:false});
            if(res){
                this.pushToLocationHistory('/doctornotes/view');
            }
            else{
                this.setState({errorMessage:"Sorry unable to save your data."});
            }
        });
        })
    }

    pushToLocationHistory(url:any){     
        this.props.history.push({ 
                pathname: url, 
                state: { UHID: this.state.UHID, 
                    AdmissionId: this.state.AdmissionId,
                    DepartmentId: this.state.DepartmentId ,
                    PatientDetails:this.state.PatientDetails
                } 
            });
    }
    onBackClick(url:any){
        this.pushToLocationHistory(url);
    }
    onEditorStateChange: Function = (editorState: any) => {
        this.setState({
            editorState,
            
        });

    };    
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
         
        return (
            <div>
               <Container fluid>
                    <div className='d-flex justify-content-between align-items-center my-3'>
                        <div><a href='javascript:void(0);' onClick={()=>this.onBackClick('/doctornotes/view')}><ChevronLeft24Regular /></a></div>
                        <div className='d-flex'><Text className='ms-2' content={this.state.PageHeadding} size="medium" weight="semibold" /></div>
                        <div></div>
                    </div>
                    {this.BuildPatientPrimaryDetails()}
                    <Row className="mt-3">
                        <div className="col-12">
                            <div className='mb-2'>
                                <Editor
                                    editorState={this.state.editorState}
                                    wrapperClassName="wrapper-class flex-fill"
                                    editorClassName="editor-class"
                                    toolbarClassName="toolbar-class"
                                    toolbar={{
                                        options: ['inline', 'blockType', 'fontSize', 'list', 'textAlign', 'history'],
                                        list: { inDropdown: true },
                                        textAlign: { inDropdown: true },
                                    }}
                                    onEditorStateChange={(e) => this.onEditorStateChange(e)}
                                />
                            </div>
                        </div>
                        <div className='col-md-4 mb-4 '>
                            <Row>

                            <div className='col-5 mb-2'>
                                        <Text className='mb-1 p-0 d-block' color="grey" content="Date" size="small" timestamp />
                                        <Datepicker
                                        allowManualInput={false}
                                            className='pikdate mb-2'
                                            inputPlaceholder=''
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
                    {this.ErrorMessageContent()}
                    <div className='py-3 d-flex justify-content-end'>
                        <Button content="Back" secondary onClick={() => this.onBackClick('/doctornotes/view')} />
                        <Button className='ms-2'  disabled={this.state.submitLoading} content={this.state.submitLoading?<Loader label="Submitting Data.." labelPosition='end'></Loader>:this.state.SubmitButtonText}  primary onClick={()=>this.submitHandler()} />
                    </div>
                </Container>           
            </div>
        );
    }
}

export default AddDoctorNotes;