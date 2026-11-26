# Data model: Review leftovers

There is no persisted data. The feature works with two values:

- **Problem example provider**: an `IExampleProvider<ProblemResult>`. Its `Status` defines the documented status.
- **DurationBucketBoundaries**: 14 ascending values in seconds, from 0.005 to 10, exposed read-only.
