# Jellyfin Plugin Theme Songs

This plugin enhances your Jellyfin experience by automatically downloading theme songs for shows and movies.

## Features

- Automatic theme song detection and download
- Integration with Jellyfin's existing media management

## Requirements

- Jellyfin Server 10.10.0.0 or later

## Installation

### Installation via Repository URL

1. Open your Jellyfin Admin Dashboard
2. Navigate to **Dashboard** > **Plugins** > **Catalog**
3. Click **Add Repository**
4. Enter the following details:
   - **Repository Name**: Theme Songs Plugin
   - **Repository URL**: `https://github.com/attractivetoad/jellyfin-plugin-themesongs/raw/main/manifest.json`
5. Click **Save**
6. Go to **Dashboard** > **Plugins** > **Catalog**
7. Find "Theme Songs" in the plugin catalog
8. Click **Install**
9. Restart your Jellyfin server

## Configuration

After installation:

1. Go to **Dashboard** > **Plugins** > **Theme Songs**
2. Click on **Start Donwnload** to trigger a manual library theme song scan/download. 

The plugin will automatically scan your library every 12 hours.

## License

This project is licensed under the terms specified in the [LICENSE](LICENSE) file.

## Acknowledgments

- Thanks to the Jellyfin team for providing an excellent media server platform
- [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) - For YouTube video processing and audio extraction
- [YouTubeSearch](https://www.nuget.org/packages/YouTubeSearch) - For searching YouTube videos programmatically

---

**Note**: This plugin is not officially endorsed by the Jellyfin project. Use at your own risk and always backup your Jellyfin configuration before installing plugins.