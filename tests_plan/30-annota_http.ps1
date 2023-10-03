param(
    [string]$HostSlideDirectory
)

Import-Module ./Infrastructure/modules/PreciPoint.DockerTestRunner/DockerTestRunner.psm1

Invoke-DockerTestRunner -Dockerfile "src/Clients/Http/Http.Annotation.Tests/http-tests.Dockerfile" -ImageName "annota.http.tests" -DotnetCoreTestEnv "docker" -DockerNetwork "precicloud_backend" -HostSlideDirectory $HostSlideDirectory