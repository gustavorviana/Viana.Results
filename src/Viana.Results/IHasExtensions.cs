using System.Collections.Generic;

namespace Viana.Results;

public interface IHasExtensions
{
    IReadOnlyDictionary<string, object?> Extensions { get; }
}