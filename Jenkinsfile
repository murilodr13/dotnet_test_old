pipeline {
    agent {
        docker {
            image 'mcr.microsoft.com/dotnet/sdk:5.0'
        }
    }
    stages {
        stage('Build') {
            steps {
                // Clonar o repositório do GitHub
                git 'https://github.com/murilodr13/dotnet_test'
                // Navegar para o diretório do projeto
                dir('.') {
                    sh 'dotnet restore' // Restaurar as dependências do projeto
                    sh 'dotnet build' // Compilar o projeto
                }
            }
        }
        stage('Test') {
            steps {
                sh 'dotnet test' // Executar os testes
            }
        }
        stage('Quality') {
            steps {
                sh 'dotnet tool install -g dotnet-sonarscanner' // Instalar o SonarScanner
                sh 'dotnet sonarscanner begin /k:"my-project-key" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="my-token"' // Iniciar análise de qualidade com SonarScanner
                sh 'dotnet build' // Compilar novamente com análise de qualidade ativada
                sh 'dotnet sonarscanner end /d:sonar.login="my-token"' // Encerrar análise de qualidade com SonarScanner
            }
        }
        stage('Generate Reports') {
            steps {
                // Aqui você pode adicionar etapas para gerar relatórios, como cobertura de código, análise estática, etc.
                // Por exemplo:
                sh 'dotnet report-generator' // Comando para gerar relatórios de cobertura de código
                // Outras etapas para gerar outros relatórios
            }
        }
        stage('Save Artifacts') {
            steps {
                // Aqui você pode adicionar etapas para salvar os artefatos gerados durante o pipeline.
                // Por exemplo, você pode salvar os relatórios gerados anteriormente.
                // Por exemplo:
                sh 'mkdir artifacts' // Criar uma pasta para os artefatos
                sh 'cp -R reports artifacts/' // Copiar os relatórios para a pasta de artefatos
                // Outras etapas para salvar outros artefatos
            }
        }
        stage('Deploy') {
            steps {
                // Aqui você pode adicionar etapas para implantar o aplicativo em um ambiente de produção.
                // Por exemplo, você pode usar ferramentas como Docker ou Azure DevOps para realizar a implantação.
                // Por exemplo:
                sh 'docker build -t my-app .' // Construir uma imagem Docker do aplicativo
                sh 'docker run -d -p 80:80 my-app' // Executar o aplicativo em um contêiner Docker
                // Outras etapas para implantar o aplicativo
            }
        }
    }
}
