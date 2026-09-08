#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using Mirror;
using kcp2k;
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
    /// with Mirror networking components (NetworkIdentity, NetworkTransformUnreliable, CharacterController, KcpTransport) attached.
    /// Uses SerializedObject / SerializedProperty for private serialized fields.
    /// Preserves existing Build Settings scenes when registering TestScene.unity.
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

            var netTrans = playerGO.GetComponent<NetworkTransformUnreliable>();
            if (!netTrans) netTrans = playerGO.AddComponent<NetworkTransformUnreliable>();

            // Set NetworkTransformUnreliable syncDirection to ClientToServer for player movement authority
            SetSerializedIntProperty(netTrans, "syncDirection", (int)SyncDirection.ClientToServer);

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

            // Create NetworkManager with KcpTransport
            GameObject netManagerGO = new GameObject("NetworkManager");
            var kcpTransport = netManagerGO.AddComponent<KcpTransport>();
            var lobby = netManagerGO.AddComponent<NetworkLobbyManager>();
            netManagerGO.AddComponent<NetworkManagerHUD>();

            SetSerializedObjectReferenceProperty(lobby, "transport", kcpTransport);

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
            SetSerializedIntProperty(rm1, "cityID", 1);

            var dep1 = reactor1.AddComponent<ReactorDepositContainer>();
            SetSerializedIntProperty(dep1, "targetCityID", 1);

            GameObject wall1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall1.name = "Wall_City1";
            wall1.transform.position = new Vector3(10f, 1.5f, 8f);
            wall1.transform.localScale = new Vector3(8f, 3f, 1f);
            wall1.AddComponent<NetworkIdentity>();
            var wallSync1 = wall1.AddComponent<CityWallHealthSync>();
            SetSerializedIntProperty(wallSync1, "cityID", 1);

            var rep1 = wall1.AddComponent<CityWallRepairPanel>();
            SetSerializedIntProperty(rep1, "targetCityID", 1);

            GameObject warehouse1 = new GameObject("Warehouse_City1");
            warehouse1.AddComponent<NetworkIdentity>();
            var wh1 = warehouse1.AddComponent<SharedInventorySync>();
            SetSerializedIntProperty(wh1, "cityID", 1);

            // Create City 2 Entities (CityID = 2)
            GameObject reactor2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            reactor2.name = "Reactor_City2";
            reactor2.transform.position = new Vector3(-10f, 1f, 0f);
            reactor2.AddComponent<NetworkIdentity>();
            var rm2 = reactor2.AddComponent<ReactorManager>();
            SetSerializedIntProperty(rm2, "cityID", 2);

            var dep2 = reactor2.AddComponent<ReactorDepositContainer>();
            SetSerializedIntProperty(dep2, "targetCityID", 2);

            GameObject wall2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall2.name = "Wall_City2";
            wall2.transform.position = new Vector3(-10f, 1.5f, 8f);
            wall2.transform.localScale = new Vector3(8f, 3f, 1f);
            wall2.AddComponent<NetworkIdentity>();
            var wallSync2 = wall2.AddComponent<CityWallHealthSync>();
            SetSerializedIntProperty(wallSync2, "cityID", 2);

            var rep2 = wall2.AddComponent<CityWallRepairPanel>();
            SetSerializedIntProperty(rep2, "targetCityID", 2);

            GameObject warehouse2 = new GameObject("Warehouse_City2");
            warehouse2.AddComponent<NetworkIdentity>();
            var wh2 = warehouse2.AddComponent<SharedInventorySync>();
            SetSerializedIntProperty(wh2, "cityID", 2);

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

            // Create Canvas HUD with real UI GameObjects and bind references to ReactorHUDUI
            GameObject canvasGO = new GameObject("Canvas_SpanishHUD");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var hudUI = canvasGO.AddComponent<ReactorHUDUI>();
            canvasGO.AddComponent<ScreenFrostPostProcessUI>();

            // Instantiate actual UI elements inside Canvas
            BuildCanvasUIElements(canvasGO, hudUI);

            // Save Scene
            EditorSceneManager.SaveScene(newScene, "Assets/Scenes/TestScene.unity");

            // Merge scene with existing Build Settings scenes (preserving existing scenes)
            PreserveAndRegisterSceneInBuildSettings("Assets/Scenes/TestScene.unity");

            Debug.Log("[BuildTestSceneAndPrefabs] Assets/Scenes/TestScene.unity registered in Build Settings and saved successfully!");
        }

        private static void BuildCanvasUIElements(GameObject canvasGO, ReactorHUDUI hudUI)
        {
            // Panel top container
            GameObject topPanel = new GameObject("TopPanel", typeof(RectTransform));
            topPanel.transform.SetParent(canvasGO.transform, false);

            // Temperature Text
            GameObject tempTextGO = new GameObject("TemperatureText", typeof(RectTransform));
            tempTextGO.transform.SetParent(topPanel.transform, false);
            var tempTMP = tempTextGO.AddComponent<TextMeshProUGUI>();
            tempTMP.text = "20.0°C";
            tempTMP.fontSize = 24;

            // Temperature Slider & Fill Image
            GameObject tempSliderGO = new GameObject("TemperatureSlider", typeof(RectTransform));
            tempSliderGO.transform.SetParent(topPanel.transform, false);
            var tempSlider = tempSliderGO.AddComponent<Slider>();
            tempSlider.minValue = -50f;
            tempSlider.maxValue = 50f;
            tempSlider.value = 20f;

            GameObject fillGO = new GameObject("FillImage", typeof(RectTransform));
            fillGO.transform.SetParent(tempSliderGO.transform, false);
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.color = new Color(1f, 0.5f, 0f);
            tempSlider.fillRect = fillGO.GetComponent<RectTransform>();

            // Ignicita Text & Slider
            GameObject ignTextGO = new GameObject("IgnicitaText", typeof(RectTransform));
            ignTextGO.transform.SetParent(topPanel.transform, false);
            var ignTMP = ignTextGO.AddComponent<TextMeshProUGUI>();
            ignTMP.text = "Ignicita: 100 / 1000";
            ignTMP.fontSize = 20;

            GameObject ignSliderGO = new GameObject("IgnicitaSlider", typeof(RectTransform));
            ignSliderGO.transform.SetParent(topPanel.transform, false);
            var ignSlider = ignSliderGO.AddComponent<Slider>();
            ignSlider.minValue = 0f;
            ignSlider.maxValue = 1f;
            ignSlider.value = 0.1f;

            // Freezing Warning Banner
            GameObject warningBannerGO = new GameObject("FreezingWarningBanner", typeof(RectTransform));
            warningBannerGO.transform.SetParent(canvasGO.transform, false);

            GameObject warningTextGO = new GameObject("WarningText", typeof(RectTransform));
            warningTextGO.transform.SetParent(warningBannerGO.transform, false);
            var warningTMP = warningTextGO.AddComponent<TextMeshProUGUI>();
            warningTMP.text = "¡ADVERTENCIA DE CONGELAMIENTO!";
            warningTMP.color = Color.red;
            warningBannerGO.SetActive(false);

            // Greenhouse Status Indicator
            GameObject ghGO = new GameObject("GreenhouseStatusIndicator", typeof(RectTransform));
            ghGO.transform.SetParent(canvasGO.transform, false);

            GameObject ghTextGO = new GameObject("GreenhouseText", typeof(RectTransform));
            ghTextGO.transform.SetParent(ghGO.transform, false);
            var ghTMP = ghTextGO.AddComponent<TextMeshProUGUI>();
            ghTMP.text = "Invernaderos: OPERATIVOS";
            ghTMP.color = Color.green;

            // Council UI Modal
            GameObject councilModalGO = new GameObject("CouncilPanelModal", typeof(RectTransform));
            councilModalGO.transform.SetParent(canvasGO.transform, false);
            councilModalGO.SetActive(false);

            // Assign references using SerializedObject
            SerializedObject so = new SerializedObject(hudUI);
            SetSOProperty(so, "temperatureText", tempTMP);
            SetSOProperty(so, "temperatureSlider", tempSlider);
            SetSOProperty(so, "temperatureFillImage", fillImg);
            SetSOProperty(so, "ignicitaText", ignTMP);
            SetSOProperty(so, "ignicitaSlider", ignSlider);
            SetSOProperty(so, "freezingWarningBanner", warningBannerGO);
            SetSOProperty(so, "warningBannerText", warningTMP);
            SetSOProperty(so, "greenhouseStatusIndicator", ghGO);
            SetSOProperty(so, "greenhouseStatusText", ghTMP);
            SetSOProperty(so, "councilPanelModal", councilModalGO);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void PreserveAndRegisterSceneInBuildSettings(string newScenePath)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            bool alreadyExists = false;
            foreach (var scene in scenes)
            {
                if (scene.path == newScenePath)
                {
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists)
            {
                scenes.Add(new EditorBuildSettingsScene(newScenePath, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void SetSerializedIntProperty(Component comp, string propertyName, int value)
        {
            if (comp == null) return;
            SerializedObject so = new SerializedObject(comp);
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                prop.intValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning($"[BuildTestSceneAndPrefabs] Property '{propertyName}' not found on {comp.GetType().Name}");
            }
        }

        private static void SetSerializedObjectReferenceProperty(Component comp, string propertyName, UnityEngine.Object obj)
        {
            if (comp == null) return;
            SerializedObject so = new SerializedObject(comp);
            SetSOProperty(so, propertyName, obj);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSOProperty(SerializedObject so, string propertyName, UnityEngine.Object obj)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                prop.objectReferenceValue = obj;
            }
            else
            {
                Debug.LogWarning($"[BuildTestSceneAndPrefabs] Property '{propertyName}' not found on {so.targetObject.GetType().Name}");
            }
        }
    }
}
#endif
