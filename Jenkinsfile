pipeline {
    agent {
        docker {
            image 'mcr.microsoft.com/dotnet/sdk:7.0'
            args '-u root:root'
        }
    }
    environment {
        DOTNET_CLI_HOME = '/tmp/dotnet_home'
    }
    stages {
        stage('Checkout') {
            steps {
                echo 'Realizando checkout do repositório...'
                checkout scm
            }
        }

        stage('Prepare .NET Home') {
            steps {
                // Cria pasta para ~/.dotnet e evita que o .NET CLI tente escrever em /.
                sh 'mkdir -p $DOTNET_CLI_HOME'
            }
        }

        stage('Restore') {
            steps {
                echo 'Restaurando pacotes NuGet...'
                sh 'dotnet restore'
            }
        }

        stage('Build') {
            steps {
                echo 'Compilando aplicação em Release...'
                sh 'dotnet build dotnet_test_old.csproj --configuration Release'
            }
        }

        stage('Testes') {
            steps {
                echo 'Executando testes automatizados com dotnet test...'
                // 1) Gera o arquivo .trx em TestResults/
                sh 'dotnet test Tests/dotnet_test_old.Tests.csproj --logger "trx;LogFileName=teste-results.trx" --results-directory TestResults'
                // 2) Ajusta permissões para leitura/escrita e troca o dono para jenkins (UID 1000)
                sh '''
                  chmod -R a+rw TestResults
                  chown -R 1000:1000 TestResults
                '''
            }
            post {
                always {
                    // Publica o .trx usando o plugin MSTest
                    mstest testResultsFile: 'TestResults/*.trx'
                }
            }
        }

        stage('Empacotar Artefatos') {
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
                    // Arquiva o .tar.gz como artefato
                    archiveArtifacts artifacts: 'artifacts/dotnet_test_old.tar.gz', fingerprint: true
                }
            }
        }

        stage('Run') {
            steps {
                echo 'Executando aplicação Hello World...'
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