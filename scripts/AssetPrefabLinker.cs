using UnityEngine;
using EcoDeLasCenizas.Gameplay;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Core
{
    /// <summary>
    /// Manages 3D art asset paths under Assets/Art/ (Models, Textures, Prefabs)
    /// and dynamically binds character models (Explorer, Engineer, Scientist, Tactician) to PlayerController,
    /// reactor models to ReactorManager, wall section meshes to CityWallHealthSync,
    /// and creature models to EnemyAI.
    /// </summary>
    public class AssetPrefabLinker : MonoBehaviour
    {
        public static AssetPrefabLinker Instance { get; private set; }

        [Header("Asset Folder Structure Paths")]
        public const string MODELS_PATH = "Assets/Art/Models/";
        public const string TEXTURES_PATH = "Assets/Art/Textures/";
        public const string PREFABS_PATH = "Assets/Art/Prefabs/";

        [Header("Class 3D Model Prefabs (Assets/Art/Prefabs/)")]
        [SerializeField] private GameObject explorerModelPrefab;
        [SerializeField] private GameObject engineerModelPrefab;
        [SerializeField] private GameObject scientistModelPrefab;
        [SerializeField] private GameObject tacticianModelPrefab;

        [Header("System 3D Model Prefabs")]
        [SerializeField] private GameObject reactorModelPrefab;
        [SerializeField] private GameObject cityWallModelPrefab;
        [SerializeField] private GameObject frozenShadowEnemyModelPrefab;

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
        /// Binds the appropriate 3D character mesh model (e.g. Explorer insulated Arctic suit model)
        /// to the PlayerController based on selected CharacterClass.
        /// </summary>
        public void BindClassModelToPlayer(PlayerController player, CharacterClass selectedClass)
        {
            if (player == null) return;

            GameObject targetPrefab = GetClassModelPrefab(selectedClass);
            if (targetPrefab != null)
            {
                // Instantiate and attach character model to player transform
                GameObject modelInstance = Instantiate(targetPrefab, player.transform);
                modelInstance.transform.localPosition = Vector3.zero;
                modelInstance.transform.localRotation = Quaternion.identity;
                Debug.Log($"[AssetPrefabLinker] Successfully linked 3D Model ({targetPrefab.name}) to Player (Class: {selectedClass}, CityID: {player.cityID}).");
            }
            else
            {
                Debug.LogWarning($"[AssetPrefabLinker] No 3D model prefab assigned for class {selectedClass} under {PREFABS_PATH}.");
            }
        }

        public GameObject GetClassModelPrefab(CharacterClass pClass)
        {
            switch (pClass)
            {
                case CharacterClass.Explorer: return explorerModelPrefab;
                case CharacterClass.Engineer: return engineerModelPrefab;
                case CharacterClass.Scientist: return scientistModelPrefab;
                case CharacterClass.Tactician: return tacticianModelPrefab;
                default: return explorerModelPrefab;
            }
        }

        public GameObject ReactorModelPrefab => reactorModelPrefab;
        public GameObject CityWallModelPrefab => cityWallModelPrefab;
        public GameObject FrozenShadowEnemyModelPrefab => frozenShadowEnemyModelPrefab;
    }
}
