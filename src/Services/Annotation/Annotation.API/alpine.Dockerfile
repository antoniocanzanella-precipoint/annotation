ARG DOTNET_VERSION=6.0-alpine3.17

FROM harbor.preci.cloud/precihub/sdk:$DOTNET_VERSION as copyStage
WORKDIR /src
COPY src/ ./
RUN find . -type f -not -name '*.csproj' -delete

FROM harbor.preci.cloud/precihub/sdk:$DOTNET_VERSION as build
WORKDIR /src

COPY ["Infrastructure/OfflineNugetPackages/*", "/mnt/OfflineNugetPackages/"]
COPY --from=copyStage /src .

WORKDIR /src/Services/Annotation/Annotation.API
RUN dotnet restore -s https://api.nuget.org/v3/index.json -s https://baget.ad.precipoint.de/v3/index.json -s /mnt/OfflineNugetPackages

WORKDIR /src
COPY src/ ./

WORKDIR /src/Services/Annotation/Annotation.API
RUN dotnet build -c Release -o /app/build --no-restore
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM harbor.preci.cloud/precihub/runtime/aspnet:$DOTNET_VERSION
LABEL maintainer=antonio.canzanella@precipoint.de

WORKDIR /app
COPY --from=build /app/publish ./
COPY --from=build /app/build/PreciPoint.Ims.*.xml ./

#new user without password, no home dir
RUN adduser -D -H aspnet_user aspnet_user && chown -R aspnet_user:aspnet_user /app

ENTRYPOINT ["sh", "-c", "update-ca-certificates && su -c 'dotnet PreciPoint.Ims.Services.Annotation.API.dll' aspnet_user"]