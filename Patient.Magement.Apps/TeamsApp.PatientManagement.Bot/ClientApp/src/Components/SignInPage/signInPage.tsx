import React from "react";
import { RouteComponentProps } from "react-router-dom";
import { Text, Button } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import "./signInPage.scss";
import { getAppBaseUrl } from "../../apis/AppConfiguration";


const SignInPage: React.FunctionComponent<RouteComponentProps> = props => {
    const history = props.history;
    function onSignIn() {
        var baseUrl=getAppBaseUrl();
        microsoftTeams.authentication.authenticate({
            url: baseUrl + "/signin-simple-start",
            successCallback: () => {
                console.log("Login succeeded!");
                history.push(baseUrl);
                //window.location.href = "/leavecalender";
                
            },
            failureCallback: (reason) => {
                console.log("Login failed: " + reason);
                //window.location.href = "/errorpage";
                history.push(baseUrl+"/errorpage");
            }
        });
    }

    return (
        <div className="sign-in-content-container">
            <Text
                content="Please sign in to continue."
                size="medium"
            />
            <div className="space"></div>
            <Button content="Sign in" primary className="sign-in-button" onClick={onSignIn} />
        </div>
    );
};

export default SignInPage;
