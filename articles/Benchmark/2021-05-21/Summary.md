# Annotation Benchmark 21-05-2021

``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19042.985 (20H2/October2020Update)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores

  [Host]     : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT
  DefaultJob : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT
```

## SignalR Connection (Start and Disconnect)

```console

|       Method |     Mean |    Error |   StdDev |
|------------- |---------:|---------:|---------:|
| StartAndStop | 38.94 ms | 1.347 ms | 3.929 ms |
```

## Get Annotation By Id (Polygons)

the client is receiving one annotation of the specified number of points

RunStrategy=Throughput

```console

|                 Method |         Mean |      Error |     StdDev |       Median |
|----------------------- |-------------:|-----------:|-----------:|-------------:|
|   ReadPolygon_100Point |     6.193 ms |  0.1314 ms |  0.3662 ms |     6.104 ms |
|  ReadPolygon_1000Point |    28.541 ms |  0.5262 ms |  0.4665 ms |    28.608 ms |
|  ReadPolygon_5000Point |   297.912 ms |  5.9215 ms | 14.5256 ms |   292.986 ms |
| ReadPolygon_10000Point | 1,026.299 ms | 11.8390 ms | 10.4950 ms | 1,026.156 ms |
```

## Get Annotations (Polygons)

the client is receiving all annotations of a specified slide image (in this case 10 annotation, for example
ReadSlideAnnotationPolygon_10000Point means 10 annotation of 10000 points each)

RunStrategy=Throughput

```console

|                                Method |         Mean |      Error |     StdDev |
|-------------------------------------- |-------------:|-----------:|-----------:|
|   ReadSlideAnnotationPolygon_100Point |     14.27 ms |   0.285 ms |   0.469 ms |
|  ReadSlideAnnotationPolygon_1000Point |    178.37 ms |   2.613 ms |   2.316 ms |
|  ReadSlideAnnotationPolygon_5000Point |  2,698.90 ms |  50.519 ms |  49.617 ms |
| ReadSlideAnnotationPolygon_10000Point | 10,271.35 ms | 195.734 ms | 217.558 ms |
```

## Insert Annotation (Polygon)

the client is inserting one annotation of the specified points number

RunStrategy=Throughput

```console

|                   Method |      Mean |    Error |    StdDev |
|------------------------- |----------:|---------:|----------:|
|   InsertPolygon_100Point |  16.01 ms | 1.086 ms |  3.115 ms |
|  InsertPolygon_1000Point |  35.80 ms | 1.284 ms |  3.644 ms |
|  InsertPolygon_5000Point | 154.70 ms | 5.082 ms | 14.581 ms |
| InsertPolygon_10000Point | 243.71 ms | 4.868 ms | 11.475 ms |
```

## Update Annotation (Polygon)

the client update one annotation with specified number points

RunStrategy=Throughput

```console

|                   Method |     Mean |    Error |   StdDev |   Median |
|------------------------- |---------:|---------:|---------:|---------:|
|   UpdatePolygon_100Point | 13.32 ms | 0.635 ms | 1.853 ms | 12.92 ms |
|  UpdatePolygon_1000Point | 22.93 ms | 1.464 ms | 4.248 ms | 22.16 ms |
|  UpdatePolygon_5000Point | 44.09 ms | 1.166 ms | 3.288 ms | 42.81 ms |
| UpdatePolygon_10000Point | 72.89 ms | 1.455 ms | 3.908 ms | 72.74 ms |
```

## Insert Annotation Counter (Polygon)

the client is inserting the specified number of counters to a specific poligon

annotation service is checking that each point is contained into annotation and flush, only the contained ones, into db

RunStrategy=Throughput

```console

|                   Method |       Mean |    Error |   StdDev |
|------------------------- |-----------:|---------:|---------:|
|   Polygon_AddCounter_100 |   155.5 ms |  6.03 ms | 17.00 ms |
|  Polygon_AddCounter_1000 |   292.0 ms |  5.80 ms | 15.09 ms |
|  Polygon_AddCounter_5000 |   939.1 ms | 18.12 ms | 21.58 ms |
| Polygon_AddCounter_10000 | 1,808.7 ms | 35.30 ms | 42.02 ms |
```

## Update Annotation Counter (Polygon)

the client update the specified number of counters to a specific poligon

this means that the old structure is erased and the new one is persisted

service is checking that each point is contained into annotation are and flush into db only the contained ones

RunStrategy=Throughput

```console

|                      Method |       Mean |    Error |   StdDev |
|---------------------------- |-----------:|---------:|---------:|
|   Polygon_UpdateCounter_100 |   147.9 ms |  2.83 ms |  3.68 ms |
|  Polygon_UpdateCounter_1000 |   494.1 ms |  9.75 ms | 22.40 ms |
|  Polygon_UpdateCounter_5000 | 2,093.1 ms | 38.49 ms | 44.33 ms |
| Polygon_UpdateCounter_10000 | 4,006.5 ms | 40.09 ms | 37.50 ms |
```
