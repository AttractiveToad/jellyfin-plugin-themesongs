using Jellyfin.Plugin.ThemeSongs.Services;
using Jellyfin.Plugin.ThemeSongs.Tasks;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.ThemeSongs
{
    /// <summary>
    /// Service registrator for the ThemeSongs plugin.
    /// </summary>
    public class ServiceRegistrator : IPluginServiceRegistrator
    {
        /// <summary>
        /// Registers services required by the ThemeSongs plugin.
        /// </summary>
        /// <param name="serviceCollection">The service collection to register services with.</param>
        /// <param name="applicationHost">The server application host instance.</param>
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            serviceCollection.AddScoped<ThemeDownloader>();
            serviceCollection.AddScoped<IScheduledTask, DownloadThemeSongsTask>();
        }
    }
}