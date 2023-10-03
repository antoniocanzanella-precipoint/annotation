function VerifyExitCode
{
    if ($LastExitCode -ne 0)
    {
        throw "Exit code is not 0"
    }
}

function VerifyReady($url)
{
    docker run --rm --network precicloud_backend busybox wget $url --no-check-certificate
    VerifyExitCode

    Write-Host "Health check for '$url' was ok." -ForegroundColor Green;
}

$maxTry = 12

For ($i = 0; $i -le $maxTry; $i++) {

    try
    {

        $url = "http://loc.imgmgt.api.preci.lab:5000/health/ready";
        VerifyReady $url;

        $url = "http://loc.wsi001.api.preci.lab:5030/health/ready";
        VerifyReady $url;

        $url = "http://loc.upload.api.preci.lab:5020/health/ready";
        VerifyReady $url;

        $url = "http://loc.annota.api.preci.lab:5050/health/ready";
        VerifyReady $url;

        exit 0

    }
    catch
    {
        Write-Host "Health check failed for '$url': this was the $i. try of $maxTry maximum health check attempts." -ForegroundColor Red
        Start-Sleep -Seconds 5
    }
}

exit 1