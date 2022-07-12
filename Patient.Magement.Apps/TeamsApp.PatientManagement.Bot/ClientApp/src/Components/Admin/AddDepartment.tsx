import React from 'react';
import { Container, Row } from 'react-bootstrap';
import {
    Text,
    Button,
    FormDropdown,
    Loader,
    Segment

} from '@fluentui/react-northstar';
import * as microsoftTeams from "@microsoft/teams-js";
import { getChannelsAPIEndpoint, getGroupsAPIEndpoint ,postSaveDepartmentAPIEndpoint,getDepartmentsFromEtherAPIEndpoint, getDepartmentByIdAPIEndpoint} from '../../apis/APIList';
interface IProps {

}

interface IState {
    departmentId:string,
    departmentName:string,
    channelId:string,
    channelName:string,
    teamId:string,
    teamName:string,
    loading:boolean,
    teamGroups:any,
    departments:any,
    channels:any,
    dropdownGroupItems:any,
    dropdownDepartmentItems:any
    dropdownChannelItems:any,
    submitLoading:any,
}

class AddDepartment extends React.Component<IProps, IState> {
    constructor(props: IProps) {
        super(props);
        this.state = {
            loading: false,
            departmentId:"0",
            departmentName:"",
            channelId:"",
            channelName:"",
            teamId:"",
            teamName:"",
            teamGroups:[],
            departments:[],
            channels:[],
            dropdownGroupItems:[],
            dropdownDepartmentItems:[],
            dropdownChannelItems:[],
            submitLoading:false
        };

    }
    componentDidMount() {
        microsoftTeams.initialize();
        microsoftTeams.getContext(context => {
            //console.log(context);
           this.getTeamsGroup();
           this.getDepartments(); 
           
            const queryString=window.location.search;
            //console.log(queryString);
            if(queryString!==""){
                const urlParams = new URLSearchParams(queryString);
                const departmentId = urlParams.get('departmentId');
                if(departmentId!=="" && departmentId!==undefined){
                   this.getDepartmentDetails(departmentId);
                }               
            }
            
        });
    }


    getTeamsGroup = () => {
        getGroupsAPIEndpoint("").then((res) => {
            //console.log(res.data );
          if (res.data) {
            //this.setState({ teamGroups: res.data });  
            let list: { key: any; label: any; content:any}[]=[]; 

            res.data.forEach( (item:any) => {
              //console.log(item);
              list.push({
                key: item.id,
                label: item.displayName,
                content:item.displayName
              });              
            }); 
            this.setState({ dropdownGroupItems: list }); 
           
            
          }
        });
      };
      getTeamsChannel = (teamId:string) => {
        getChannelsAPIEndpoint(teamId).then((res) => {
           //console.log(res.data );  
            if (res.data) {
              //this.setState({ departments: res.data });
              let list: { key: any; label: any; content:any}[]=[];         
              res.data.forEach( (item:any) => {
                //console.log(item);
                list.push({
                  key: item.id,
                  label: item.displayName,
                  content:item.displayName
                });              
              }); 
              this.setState({ dropdownChannelItems: list }); 
            }         
        });
      };
      getDepartments = () => {
        getDepartmentsFromEtherAPIEndpoint().then((res) => {
           // console.log(res.data );
          if (res.data.status==="success") {
            //this.setState({ departments: res.data });
            let list: { key: any; label: any; content:any}[]=[];         
            res.data.body.forEach( (item:any) => {
              //console.log(item);
              list.push({
                key: item.DepartmentId,
                label: item.DepartmentName,
                content:item.DepartmentName
              });              
            }); 
            this.setState({ dropdownDepartmentItems: list }); 
          }
        });
      };
      getDepartmentDetails = (departmentId:any) => {
        this.setState({loading:true});
        getDepartmentByIdAPIEndpoint(departmentId).then((res) => {
          this.setState({loading:false});
            //console.log(res.data );
            if (res.data!==null) {
              this.setState({loading:false,
                departmentId:res.data.departmentId,
                departmentName:res.data.departmentName,
                channelId:res.data.channelId,
                channelName:res.data.channelName,
                teamId:res.data.teamId,
                teamName:res.data.teamName
              });
              this.getTeamsChannel(res.data.teamId);
            }
        });
      };
    submitHandler = () => {
      this.setState({submitLoading:true});
        const dataObjects = {
          departmentId: this.state.departmentId,
          departmentName: this.state.departmentName,
          channelId: this.state.channelId,
          channelName: this.state.channelName,
          teamId: this.state.teamId,
          teamName: this.state.teamName
        };    
        //console.log(dataObjects);    
        postSaveDepartmentAPIEndpoint(dataObjects).then((res: any) => {
          this.setState({submitLoading:false});
          if (res.data) {
            microsoftTeams.tasks.submitTask();
          }
          //console.log("save department", res.data);
        });
      };

      onDepartmentDropdownChange(evt:any,data:any){
        //console.log(data);
        if(data!=null){
        this.setState({ departmentId: data.value.key ,departmentName:data.value.label}); 
        }
      }
      onGroupdownChange(evt:any,data:any){
        //console.log(data);
        if(data!=null){
        this.setState({ teamId: data.value.key ,teamName:data.value.label}); 
        this.getTeamsChannel(data.value.key);
        }
      }
      onChannelDropdownChange(evt:any,data:any){
        //console.log(data);
        if(data!==null){
        this.setState({ channelId: data.value.key ,channelName:data.value.label}); 
        }
      }
    render() {
        if(!this.state.loading){
        return (
            <div>
              <Segment>
                <Container fluid>                    
                    <Row>
                        <div className='col-md-4 mb-2'>
                            <Text className='mb-1 p-0 d-block' color="grey" content="Department" size="small" timestamp />
                            <FormDropdown
                                fluid
                                items={this.state.dropdownDepartmentItems}
                                placeholder="Department"
                                onChange={(evt,data)=>this.onDepartmentDropdownChange(evt,data)}
                                value={this.state.departmentName}
                            />
                        </div>
                        <div className='col-md-4 mb-2'>
                            <Text className='mb-1 p-0 d-block' color="grey" content="Team" size="small" timestamp />
                            <FormDropdown
                                fluid
                                items={this.state.dropdownGroupItems}
                                placeholder="Please select"                                
                                onChange={(evt,data)=>this.onGroupdownChange(evt,data)}
                                value={this.state.teamName}
                            />
                        </div>
                        <div className='col-md-4 mb-2'>
                            <Text className='mb-1 p-0 d-block' color="grey" content="Channel" size="small" timestamp />
                            <FormDropdown
                                fluid
                                items={this.state.dropdownChannelItems}
                                disabled={!this.state.dropdownChannelItems.length}
                                placeholder="Channel"
                                value={this.state.channelName}
                                onChange={(evt,data)=>this.onChannelDropdownChange(evt,data)}
                            />
                        </div>
                    </Row>
                    <div className='py-3 d-flex justify-content-end'>                        
                        <Button className='ms-2' disabled={this.state.submitLoading || (!this.state.departmentName || !this.state.teamName || !this.state.channelName)} content={this.state.submitLoading?<Loader size='small' label="Submitting Data.." labelPosition='end'/>:"Submit"} primary  onClick={()=>this.submitHandler()}/>
                    </div>
                </Container>
                </Segment>
            </div>
        );
        }
        else{
          return <div><Loader/></div>
        }
    }
}

export default AddDepartment;