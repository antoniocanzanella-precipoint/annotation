# Annotation

Contains the dedicated resources to enable backend provisioning of annotation service related to slide images.

## Clone this repository, the correct way.

With version 2.13 of Git and later, --recurse-submodules can be used instead of --recursive:

```shell
git clone --recurse-submodules -j8 https://precipointea.ad.precipoint.de/PreciCloud/Annotation.git
```

Note: -j8 is an optional performance optimization that became available in version 2.8, and fetches up to 8 submodules
at a time in parallel — see man git-clone

## Add new EF core migration

If you just want to add a new migration, usually you don't even need to connect to the database.

Inform us via `-- --skip-connection` and we won't even try to extract database related environment variables.

```shell
cd src\Services\Annotation\Annotation.Database
dotnet ef migrations add <MigrationName> -- --skip-connection
```

## Add Rider Settings/Scopes

this is useful to speed up the "Code clean up" functionality. Open rider setting and add a new Scope called "Exclude
Build" and paste the value.

```shell
!file:*/bin//*&&!file:*/obj//*&&!file[rider.module]:Infrastructure//*&&!file[rider.module]:CodeStyle//*&&!file:*/Migrations/*.cs&&!file:*/*.Localization//*&&!file[rider.module]:articles//*
```
