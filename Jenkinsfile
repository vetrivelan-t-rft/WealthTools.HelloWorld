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
                    description: 'Run a Full Build of the application.Includes FULL_BUILD package')

                    booleanParam(defaultValue: false,
                    name: 'BLACKDUCK_SCAN',
                    description: 'Run a BLACKDUCK SCAN Scan of the application. ')

					booleanParam(defaultValue: false,
                    name: 'VERACODE_SCAN',
                    description: 'Run a VERACODE SCAN of the application. ')
        }
        environment {
				FULLVERSION = "0.0.${BUILD_NUMBER}"
				COMPONENTNAME = "proposal_api"
					//The product type is used to group products together in directories as well as assist in deployments
				COMPONENTGROUP = "WM-ServiceBureau"
				PROJECT_NAME = "WealthTools.WebAPI.Proposals"
				BAMSURI = "https://bams-aws.refinitiv.com/artifactory/api/nuget/default.nuget.cloud/nawm/${COMPONENTGROUP}/${COMPONENTNAME}/"
				BAMS_CREDS = credentials('s.tr.wmbot_BAMS_AWS_APIKEY')
				CONFIGSDIR = "D://Temp//Choco"
				BLACK_DUCK_API_KEY= credentials('BLACK_DUCK_API_KEY')
				MS_TEAM_WEBHOOK = "https://outlook.office.com/webhook/61080308-7395-46af-bb0f-1f25fbda6031@71ad2f62-61e2-44fc-9e85-86c2827f6de9/JenkinsCI/e516048a0bb44cbea04e53e2eeaf0fe2/1a737938-56f9-4055-85a1-7fb6cf96dc5f"
        }

    /*   triggers {
        github(triggerOnPush: true, triggerOnMergeRequest: true)
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
				when {
                    anyOf { branch 'develop'; branch 'master'}
                }
                steps {
                    //Build all the projects
                   powershell 'dotnet build  -p:Version=$env:FULLVERSION -c Release'
                }
            }
	  		 stage('Publish') {
				   when {
                    anyOf { branch 'develop'; branch 'master'}
                }
                steps {
                    dir("${WORKSPACE}\\src\\${PROJECT_NAME}"){
       			 powershell "dotnet publish -c Release  --no-build -p:Version=${FULLVERSION} -o ${WORKSPACE}\\\\dist"
			}
                }
            }
           stage('unit test') {
                when {
                    anyOf { branch 'develop'; branch 'master'}
                }
                steps {
                script {
                try {
                    echo " *****************************************************************"
                    echo "                   UNIT TEST ENABLED: "
                    echo " ***************************************************************** "
                    script {
                        dir("${env.WORKSPACE}") {
                            echo " *****************************************************************"
                            echo "         Cobertura CodeCoverge Started...."
                            echo " *****************************************************************"
                            powershell '''
                                $Folders= Get-ChildItem -Path "$env:WORKSPACE\\Test" -Directory -Force -ErrorAction SilentlyContinue
                                foreach ($Folder in $Folders) {
                                $Name=$Folder.Name
                                Set-Location $env:WORKSPACE\\Test\\$Name
                                powershell 'dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Exclude="[xunit.*]*"'
                                }
                                set-location $env:WORKSPACE\\Test 
                           '''
                            echo " *****************************************************************"
                            echo "         Publlish Cobertura Codecoverage to Jenkins"
                            echo " *****************************************************************"
                           cobertura autoUpdateHealth: false, autoUpdateStability: false, coberturaReportFile: '**\\coverage.cobertura.xml', conditionalCoverageTargets: '70, 0, 0', failUnhealthy: false, failUnstable: false, lineCoverageTargets: '80, 0, 0', maxNumberOfBuilds: 0, methodCoverageTargets: '80, 0, 0', onlyStable: false, sourceEncoding: 'ASCII', zoomCoverageChart: false
                        }
                        dir("${env.WORKSPACE}") {
                            echo " *****************************************************************"
                            echo "        OpenCover CodeCoverge Started...."
                            echo " *****************************************************************"
                            powershell '''
                                $Folders= Get-ChildItem -Path "$env:WORKSPACE\\Test" -Directory -Force -ErrorAction SilentlyContinue
                                foreach ($Folder in $Folders) {
                                $Name=$Folder.Name
                                Set-Location $env:WORKSPACE\\Test\\$Name
                                powershell 'dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:Exclude="[xunit.*]*"'
                                }
                                set-location $env:WORKSPACE\\Test 
                           '''
                        }                        
                        dir("${env.WORKSPACE}") {
                            powershell 'dotnet test  /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput="..\\..\\\\report\\\\result.json"   /p:Include="[WealthTools.*]*"'
                            echo " *****************************************************************"
                            echo "         Unit TestCase started....."
                            echo " ***************************************************************** "
                            bat returnStatus: true, script: "dotnet test --logger \"trx;LogFileName=unit_tests.xml\" --no-build"
                        }
                        }
                    }  
                    finally {
                        script {
                        dir("${env.WORKSPACE}")
                        {
                        echo " *****************************************************************"
                        echo       "Publish Unittest results to Jenkins"
                        echo " ***************************************************************** "
					    step([$class: 'MSTestPublisher', testResultsFile:"**\\TestResults\\unit_tests.xml", failOnError: true, keepLongStdio: true])
                                }
                            }
                        }
                    }
                }
            }  
					stage('Scanners') {
						when {     
							allOf  {
									branch 'master'
									//expression { params.FULL_BUILD }              
								}	
							} 
							parallel {
							stage('SonarQube') {
							steps {
							println "Scanners - SonarQube"
							withSonarQubeEnv('SonarQube AWS Server') {
							bat '''
							sonar-scanner -Dsonar.login=%SONAR_AUTH_TOKEN% -Dsonar.host.url=%SONAR_HOST_URL%^
					-Dsonar.projectKey="NAWM_%COMPONENTGROUP%_%COMPONENTNAME%" -Dsonar.projectName="NAWM_%COMPONENTNAME%" -Dsonar.projectVersion=%FULLVERSION%^
					-Dsonar.projectBaseDir="%WORKSPACE%" -Dsonar.home="%WORKSPACE%"^
					-Dsonar.sourceEncoding=UTF-8^
					-Dsonar.exclusions="**\\bin\\**,**\\obj\\**"^
					-Dsonar.sources="%WORKSPACE%\\src%"  -Dsonar.verbose=true^
					-Dsonar.cs.opencover.reportsPaths="%WORKSPACE%\\Test\\**\\coverage.opencover.xml"^ 
					-Dsonar.cs.vstest.reportsPaths="%WORKSPACE%\\test\\%COMPONENTNAME%.Test\\TestResults\\*.trx"^ 
							''' 
									}
								} //end steps
							} //end parallel stage
						} //end parallel
					} // end scanners

					stage('VeracodeScan') { 
					when {     
							allOf  {
									branch 'master'
									expression { params.VERACODE_SCAN }              
								}	
							} 

					steps {
						script {  
								//Dot net publish 
								powershell '''
								Add-Type -AssemblyName System.IO.Compression.FileSystem
								$zipName = "$env:COMPONENTNAME"
								$zipName = $zipName.replace(".","") + ".zip"
								$_source="$env:WORKSPACE\\Veracode"
								$inputFile ="$env:WORKSPACE\\build\\VeracodeScanFiles.txt"
								$_output="$env:WORKSPACE\\Veracode_Src"
								$_destination="$env:WORKSPACE\\$zipName"
								
								Write-Output "Creating a debug version for Veracode"
								dotnet.exe publish --output $_output
								New-Item -Force -ItemType directory -Path "$env:WORKSPACE\\Veracode"

								foreach($line in Get-Content $inputFile) {
								get-childitem  $_output -recurse | where { $_.Name -eq $line} | Select-Object -First 1 | % {
								Write-Host $_.FullName
								Copy-Item $_.FullName $_source
									} 
								}

								Write-Output "Create Zip file"
								if(Test-Path $_destination) {
									Write-Output "Deleting existing zip : $_destination"
									Remove-Item $_destination -Recurse
								}
								[System.IO.Compression.ZipFile]::CreateFromDirectory($_source, $_destination)

								'''

							}
						}
              }//end of Veracode stage

			 stage('BAMS Push'){
               when {     
							allOf  {
									branch 'master'            
								}	
							} 
                steps {
                        dir("${env.WORKSPACE}\\dist") {
                        script {
                            powershell '''
                             try {
        			   				robocopy  "$env:CONFIGSDIR\\tools" "$env:WORKSPACE\\build\\tools" chocolateyinstall.ps1
                                    $apiKey = "s.tr.wmbot:$env:BAMS_CREDS"
                                    if($env:FULL_BUILD -eq "true") {
                                    $nupkgName = "$env:COMPONENTNAME.$env:FULLVERSION.nupkg"
                                    choco pack "$env:WORKSPACE\\build\\$env:COMPONENTNAME.nuspec" --version "$env:FULLVERSION"
                                    } else {
                                    $nupkgName = "$env:COMPONENTNAME.$env:FULLVERSION-snapshot.nupkg"
                                    choco pack "$env:WORKSPACE\\build\\$env:COMPONENTNAME.nuspec" --version "$env:FULLVERSION-snapshot"
                                    }
                                    
                                    choco push $nupkgName --Source $env:BAMSURI --ApiKey $apiKey
                                }
                                catch {
                                    Write-Output "Publish failed : $PSItem"
                                    exit 1
                                }
                        '''
                        }
                    }
                }
            }  
	 stage('BlackDuckScan') {
                when {
                    anyOf { branch 'master'; branch 'release/*'}
                    expression { params.BLACKDUCK_BUILD }
                }    
                steps {
                   dir("${WORKSPACE}\\dist") {
                   	powershell '''
						powershell "[Net.ServicePointManager]::SecurityProtocol = 'tls12'; irm https://detect.synopsys.com/detect.ps1?$(Get-Random) | iex; detect" --blackduck.url=https://refinitiv.app.blackduck.com --blackduck.username=wealthtoolsbdservice --blackduck.api.token=$env:BLACK_DUCK_API_KEY --detect.project.name=WealthTools --detect.project.version.name=Nightly" "--detect.code.location.name=$env:PROJECT_NAME"
					'''
                   }
                }
 		} 

     }//end of stages
	post {

            failure {
                updateGitlabCommitStatus name: 'build', state: 'failed'
                script {
                    office365ConnectorSend message: "Build failure: ${env.JOB_NAME} ${env.BUILD_NUMBER} (<${env.BUILD_URL}|Open>) \n\n", status:"failure", webhookUrl:"${env.MS_TEAM_WEBHOOK}",color:"d00000"				
                }
            }
            success {
                updateGitlabCommitStatus name: 'build', state: 'success'
				script {
                	office365ConnectorSend message: "Build Success: ${env.JOB_NAME} ${env.BUILD_NUMBER} (<${env.BUILD_URL}|Open>) \n\n", status:"Success", webhookUrl:"${env.MS_TEAM_WEBHOOK}",color:"05b222"				
            	}
			}
        } //end of post

    } //end of pipeline
