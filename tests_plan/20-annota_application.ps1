Import-Module ./Infrastructure/modules/PreciPoint.DockerTestRunner/DockerTestRunner.psm1

Invoke-DockerTestRunner -Dockerfile "src/Services/Annotation/Annotation.Application.Tests/application-tests.Dockerfile" -ImageName "annota.application.tests" -DotnetCoreTestEnv "docker"