// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Exchange;

/// <summary>
/// A configuration for making a connection to the underlying exchange,
/// including information for the host, port, virtual host, and user.
/// </summary>
public class ConnectionSettings
{
    /// <summary>
    /// Constructs default state.
    /// </summary>
    /// <param name="hostName">The name of the host server</param>
    /// <param name="port">Thr port number on the host server, or -1</param>
    /// <param name="virtualHost">The name of the virtual host</param>
    /// <param name="username">The name of the user, or null</param>
    /// <param name="password">The password of the user, or null</param>
    private ConnectionSettings(string hostName, int port, string virtualHost, string? username, string? password)
    {
        HostName = hostName;
        Port = port;
        VirtualHost = virtualHost;
        Username = username;
        Password = password;
    }

    public static int UndefinedPort { get; } = -1;

    /** My hostName, which is the name of the host server. */
    public string HostName { get; }

    /** My password, which is the password of the connecting user. */
    public string? Password { get; }

    /** My port, which is the host server port. */
    public int Port { get; }

    /** My username, which is the name of the connecting user. */
    public string? Username { get; }

    /** My virtualHost, which is the name of the RabbitMQ virtual host. */
    public string VirtualHost { get; }

    /// <summary>
    /// Gets <code>true</code> if the port is included
    /// </summary>
    public bool HasPort => Port > 0;

    /// <summary>
    /// Gets <code>true</code> whether the user credentials are included.
    /// </summary>
    public bool HasUserCredentials => Username != null && Password != null;

    /// <summary>
    /// Gets a new <see cref="ConnectionSettings"/> with defaults, used for tests only.
    /// </summary>
    /// <returns><see cref="ConnectionSettings"/></returns>
    internal static ConnectionSettings Instance() => new ConnectionSettings("localhost", UndefinedPort, "/", null, null);
        
    /// <summary>
    /// Gets a new <see cref="ConnectionSettings"/> with a specific host name and virtual host and remaining defaults.
    /// </summary>
    /// <param name="hostName">The name of the host server</param>
    /// <param name="virtualHost">The name of the virtual host</param>
    /// <returns><see cref="ConnectionSettings"/></returns>
    public static ConnectionSettings Instance(string hostName, string virtualHost) =>
        new ConnectionSettings(hostName, UndefinedPort, virtualHost, null, null);
        
    /// <summary>
    /// Gets a new <see cref="ConnectionSettings"/>.
    /// </summary>
    /// <param name="hostName">The name of the host server</param>
    /// <param name="port">Thr port number on the host server, or -1</param>
    /// <param name="virtualHost">The name of the virtual host</param>
    /// <param name="username">The name of the user, or null</param>
    /// <param name="password">The password of the user, or null</param>
    /// <returns></returns>
    public static ConnectionSettings Instance(string hostName, int port, string virtualHost, string? username = null, string? password = null) =>
        new ConnectionSettings(hostName, port, virtualHost, username, password);
}