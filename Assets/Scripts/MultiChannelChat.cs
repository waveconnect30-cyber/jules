using UnityEngine;
using Mirror;
using System;
using EcoDeLasCenizas.Gameplay;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Networking
{
    public enum ChatChannel
    {
        City,       // Filtered by sender's cityID
        Global,     // Broadcast to all players on server
        Alliance    // Broadcast to allied cityIDs
    }

    public enum QuickPingType
    {
        NeedIgnicita,
        DefendWall,
        EnemySpotted,
        RaidTarget
    }

    [System.Serializable]
    public struct ChatMessage
    {
        public string senderName;
        public int senderCityID;
        public ChatChannel channel;
        public string messageText;
        public string timestamp;
    }

    /// <summary>
    /// Multi-channel chat system with server-side sender validation using connectionToClient.
    /// Filters messages and sends TargetRpc or targeted connection broadcasts for City and Alliance channels.
    /// </summary>
    public class MultiChannelChat : NetworkBehaviour
    {
        public static MultiChannelChat Instance { get; private set; }

        public event Action<ChatMessage> OnChatMessageReceived;
        public event Action<string, Vector3, int> OnQuickPingTriggered;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Sends a chat message over the network with server-side sender identity validation.
        /// Client text is verified and senderCityID/senderName is resolved securely from sender's player component.
        /// </summary>
        [Command(requiresAuthority = false)]
        public void CmdSendMessage(ChatChannel channel, string text, NetworkConnectionToClient senderConn = null)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            // Resolve true sender identity from connection
            NetworkConnectionToClient conn = senderConn ?? connectionToClient;
            string validatedName = "Sobreviviente";
            int validatedCityID = 1;

            if (conn != null && conn.identity != null)
            {
                var pController = conn.identity.GetComponent<PlayerController>();
                if (pController != null)
                {
                    validatedCityID = pController.cityID;
                    validatedName = conn.identity.name;
                }
            }

            ChatMessage msg = new ChatMessage
            {
                senderName = validatedName,
                senderCityID = validatedCityID,
                channel = channel,
                messageText = text,
                timestamp = DateTime.Now.ToString("HH:mm")
            };

            // Distribute message based on channel
            foreach (var targetConn in NetworkServer.connections.Values)
            {
                if (targetConn == null || targetConn.identity == null) continue;

                var targetPlayer = targetConn.identity.GetComponent<PlayerController>();
                if (targetPlayer == null) continue;

                bool shouldSend = false;

                if (channel == ChatChannel.Global)
                {
                    shouldSend = true;
                }
                else if (channel == ChatChannel.City && targetPlayer.cityID == validatedCityID)
                {
                    shouldSend = true;
                }
                else if (channel == ChatChannel.Alliance)
                {
                    if (targetPlayer.cityID == validatedCityID)
                    {
                        shouldSend = true;
                    }
                    else if (DiplomacyManager.Instance != null && DiplomacyManager.Instance.GetRelation(targetPlayer.cityID, validatedCityID) == DiplomacyRelation.Alliance)
                    {
                        shouldSend = true;
                    }
                }

                if (shouldSend)
                {
                    TargetReceiveMessage(targetConn, msg);
                }
            }
        }

        [TargetRpc]
        private void TargetReceiveMessage(NetworkConnection target, ChatMessage msg)
        {
            Debug.Log($"[{msg.channel.ToString().ToUpper()} CHAT] [{msg.timestamp}] {msg.senderName} (City {msg.senderCityID}): {msg.messageText}");
            OnChatMessageReceived?.Invoke(msg);
        }

        [Command(requiresAuthority = false)]
        public void CmdSendQuickPing(QuickPingType pingType, Vector3 worldPosition, NetworkConnectionToClient senderConn = null)
        {
            NetworkConnectionToClient conn = senderConn ?? connectionToClient;
            int validatedCityID = 1;
            if (conn != null && conn.identity != null)
            {
                var pController = conn.identity.GetComponent<PlayerController>();
                if (pController != null) validatedCityID = pController.cityID;
            }

            foreach (var targetConn in NetworkServer.connections.Values)
            {
                if (targetConn == null || targetConn.identity == null) continue;
                var targetPlayer = targetConn.identity.GetComponent<PlayerController>();
                if (targetPlayer == null) continue;

                if (targetPlayer.cityID == validatedCityID ||
                   (DiplomacyManager.Instance != null && DiplomacyManager.Instance.GetRelation(targetPlayer.cityID, validatedCityID) == DiplomacyRelation.Alliance))
                {
                    TargetReceiveQuickPing(targetConn, pingType.ToString(), worldPosition, validatedCityID);
                }
            }
        }

        [TargetRpc]
        private void TargetReceiveQuickPing(NetworkConnection target, string pingTypeStr, Vector3 worldPosition, int senderCityID)
        {
            Debug.Log($"[TACTICAL PING] City {senderCityID} pinged: '{pingTypeStr}' at {worldPosition}");
            OnQuickPingTriggered?.Invoke(pingTypeStr, worldPosition, senderCityID);
        }
    }
}
