pipeline {
    agent none

    environment {
        DOTNET_CLI_HOME       = '/tmp/dotnet_home'
        SONAR_ADMIN_NEW_PASS  = credentials('sonar-admin-newpass')  
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
                echo 'Realizando checkout do repositório…'
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
                echo 'Restaurando pacotes NuGet…'
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
                echo 'Compilando aplicação em Release…'
                sh 'dotnet build dotnet_test_old.csproj --configuration Release'
            }
        }

        stage('Start SonarQube') {
            agent any
            steps {
                echo 'Removendo container SonarQube antigo (se existir)…'
                sh 'docker rm -f sonarqube || true'

                echo 'Iniciando novo container SonarQube…'
                sh 'docker run -d --name sonarqube -u root:root -p 9000:9000 sonarqube:lts'

                echo 'Aguardando SonarQube ficar disponível…'
                // Em vez de sleep fixo, podemos usar um loop que testa HTTP 200 repetidamente:
                sh '''
                    for i in $(seq 1 20); do
                      echo "Tentativa $i de conectar no SonarQube..."
                      code=$(curl --connect-timeout 5 -s -o /dev/null -w "%{http_code}" http://localhost:9000)
                      if [ "$code" -ge 200 ] && [ "$code" -lt 300 ]; then
                        echo "SonarQube subiu com sucesso!"
                        exit 0
                      fi
                      echo "Ainda não está pronto (http_code=$code). Aguardando 5s..."
                      sleep 5
                    done
                    echo "Erro: SonarQube não respondeu em tempo hábil."
                    exit 1
                '''
            }
        }

        stage('Change Sonar Admin Password') {
            agent any
            steps {
                echo 'Alterando senha padrão “admin” para a nova senha segura…'
                sh '''
                    # Se você guardou a senha antiga em credencial, use "-u admin:${SONAR_ADMIN_OLD_PASS}"
                    # Caso confie que a senha antiga é “admin”, basta:
                    docker run --rm curlimages/curl:latest -s -u admin:admin \
                      -X POST "http://localhost:9000/api/users/change_password" \
                      -d "login=admin" \
                      -d "previousPassword=admin" \
                      -d "newPassword=${SONAR_ADMIN_NEW_PASS}"
                '''
            }
        }

        stage('Generate Sonar Token') {
            agent any
            steps {
                echo 'Gerando token no SonarQube via API…'
                sh '''
                    RESPONSE=$(docker run --rm curlimages/curl:latest -s -u admin:${SONAR_ADMIN_NEW_PASS} \
                      -X POST "http://localhost:9000/api/user_tokens/generate" \
                      -d "name=jenkins-auto-token")
                    
                    if [ -z "$RESPONSE" ]; then
                      echo "ERRO: resposta vazia ao gerar token SonarQube. Verifique se a senha já foi alterada com sucesso."
                      exit 1
                    fi

                    # Extrai o campo "token" do JSON retornado: {"name":"jenkins-auto-token","token":"<o_token_aqui>"}
                    TOKEN=$(echo "$RESPONSE" | grep -o '"token":"[^"]*' | cut -d'"' -f4)
                    if [ -z "$TOKEN" ]; then
                      echo "ERRO: não conseguiu extrair token do JSON:"
                      echo "$RESPONSE"
                      exit 1
                    fi

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
                sh 'export SONAR_TOKEN=$(cat sonar-generated.token)'

                echo 'Instalando SonarScanner para .NET…'
                sh 'dotnet tool install --global dotnet-sonarscanner --version 5.0.0'
                sh 'export PATH="$PATH:/root/.dotnet/tools"'

                echo 'Executando dotnet-sonarscanner begin…'
                sh """
                   dotnet sonarscanner begin \
                     /k:"dotnet_test_old" \
                     /d:sonar.host.url="http://localhost:9000" \
                     /d:sonar.login="$SONAR_TOKEN"
                """

                echo 'Rebuild da solução para coletar dados de análise…'
                sh 'dotnet build dotnet_test_old.csproj --configuration Release'

                echo 'Finalizando análise SonarQube (dotnet-sonarscanner end)…'
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
                echo 'Executando testes com dotnet test…'
                sh 'dotnet test Tests/dotnet_test_old.Tests.csproj --logger "trx;LogFileName=teste-results.trx" --results-directory TestResults'
                echo 'Ajustando permissões para publicar resultados…'
                sh '''
                  chmod -R a+rw TestResults
                  chown -R 1000:1000 TestResults
                '''
            }
            post {
                always {
                    echo 'Publicando resultados de teste (.trx) no Jenkins…'
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
                echo 'Executando dotnet publish e compactando em tar.gz…'
                sh '''
                  mkdir -p artifacts
                  dotnet publish dotnet_test_old.csproj -c Release -o publish
                  tar -czf artifacts/dotnet_test_old.tar.gz -C publish .
                '''
            }
            post {
                success {
                    echo 'Arquivando artifacts/dotnet_test_old.tar.gz no Jenkins…'
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
                echo 'Executando dotnet run (Hello World)…'
                sh 'dotnet run --project dotnet_test_old.csproj --configuration Release'
            }
        }

        stage('Stop SonarQube') {
            agent any
            steps {
                echo 'Parando e removendo container SonarQube…'
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