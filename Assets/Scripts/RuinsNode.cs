using UnityEngine;
using Mirror;

namespace EcoDeLasCenizas.Gameplay
{
    public enum RuinCategory
    {
        MainRuin,
        SecondaryRuin
    }

    public enum RuinType
    {
        // 4 Main Central Ruins
        MilitaryMain,      // Grants +25% Attack Power to controlling city
        IndustrialMain,    // Grants +30% Harvest/Harvesting Speed
        HealthMain,        // Grants +30% Max HP & Passive Health Regen
        EnergyMain,        // Grants +40% Fuel Efficiency to City Reactor

        // 8 Secondary Support Ruins
        FortressAlpha,
        FortressBeta,
        WorkshopEast,
        WorkshopWest,
        BioLabNorth,
        BioLabSouth,
        SubstationOne,
        SubstationTwo
    }

    /// <summary>
    /// Capturable ruins across the World Map.
    /// Controls faction bonuses (harvest speed, attack power, max HP, energy efficiency)
    /// for all players sharing the controlling cityID.
    /// </summary>
    public class RuinsNode : NetworkBehaviour
    {
        [Header("Ruin Configuration")]
        [SerializeField] private string ruinName = "Ruina Industrial Central";
        [SerializeField] private RuinCategory category = RuinCategory.MainRuin;
        [SerializeField] private RuinType ruinType = RuinType.IndustrialMain;
        [SerializeField] private float captureTimeRequired = 15.0f;

        [Header("State (Synced)")]
        [SyncVar] public int controllingCityID = 0; // 0 = Neutral
        [SyncVar] public float captureProgress = 0f;
        [SyncVar] public int capturingCityID = 0;

        public RuinType Type => ruinType;
        public RuinCategory Category => category;
        public string RuinName => ruinName;

        private void OnTriggerStay(Collider other)
        {
            if (!isServer) return;

            var player = other.GetComponent<EcoDeLasCenizas.Player.PlayerController>();
            if (player == null) return;

            int playerCityID = player.cityID;
            if (playerCityID == controllingCityID) return;

            if (capturingCityID != playerCityID)
            {
                capturingCityID = playerCityID;
                captureProgress = 0f;
            }

            captureProgress += Time.deltaTime;

            if (captureProgress >= captureTimeRequired)
            {
                controllingCityID = playerCityID;
                captureProgress = 0f;
                Debug.Log($"[RuinsNode SERVER] {ruinName} ({ruinType}) CAPTURED by City {controllingCityID}!");
                RpcAnnounceRuinCaptured(ruinName, ruinType.ToString(), controllingCityID);
            }
        }

        [ClientRpc]
        private void RpcAnnounceRuinCaptured(string name, string typeStr, int cityID)
        {
            Debug.LogWarning($"[RUINS ANNOUNCEMENT] {name} ({typeStr}) is now controlled by City {cityID}! Global city buffs active.");
        }
    }
}
