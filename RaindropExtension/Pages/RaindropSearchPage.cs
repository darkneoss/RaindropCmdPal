using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace RaindropExtension;

internal sealed partial class RaindropSearchPage : DynamicListPage
{
    private readonly RaindropClient _client = new();
    private readonly SettingsManager _settings;
    private readonly object _gate = new();
    private readonly Dictionary<string, (DateTimeOffset Expires, IListItem[] Items)> _cache = new();
    private CancellationTokenSource? _pending;
    private IListItem[] _items = [];
    private string _token = string.Empty;
    private long _generation;

    public RaindropSearchPage(SettingsManager settings)
    {
        _settings = settings;
        Icon = IconHelpers.FromRelativePath(Icons.Logo);
        Title = "Search Raindrop";
        Name = "Search";
        PlaceholderText = "Search bookmarks";
        _settings.Settings.SettingsChanged += (_, _) => ClearCache();
        UpdateSearchText("", "");
    }

    public void ClearCache()
    {
        lock (_gate)
        {
            _cache.Clear();
        }

        UpdateSearchText(SearchText, SearchText);
    }

    public override IListItem[] GetItems()
    {
        lock (_gate)
        {
            return _items;
        }
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        CancellationTokenSource? previous;
        CancellationTokenSource? current = null;
        long generation;
        var query = (newSearch ?? string.Empty).Trim();
        var token = _settings.Token.Trim();
        lock (_gate)
        {
            previous = _pending;
            _pending = null;
            generation = ++_generation;
            if (_token != token)
            {
                _cache.Clear();
                _token = token;
            }

            if (token.Length == 0)
            {
                _items = [new ListItem(_settings.Settings.SettingsPage) { Title = "Set your API token in settings" }];
            }
            else if (query.Length == 0)
            {
                _items = [new ListItem(new NoOpCommand()) { Title = "Type to search your bookmarks" }];
            }
            else if (_cache.TryGetValue(query, out var cached) && cached.Expires > DateTimeOffset.UtcNow)
            {
                _items = cached.Items;
            }
            else
            {
                current = new CancellationTokenSource();
                _pending = current;
                _items = [];
            }

            IsLoading = current is not null;
            previous?.Cancel();
        }

        RaiseItemsChanged();
        if (current is not null)
        {
            _ = SearchAsync(query, token, generation, current);
        }
    }

    private async Task SearchAsync(string query, string token, long generation, CancellationTokenSource source)
    {
        try
        {
            await Task.Delay(300, source.Token);
            var bookmarks = await _client.SearchAsync(query, token, source.Token);
            var items = bookmarks.Select(CreateItem).ToArray();
            IListItem[] result = items.Length == 0
                ? [new ListItem(new NoOpCommand()) { Title = "No results" }]
                : items;
            lock (_gate)
            {
                if (generation != _generation)
                {
                    return;
                }

                var now = DateTimeOffset.UtcNow;
                foreach (var expired in _cache.Where(e => e.Value.Expires <= now).Select(e => e.Key).ToList())
                {
                    _cache.Remove(expired);
                }

                // Don't cache empty results: a bookmark saved a minute ago should show up without waiting out the TTL.
                if (items.Length > 0)
                {
                    _cache[query] = (now.Add(_settings.Ttl), result);
                }
            }

            Publish(generation, result);
        }
        catch (RaindropUnauthorizedException)
        {
            Publish(generation, [new ListItem(_settings.Settings.SettingsPage) { Title = "Invalid API token" }]);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Publish(generation, [new ListItem(new NoOpCommand()) { Title = "Search failed", Subtitle = ex.Message }]);
        }
        finally
        {
            source.Dispose();
        }
    }

    private void Publish(long generation, IListItem[] items)
    {
        lock (_gate)
        {
            if (generation != _generation)
            {
                return;
            }

            _pending = null;
            _items = items;
            IsLoading = false;
        }

        RaiseItemsChanged();
    }

    private static IListItem CreateItem(Raindrop item)
    {
        var tags = item.Tags.Select(t => (ITag)new Tag(t)).ToArray();
        var hasCover = Uri.TryCreate(item.Cover, UriKind.Absolute, out var uri)
            && (uri.Scheme == "https" || uri.Scheme == "http");
        var editUrl = "https://app.raindrop.io/my/0/item/" + Uri.EscapeDataString(item.Id) + "/edit";
        return new ListItem(new OpenUrlCommand(item.Link))
        {
            Title = item.Title,
            Subtitle = string.IsNullOrWhiteSpace(item.Excerpt) ? item.Domain : item.Excerpt,
            Tags = tags,
            Icon = hasCover ? new IconInfo(item.Cover) : IconHelpers.FromRelativePath(Icons.Logo),
            MoreCommands =
            [
                new CommandContextItem(new CopyTextCommand(item.Link)) { Title = "Copy link" },
                new CommandContextItem(new OpenUrlCommand(editUrl)) { Title = "Open in Raindrop" },
            ],
        };
    }
}
