using MashGamemodeLibrary.networking.Validation.Routes;

namespace MashGamemodeLibrary.networking.Validation;

public static class CommonNetworkRoutes
{
    /// <summary>
    /// A route that allows communication in both directions between the host and remote clients.
    /// </summary>
    public static readonly INetworkRoute BiDirectional = new BiDirectionalNetworkRoute();
    /// <summary>
    /// A route that allows communication from the host to remote clients, but not the other way around.
    /// Doesn't get called on the caller
    /// </summary>
    public static readonly INetworkRoute HostToRemote = new HostToRemoteNetworkRoute();
    /// <summary>
    /// A route that allows communication from remote clients to the host, but not the other way around.
    /// Doesn't get called on the caller
    /// </summary>
    public static readonly INetworkRoute RemoteToHost = new RemoteToHostNetworkRoute();
    /// <summary>
    /// A route that allows communication from all clients to the host, but not between remote clients.
    /// If the host calls this, it also runs locally.
    /// </summary>
    public static readonly INetworkRoute AllToHost = new AllToHostNetworkRoute();
    /// <summary>
    /// A route that allows communication from the host to all clients, but not between remote clients.
    /// If the host calls this, it also runs locally.
    /// </summary>
    public static readonly INetworkRoute HostToAll = new HostToAllNetworkRoute();
    /// <summary>
    /// A route that allows communication between all clients, including the host.
    /// Calling this will also run it locally
    /// </summary>
    public static readonly INetworkRoute AllToAll = new AllToAllNetworkRoute();
}