using System;
using Mirror;
using Mirror.Discovery;
using UnityEngine;

public class SimpleNetworkDiscovery : NetworkDiscoveryBase<ServerRequest, ServerResponse>
{
    #region Server

    // Use ServerId from base class
    
    [Tooltip("Advertise the server to the network")]
    public bool advertiseServer = true;

    [Tooltip("Interval at which the server is advertised (in seconds)")]
    public float advertiseInterval = 1.0f;

    float nextAdvertiseTime;

    public override void Start()
    {
        // ServerId is automatically set by base class
        
        // Get transport from NetworkManager if not assigned
        if (transport == null)
        {
            NetworkManager manager = NetworkManager.singleton;
            if (manager != null)
                transport = manager.transport;
        }
        
        base.Start();
    }

    /// <summary>
    /// Process the request from a client
    /// </summary>
    /// <remarks>
    /// Override if you wish to provide more information to the clients
    /// such as the name of the host player
    /// </remarks>
    /// <param name="request">Request coming from client</param>
    /// <param name="endpoint">Address of the client that sent the request</param>
    /// <returns>A message containing information about this server</returns>
    protected override ServerResponse ProcessRequest(ServerRequest request, System.Net.IPEndPoint endpoint)
    {
        // this server is active
        try
        {
            return new ServerResponse
            {
                serverId = ServerId,
                uri = transport.ServerUri()
            };
        }
        catch (System.NotImplementedException)
        {
            Debug.LogError($"Transport {transport} does not support network discovery");
            throw;
        }
    }

    #endregion

    #region Client

    /// <summary>
    /// Create a message that will be broadcasted on the network to discover servers
    /// </summary>
    /// <remarks>
    /// Override if you wish to include additional data in the discovery message
    /// such as desired game mode, language, difficulty, etc... </remarks>
    /// <returns>An instance of ServerRequest with data to be broadcasted</returns>
    protected override ServerRequest GetRequest() => new ServerRequest();

    /// <summary>
    /// Process the answer from a server
    /// </summary>
    /// <remarks>
    /// A client receives a reply from a server, this method processes the
    /// reply and raises an event
    /// </remarks>
    /// <param name="response">Response that came from the server</param>
    /// <param name="endpoint">Address of the server that replied</param>
    protected override void ProcessResponse(ServerResponse response, System.Net.IPEndPoint endpoint) => OnServerFound.Invoke(response);

    #endregion
}

public class ServerRequest : NetworkMessage
{
    // Add properties for whatever information you want sent by clients
    // in their broadcast messages that servers will consume.
}

public class ServerResponse : NetworkMessage
{
    // Add properties for whatever information you want the server to return to
    // clients for them to display or consume for establishing a connection.

    public long serverId;
    public Uri uri;

    // Prevent duplicate server appearance when a connection can be made via LAN on multiple NICs
    public override int GetHashCode() => (int)serverId;

    public override bool Equals(object obj)
    {
        if (obj is ServerResponse sr)
            return serverId == sr.serverId;

        return false;
    }
}

