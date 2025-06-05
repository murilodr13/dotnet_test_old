pipeline {
    agent {
        docker {
            image 'mcr.microsoft.com/dotnet/sdk:7.0'
            args '-u root:root'
        }
    }
    environment {
        DOTNET_CLI_HOME='/tmp/dotnet_home'
    }
    stages {
        stage('Checkout') {
            steps {
                echo 'Realizando checkout do repositório...'
                checkout scm
            }
        }
        stage('Prepare .NET Home'){
            steps {
                echo 'Preparando diretório .NET Home...'
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
                echo 'Compilando aplicação...'
                sh 'dotnet build --configuration Release'
            }
        }
        stage('Tests') {
            steps {
                sh 'dotnet test Tests/dotnet_test_old.Tests.csproj --logger \"trx;LogFileName=teste-results.trx\" --results-directory TestResults'
                sh '''
                    chmod -R a+rw TestResults
                    chown -R 1000:1000 TestResults
                '''
            }
            post {
                always {
                    mstest testResultsFile: 'TestResults/*.trx'
                }
            }
        }
        stage('BuildArtifact'){
            steps {
                echo 'Empacotando executáveis em ZIP...'
                sh '''
                    mkdir -p artifacts
                    dotnet publish dotnet_test_old.csproj -c Release -o publish
                    tar -czf artifacts/dotnet_test_old.tar.gz -C publish .
                '''
            }
            post {
                success{
                    archiveArtifacts artifacts: 'artifacts/*.zip', fingerprint: true
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
            echo 'Pipeline executado com sucesso!'
        }
        failure {
            echo 'Pipeline falhou. Verifique os logs para detalhes.'
        }
    }
}
