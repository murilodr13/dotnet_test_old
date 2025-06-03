pipeline {
    agent {
        docker {
            image 'mcr.microsoft.com/dotnet/sdk:7.0'
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
        stage('Run') {
            steps {
                echo 'Executando aplicação Hello World...'
                sh 'dotnet run --configuration Release'
            }
        }
    }
}
