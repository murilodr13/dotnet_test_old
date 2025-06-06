pipeline {
    agent none

    environment {
        // Onde o .NET vai instalar as tools globais
        DOTNET_CLI_HOME   = '/tmp/dotnet_home'
        // Credencial “Secret Text” contendo o token do SonarQube (você já criou manualmente)
        SONAR_TOKEN       = credentials('sonar-token')
        // URL do SonarQube que já está rodando (não vamos mais subir container aqui)
        SONAR_HOST_URL    = 'http://localhost:9000'
        SONAR_PROJECT_KEY = 'dotnet_test_old'
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

        stage('SonarQube Analysis') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:7.0'
                    args  '-u root:root'
                }
            }
            steps {
                echo 'Executando análise SonarQube (begin → build → end) num único bloco...'
                sh '''#!/usr/bin/env bash
                   set -e

                   # 1) Instala (ou mantém instalada) a ferramenta global dotnet-sonarscanner
                   dotnet tool install --global dotnet-sonarscanner --version 5.0.0 || true

                   # 2) Ajusta o PATH para incluir a pasta correta (/tmp/dotnet_home/.dotnet/tools)
                   export PATH="$PATH:$DOTNET_CLI_HOME/.dotnet/tools"

                   # 3) Inicia o SonarScanner (begin)
                   dotnet sonarscanner begin \
                     /k:"'"$SONAR_PROJECT_KEY"'" \
                     /d:sonar.host.url="'"$SONAR_HOST_URL"'" \
                     /d:sonar.login="'"$SONAR_TOKEN"'"

                   # 4) Rebuild da solução para coletar métricas
                   dotnet build dotnet_test_old.csproj --configuration Release

                   # 5) Finaliza/anexa resultados ao SonarQube (end)
                   dotnet sonarscanner end /d:sonar.login="'"$SONAR_TOKEN"'"
                '''
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
                echo 'Executando testes automatizados (dotnet test)...'
                sh 'dotnet test Tests/dotnet_test_old.Tests.csproj --logger "trx;LogFileName=teste-results.trx" --results-directory TestResults'

                echo 'Ajustando permissões para TestResults...'
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