import React, { Component } from 'react';
import { Link } from 'react-router-dom';
import { Container, Row, Col } from 'react-bootstrap';
import './../../style.scss';
import { ReactComponent as Jotaro } from "../../images/addVitalbg.svg";
import { Editor } from "react-draft-wysiwyg";
import "../../../node_modules/react-draft-wysiwyg/dist/react-draft-wysiwyg.css";
import {
    Text,
    Button
} from '@fluentui/react-northstar';

import {
    ChevronLeft24Regular
} from "@fluentui/react-icons";
//import { CallVideoIcon } from '@fluentui/react-icons-northstar';
import { IPersonaSharedProps, Persona, PersonaInitialsColor, PersonaSize } from '@fluentui/react';
import ReactDOM from 'react-dom';
import 'bootstrap/dist/css/bootstrap.min.css';
interface IProps {

}

interface IState {

}

class AddPrescription extends React.Component<IProps, IState> {
    constructor(props: IProps) {
        super(props);
        this.state = {
            loading: false

        };

    }
    componentDidMount() {

    }
    
    render() {
        
        return (
            <div>
               <Container fluid>
                    <div className='d-flex justify-content-between align-items-center my-3'>
                        <div><a href='/prescription/list'><ChevronLeft24Regular /></a></div>
                        <div className='d-flex'><Text className='ms-2' content="Add Prescription" size="medium" weight="semibold" /></div>
                        <div></div>
                    </div>
                    <Row className="mt-3">
                        <div className="col-12">
                            <div className='mb-2'>
                                {/* <Text className='mb-1 p-0 d-block' color="grey" content="BMI" size="small" timestamp /> */}
                                <Editor
                                    wrapperClassName="wrapper-class flex-fill"
                                    editorClassName="editor-class"
                                    toolbarClassName="toolbar-class"
                                    toolbar={{
                                        options: ['inline', 'blockType', 'fontSize', 'list', 'textAlign', 'history'],
                                        list: { inDropdown: true },
                                        textAlign: { inDropdown: true },
                                    }}
                                />
                            </div>
                        </div>
                    </Row>
                    <div className='py-3 d-flex justify-content-end'>
                        <Link to="/prescription/list"><Button content="Back" secondary /></Link>
                        <Link to="/prescription/add"><Button className='ms-2' content="Add prescription" primary /></Link>

                    </div>
                </Container>          
            </div>
        );
    }
}

export default AddPrescription;