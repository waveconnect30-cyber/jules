using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Gameplay;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Core
{
    /// <summary>
    /// Manages core player attributes (attackPower, maxHP, harvestSpeed, thermalResistance)
    /// and dynamically calculates bonuses based on Ruins controlled by the player's cityID.
    /// </summary>
    public class PlayerStatsManager : NetworkBehaviour
    {
        [Header("Base Attributes")]
        [SyncVar] public float baseAttackPower = 20.0f;
        [SyncVar] public float baseMaxHP = 100.0f;
        [SyncVar] public float baseHarvestSpeed = 1.0f;
        [SyncVar] public float baseThermalResistance = 10.0f;

        [Header("Effective Calculated Stats")]
        public float EffectiveAttackPower { get; private set; }
        public float EffectiveMaxHP { get; private set; }
        public float EffectiveHarvestSpeed { get; private set; }
        public float EffectiveThermalResistance { get; private set; }

        private PlayerController playerController;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            RecalculateEffectiveStats();
        }

        private void Update()
        {
            if (Time.frameCount % 60 == 0)
            {
                RecalculateEffectiveStats();
            }
        }

        /// <summary>
        /// Recalculates effective player stats by inspecting Ruins controlled by player's cityID.
        /// </summary>
        public void RecalculateEffectiveStats()
        {
            float attackMultiplier = 1.0f;
            float hpMultiplier = 1.0f;
            float harvestMultiplier = 1.0f;
            float thermalBonus = 0f;

            if (playerController != null)
            {
                int pCityID = playerController.cityID;

                RuinsNode[] allRuins = FindObjectsOfType<RuinsNode>();
                foreach (var ruin in allRuins)
                {
                    if (ruin.controllingCityID == pCityID)
                    {
                        switch (ruin.Type)
                        {
                            case RuinType.MilitaryMain:
                                attackMultiplier += 0.25f; // +25% Attack Power
                                break;
                            case RuinType.IndustrialMain:
                                harvestMultiplier += 0.30f; // +30% Harvesting Speed
                                break;
                            case RuinType.HealthMain:
                                hpMultiplier += 0.30f; // +30% Max HP
                                break;
                            case RuinType.EnergyMain:
                                thermalBonus += 15.0f; // +15 Thermal Resistance
                                break;
                        }
                    }
                }
            }

            EffectiveAttackPower = baseAttackPower * attackMultiplier;
            EffectiveMaxHP = baseMaxHP * hpMultiplier;
            EffectiveHarvestSpeed = baseHarvestSpeed * harvestMultiplier;
            EffectiveThermalResistance = baseThermalResistance + thermalBonus;
        }
    }
}
