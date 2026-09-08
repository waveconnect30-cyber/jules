using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using EcoDeLasCenizas.Gameplay;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.UI
{
    /// <summary>
    /// UI Manager for selecting survivor classes (Explorer, Engineer, Scientist, Tactician)
    /// before spawning into La Caldera.
    /// Calls CmdSelectClass on the local player's PlayerController NetworkBehaviour to assign class,
    /// recalculate stats, and bind 3D models on the server.
    /// </summary>
    public class ClassSelectionUI : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject classSelectionModalPanel;

        [Header("Class Info Display")]
        [SerializeField] private TextMeshProUGUI selectedClassNameText;
        [SerializeField] private TextMeshProUGUI selectedClassRoleText;
        [SerializeField] private TextMeshProUGUI selectedClassStatsText;
        [SerializeField] private TextMeshProUGUI selectedClassAbilityText;

        [Header("Spawn Action")]
        [SerializeField] private Button spawnInCityButton;

        private CharacterClass currentSelectedClass = CharacterClass.Explorer;

        public event System.Action<CharacterClass> OnClassConfirmedAndSpawned;

        private void Start()
        {
            SelectClass(CharacterClass.Explorer);

            if (spawnInCityButton != null)
            {
                spawnInCityButton.onClick.AddListener(ConfirmSelectionAndSpawn);
            }
        }

        public void SelectClass(CharacterClass pClass)
        {
            currentSelectedClass = pClass;

            switch (pClass)
            {
                case CharacterClass.Explorer:
                    UpdateClassInfo("EXPLORADOR", "Extracción y Incursiones Exteriores",
                        "HP: 100 | Vel: 6.0 m/s\nResistencia Frío: Alta",
                        "Habilidad: Escáner Geotérmico y Gancho de Agarre");
                    break;

                case CharacterClass.Engineer:
                    UpdateClassInfo("INGENIERO", "Mantenimiento e Infraestructura",
                        "HP: 120 | Vel: 4.5 m/s\nEficiencia Reparación: +50%",
                        "Habilidad: Torreta Reparadora Automática y Soldadura Sobrecargada");
                    break;

                case CharacterClass.Scientist:
                    UpdateClassInfo("CIENTÍFICO", "Botánica y Filtros de Toxinas",
                        "HP: 80 | Vel: 4.8 m/s\nRendimiento Cúpulas: +100%",
                        "Habilidad: Estimulante Térmico y Optimización de Catalizador");
                    break;

                case CharacterClass.Tactician:
                    UpdateClassInfo("TÁCTICO", "Comando y Defensa de Murallas",
                        "HP: 150 | Vel: 5.0 m/s\nAura de Bastión: +20% Def",
                        "Habilidad: Grito de Batalla y Marcador Táctico de Hordas");
                    break;
            }
        }

        private void UpdateClassInfo(string name, string role, string stats, string ability)
        {
            if (selectedClassNameText) selectedClassNameText.text = name;
            if (selectedClassRoleText) selectedClassRoleText.text = role;
            if (selectedClassStatsText) selectedClassStatsText.text = stats;
            if (selectedClassAbilityText) selectedClassAbilityText.text = ability;
        }

        public void ConfirmSelectionAndSpawn()
        {
            Debug.Log($"[ClassSelectionUI] Class selected: {currentSelectedClass}. Spawning player into La Caldera...");

            if (classSelectionModalPanel != null)
            {
                classSelectionModalPanel.SetActive(false);
            }

            var localPlayer = NetworkClient.localPlayer != null
                ? NetworkClient.localPlayer.GetComponent<PlayerController>()
                : FindObjectOfType<PlayerController>();

            if (localPlayer != null)
            {
                localPlayer.CmdSelectClass(currentSelectedClass);
            }

            OnClassConfirmedAndSpawned?.Invoke(currentSelectedClass);
        }

        // Button Event Handlers
        public void OnClickSelectExplorer() => SelectClass(CharacterClass.Explorer);
        public void OnClickSelectEngineer() => SelectClass(CharacterClass.Engineer);
        public void OnClickSelectScientist() => SelectClass(CharacterClass.Scientist);
        public void OnClickSelectTactician() => SelectClass(CharacterClass.Tactician);
    }
}
