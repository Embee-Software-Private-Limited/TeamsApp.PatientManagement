### Patient Management App

<table>
<thead>
<tr>
<th><a href="https://github.com/Embee-Software-Private-Limited/TeamsApp.PatientManagement/wiki/Solution-Overview">Architecture</a></th>
<th><a href="https://github.com/Embee-Software-Private-Limited/TeamsApp.PatientManagement/wiki">Documentation</a></th>
<th><a href="https://github.com/Embee-Software-Private-Limited/TeamsApp.PatientManagement/wiki/Deployment-guide">Deployment guide</a></th>
</tr>
</thead>
</table>
A custom-built patient management solution that runs on MS Teams and manages patient information flow from registration to discharge.  

During the nursing of a patient, while being admitted to the hospital, nursing staff are expected to report on the vitals, administration of medicines etc. to the doctor.  

This will be used by the doctor to add to patient notes on the course of treatment. The hospital staff uses the team’s channel conversation thread linked to each patient to post regarding the same so that everyone is aware / updated of the patient’s information.  

All these activities are also carried out on the Teams mobile app to have easy and immediate access. 

### Key Features 
 - Provision to configure the hospital’s department with that of Teams group for the information to flow to the appropriate team / department based on the patient information.
 - Whenever a new patient admission information is transferred from Either (Hospital Information System) app by calling patient management teams app endpoint, an adaptive card is sent in the specific dept. which are mapped to teams' group and the message will be posted on the general channel.
 - All the below mentioned information pertaining to a patient and related admission can be either viewed / added / updated on Teams platform:
   - View Patient Details like Unique Identification No., Admission No., Date of Admission, Personal Details etc. 
   - View the Lab Report(s)
   - View Medical History 
   - View the Prescription 
   - Add / Update / View the Vitals like Temperature, Weight, Height etc.
   - Add / Update / View the Intake & Output details
   - Add / Update / View the Doctor Notes  

### Business Benefit 

- Set aside the manual paperwork and keep track of patient records, it will help doctors to keep track of patient current and previous health-related medical history records. 
- Doctors and Nurses will get a better experience from the initial phase of registration to the time they are discharged.  
- No risks of data loss and patients' data remains completely protected. Everything is done through a secure system, only authorized individuals have access to the specific data collection.  
- With a centralized system, doctors, administrative personnel, and other employees can access data in real-time, enabling them to make the best decisions possible. 
 
More screenshots and tips on how to use the app are in the [Wiki](https://github.com/Embee-Software-Private-Limited/TeamsApp.PatientManagement/wiki) of this repository.

  - New admission channel notification
  
  ![Capture1](https://user-images.githubusercontent.com/81224711/181476497-127d5840-da2d-46bf-bad8-1fce1db2f39f.png)

  - Patient Detail Information
  
  ![Capture2](https://user-images.githubusercontent.com/81224711/181476721-57325bb2-0be7-43ec-88fc-fadf72d97611.PNG)

  - Vital's addition
  
  ![Capture5](https://user-images.githubusercontent.com/81224711/181476959-b4f51dfb-12ab-4915-832d-7133fff1aa24.png)

  - Vital's addition notification in team's channel

  ![Capture6](https://user-images.githubusercontent.com/81224711/181477062-34f34972-f2a3-4d5c-950c-d009ae6692fc.png)



### Tools and Technologies
- Azure Bot Service 
- App Service Plan 
- Azure Monitor (Application Insights) 
- Storage Account (Table) 
- .Net Core 3.1
- C#
- React Js
- Type Script
- Node Js
- MS Graph API
- MS Microsoft Bot Framework v4

### About Embee

Embee helps in deploying technologies for efficient delivery processes, better utilization of resources, and optimization to scale performance.   

 - 30+ years of experience in providing customized IT solutions
 - Served customers across Indian and Global Markets    
 - 16+ awards for significant contribution to Indian IT landscape   
 - PAN India presence with 11 offices across the country 
 
Track, store, and access patient information with the Patient Management System by Embee. Interested?  

### Legal Notice

This app template is provided under the MIT License terms. In addition to these terms, by using this app template you agree to the following:

 - You, not Embee Software, will license the use of your app to users or organization.

 - This app template is not intended to substitute your own regulatory due diligence or make you or your app compliant with respect to any applicable regulations, including but not limited to privacy, healthcare, employment, or financial regulations.

- You are responsible for complying with all applicable privacy and security regulations including those related to use, collection and handling of any personal data by your app. This includes complying with all internal privacy and security policies of your organization if your app is developed to be sideloaded internally within your organization. Where applicable, you may be responsible for data related incidents or data subject requests for data collected through your app.

- Any trademarks or registered trademarks of Embee Software in the India and/or other countries and logos included in this repository are the property of Embee Software, and the license for this project does not grant you rights to use any Embee Software names, logos or trademarks outside of this repository. Embee Software’s general trademark guidelines can be found here.

- Use of this template does not guarantee acceptance of your app to the Teams app store. To make this app available in the Teams app store, you will have to comply with the submission and validation process, and all associated requirements such as including your own privacy statement and terms of use for your app.

### Getting Started

Begin with the Solution overview to read about what the app does and how it works.

- [Deployment guide](https://github.com/Embee-Software-Private-Limited/TeamsApp.PatientManagement/wiki/Deployment-guide).
   - Use this option to deploy the app manually.
