import React from 'react';
import { Row } from 'react-bootstrap';
import { Text, Header } from '@fluentui/react-northstar';
import {PersonCircle24Regular} from '@fluentui/react-icons';
import Moment from 'react-moment';

interface ppdProps{
    patientPrimaryDetails:any
}
interface ppdState{}

export default class PatientPrimaryDetails extends React.Component<ppdProps, ppdState>{
    constructor(props:ppdProps){
        super(props);
    }

    render(){
        if(this.props.patientPrimaryDetails){
            var Salutation=this.props.patientPrimaryDetails.Salutation ?this.props.patientPrimaryDetails.Salutation :"";
            var PatName=this.props.patientPrimaryDetails.PatientName ?this.props.patientPrimaryDetails.PatientName :"";
        return(
            <Row className='justify-content-between'>
                <div className='col-md-6 col-6'>
                    <div className='d-flex'>
                        <div>
                            <Text className='d-block' color="grey" content="Name" size="small" timestamp />
                            <Text className='d-block' content={Salutation+" "+PatName} size="medium" weight="bold" />
                        </div>
                    </div>
                </div>
                <div className='col-md-6 col-6'>
                    <div className='d-flex flex-row-reverse'>
                        <div className='mx-3'>
                            <Text className='d-block' color="grey" content="UHID" size="small" timestamp />
                            <Text className='d-block' content={this.props.patientPrimaryDetails.UHID?this.props.patientPrimaryDetails.UHID:""} size="medium" weight="bold" />
                        </div>
                    </div>
                </div>
            </Row>
        );
    
}
else{
    return (<div></div>);
}
}
}

export class PatientPrimaryDetailsHeroSection extends React.Component<ppdProps, ppdState>{
    constructor(props:ppdProps){
        super(props);
    }

    render(){
        if(this.props.patientPrimaryDetails){
        return(
            <div className='hero_sec py-4 px-3'>
                <div className='d-flex justify-content-between mb-3'>
                    <div className='d-flex align-items-center'><PersonCircle24Regular color="#fff" /><Header className='mb-0 ms-1 text-light' as="h4" content={this.props.patientPrimaryDetails.Salutation +" "+this.props.patientPrimaryDetails.PatientName} /></div>
                </div>
                <Row>
                    <div className='col-md-4 col-4'>
                        <div className='d-flex'>
                            <div>
                                <Text className='d-block text-light' color="grey" content="Age" size="small" timestamp />
                                <Text className='d-block text-light' content={this.props.patientPrimaryDetails.RegistrationAge} size="medium" weight="regular" />
                            </div>
                        </div>
                    </div>
                    <div className='col-md-4 col-4'>
                        <div className='d-flex'>
                            <div>
                                <Text className='d-block text-light' color="grey" content="UHID" size="small" timestamp />
                                <Text className='d-block text-light' content={this.props.patientPrimaryDetails.UHID} size="medium" weight="regular" />
                            </div>
                        </div>
                    </div>
                    <div className='col-md-4 col-4'>
                        <div className='d-flex'>
                            <div>
                                <Text className='d-block text-light' color="grey" content="DOA" size="small" timestamp />
                                <Text className='d-block text-light' content={<Moment format='MMM Do YYYY'>{this.props.patientPrimaryDetails.RegistrationDate}</Moment>} size="medium" weight="regular" />
                            </div>
                        </div>
                    </div>
                </Row>
            </div>
        );}
        else{
            return (<div></div>);
        }
    }
}