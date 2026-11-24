# Privacy policy: Raindrop for Command Palette

*Last updated: 2026-11-24*

Raindrop for Command Palette is an unofficial, open-source extension for PowerToys Command Palette. It is not
affiliated with or endorsed by Raindrop.io.

## What the extension handles

- **Your Raindrop.io API token**, which you paste into the extension settings. It is stored locally on your PC, in the
  extension's own app data folder, and is used only to authenticate requests to Raindrop.io.
- **Your search text**, which is sent to the Raindrop.io API to find your bookmarks.
- **Search results** (titles, excerpts, links, cover images) returned by Raindrop.io. They are kept in memory for the
  cache duration you choose and are discarded when Command Palette closes.

## Where data goes

The extension talks only to `https://api.raindrop.io`. Bookmark cover images are loaded from the URLs that
Raindrop.io returns. Raindrop.io's handling of your data is governed by its own privacy policy:
https://help.raindrop.io/privacy

## What the extension does not do

- It does not collect analytics or telemetry.
- It does not send your token, searches or bookmarks to the developer or any third party.
- It does not run any server of its own.

## Removing your data

Uninstalling the extension deletes its settings, including your token. You can also revoke the token at any time at
https://app.raindrop.io/settings/integrations.

## Contact

Questions or issues: https://github.com/darkneoss/RaindropCmdPal/issues
