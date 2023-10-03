``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19042.985 (20H2/October2020Update)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.203
  [Host]     : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT
  Job-HUMLPW : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT

InvocationCount=1  RunStrategy=Throughput  UnrollFactor=1  

```

|                   Method |      Mean |    Error |    StdDev |
|------------------------- |----------:|---------:|----------:|
|   InsertPolygon_100Point |  16.01 ms | 1.086 ms |  3.115 ms |
|  InsertPolygon_1000Point |  35.80 ms | 1.284 ms |  3.644 ms |
|  InsertPolygon_5000Point | 154.70 ms | 5.082 ms | 14.581 ms |
| InsertPolygon_10000Point | 243.71 ms | 4.868 ms | 11.475 ms |
