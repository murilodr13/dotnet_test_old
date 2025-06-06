pipeline {
    agent none

    environment {
        DOTNET_CLI_HOME = '/tmp/dotnet_home'
        // Caso você já tenha configurado manualmente um credential "sonar-token" no Jenkins,
        // e queira usá-lo diretamente, mantenha esta linha. Se preferir gerar via API,
        // remova ou comente esta credencial e leia o token gerado em disco (ver stage Generate Sonar Token).
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

                echo 'Aguardando SonarQube ficar disponível…'
                // Opção “mais simples”: sleep mais longo
                // you can replace with: sleep 120
                // Mas o recomendado em produção é esperar via curl em loop:
                sh '''
                    # Tenta até 20 vezes, a cada 5 segundos, até receber HTTP 200 do Sonar
                    for i in $(seq 1 20); do
                      echo "Tentativa $i para conectar no SonarQube..."
                      if curl --connect-timeout 5 -s -o /dev/null -w "%{http_code}" http://localhost:9000 | grep -q "^2"; then
                        echo "SonarQube está pronto!"
                        exit 0
                      fi
                      echo "Ainda não disponível, aguardando 5s…"
                      sleep 5
                    done
                    echo "SonarQube não subiu em 100 segundos. Abandonando."
                    exit 1
                '''
            }
        }

        stage('Generate Sonar Token') {
            agent any
            steps {
                echo 'Gerando token no SonarQube via API…'
                sh '''
                    # Aqui chamamos a API de geração de token. 
                    # Se você já criou e salvou manualmente no Jenkins (credencial 'sonar-token'),
                    # basta comentar estas linhas e usar diretamente SONAR_TOKEN.
                    RESPONSE=$(docker run --rm curlimages/curl:latest -s -u admin:admin \
                        -X POST "http://localhost:9000/api/user_tokens/generate" \
                        -d "name=jenkins-auto-token")
                    
                    if [ -z "$RESPONSE" ]; then
                      echo "ERRO: resposta vazia ao gerar token SonarQube. Verifique se o Sonar subiu corretamente."
                      exit 1
                    fi
                    
                    TOKEN=$(echo "$RESPONSE" | grep -o '"token":"[^"]*' | cut -d'"' -f4)
                    if [ -z "$TOKEN" ]; then
                      echo "ERRO: não conseguiu extrair campo 'token' do JSON retornado:"
                      echo "$RESPONSE"
                      exit 1
                    fi

                    echo "Token gerado pelo SonarQube: $TOKEN"
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
                // Caso queira usar o token gerado dinamicamente, lê-lo de arquivo:
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

                echo 'Ajustando permissões em TestResults para publicação…'
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