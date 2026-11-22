# Baseline – Viana.Results 2.1.0

- **Core version**: 2.1.0
- **Date**: 2026-11-21
- **Commit**: `c0a5597` (plus the benchmarks project, which adds no core changes)
- **Command**: `dotnet run -c Release --project benchmarks/Viana.Results.Benchmarks -- --filter '*' --exporters github`

Absolute times are only comparable on the same machine; allocations are the stable signal.

## Environment

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26300.9974)
11th Gen Intel Core i7-11390H 3.40GHz (Max: 2.92GHz), 1 CPU, 8 logical and 4 physical cores
.NET SDK 10.0.401
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4


```

## ResultBenchmarks

| Method              | Mean      | Error     | StdDev    | Gen0   | Allocated |
|-------------------- |----------:|----------:|----------:|-------:|----------:|
| Success             |  4.239 ns | 0.1509 ns | 0.2014 ns | 0.0064 |      40 B |
| SuccessWithValue    |  3.988 ns | 0.1603 ns | 0.2140 ns | 0.0051 |      32 B |
| Failure             | 29.691 ns | 0.6743 ns | 1.0696 ns | 0.0395 |     248 B |
| FailureTyped        | 28.092 ns | 0.6273 ns | 1.5967 ns | 0.0382 |     240 B |
| ImplicitFromValue   |  4.264 ns | 0.1398 ns | 0.2176 ns | 0.0064 |      40 B |
| ImplicitFromFailure |  4.378 ns | 0.1672 ns | 0.4345 ns | 0.0064 |      40 B |

## ProblemBenchmarks

| Method              | Mean     | Error    | StdDev   | Gen0   | Allocated |
|-------------------- |---------:|---------:|---------:|-------:|----------:|
| CtorWithTitle       | 23.83 ns | 1.027 ns | 2.964 ns | 0.0331 |     208 B |
| CtorWithoutTitle    | 23.30 ns | 0.544 ns | 0.953 ns | 0.0331 |     208 B |
| BuilderWithTitle    | 30.15 ns | 0.680 ns | 1.059 ns | 0.0459 |     288 B |
| BuilderWithoutTitle | 30.74 ns | 0.681 ns | 1.296 ns | 0.0459 |     288 B |

## JsonBenchmarks

| Method                    | Mean     | Error    | StdDev   | Gen0   | Allocated |
|-------------------------- |---------:|---------:|---------:|-------:|----------:|
| Serialize                 | 231.8 ns |  4.75 ns |  8.80 ns | 0.0215 |     136 B |
| SerializeWithExtensions   | 440.5 ns | 12.08 ns | 32.25 ns | 0.0443 |     280 B |
| Deserialize               | 334.9 ns |  6.78 ns | 10.75 ns | 0.0763 |     480 B |
| DeserializeWithExtensions | 772.3 ns | 15.55 ns | 43.62 ns | 0.1526 |     960 B |

