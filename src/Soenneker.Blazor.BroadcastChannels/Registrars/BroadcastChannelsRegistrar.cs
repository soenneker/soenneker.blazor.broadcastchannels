using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Blazor.BroadcastChannels.Abstract;
using Soenneker.Blazor.Utils.ModuleImport.Registrars;


namespace Soenneker.Blazor.BroadcastChannels.Registrars;

/// <summary>
/// Registration for the interop and utility services.
/// </summary>
public static class BroadcastChannelsRegistrar
{
    /// <summary>
    /// Adds <see cref="IBroadcastChannelsInterop"/> and <see cref="IBroadcastChannels"/> as scoped services.
    /// </summary>
    public static IServiceCollection AddBroadcastChannelsAsScoped(this IServiceCollection services)
    {
        services.AddModuleImportUtilAsScoped()
                .TryAddScoped<IBroadcastChannelsInterop, BroadcastChannelsInterop>();

        services.TryAddScoped<IBroadcastChannels, BroadcastChannels>();

        return services;
    }
}
