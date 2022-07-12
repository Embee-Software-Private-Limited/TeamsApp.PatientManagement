import React, { Fragment } from 'react';
import { Container} from 'react-bootstrap';
//import './../../style.scss';
import DataTable, { TableColumn } from 'react-data-table-component';
import {
    Text,
    Button,
    Loader,
    Segment
} from '@fluentui/react-northstar';

import {
    AddRegular
} from "@fluentui/react-icons";
import * as microsoftTeams from "@microsoft/teams-js";
import { getDepartmentsAPIEndpoint } from '../../apis/APIList';
import { getAppBaseUrl } from '../../apis/AppConfiguration';
import { ITaskInfo, TaskModuleMediumHeight, TaskModuleMediumWidth } from '../Interfaces/CommonInterfaces';

interface IProps {
}

interface IState {
    departmentDetails: any;
    loading:boolean,
    tableColumns:TableColumn<DataRow>[]
}
interface DataRow {
    departmentId: string;
    departmentName: string;
    teamName: string;
}
class ListDepartment extends React.Component<IProps, IState> {
    
    constructor(props: IProps) {
        super(props);
        this.state = {
            loading: true,
            departmentDetails: [],
            tableColumns:[]
        };
    }
    componentDidMount() {
        microsoftTeams.initialize();
        microsoftTeams.getContext(context => {
           // console.log(context);
            this.setState({ tableColumns: [
                {
                    name: 'Department Id',
                    selector: (row: { departmentId: any; }) => row.departmentId,
                    sortable: true
                },
                {
                    name: 'Department Name',
                    selector: (row: { departmentName: any; }) => row.departmentName,
                    sortable: true
                },
                {
                    name: 'Teams Name',
                    selector: (row: { teamName: any; }) => row.teamName,
                },
                {
                    name: 'Action',
                    sortable: false ,
                    cell: (record) => {
                        return (
                          <Fragment>
                            <button
                              className="btn btn-primary btn-sm"
                              onClick={() => {
                                this.openEditForm(record);
                              }}
                            >
                              Edit
                            </button>
                          </Fragment>
                        );
                       }
                }
            ] });


            this.loadDepartments();
        });
    }
    loadDepartments = () => {
        this.setState({ loading: true });
        getDepartmentsAPIEndpoint().then((res) => {
            //console.log('getDepartmentsAPIEndpoint',res.data );
          if (res.data) {
            this.setState({ departmentDetails: res.data });
          }
          this.setState({ loading: false });
        });
      };

    //Open new department form
    openNewForm = (department?: any) => {
        const taskInfo: ITaskInfo = {
        title: "New Department",
        height: TaskModuleMediumHeight,
        width: TaskModuleMediumWidth,
        url: getAppBaseUrl() + "/admin/department/add",
        fallbackUrl:  getAppBaseUrl() + "/admin/department/add",
        };
        const submitHandler = (err: any, result: any) => {
           // console.log(`Submit handler - err: ${err}`);
            if(result!=null){
            
            }
            this.loadDepartments();
        };
        microsoftTeams.tasks.startTask(taskInfo, submitHandler);
    };

    //Open edit department form
    openEditForm = (department: any) => {
        const taskInfo: ITaskInfo = {
          title: "Edit Department",
          height: TaskModuleMediumHeight,
          width: TaskModuleMediumWidth,
          url: getAppBaseUrl() +"/admin/department/add?departmentId=" +department.departmentId,
          fallbackUrl: getAppBaseUrl() +"/admin/department/add?departmentId=" +department.departmentId
        };
    
        const submitHandler = (err: any, result: any) => {
            this.loadDepartments();
        };
        microsoftTeams.tasks.startTask(taskInfo, submitHandler);
      };

      commonHeaderContent(){
        return (
             <div className='d-flex justify-content-between align-items-center'>
                <div><Text content="Department Listing" size="medium" weight="semibold" /></div>
                <div><Button icon={<AddRegular />} onClick={()=>this.openNewForm(null)} content="Add New Department" iconPosition="before" primary size="small"/></div>
            </div>                    
        )
      }
     //In no record exists then render this content
     NoRecordFoundContent(){
        return (
            <div>
                <Segment>
                <div className="command_bar mb-2 py-2 px-3">
                    {this.commonHeaderContent()}
                    <div className='mt-5 text-center'>
                        <Text className='d-block mb-2' content="No Records Exits" size="large" weight="bold" />
                </div>
                </div>
                </Segment>
            </div>   
        )
     }
     //In  record exists then render this content
    RecordFoundContent(){
        return (
            <div>
                <Segment>
                <div className="command_bar mb-2 py-2 px-3">
                    <div className='d-flex justify-content-between align-items-center'>
                        <div><Text content="Department Listing" size="medium" weight="semibold" /></div>
                        <div><Button icon={<AddRegular />} onClick={()=>this.openNewForm(null)} content="Add New Department" iconPosition="before" primary size="small"/></div>
                    </div>
                </div>
                <Container fluid>
                <DataTable
                        columns={this.state.tableColumns}
                        data={this.state.departmentDetails}
                        pagination
                    />
                </Container>
                </Segment>
            </div>
        )
    }
    render() {

        if(!this.state.loading){
            const isRecordExists=this.state.departmentDetails.length>0?true:false; 
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
            return (
                <div>
                    <Loader></Loader>
                </div>
            );
        }
    }
}
export default ListDepartment;



