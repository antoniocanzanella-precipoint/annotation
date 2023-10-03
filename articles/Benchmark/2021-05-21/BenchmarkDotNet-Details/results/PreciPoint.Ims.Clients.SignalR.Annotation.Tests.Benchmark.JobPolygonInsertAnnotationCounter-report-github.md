``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19042.985 (20H2/October2020Update)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.203
  [Host]     : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT
  Job-HUMLPW : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT

InvocationCount=1  RunStrategy=Throughput  UnrollFactor=1  

```

|                   Method |       Mean |    Error |   StdDev |
|------------------------- |-----------:|---------:|---------:|
|   Polygon_AddCounter_100 |   155.5 ms |  6.03 ms | 17.00 ms |
|  Polygon_AddCounter_1000 |   292.0 ms |  5.80 ms | 15.09 ms |
|  Polygon_AddCounter_5000 |   939.1 ms | 18.12 ms | 21.58 ms |
| Polygon_AddCounter_10000 | 1,808.7 ms | 35.30 ms | 42.02 ms |
