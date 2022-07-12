import React from 'react';
import { Container, Row } from 'react-bootstrap';
import { ReactComponent as Jotaro } from "../../images/addVitalbg.svg";
//------------
import pdfImg from "../../images/pdf-download.svg"
import pdfImglg from "../../images/pdf-download_lg.svg"
//------------
import {
    Loader,
    Segment,
    Text, 
    Button
} from '@fluentui/react-northstar';
import * as microsoftTeams from "@microsoft/teams-js";
import { ChevronLeft24Regular} from "@fluentui/react-icons";
import {  Persona, PersonaSize } from '@fluentui/react';
import { getLabReportsFromEtherAPIEndpoint } from '../../apis/APIList';
import moment from 'moment';
import Moment from 'react-moment';
import { getPatientsDetailsFromEtherAPIEndpoint } from '../../apis/APIList';
import PatientPrimaryDetails from '../Interfaces/patientPrimaryDetails';
import ViewLabReportPdf from './ViewLabReportPdf';


interface IProps {
    history?:any,
    location?:any
}

interface IState {
    labReportList: any;
    timeLineDatesList: any;
    loading:boolean,
    UHID?:any
    AdmissionId?:any,
    DepartmentId?:any,
    patientPrimaryDetails: any,
    AccessToken?:any,
    base64?:any,
    pdfTestName?:any
}

class ViewLabReport extends React.Component<IProps, IState> {

    constructor(props: IProps) {
        super(props);
        this.state = {
            loading: true,
            labReportList:[],
            timeLineDatesList: [],
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
            //console.log(context);
            if(queryString!=""){
                const urlParams = new URLSearchParams(queryString);
                const UHID = urlParams.get('UHID');
                const AdmissionId = urlParams.get('AdmissionId');
                const DepartmentId=urlParams.get('DepartmentId');
                if(UHID!==null && UHID!=="" && AdmissionId!==null && AdmissionId!=="" && DepartmentId!==null && DepartmentId!==""){
                    this.loadLabReports(UHID,AdmissionId);
                    this.setState({UHID:UHID,AdmissionId:AdmissionId,DepartmentId:DepartmentId});
                    this.getPatientDetails(UHID)
                }               
            }
            else{

                this.setState({
                    UHID:this.props.location.state.UHID,
                    AdmissionId:this.props.location.state.AdmissionId,
                    DepartmentId:this.props.location.state.DepartmentId,
                    patientPrimaryDetails:this.props.location.state.PatientDetails
                },
                ()=>{
                    this.loadLabReports(this.state.UHID,this.state.AdmissionId);
                    //this.getPatientDetails(this.state.UHID)
                }
                );            
            }
    }
    getPatientDetails(UHID:any){
        getPatientsDetailsFromEtherAPIEndpoint(UHID,this.state.AccessToken).then((res) => {
           // console.log(res.data );
            if(res.data.status==="success" && res.data.body.length>0){
                const responseData=res.data.body[0];
                this.setState({ patientPrimaryDetails: responseData}); 
            }
            
        });
    }


    loadLabReports = (UHID:any,AdmissionId:any) => {
        this.setState({loading:true});
        getLabReportsFromEtherAPIEndpoint(UHID,AdmissionId,this.state.AccessToken).then((res) => {
           // console.log(res.data );
          if (res.data.status=="success") {
            this.setState({ labReportList: res.data.body, timeLineDatesList:res.data.timeLineDates });
          }
          this.setState({loading:false});
        });
      };
      formateTimeLineDate=(date:any)=>{
        var today = moment().endOf('day').format('DD MMMM YYYY');
        var mydate = moment(date).format('DD MMMM YYYY');
        var yesterday = new Date();
        var yesterdayText=moment(yesterday.setDate((new Date()).getDate()-1)).format('DD MMMM YYYY');
     

        if (mydate===today) return 'Today'
        if (mydate===yesterdayText) return 'Yesterday'
        return moment(date).format('DD MMMM YYYY');        
         
     }
     convertBase64ToBlob=(data:any)=>{
            // Cut the prefix `data:application/pdf;base64` from the raw base 64
            var base64WithoutPrefix = data;//.substr(`data:${pdfContentType};base64,`.length);

            const bytes = atob(base64WithoutPrefix);
            let length = bytes.length;
            let out = new Uint8Array(length);

            while (length--) {
                out[length] = bytes.charCodeAt(length);
            }

            var blb= new Blob([out], { type: "data:application/pdf;base64" });

           return  URL.createObjectURL(blb);
     }
    pushToLocationHistory(){
        var url="/patients/details";        
        this.props.history.push({ 
            pathname: url, 
            state: { UHID: this.state.UHID, 
                AdmissionId: this.state.AdmissionId,
                DepartmentId:this.state.DepartmentId,
                PatientDetails: this.state.patientPrimaryDetails,
                AccessToken:this.state.AccessToken
            } 
        });
    }
     //In no record exists then render this content
     NoRecordFoundContent(){
        return (
            <Container fluid>
                    <div className='d-flex justify-content-between align-items-center my-3'>
                    <div><a href="javascript:void(0)" onClick={()=>this.pushToLocationHistory()} ><ChevronLeft24Regular /></a></div>
                        <div className='d-flex'><Text className='ms-2' content="Lab Reports" size="medium" weight="semibold" /></div>
                        <div></div>
                    </div>
                    <div className='mx-3'>
                    {this.BuildPatientPrimaryDetails()}
                    </div>
                    <div className='mt-5 text-center'>
                        <div><Jotaro  height={150}/></div>
                        <Text className='d-block mb-2' content="No Lab Reports added" size="large" weight="bold" />
                        <Text className='d-block' content="Lab Reports of the patient show up here" size="medium" weight="regular" />                        
                    </div>
                </Container>    
        )
     }

     BuildItemDetailDateWise = (date: any) => {
        return (
            <div className="tl-item active" key={date}>
            <div className="tl-dot"></div>
            <div className="d-flex flex-column w-100">
                <div><span className="badge bg-gadient-green mb-1">{this.formateTimeLineDate(date)}</span></div>
                {this.state.labReportList.filter((x:any)=>moment(x.ModifiedOn).format('L')===moment(date).format('L')).map((item: any) => this.BuildItemDetail(item))}
            </div>
            </div>
        );
     };


      b64toBlob = (b64Data:any, contentType='', sliceSize=512) => {
        const byteCharacters = atob(b64Data);
        const byteArrays = [];
      
        for (let offset = 0; offset < byteCharacters.length; offset += sliceSize) {
          const slice = byteCharacters.slice(offset, offset + sliceSize);
      
          const byteNumbers = new Array(slice.length);
          for (let i = 0; i < slice.length; i++) {
            byteNumbers[i] = slice.charCodeAt(i);
          }
      
          const byteArray = new Uint8Array(byteNumbers);
          byteArrays.push(byteArray);
        }
      
        const blob = new Blob(byteArrays, {type: contentType});
        return blob;
      }
     pdfViewerFunction = (base64Url:any) => {
        // console.log(base64Url)
        const blob = this.b64toBlob(base64Url, 'application/pdf');
        const blobUrl = URL.createObjectURL(blob);
        //alert(blobUrl);
        window.open(blobUrl, "_blank", "resizable=yes, scrollbars=yes, titlebar=yes")
        // this.props.history.push({
        //     pathname: '/labreport/viewPdf',
        //     state: {
        //         base64Url: base64Url
        //     }
        // })
     }
     BuildItemDetail = (item: any) => { 
        return (
            <div className="card w-100" key={ item.LabRequestId}>
                <div className="accordion accordion-flush" id={"accordionOne-" + item.LabRequestId}>
                    <div className="accordion-item">
                        <h2 className="accordion-header" id={"headingOne-" + item.LabRequestId}>
                            <div className='d-flex justify-content-between align-items-center accordion-button collapsed' data-bs-toggle="collapse" data-bs-target={"#collapseOne-" + item.LabRequestId} aria-expanded="false" aria-controls={"collapseOne-" + item.LabRequestId}>
                                <div>
                                    <Text className='d-block' content={item.TestName} size="medium" weight="semibold" />
                                    <Text className='d-block' content={<Moment format='h:mm a'>{item.ModifiedOn}</Moment>} size="small" />
                                </div>
                            </div>
                        </h2>
                        <div id={"collapseOne-" + item.LabRequestId} className="accordion-collapse collapse" aria-labelledby={"headingOne-" + item.LabRequestId}  data-bs-parent={"#accordionOne-" + item.LabRequestId}>
                            <div className="accordion-body ">
                                <div className='report_download text-center p-3 mb-3'>
                                    <div className='mb-2'>
                                        <img src={pdfImglg} alt="Report" />
                                    </div>
                                    <Button content="View report" inverted  onClick={()=>{this.ShowPdf(item.TestName,item.ReportFileUrl.id)}}/>
                                </div>
                                <Row className='mb-2'>
                                    <div className='col-md-4 col-6 mb-2'>
                                        <div className='d-flex'>
                                            <div>
                                                <Text className='d-block' color="grey" content="Sample Number" size="small" timestamp />
                                                <Text className='d-block' content={item.Name} size="medium" weight="semibold" />
                                            </div>
                                        </div>
                                    </div>
                                    <div className='col-md-4 col-6 mb-2'>
                                        <div className='d-flex'>
                                            <div>
                                                <Text className='d-block' color="grey" content="Priority" size="small" timestamp />
                                                <Text className='d-block' content={item.Priority} size="medium" weight="semibold" />
                                            </div>
                                        </div>
                                    </div>
                                    <div className='col-md-4 col-6 mb-2'>
                                        <div className='d-flex'>
                                            <div>
                                                <Text className='d-block' color="grey" content="Sample Date" size="small" timestamp />
                                                <Text className='d-block' content={<Moment format='MMM Do YYYY h:mm a'>{item.DoneDate?item.DoneDate:item.CreatedOn}</Moment>} size="medium" weight="semibold" />
                                            </div>
                                        </div>
                                    </div>
                                    <div className='col-md-4 col-6 mb-2'>
                                        <div className='d-flex'>
                                            <div>
                                                <Text className='d-block' color="grey" content="Report Date" size="small" timestamp />
                                                <Text className='d-block' content={item.ReportFileUrl.date_entered} size="medium" weight="semibold" />
                                            </div>
                                        </div>
                                    </div>
                                </Row>
                            </div>
                        </div>
                    </div>
                </div>
                <div className="card-body">
                    <div className='d-flex justify-content-between align-items-center'>
                        <div><Persona text={item.CreatedBy} size={PersonaSize.size24} /></div>
                        <div onClick={()=>{this.ShowPdf(item.TestName,item.ReportFileUrl.id)}}>
                            <img src={pdfImg} alt="Report" />
                        </div>
                    </div>
                </div>
            </div>
        );
      };

    ShowPdf=(testName:string, base64:string)=>{
        console.log(base64);
        if(base64){

            this.setState({base64:base64,pdfTestName:testName});
        }
        else{
            this.setState({base64:"",pdfTestName:""});
        }        

    }
    backClickFromPdfView(){
        this.setState({base64:"",pdfTestName:""});
    }
     //In  record exists then render this content
    RecordFoundContent(){
        if(this.state.base64){
            return (
                <Segment>
                <div className='d-flex justify-content-between align-items-center my-3'>
                        <div><a href="javascript:void(0)" onClick={()=>this.backClickFromPdfView()}><ChevronLeft24Regular /></a></div>
                        <div className='d-flex'><Text className='ms-2' content={this.state.pdfTestName} size="medium" weight="semibold" /></div>
                        <div></div>
                    </div>
                <ViewLabReportPdf base64Data={this.state.base64}/>  
                </Segment>         
            );
        }
        else{
        return (   
                <Segment>
                    <div className='d-flex justify-content-between align-items-center my-3'>
                        <div><a href="javascript:void(0)" onClick={()=>this.pushToLocationHistory()}><ChevronLeft24Regular /></a></div>
                        <div className='d-flex'><Text className='ms-2' content="Lab Reports" size="medium" weight="semibold" /></div>
                        <div></div>
                    </div>
                    <div className='mx-3'>
                    {this.BuildPatientPrimaryDetails()}
                    </div>
                        {this.state.timeLineDatesList.map((date: any) => this.BuildItemDetailDateWise(date))}
                        <div className="tl-item">
                            <div className="tl-dot"></div>
                            <div className="d-flex flex-column w-100">
                                <div><span className="badge bg-gadient-grey mb-1">No further history</span></div>
                             
                            </div>
                        </div>
                </Segment>
            
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
            const isRecordExists=this.state.labReportList.length>0?true:false; 
        let renderContent;
        if (isRecordExists) {
            renderContent = this.RecordFoundContent();
        } else {
            renderContent =this.NoRecordFoundContent();
        }
        return (
            <div>
                {renderContent}                
            </div>
        );
    }
    else{
        return <div>

        <Segment><Loader/></Segment>              
            </div>
    }
    }
}
export default ViewLabReport;



