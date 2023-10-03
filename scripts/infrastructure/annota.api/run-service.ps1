try
{
    $service = "annota.api"
    $submoduleDir = "../../../Infrastructure/source"
    $executionDir = Get-Location

    Set-Location -Path $submoduleDir

    $files = "-f ./services/$service/service.yml -f ../../scripts/infrastructure/$service/service.yml"

    Invoke-Expression -Command "docker compose $files pull"
    Invoke-Expression -Command "docker compose $files up --build"

    exit 0
}
catch
{
    Write-Host "An error occurred:" -ForegroundColor Red
    Write-Host $_ -ForegroundColor Red

    exit 1
}
finally
{
    Set-Location -Path $executionDir
    docker stop "loc.$service.preci.lab"
    docker rm "loc.$service.preci.lab"
}