using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace EcoDeLasCenizas.Gameplay
{
    public enum DiplomacyRelation
    {
        War,
        Neutral,
        Alliance
    }

    [System.Serializable]
    public struct FactionRelationPair
    {
        public int cityA;
        public int cityB;
        public DiplomacyRelation status;
    }

    /// <summary>
    /// Manages diplomatic relationships (Alliance, Neutral, War) between cityIDs.
    /// Prevents PvP damage and raiding between cities marked as Alliance.
    /// </summary>
    public class DiplomacyManager : NetworkBehaviour
    {
        public static DiplomacyManager Instance { get; private set; }

        [Header("Diplomatic Relations List")]
        public readonly SyncList<FactionRelationPair> diplomacyTable = new SyncList<FactionRelationPair>();

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
        /// Gets diplomatic relation status between two city IDs.
        /// </summary>
        public DiplomacyRelation GetRelation(int cityA, int cityB)
        {
            if (cityA == cityB) return DiplomacyRelation.Alliance;

            foreach (var pair in diplomacyTable)
            {
                if ((pair.cityA == cityA && pair.cityB == cityB) || (pair.cityA == cityB && pair.cityB == cityA))
                {
                    return pair.status;
                }
            }

            return DiplomacyRelation.War; // Default to War in PvPvE mode
        }

        /// <summary>
        /// Sets diplomatic relation between two city IDs.
        /// </summary>
        [Server]
        public void SetRelation(int cityA, int cityB, DiplomacyRelation relation)
        {
            if (cityA == cityB) return;

            for (int i = 0; i < diplomacyTable.Count; i++)
            {
                var pair = diplomacyTable[i];
                if ((pair.cityA == cityA && pair.cityB == cityB) || (pair.cityA == cityB && pair.cityB == cityA))
                {
                    pair.status = relation;
                    diplomacyTable[i] = pair;
                    Debug.Log($"[DiplomacyManager] Relation between City {cityA} and City {cityB} updated to {relation}.");
                    RpcAnnounceDiplomacyChange(cityA, cityB, relation);
                    return;
                }
            }

            diplomacyTable.Add(new FactionRelationPair { cityA = cityA, cityB = cityB, status = relation });
            Debug.Log($"[DiplomacyManager] New Relation: City {cityA} and City {cityB} set to {relation}.");
            RpcAnnounceDiplomacyChange(cityA, cityB, relation);
        }

        /// <summary>
        /// Checks if combat/damage is allowed between two city IDs.
        /// Returns true if cities are at War or Neutral, false if Allies.
        /// </summary>
        public bool IsPvPAllowed(int attackerCityID, int targetCityID)
        {
            if (attackerCityID == targetCityID) return false;
            return GetRelation(attackerCityID, targetCityID) != DiplomacyRelation.Alliance;
        }

        [ClientRpc]
        private void RpcAnnounceDiplomacyChange(int cityA, int cityB, DiplomacyRelation relation)
        {
            Debug.LogWarning($"[Diplomacy DIPLOMATIC DRAFT] City {cityA} and City {cityB} are now in state: {relation}!");
        }
    }
}
