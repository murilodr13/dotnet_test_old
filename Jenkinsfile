pipeline {
    agent none

    environment {
        DOTNET_CLI_HOME = '/tmp/dotnet_home'

        // Se você criou uma credencial no Jenkins com ID "sonar-token", aqui a usamos:
        SONAR_TOKEN = credentials('sonar-token')
    }
    stages {
        stage('Checkout') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:7.0'
                    args '-u root:root'
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
                    args '-u root:root'
                }
            }
            steps {
                // Garante que o .NET CLI não tente escrever em / como root
                sh 'mkdir -p $DOTNET_CLI_HOME'
            }
        }

        stage('Restore') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:7.0'
                    args '-u root:root'
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
                    args '-u root:root'
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
                echo 'Iniciando container SonarQube local...'
                // 1) Puxa e inicia o SonarQube (imagem oficial LTS)
                sh 'docker run -d --name sonarqube -p 9000:9000 sonarqube:lts'

                // 2) Aguarda alguns segundos para o SonarQube “subir”:
                echo 'Aguardando SonarQube ficar disponível (aprox. 60s)...'
                // Ajuste o tempo, se necessário; pode ser menor em máquinas rápidas.
                sh 'sleep 60'
            }
        }

        stage('SonarQube Analysis') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:7.0'
                    args '-u root:root'
                }
            }
            steps {
                echo 'Executando análise SonarQube no código .NET...'

                // 1) Instala o SonarScanner para .NET como tool global
                sh 'dotnet tool install --global dotnet-sonarscanner --version 5.0.0'

                // 2) Garante que ~/.dotnet/tools esteja no PATH
                sh 'export PATH="$PATH:/root/.dotnet/tools"'

                // 3) Executa SonarScanner Begin
                sh """
                   dotnet sonarscanner begin \
                     /k:"dotnet_test_old" \
                     /d:sonar.host.url="http://localhost:9000" \
                     /d:sonar.login="$SONAR_TOKEN" \
                     /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"
                   """

                // 4) Recompila o projeto para coletar dados de análise
                sh 'dotnet build dotnet_test_old.csproj --configuration Release'

                // 5) Finaliza análise SonarQube
                sh """
                   dotnet sonarscanner end \
                     /d:sonar.login="$SONAR_TOKEN"
                   """
            }
        }

        stage('Testes') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:7.0'
                    args '-u root:root'
                }
            }
            steps {
                echo 'Executando testes automatizados com dotnet test...'
                // 1) Gera o arquivo .trx em TestResults/
                sh 'dotnet test Tests/dotnet_test_old.Tests.csproj --logger "trx;LogFileName=teste-results.trx" --results-directory TestResults'
                // 2) Ajusta permissões e dono para jenkins (UID 1000:1000)
                sh '''
                  chmod -R a+rw TestResults
                  chown -R 1000:1000 TestResults
                '''
            }
            post {
                always {
                    // Publica o .trx usando o plugin MSTest no Jenkins
                    mstest testResultsFile: 'TestResults/*.trx'
                }
            }
        }

        stage('Empacotar Artefatos') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:7.0'
                    args '-u root:root'
                }
            }
            steps {
                echo 'Empacotando executáveis em TAR.GZ...'
                sh '''
                  mkdir -p artifacts
                  dotnet publish dotnet_test_old.csproj -c Release -o publish
                  tar -czf artifacts/dotnet_test_old.tar.gz -C publish .
                '''
            }
            post {
                success {
                    archiveArtifacts artifacts: 'artifacts/dotnet_test_old.tar.gz', fingerprint: true
                }
            }
        }

        stage('Run') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:7.0'
                    args '-u root:root'
                }
            }
            steps {
                echo 'Executando aplicação Hello World...'
                sh 'dotnet run --project dotnet_test_old.csproj --configuration Release'
            }
        }
    }

    post {
        always {
            script {
                echo 'Parando o container SonarQube...'
                sh 'docker stop sonarqube || true'
            }
        }
        success {
            echo 'Pipeline finalizado com sucesso!'
        }
        failure {
            echo 'Pipeline falhou. Verifique os logs para detalhes.'
        }
    }
}
