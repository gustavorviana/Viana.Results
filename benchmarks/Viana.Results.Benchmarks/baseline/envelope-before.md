# Envelope results (014): before

ShortRun job (`--job short`), Release, net10.0. Run before any 014 change.

## AspNetCoreBenchmarks

| Method            | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------ |---------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| MvcSuccess        | 1.467 μs | 1.6724 μs | 0.0917 μs |  1.00 |    0.08 | 0.4349 | 0.0019 |   2.67 KB |        1.00 |
| AspNetCoreSuccess | 1.881 μs | 0.9255 μs | 0.0507 μs |  1.29 |    0.08 | 0.5188 |      - |   3.22 KB |        1.20 |
| MvcFailure        | 1.190 μs | 0.6257 μs | 0.0343 μs |  0.81 |    0.05 | 0.4158 | 0.0019 |   2.55 KB |        0.96 |
| AspNetCoreFailure | 2.753 μs | 9.1263 μs | 0.5002 μs |  1.88 |    0.31 | 0.6065 | 0.0038 |   3.73 KB |        1.39 |
| MinimalSuccess    | 1.762 μs | 2.0581 μs | 0.1128 μs |  1.20 |    0.09 | 0.4883 | 0.0019 |      3 KB |        1.12 |
| MinimalFailure    | 1.878 μs | 0.7216 μs | 0.0396 μs |  1.28 |    0.07 | 0.6065 | 0.0019 |   3.72 KB |        1.39 |
| MvcPaged          | 2.985 μs | 1.6413 μs | 0.0900 μs |  2.04 |    0.12 | 0.5531 | 0.0038 |   3.39 KB |        1.27 |
| AspNetCorePaged   | 3.436 μs | 0.6791 μs | 0.0372 μs |  2.35 |    0.13 | 0.6409 |      - |   3.94 KB |        1.47 |
| MinimalPaged      | 3.473 μs | 0.7949 μs | 0.0436 μs |  2.37 |    0.13 | 0.6065 | 0.0038 |   3.72 KB |        1.39 |

## MvcBenchmarks

| Method  | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|-------- |---------:|----------:|----------:|-------:|-------:|----------:|
| Success | 2.277 μs | 8.5039 μs | 0.4661 μs | 0.4349 | 0.0019 |   2.67 KB |
| Failure | 1.115 μs | 0.2090 μs | 0.0115 μs | 0.4158 | 0.0019 |   2.55 KB |
| Paged   | 3.035 μs | 2.8082 μs | 0.1539 μs | 0.5531 | 0.0038 |   3.39 KB |
