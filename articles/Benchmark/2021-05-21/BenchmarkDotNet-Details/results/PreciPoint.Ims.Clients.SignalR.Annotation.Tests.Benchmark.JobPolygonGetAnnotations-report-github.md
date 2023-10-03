``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19042.985 (20H2/October2020Update)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.203
  [Host]     : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT
  Job-ULSDTJ : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT

RunStrategy=Throughput  

```

|                                Method |         Mean |      Error |     StdDev |
|-------------------------------------- |-------------:|-----------:|-----------:|
|   ReadSlideAnnotationPolygon_100Point |     14.27 ms |   0.285 ms |   0.469 ms |
|  ReadSlideAnnotationPolygon_1000Point |    178.37 ms |   2.613 ms |   2.316 ms |
|  ReadSlideAnnotationPolygon_5000Point |  2,698.90 ms |  50.519 ms |  49.617 ms |
| ReadSlideAnnotationPolygon_10000Point | 10,271.35 ms | 195.734 ms | 217.558 ms |
