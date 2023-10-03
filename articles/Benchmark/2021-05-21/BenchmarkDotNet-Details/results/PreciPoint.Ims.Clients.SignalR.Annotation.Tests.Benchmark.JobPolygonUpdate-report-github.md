``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19042.985 (20H2/October2020Update)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.203
  [Host]     : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT
  Job-ULSDTJ : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT

RunStrategy=Throughput  

```

|                   Method |     Mean |    Error |   StdDev |   Median |
|------------------------- |---------:|---------:|---------:|---------:|
|   UpdatePolygon_100Point | 13.32 ms | 0.635 ms | 1.853 ms | 12.92 ms |
|  UpdatePolygon_1000Point | 22.93 ms | 1.464 ms | 4.248 ms | 22.16 ms |
|  UpdatePolygon_5000Point | 44.09 ms | 1.166 ms | 3.288 ms | 42.81 ms |
| UpdatePolygon_10000Point | 72.89 ms | 1.455 ms | 3.908 ms | 72.74 ms |
