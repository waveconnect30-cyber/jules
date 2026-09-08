#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Mirror;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Gameplay;
using EcoDeLasCenizas.Networking;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Editor
{
    /// <summary>
    /// Reproducible Unity Editor Generator Script.
    /// Creates valid, native Unity 2022.3 scene (Assets/Scenes/TestScene.unity) and prefabs (Assets/Prefabs/)
    /// with Mirror networking components (NetworkIdentity, NetworkTransform, CharacterController) attached.
    /// Run in Unity Editor via MenuItem: EcoDeLasCenizas -> Build Real Test Scene and Prefabs.
    /// </summary>
    public static class BuildTestSceneAndPrefabs
    {
        [MenuItem("EcoDeLasCenizas/Build Real Test Scene and Prefabs")]
        public static void GenerateSceneAndPrefabs()
        {
            // Ensure directories exist
            if (!Directory.Exists("Assets/Prefabs")) Directory.CreateDirectory("Assets/Prefabs");
            if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");

            Debug.Log("[BuildTestSceneAndPrefabs] Generating real Unity Prefabs under Assets/Prefabs/...");

            // 1. Generate PlayerPrefab
            GameObject playerGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerGO.name = "PlayerPrefab";
            if (!playerGO.GetComponent<NetworkIdentity>()) playerGO.AddComponent<NetworkIdentity>();
            if (!playerGO.GetComponent<NetworkTransformUnreliable>()) playerGO.AddComponent<NetworkTransformUnreliable>();
            if (!playerGO.GetComponent<PlayerController>()) playerGO.AddComponent<PlayerController>();
            if (!playerGO.GetComponent<PlayerStatsManager>()) playerGO.AddComponent<PlayerStatsManager>();
            if (!playerGO.GetComponent<ClassAbilities>()) playerGO.AddComponent<ClassAbilities>();

            PrefabUtility.SaveAsPrefabAsset(playerGO, "Assets/Prefabs/PlayerPrefab.prefab");
            Object.DestroyImmediate(playerGO);

            // 2. Generate ReactorPrefab
            GameObject reactorGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            reactorGO.name = "ReactorPrefab";
            if (!reactorGO.GetComponent<NetworkIdentity>()) reactorGO.AddComponent<NetworkIdentity>();
            if (!reactorGO.GetComponent<ReactorManager>()) reactorGO.AddComponent<ReactorManager>();
            if (!reactorGO.GetComponent<ReactorDepositContainer>()) reactorGO.AddComponent<ReactorDepositContainer>();

            PrefabUtility.SaveAsPrefabAsset(reactorGO, "Assets/Prefabs/ReactorPrefab.prefab");
            Object.DestroyImmediate(reactorGO);

            // 3. Generate CityWallPrefab
            GameObject wallGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallGO.name = "CityWallPrefab";
            if (!wallGO.GetComponent<NetworkIdentity>()) wallGO.AddComponent<NetworkIdentity>();
            if (!wallGO.GetComponent<CityWallHealthSync>()) wallGO.AddComponent<CityWallHealthSync>();
            if (!wallGO.GetComponent<CityWallRepairPanel>()) wallGO.AddComponent<CityWallRepairPanel>();

            PrefabUtility.SaveAsPrefabAsset(wallGO, "Assets/Prefabs/CityWallPrefab.prefab");
            Object.DestroyImmediate(wallGO);

            // 4. Generate ResourceNodePrefab
            GameObject nodeGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nodeGO.name = "ResourceNodePrefab";
            if (!nodeGO.GetComponent<NetworkIdentity>()) nodeGO.AddComponent<NetworkIdentity>();
            if (!nodeGO.GetComponent<IgnicitaHarvestNode>()) nodeGO.AddComponent<IgnicitaHarvestNode>();

            PrefabUtility.SaveAsPrefabAsset(nodeGO, "Assets/Prefabs/ResourceNodePrefab.prefab");
            Object.DestroyImmediate(nodeGO);

            Debug.Log("[BuildTestSceneAndPrefabs] Prefabs generated successfully!");

            // 5. Generate Real Test Scene
            Debug.Log("[BuildTestSceneAndPrefabs] Creating native Assets/Scenes/TestScene.unity...");
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Create Directional Light
            GameObject lightGO = new GameObject("Directional Light");
            Light lightComp = lightGO.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Create Ground Plane
            GameObject groundGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
            groundGO.name = "Ground Plane";
            groundGO.transform.localScale = new Vector3(10f, 1f, 10f);

            // Create NetworkManager & Lobby
            GameObject netManagerGO = new GameObject("NetworkManager");
            var lobby = netManagerGO.AddComponent<NetworkLobbyManager>();
            netManagerGO.AddComponent<NetworkManagerHUD>();

            // Assign Player Prefab to NetworkManager
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPrefab.prefab");
            if (playerPrefab != null && lobby != null)
            {
                lobby.playerPrefab = playerPrefab;
            }

            // Create GameManager & Core Systems
            GameObject gameManagerGO = new GameObject("GameManager");
            gameManagerGO.AddComponent<GameManager>();

            GameObject diplomacyGO = new GameObject("DiplomacyManager");
            diplomacyGO.AddComponent<DiplomacyManager>();

            GameObject globalEventGO = new GameObject("GlobalEventManager");
            globalEventGO.AddComponent<GlobalEventManager>();

            GameObject seasonGO = new GameObject("SeasonManager");
            seasonGO.AddComponent<SeasonManager>();

            // Create Spawn Points
            GameObject spawn1 = new GameObject("SpawnPoint_City1");
            spawn1.transform.position = new Vector3(10f, 0.5f, 0f);

            GameObject spawn2 = new GameObject("SpawnPoint_City2");
            spawn2.transform.position = new Vector3(-10f, 0.5f, 0f);

            // Save Scene
            EditorSceneManager.SaveScene(newScene, "Assets/Scenes/TestScene.unity");
            Debug.Log("[BuildTestSceneAndPrefabs] Assets/Scenes/TestScene.unity generated and saved successfully!");
        }
    }
}
#endif
