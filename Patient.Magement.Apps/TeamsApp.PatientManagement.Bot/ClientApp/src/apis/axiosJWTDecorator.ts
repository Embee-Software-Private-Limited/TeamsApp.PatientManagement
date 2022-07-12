// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import axios, { AxiosResponse, AxiosRequestConfig } from "axios";
import * as microsoftTeams from "@microsoft/teams-js";
// import i18n from '../i18n';

export class AxiosJWTDecorator {
    public async get<T = any, R = AxiosResponse<T>>(
        url: string,
        handleError: boolean = true,
        needAuthorizationHeader: boolean = true,
        accessToken:any="",
        config?: AxiosRequestConfig,
       
    ): Promise<R> {
        try {
            if (needAuthorizationHeader) {
                if (accessToken!==""){
                    config = await this.setupAuthorizationHeaderUserToken(config,accessToken);
                }
                else{
                    config = await this.setupAuthorizationHeader(config);
                }               
            }
            return await axios.get(url, config);
        } catch (error) {
            if (handleError) {
                this.handleError(error);
                throw error;
            }
            else {
                throw error;
            }
        }
    }

    public async delete<T = any, R = AxiosResponse<T>>(
        url: string,
        handleError: boolean = true,
        config?: AxiosRequestConfig
    ): Promise<R> {
        try {
            config = await this.setupAuthorizationHeader(config);
            return await axios.delete(url, config);
        } catch (error) {
            if (handleError) {
                this.handleError(error);
                throw error;
            }
            else {
                throw error;
            }
        }
    }

    public async post<T = any, R = AxiosResponse<T>>(
        url: string,
        data?: any,
        handleError: boolean = true,
        needAuthorizationHeader: boolean = true,
        accessToken:any="",
        config?: AxiosRequestConfig,

    ): Promise<R> {
        try {
            //config = await this.setupAuthorizationHeader(config);
            if (needAuthorizationHeader) {
                if (accessToken!==""){
                    config = await this.setupAuthorizationHeaderUserToken(config,accessToken);
                }
                else{
                    config = await this.setupAuthorizationHeader(config);
                }    
            }
            return await axios.post(url, data, config);
        } catch (error) {
            if (handleError) {
                this.handleError(error);
                throw error;
            }
            else {
                throw error;
            }
        }
    }

    public async put<T = any, R = AxiosResponse<T>>(
        url: string,
        data?: any,
        handleError: boolean = true,
        config?: AxiosRequestConfig
    ): Promise<R> {
        try {
            config = await this.setupAuthorizationHeader(config);
            return await axios.put(url, data, config);
        } catch (error) {
            if (handleError) {
                this.handleError(error);
                throw error;
            }
            else {
                throw error;
            }
        }
    }

    private handleError(error: any): void {
        if (error.hasOwnProperty("response")) {
            const errorStatus = error.response.status;
            console.log(error.response.status);
            if (errorStatus === 403) {
                window.location.href = `/errorpage/?id=403&locale=en-US`;
            } else if (errorStatus === 401) {
                //window.location.href = `/errorpage/?id=401&locale=en-US`;
            } else {
                window.location.href = `/errorpage?locale=en-US`;
            }
        } else {
            window.location.href = `/errorpage?locale=en-US`;
        }
    }

    private async setupAuthorizationHeader(
        config?: AxiosRequestConfig
    ): Promise<AxiosRequestConfig> {
        microsoftTeams.initialize();

        return new Promise<AxiosRequestConfig>((resolve, reject) => {
            const authTokenRequest = {
                successCallback: (token: string) => {
                    if (!config) {
                        config = axios.defaults;
                    }
                    config.headers["Authorization"] = `Bearer ${token}`;
                    config.headers["Accept-Language"] = 'en-US';
                    resolve(config);
                },
                failureCallback: (error: string) => {
                    // When the getAuthToken function returns a "resourceRequiresConsent" error, 
                    // it means Azure AD needs the user's consent before issuing a token to the app. 
                    // The following code redirects the user to the "Sign in" page where the user can grant the consent. 
                    // Right now, the app redirects to the consent page for any error.
                    console.error("Error from getAuthToken: ", error);
                    window.location.href = `/signin?locale=en-US`;
                },
                resources: []
            };
            microsoftTeams.authentication.getAuthToken(authTokenRequest);
        });
    }

    private setupAuthorizationHeaderUserToken(
        config?: AxiosRequestConfig, token?: any
    ): AxiosRequestConfig {

        if (!config) {
            config = axios.defaults;
        }
        config.headers["Authorization"] = `Bearer ${token}`;
        config.headers["Accept-Language"] = 'en-US';
        return config;
    }

}

const axiosJWTDecoratorInstance = new AxiosJWTDecorator();
export default axiosJWTDecoratorInstance;