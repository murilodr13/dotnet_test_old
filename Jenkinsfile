pipeline {
    agent none

    environment {
        DOTNET_CLI_HOME = '/tmp/dotnet_home'
        SONAR_TOKEN     = credentials('sonar-token')
    }

    stages {
        stage('Checkout') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:7.0'
                    args  '-u root:root'
                }
            }
            steps {
                echo 'Realizando checkout do repositório...'
                checkout scm
            }
        }

        stage('Prepare .NET Home') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:7.0'
                    args  '-u root:root'
                }
            }
            steps {
                sh 'mkdir -p $DOTNET_CLI_HOME'
            }
        }

        stage('Restore') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:7.0'
                    args  '-u root:root'
                }
            }
            steps {
                echo 'Restaurando pacotes NuGet...'
                sh 'dotnet restore'
            }
        }

        stage('Build') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:7.0'
                    args  '-u root:root'
                }
            }
            steps {
                echo 'Compilando aplicação em Release...'
                sh 'dotnet build dotnet_test_old.csproj --configuration Release'
            }
        }

        stage('Start SonarQube') {
            agent any

            steps {
                echo 'Removendo container SonarQube antigo (se existir)...'
                sh 'docker rm -f sonarqube || true'

                echo 'Iniciando novo container SonarQube...'
                sh 'docker run -d --name sonarqube -u root:root -p 9000:9000 sonarqube:lts'

                echo 'Aguardando SonarQube ficar disponível (60s)...'
                sh 'sleep 60'
            }
        }

        stage('Generate Sonar Token') {
            agent any
            steps {
                echo 'Gerando token no SonarQube via API…'
                sh '''
                apt-get update && apt-get install -y jq curl
                RESPONSE=$(curl -s -u admin:admin -X POST "http://localhost:9000/api/user_tokens/generate" -d "name=jenkins-auto-token")
                TOKEN=$(echo "$RESPONSE" | grep -o '"token":"[^"]*' | cut -d'"' -f4)
                echo "Token gerado: $TOKEN"
                echo "$TOKEN" > sonar-generated.token
                '''
            }
        }

        stage('SonarQube Analysis') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:7.0'
                    args  '-u root:root'
                }
            }
            steps {
                echo 'Instalando SonarScanner para .NET...'
                sh 'cp sonar-generated.token /tmp/sonar-token.txt'
                sh 'export SONAR_TOKEN=$(cat /tmp/sonar-token.txt)'
                
                sh 'dotnet tool install --global dotnet-sonarscanner --version 5.0.0'
                sh 'export PATH="$PATH:/root/.dotnet/tools"'

                echo 'Executando dotnet-sonarscanner begin...'
                sh """
                   dotnet sonarscanner begin \
                     /k:"dotnet_test_old" \
                     /d:sonar.host.url="http://localhost:9000" \
                     /d:sonar.login="$SONAR_TOKEN"
                """

                echo 'Rebuild da solução para coletar dados de análise...'
                sh 'dotnet build dotnet_test_old.csproj --configuration Release'

                echo 'Finalizando análise SonarQube (dotnet-sonarscanner end)...'
                sh 'dotnet sonarscanner end /d:sonar.login="$SONAR_TOKEN"'
            }
        }

        stage('Testes') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:7.0'
                    args  '-u root:root'
                }
            }
            steps {
                echo 'Executando testes com dotnet test...'
                sh 'dotnet test Tests/dotnet_test_old.Tests.csproj --logger "trx;LogFileName=teste-results.trx" --results-directory TestResults'

                echo 'Ajustando permissões em TestResults para publicação...'
                sh '''
                  chmod -R a+rw TestResults
                  chown -R 1000:1000 TestResults
                '''
            }
            post {
                always {
                    echo 'Publicando resultados de teste (.trx) no Jenkins...'
                    mstest testResultsFile: 'TestResults/*.trx'
                }
            }
        }

        stage('Empacotar Artefatos') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:7.0'
                    args  '-u root:root'
                }
            }
            steps {
                echo 'Executando dotnet publish e compactando em tar.gz...'
                sh '''
                  mkdir -p artifacts
                  dotnet publish dotnet_test_old.csproj -c Release -o publish
                  tar -czf artifacts/dotnet_test_old.tar.gz -C publish .
                '''
            }
            post {
                success {
                    echo 'Arquivando artifacts/dotnet_test_old.tar.gz no Jenkins...'
                    archiveArtifacts artifacts: 'artifacts/dotnet_test_old.tar.gz', fingerprint: true
                }
            }
        }

        stage('Run') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:7.0'
                    args  '-u root:root'
                }
            }
            steps {
                echo 'Executando dotnet run (Hello World)...'
                sh 'dotnet run --project dotnet_test_old.csproj --configuration Release'
            }
        }

        stage('Stop SonarQube') {
            agent any
            steps {
                echo 'Parando e removendo container SonarQube...'
                sh 'docker stop sonarqube || true'
                sh 'docker rm sonarqube   || true'
            }
        }
    }

    post {
        success {
            echo 'Pipeline finalizado com sucesso!'
        }
        failure {
            echo 'Pipeline falhou. Verifique os logs para detalhes.'
        }
    }
}
