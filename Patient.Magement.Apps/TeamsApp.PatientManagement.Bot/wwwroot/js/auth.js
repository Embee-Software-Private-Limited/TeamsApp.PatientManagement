﻿let accessToken;

$(document).ready(function () {
    microsoftTeams.initialize();
   
    getClientSideToken()
        .then((clientSideToken) => {            
            //return getServerSideToken(clientSideToken);
        })
        .catch((error) => {
            console.log(error);
            if (error === "invalid_grant") {
                // Display in-line button so user can consent
                $("#divError").text("Error while exchanging for Server token - invalid_grant - User or admin consent is required.");
                $("#divError").show();
                $("#consent").show();
            } else {
                // Something else went wrong
            }
        });
});

function requestConsent() {
    getToken()
        .then(data => {
        $("#consent").hide();
        $("#divError").hide();
        accessToken = data.accessToken;
        microsoftTeams.getContext((context) => {
            getUserInfo(context.userPrincipalName);
        });
    });
}

function getToken() {
    return new Promise((resolve, reject) => {
        microsoftTeams.authentication.authenticate({
            url: window.location.origin + "/Auth/Start.html",
            width: 600,
            height: 535,
            successCallback: result => {
               
                resolve(result);
            },
            failureCallback: reason => {
                
                reject(reason);
            }
        });
    });
}

function getClientSideToken() {

    return new Promise((resolve, reject) => {
        microsoftTeams.authentication.getAuthToken({
            successCallback: (result) => {               
                resolve(result);
                getMyProfile(result);
            },
            failureCallback: function (error) {                
                reject("Error getting token: " + error);
            }
        });

    });

}

function getServerSideToken(clientSideToken) {
    return new Promise((resolve, reject) => {
        microsoftTeams.getContext((context) => {
            var scopes = ["https://graph.microsoft.com/User.Read"];
            fetch(window.location.origin +'/api/v1.0/GetUserAccessToken', {
                method: 'get',
                headers: {
                    "Content-Type": "application/text",
                    "Authorization": "Bearer " + clientSideToken
                },
                cache: 'default'
            })
                .then((response) => {
                    if (response.ok) {
                        return response.text();
                    } else {
                        reject(response.error);
                    }
                })
                .then((responseJson) => {
                    if (IsValidJSONString(responseJson)) {
                        if (JSON.parse(responseJson).error)
                            reject(JSON.parse(responseJson).error);
                    } else if (responseJson) {
                        accessToken = responseJson;
                        getUserInfo(context.userPrincipalName);
                    }
                });
        });
    });
}

function IsValidJSONString(str) {
    try {
        JSON.parse(str);
    } catch (e) {
        return false;
    }
    return true;
}


function getUserInfo(principalName) {
    if (principalName) {
        let graphUrl = "https://graph.microsoft.com/v1.0/users/" + principalName;
        $.ajax({
            url: graphUrl,
            type: "GET",
            beforeSend: function (request) {
                request.setRequestHeader("Authorization", `Bearer ${accessToken}`);
            },
            success: function (profile) {
                let profileDiv = $("#divGraphProfile");
                profileDiv.empty();
                for (let key in profile) {
                    if ((key[0] !== "@") && profile[key]) {
                        $("<div>")
                            .append($("<b>").text(key + ": "))
                            .append($("<span>").text(profile[key]))
                            .appendTo(profileDiv);
                    }
                }
                $("#divGraphProfile").show();
            },
            error: function () {
                console.log("Failed");
            },
            complete: function (data) {
            }
        });
    }
}

function getMyProfile(clientSideToken) {
    if (clientSideToken) {
        let apiUrl = window.location.origin + '/api/v1.0/common/GetMyProfile';
        $.ajax({
            url: apiUrl,
            type: "GET",
            headers: {
                "Content-Type": "application/json",
                "Authorization": "Bearer " + clientSideToken
            },
            success: function (divisions) {
                console.log(divisions)
            },
            error: function () {
                console.log("Failed");
            },
            complete: function (data) {
            }
        });
    }
}

function getFilteredUsers(clientSideToken) {
    if (clientSideToken) {
        let apiUrl = window.location.origin + '/api/v1.0/user/GetFilteredUsers';
        $.ajax({
            url: apiUrl,
            type: "GET",
            headers: {
                "Content-Type": "application/json",
                "Authorization": "Bearer " + clientSideToken
            },
            success: function (divisions) {
                console.log(divisions)
            },
            error: function () {
                console.log("Failed");
            },
            complete: function (data) {
            }
        });
    }
}