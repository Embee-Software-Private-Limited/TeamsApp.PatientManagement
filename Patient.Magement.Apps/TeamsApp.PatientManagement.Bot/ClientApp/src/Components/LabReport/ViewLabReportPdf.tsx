import React from 'react';
import { Viewer,Worker,SpecialZoomLevel } from '@react-pdf-viewer/core';
import '@react-pdf-viewer/core/lib/styles/index.css';
interface IProps{
    base64Data?:any
}
interface IState{
}
const pdfContentType = 'application/pdf';
export default class ViewLabReportPdf extends React.Component<IProps, IState>{
    constructor(props:IProps){
        super(props);        
    }

    componentDidMount(){
        
    }
    convertBase64ToBlob = (data: string) => {
        // Cut the prefix `data:application/pdf;base64` from the raw base 64
        const base64WithoutPrefix = data;//.substring(`data:${pdfContentType};base64,`.length);    
        const bytes = atob(base64WithoutPrefix);
        let length = bytes.length;
        let out = new Uint8Array(length);
    
        while (length--) {
            out[length] = bytes.charCodeAt(length);
        }    
        return new Blob([out], { type: pdfContentType });
    };    
    createObjectURL=(base64:string)=>{
        return URL.createObjectURL(this.convertBase64ToBlob(base64));
    }
    
    render(){
        if(this.props.base64Data){
        return(            
            <div
                style={{
                    flex: 1,
                    overflow: 'hidden',
                }}
            >
                <Worker  workerUrl="https://unpkg.com/pdfjs-dist@2.5.207/build/pdf.worker.min.js">
                <Viewer fileUrl={this.createObjectURL(this.props.base64Data)} defaultScale={SpecialZoomLevel.PageFit}/>
                </Worker>
            </div>
        );
        }
    }
}