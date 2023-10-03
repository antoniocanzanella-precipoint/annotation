ARG DOTNET_VERSION=6.0-alpine3.17

FROM harbor.preci.cloud/precihub/sdk:$DOTNET_VERSION as copyStage

WORKDIR /src

COPY src/ .
RUN find . -type f -not -name '*.csproj' -delete

FROM harbor.preci.cloud/precihub/sdk:$DOTNET_VERSION as build
LABEL maintainer=antonio.canzanella@precipoint.de
WORKDIR /src

COPY --from=copyStage /src .

WORKDIR /src/Clients/Http/Http.Annotation.Tests

RUN dotnet restore -s https://api.nuget.org/v3/index.json -s https://baget.ad.precipoint.de/v3/index.json

WORKDIR /src
COPY src/ .

ENTRYPOINT ["sh", "-c", "update-ca-certificates && dotnet test --no-restore --filter 'TestCategory=Integration'"]