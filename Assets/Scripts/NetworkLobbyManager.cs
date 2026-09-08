using UnityEngine;
using Mirror;

namespace EcoDeLasCenizas.Networking
{
    /// <summary>
    /// Manages multiplayer lobby creation, client connections, and player capacity (4 to 8 players per session).
    /// </summary>
    public class NetworkLobbyManager : NetworkManager
    {
        public static NetworkLobbyManager LobbyInstance { get; private set; }

        [Header("Lobby Configuration")]
        [SerializeField] private int minPlayersToStart = 2;
        [SerializeField] private int maxLobbyCapacity = 8;

        public override void Awake()
        {
            base.Awake();
            if (LobbyInstance != null && LobbyInstance != this)
            {
                Destroy(gameObject);
                return;
            }
            LobbyInstance = this;
            maxConnections = maxLobbyCapacity;
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            Debug.Log($"[NetworkLobby] Player connected from {conn.address}. Total players: {numPlayers}/{maxConnections}");
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            Debug.Log($"[NetworkLobby] Player disconnected. Remaining players: {numPlayers}");
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            Debug.Log("[NetworkLobby] Client successfully connected to La Caldera server session.");
        }

        /// <summary>
        /// Creates a host session for 4-8 coop players.
        /// </summary>
        public void CreateCoopLobby(int maxCapacity = 4)
        {
            maxConnections = Mathf.Clamp(maxCapacity, 4, 8);
            StartHost();
            Debug.Log($"[NetworkLobby] Created Coop Lobby with max capacity of {maxConnections} survivors.");
        }

        /// <summary>
        /// Joins an existing lobby session by IP address.
        /// </summary>
        public void JoinCoopLobby(string serverAddress = "localhost")
        {
            networkAddress = serverAddress;
            StartClient();
            Debug.Log($"[NetworkLobby] Joining Coop Lobby at {serverAddress}...");
        }
    }
}
