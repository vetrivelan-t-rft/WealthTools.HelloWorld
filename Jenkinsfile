#!groovy
pipeline {

        agent {
            node {
                label 'DotNetCore'
            }
        }

        parameters {
            booleanParam(defaultValue: false,
                    name: 'FULL_BUILD',
                    description: 'Run a Full Build of the application. Includes Veracode, Black Duck, SonarQube and marking a package as Release Ready')

                    booleanParam(defaultValue: false,
                    name: 'BLACKDUCK_BUILD',
                    description: 'Run a Full Build of the application. Includes Veracode, Black Duck, SonarQube and marking a package as Release Ready')
        }
        environment {
        FULLVERSION = "0.0.${BUILD_NUMBER}"
        }

      /*  environment {

            //User Controlled Variables
            ASPNETCORE_ENVIRONMENT = 'Development'

            //The Major.Minor.Patch for the product, this will be used for deploy as well as attaching the build number for versioning the app
            MAJORVERSION = "${pipelineParams.MAJORVERSION}"
            //The Athena Entitlement for the application
            ENTITLEMENT = "${pipelineParams.ENTITLEMENT}"
            //The name of the Git directory so it can be used in the making of directories and pull code
            COMPONENTNAME = "${pipelineParams.COMPONENTNAME}"
            //The product type is used to group products together in directories as well as assist in deployments
            COMPONENTGROUP = "${pipelineParams.COMPONENTGROUP}"

            RUNDECK_UUID = "32c2b10a-6e8e-4f72-aa9d-ef8e5b74e863"

            //Credentials
            GIT_CREDS = credentials('s.tr.wmbot - Gitlab')
            COMPASS_API_KEY = credentials('s.tr.wmbot_COMPASS_APIKEY')
            BAMS_CREDS = credentials('s.tr.wmbot_BAMS_AWS_APIKEY')
            SLACK_CREDS = credentials('REFINITIVBETA_SLACK_API_KEY')
            //MS_TEAM_CREDS = credentials('MICROSOFT_TEAMS_API_KEY')
            MS_TEAM_WEBHOOK = "${pipelineParams.MS_TEAM_WEBHOOK}"
            //Veracode

            ZIP_WORKSPACE = "${WORKSPACE}"
            APP_NAME = "${pipelineParams.APP_NAME }"
            VERACODE_COMPONENTNAME = "${pipelineParams.COMPONENTNAME}".replace(".", "")
            FULLVERSION = "0.0.${BUILD_NUMBER}"
            VERACODE_COMPONENTNAME_EXE = "${VERACODE_COMPONENTNAME}" + '.exe'
            VERACODE_API_KEY = credentials('Veracode_API_SEN_KEY')


            //Constants
            SCRIPTSDIR = "${WORKSPACE}\\build\\scripts"
            CONFIGSDIR = "${WORKSPACE}\\build\\configs"
            BAMSURI = "https://bams-aws.refinitiv.com/artifactory/api/nuget/default.nuget.cloud/nawm/${COMPONENTGROUP}/${COMPONENTNAME}/"
            DEPLOY_PATH = "${pipelineParams.DEPLOY_PATH}"
            DEPLOY_SERVER = "${pipelineParams.DEPLOY_SERVER}"
            DEPLOY_SERVER_IP = "${pipelineParams.DEPLOY_SERVER_IP}"
            APP_POOL_NAME = "${pipelineParams.COMPONENTNAME}"
            PROJECT_NAME = "${pipelineParams.PROJECT_NAME}"

            DEPLOY_LOCATION =  "\\\\${DEPLOY_SERVER_IP}\\d\$\\${COMPONENTGROUP}\\${COMPONENTNAME}\\"
            TEST_PROJECT_DIR = "${pipelineParams.PROJECT_NAME}.tests"
            //BlackDuck
            SYNOPSYS_JAR_LOC="D:\\Black_Duck\\synopsys-detect-5.6.2.jar"
            BLACK_DUCK_API_KEY= credentials('BLACK_DUCK_API_KEY')


            //Conditions
            HasTests = "${pipelineParams.HasTests}";


        } */

      /*  triggers {
        gitlab(triggerOnPush: true, triggerOnMergeRequest: true)
        }*/

        stages {

           stage('Clone') {
                steps {
                    parallel(
                            Jenkins: {
                                dir("${WORKSPACE}") {
                                    git([url: "https://github.com/vetrivelan-t-rft/WealthTools.HelloWorld", credentialsId: 'vetrivelan-t-rft'])
                                }
                            }
                    ) // end parallel
                } // end steps
            } // end Clone
                
            stage('Build') {
                steps {
                    //Build all the projects
                   powershell 'dotnet build  -p:Version=$env:FULLVERSION -c Release'
                }
            }

        }//end of stages

    } //end of pipeline
