``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19042.985 (20H2/October2020Update)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.203
  [Host]     : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT
  Job-ULSDTJ : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT

RunStrategy=Throughput  

```

|                 Method |         Mean |      Error |     StdDev |       Median |
|----------------------- |-------------:|-----------:|-----------:|-------------:|
|   ReadPolygon_100Point |     6.193 ms |  0.1314 ms |  0.3662 ms |     6.104 ms |
|  ReadPolygon_1000Point |    28.541 ms |  0.5262 ms |  0.4665 ms |    28.608 ms |
|  ReadPolygon_5000Point |   297.912 ms |  5.9215 ms | 14.5256 ms |   292.986 ms |
| ReadPolygon_10000Point | 1,026.299 ms | 11.8390 ms | 10.4950 ms | 1,026.156 ms |
