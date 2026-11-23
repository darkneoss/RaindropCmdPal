using System.Collections.Generic;

namespace RaindropExtension;

internal sealed record Raindrop(
    string Id,
    string Title,
    string Excerpt,
    string Link,
    string Cover,
    string Domain,
    IReadOnlyList<string> Tags);
