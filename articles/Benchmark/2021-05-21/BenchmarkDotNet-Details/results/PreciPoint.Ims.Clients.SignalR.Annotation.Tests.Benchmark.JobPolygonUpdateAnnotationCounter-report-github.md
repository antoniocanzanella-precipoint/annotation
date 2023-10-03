``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19042.985 (20H2/October2020Update)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.203
  [Host]     : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT
  Job-ULSDTJ : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT

RunStrategy=Throughput  

```

|                      Method |       Mean |    Error |   StdDev |
|---------------------------- |-----------:|---------:|---------:|
|   Polygon_UpdateCounter_100 |   147.9 ms |  2.83 ms |  3.68 ms |
|  Polygon_UpdateCounter_1000 |   494.1 ms |  9.75 ms | 22.40 ms |
|  Polygon_UpdateCounter_5000 | 2,093.1 ms | 38.49 ms | 44.33 ms |
| Polygon_UpdateCounter_10000 | 4,006.5 ms | 40.09 ms | 37.50 ms |
