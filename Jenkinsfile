pipeline {
    agent {
        docker {
            image 'mcr.microsoft.com/dotnet/sdk:7.0'
            args '-u root:root'
        }
    }
    stages {
        stage('Checkout') {
            steps {
                echo 'Realizando checkout do repositório...'
                checkout scm
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
                sh '''
                    dotnet tool install --global trx2junit --version 1.3.0
                    export PATH="$PATH:~/.dotnet/tools"
                    
                    dotnet test Tests/dotnet_test_old.Tests.csproj --logger \"trx;LogFileName=teste-results.trx\" --results-directory TestResults
                    
                    trx2junit --trx TestResults/teste-results.trx --output TestResults/junit.xml
                '''
            }
            post {
                always {
                    junit 'TestResults/junit.xml'
                }
            }
        }
        stage('BuildArtifact'){
            steps {
                echo 'Empacotando executáveis em ZIP...'
                sh '''
                    mkdir -p artifacts
                    dotnet publish dotnet_test_old.csproj -c Release -o publish
                    zip -r artifacts/dotnet_test_old.zip publish
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
                sh 'dotnet run --configuration Release'
            }
        }
    }
}
