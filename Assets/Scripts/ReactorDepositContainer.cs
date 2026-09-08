using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    /// <summary>
    /// Concrete IInteractable reactor container.
    /// Transfers carried Ignicita from player inventory to the city reactor via [Command].
    /// </summary>
    public class ReactorDepositContainer : NetworkBehaviour, IInteractable
    {
        [SerializeField] private int targetCityID = 1;

        public string GetInteractionPrompt()
        {
            return "Presiona [F] para depositar Ignicita en el Reactor";
        }

        public void Interact(PlayerController player)
        {
            if (player == null || player.CarriedIgnicita <= 0f) return;

            if (player.cityID != targetCityID)
            {
                Debug.LogWarning($"[ReactorDepositContainer] Rejected deposit from enemy Player CityID:{player.cityID}");
                return;
            }

            float depositAmount = player.CarriedIgnicita;
            player.ConsumeCarriedIgnicita(depositAmount);

            CmdDepositIgnicita(depositAmount, player.cityID);
        }

        [Command(requiresAuthority = false)]
        private void CmdDepositIgnicita(float amount, int pCityID)
        {
            if (ReactorManager.Instance != null && ReactorManager.Instance.CityID == targetCityID)
            {
                ReactorManager.Instance.DepositIgnicita(amount, pCityID);
                Debug.Log($"[ReactorDepositContainer SERVER] Deposit of {amount} Ignicita accepted from CityID:{pCityID}");
            }
        }
    }
}
