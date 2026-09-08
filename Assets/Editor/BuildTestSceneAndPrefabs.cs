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
using EcoDeLasCenizas.UI;

namespace EcoDeLasCenizas.Editor
{
    /// <summary>
    /// Reproducible Unity Editor Generator Script.
    /// Creates valid, native Unity 2022.3 scene (Assets/Scenes/TestScene.unity) and prefabs (Assets/Prefabs/)
    /// with Mirror networking components (NetworkIdentity, NetworkTransform, CharacterController) attached.
    /// Instantiates Reactors, Walls, Warehouses, Nodes, Cameras, AudioListeners, NetworkStartPositions,
    /// and a Spanish Host/Client HUD.
    /// Run in Unity Editor via MenuItem: EcoDeLasCenizas -> Build Real Test Scene and Prefabs.
    /// </summary>
    public static class BuildTestSceneAndPrefabs
    {
        [MenuItem("EcoDeLasCenizas/Build Real Test Scene and Prefabs")]
        public static void GenerateSceneAndPrefabs()
        {
            // Prompt to save open modified scenes before generating
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[BuildTestSceneAndPrefabs] Generation cancelled by user.");
                return;
            }

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

            // 5. Generate Real Test Scene in isolated new scene
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

            // Create Main Camera & AudioListener
            GameObject cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            Camera cam = cameraGO.AddComponent<Camera>();
            cameraGO.AddComponent<AudioListener>();
            cameraGO.transform.position = new Vector3(0f, 8f, -12f);
            cameraGO.transform.rotation = Quaternion.Euler(30f, 0f, 0f);

            // Create NetworkManager & Spanish HUD
            GameObject netManagerGO = new GameObject("NetworkManager");
            var lobby = netManagerGO.AddComponent<NetworkLobbyManager>();
            netManagerGO.AddComponent<NetworkManagerHUD>();

            // Assign Player Prefab to NetworkManager
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPrefab.prefab");
            if (playerPrefab != null && lobby != null)
            {
                lobby.playerPrefab = playerPrefab;
            }

            // Create GameManager & Core Systems (including AssetPrefabLinker)
            GameObject gameManagerGO = new GameObject("GameManager");
            gameManagerGO.AddComponent<GameManager>();
            gameManagerGO.AddComponent<AssetPrefabLinker>();

            GameObject diplomacyGO = new GameObject("DiplomacyManager");
            diplomacyGO.AddComponent<DiplomacyManager>();

            GameObject globalEventGO = new GameObject("GlobalEventManager");
            globalEventGO.AddComponent<GlobalEventManager>();

            GameObject seasonGO = new GameObject("SeasonManager");
            seasonGO.AddComponent<SeasonManager>();

            // Create City 1 Entities (CityID = 1)
            GameObject reactor1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            reactor1.name = "Reactor_City1";
            reactor1.transform.position = new Vector3(10f, 1f, 0f);
            reactor1.AddComponent<NetworkIdentity>();
            var rm1 = reactor1.AddComponent<ReactorManager>();
            rm1.cityID = 1;
            reactor1.AddComponent<ReactorDepositContainer>();

            GameObject wall1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall1.name = "Wall_City1";
            wall1.transform.position = new Vector3(10f, 1.5f, 8f);
            wall1.transform.localScale = new Vector3(8f, 3f, 1f);
            wall1.AddComponent<NetworkIdentity>();
            var wallSync1 = wall1.AddComponent<CityWallHealthSync>();
            wallSync1.cityID = 1;
            wall1.AddComponent<CityWallRepairPanel>();

            GameObject warehouse1 = new GameObject("Warehouse_City1");
            warehouse1.AddComponent<NetworkIdentity>();
            var wh1 = warehouse1.AddComponent<SharedInventorySync>();
            wh1.cityID = 1;

            // Create City 2 Entities (CityID = 2)
            GameObject reactor2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            reactor2.name = "Reactor_City2";
            reactor2.transform.position = new Vector3(-10f, 1f, 0f);
            reactor2.AddComponent<NetworkIdentity>();
            var rm2 = reactor2.AddComponent<ReactorManager>();
            rm2.cityID = 2;
            reactor2.AddComponent<ReactorDepositContainer>();

            GameObject wall2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall2.name = "Wall_City2";
            wall2.transform.position = new Vector3(-10f, 1.5f, 8f);
            wall2.transform.localScale = new Vector3(8f, 3f, 1f);
            wall2.AddComponent<NetworkIdentity>();
            var wallSync2 = wall2.AddComponent<CityWallHealthSync>();
            wallSync2.cityID = 2;
            wall2.AddComponent<CityWallRepairPanel>();

            GameObject warehouse2 = new GameObject("Warehouse_City2");
            warehouse2.AddComponent<NetworkIdentity>();
            var wh2 = warehouse2.AddComponent<SharedInventorySync>();
            wh2.cityID = 2;

            // Create Resource Node in Fog
            GameObject node1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            node1.name = "ResourceNode_Ignicita";
            node1.transform.position = new Vector3(0f, 0.5f, 15f);
            node1.AddComponent<NetworkIdentity>();
            node1.AddComponent<IgnicitaHarvestNode>();

            // Create NetworkStartPositions
            GameObject spawn1 = new GameObject("SpawnPoint_City1");
            spawn1.transform.position = new Vector3(10f, 0.5f, -5f);
            spawn1.AddComponent<NetworkStartPosition>();

            GameObject spawn2 = new GameObject("SpawnPoint_City2");
            spawn2.transform.position = new Vector3(-10f, 0.5f, -5f);
            spawn2.AddComponent<NetworkStartPosition>();

            // Create Canvas HUD
            GameObject canvasGO = new GameObject("Canvas_SpanishHUD");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            canvasGO.AddComponent<ReactorHUDUI>();
            canvasGO.AddComponent<ScreenFrostPostProcessUI>();

            // Save Scene
            EditorSceneManager.SaveScene(newScene, "Assets/Scenes/TestScene.unity");

            // Register Scene in Build Settings
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/TestScene.unity", true)
            };
            EditorBuildSettings.scenes = scenes;

            Debug.Log("[BuildTestSceneAndPrefabs] Assets/Scenes/TestScene.unity registered in Build Settings and saved successfully!");
        }
    }
}
#endif
