using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Networking;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    /// <summary>
    /// Concrete IInteractable wall repair station.
    /// Consumes materials and restores HP to the designated wall section via [Command].
    /// </summary>
    public class CityWallRepairPanel : NetworkBehaviour, IInteractable
    {
        [SerializeField] private int targetCityID = 1;
        [SerializeField] private WallSection sectionToRepair = WallSection.North;
        [SerializeField] private float repairHPBonus = 250f;

        public string GetInteractionPrompt()
        {
            return $"Presiona [F] para reparar Muralla {sectionToRepair} (+{repairHPBonus} HP)";
        }

        public void Interact(PlayerController player)
        {
            if (player == null) return;

            if (player.cityID != targetCityID)
            {
                Debug.LogWarning($"[CityWallRepairPanel] Rejected repair attempt from enemy Player CityID:{player.cityID}");
                return;
            }

            CmdRepairWallSection(sectionToRepair, repairHPBonus, player.cityID);
        }

        [Command(requiresAuthority = false)]
        private void CmdRepairWallSection(WallSection section, float amount, int pCityID)
        {
            if (CityWallHealthSync.Instance != null && CityWallHealthSync.Instance.cityID == targetCityID)
            {
                CityWallHealthSync.Instance.RepairWall(section, amount, pCityID);
                Debug.Log($"[CityWallRepairPanel SERVER] Wall section {section} repaired +{amount} HP by CityID:{pCityID}");
            }
        }
    }
}
