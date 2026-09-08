using UnityEngine;
using Mirror;
using System;

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
    /// Multi-channel chat system supporting City, Global, and Alliance channels,
    /// alongside fast tactical voice/ping command RPCs.
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
        /// Sends a chat message over the network.
        /// </summary>
        [Command(requiresAuthority = false)]
        public void CmdSendMessage(string senderName, int senderCityID, ChatChannel channel, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            ChatMessage msg = new ChatMessage
            {
                senderName = senderName,
                senderCityID = senderCityID,
                channel = channel,
                messageText = text,
                timestamp = DateTime.Now.ToString("HH:mm")
            };

            RpcReceiveMessage(msg);
        }

        [ClientRpc]
        private void RpcReceiveMessage(ChatMessage msg)
        {
            // Filter city channel messages to match local player's cityID if needed
            Debug.Log($"[{msg.channel.ToString().ToUpper()} CHAT] [{msg.timestamp}] {msg.senderName} (City {msg.senderCityID}): {msg.messageText}");
            OnChatMessageReceived?.Invoke(msg);
        }

        /// <summary>
        /// Triggers a quick tactical ping command in world space.
        /// </summary>
        [Command(requiresAuthority = false)]
        public void CmdSendQuickPing(QuickPingType pingType, Vector3 worldPosition, int senderCityID)
        {
            RpcReceiveQuickPing(pingType.ToString(), worldPosition, senderCityID);
        }

        [ClientRpc]
        private void RpcReceiveQuickPing(string pingTypeStr, Vector3 worldPosition, int senderCityID)
        {
            Debug.Log($"[TACTICAL PING] City {senderCityID} pinged: '{pingTypeStr}' at {worldPosition}");
            OnQuickPingTriggered?.Invoke(pingTypeStr, worldPosition, senderCityID);
        }
    }
}
