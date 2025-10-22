using MashGamemodeLibrary.networking.Validation.Routes;

namespace MashGamemodeLibrary.networking.Validation;

public static class CommonNetworkRoutes
{
    public static readonly INetworkRoute BiDirectional = new BiDirectionalNetworkRoute();
    public static readonly INetworkRoute HostToClient = new HostToClientNetworkRoute();
    public static readonly INetworkRoute ClientToHost = new ClientToHostNetworkRoute();
    public static readonly INetworkRoute AllToHost = new AllToHostNetworkRoute();
    public static readonly INetworkRoute HostToAll = new HostToAllNetworkRoute();
    public static readonly INetworkRoute AllToAll = new AllToAllNetworkRoute();
}