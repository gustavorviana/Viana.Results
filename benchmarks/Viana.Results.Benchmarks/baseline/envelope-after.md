# Envelope results (014): after

ShortRun job (`--job short`), Release, net10.0. Run after 014 (envelope shaping only when the contract writes Status/Problem/Extensions; envelope type check cached).

## AspNetCoreBenchmarks

| Method            | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------ |---------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| MvcSuccess        | 1.386 μs | 1.3221 μs | 0.0725 μs |  1.00 |    0.06 | 0.4349 | 0.0019 |   2.67 KB |        1.00 |
| AspNetCoreSuccess | 1.918 μs | 0.3283 μs | 0.0180 μs |  1.39 |    0.06 | 0.5226 | 0.0038 |   3.22 KB |        1.20 |
| MvcFailure        | 1.477 μs | 1.1288 μs | 0.0619 μs |  1.07 |    0.06 | 0.4158 | 0.0019 |   2.55 KB |        0.96 |
| AspNetCoreFailure | 2.279 μs | 2.5917 μs | 0.1421 μs |  1.65 |    0.12 | 0.6065 | 0.0038 |   3.73 KB |        1.39 |
| MinimalSuccess    | 1.753 μs | 1.6425 μs | 0.0900 μs |  1.27 |    0.08 | 0.4883 | 0.0019 |      3 KB |        1.12 |
| MinimalFailure    | 1.838 μs | 0.9990 μs | 0.0548 μs |  1.33 |    0.07 | 0.6065 |      - |   3.72 KB |        1.39 |
| MvcPaged          | 2.997 μs | 0.6099 μs | 0.0334 μs |  2.17 |    0.10 | 0.5379 | 0.0038 |   3.31 KB |        1.24 |
| AspNetCorePaged   | 3.713 μs | 0.6623 μs | 0.0363 μs |  2.68 |    0.12 | 0.6256 |      - |   3.86 KB |        1.44 |
| MinimalPaged      | 3.315 μs | 1.0809 μs | 0.0592 μs |  2.40 |    0.11 | 0.5913 | 0.0038 |   3.64 KB |        1.36 |

## MvcBenchmarks

| Method  | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|-------- |---------:|----------:|----------:|-------:|-------:|----------:|
| Success | 1.487 μs | 0.6740 μs | 0.0369 μs | 0.4349 | 0.0019 |   2.67 KB |
| Failure | 1.214 μs | 0.9351 μs | 0.0513 μs | 0.4158 | 0.0019 |   2.55 KB |
| Paged   | 3.057 μs | 0.5535 μs | 0.0303 μs | 0.5379 | 0.0038 |   3.31 KB |
