# Eco de las Cenizas - Modo PvPvE Cross-Play (PC y Android)

¡Bienvenido al repositorio de **Eco de las Cenizas**, un videojuego multijugador PvPvE cooperativo y competitivo 3D en un mundo helado con soporte **Cross-Play entre PC y Android**.

---

## 📌 Referencia de Entrega
**CODEX-UNITY-6000-5-10F1**

---

## 📄 Documentación del Proyecto

- 📖 **Game Design Document (GDD PvPvE):** [`GDD_Eco_de_las_Cenizas.md`](./GDD_Eco_de_las_Cenizas.md)
- 📊 **Especificaciones Técnicas:** [`docs/Technical_Specifications.md`](./docs/Technical_Specifications.md)
- ⚙️ **Dependencias Unity (Packages):** [`Packages/manifest.json`](./Packages/manifest.json)
- ⚙️ **Versión de Unity Editor:** [`ProjectSettings/ProjectVersion.txt`](./ProjectSettings/ProjectVersion.txt) (`6000.5.10f1`)
- 🤖 **Manifiesto de Permisos Android:** [`Assets/Plugins/Android/AndroidManifest.xml`](./Assets/Plugins/Android/AndroidManifest.xml)
- 🛠️ **Generador de Escena y Prefabs:** [`Assets/Editor/BuildTestSceneAndPrefabs.cs`](./Assets/Editor/BuildTestSceneAndPrefabs.cs)
- 🎮 **Escena de Prueba Multijugador:** [`Assets/Scenes/TestScene.unity`](./Assets/Scenes/TestScene.unity)

---

## 🚀 Migración a Unity 6 (6000.5.10f1) y Paquetes Oficiales

1. **Versión del Editor:**
   - Se ha fijado `m_EditorVersion: 6000.5.10f1` en `ProjectSettings/ProjectVersion.txt`.
2. **Distribución de Red Mirror 96.11.2:**
   - La dependencia Git UPM de Mirror que causaba conflictos fue eliminada de `Packages/manifest.json`.
   - Se utiliza la importación limpia y oficial de **Mirror 96.11.2** alojada directamente en `Assets/Mirror`.
3. **Versiones de Paquetes en UPM (`Packages/manifest.json`):**
   - Universal Render Pipeline (URP): `com.unity.render-pipelines.universal` -> `17.0.3`
   - Input System: `com.unity.inputsystem` -> `1.13.0`
   - uGUI: `com.unity.ugui` -> `2.0.0`
   - TextMeshPro: `com.unity.textmeshpro` -> `3.0.7`

---

## 🛠️ Generador de Escena, Prefabs y HUD en Unity 6 (6000.5.10f1)

Para generar la escena, los prefabs y el HUD de Canvas real con componentes nativos de Unity y Mirror Networking:

1. Abre el proyecto en **Unity 6000.5.10f1**.
2. En la barra de menú superior, selecciona:
   `EcoDeLasCenizas -> Build Real Test Scene and Prefabs`
3. Se generarán automáticamente:
   - `Assets/Prefabs/PlayerPrefab.prefab` (Capsule + NetworkIdentity + NetworkTransformUnreliable [ClientToServer authority] + PlayerController + PlayerStatsManager + ClassAbilities)
   - `Assets/Prefabs/ReactorPrefab.prefab` (Cylinder + NetworkIdentity + ReactorManager + ReactorDepositContainer)
   - `Assets/Prefabs/CityWallPrefab.prefab` (Cube + NetworkIdentity + CityWallHealthSync + CityWallRepairPanel)
   - `Assets/Prefabs/ResourceNodePrefab.prefab` (Sphere + NetworkIdentity + IgnicitaHarvestNode)
   - `Assets/Scenes/TestScene.unity` (Light, Ground Plane, Camera, AudioListener, NetworkManager [KcpTransport + NetworkLobbyManager + NetworkManagerHUD], GameManager, AssetPrefabLinker, Canvas HUD con TextMeshProUGUI/Sliders/WarningBanner, SpawnPoints)
4. `Assets/Scenes/TestScene.unity` se registrará en `EditorBuildSettings.scenes` conservando las escenas previas existentes.

---

## 🔒 Configuración de Red y Transmisión Serializada

1. **Configuración por SerializedProperty (`BuildTestSceneAndPrefabs.cs`):**
   - Asigna campos privados/protegidos (`cityID`, `targetCityID`) en `ReactorManager`, `CityWallHealthSync`, `SharedInventorySync`, `ReactorDepositContainer` y `CityWallRepairPanel` para Ciudad 1 (`cityID = 1`, `targetCityID = 1`) y Ciudad 2 (`cityID = 2`, `targetCityID = 2`).
   - Asigna `syncDirection = ClientToServer` en `NetworkTransformUnreliable` para control autoritativo del cliente.
   - Enlaza `KcpTransport` a `NetworkManager`.

2. **Gestión de Delegates en Servidor (`GameManager.cs`):**
   - Mantiene un diccionario privado de handlers explícitos (`reactorFreezeHandlers`) para desuscribir eventos específicos de `OnCityFrozenSolid` al reconstruir los registros multi-ciudad sin usar llamadas globales destructivas.

---

## 📝 Lista Honesta de Pendientes para Ejecución Local en Unity 6 Editor

1. **Compilación C#, Weaver y Play Mode:** Se ejecutaron verificaciones sintácticas e integrales de estructura en los scripts C#. La compilación de bytecode IL, el pase de tejido Mirror Weaver y la prueba de Play Mode / ejecutable quedan pendientes de ejecución en el entorno local de Unity 6000.5.10f1.
2. **Invocación del MenuItem Generador:** Invocar `EcoDeLasCenizas -> Build Real Test Scene and Prefabs` en el Editor de Unity 6 para instanciar `Assets/Scenes/TestScene.unity` y los prefabs con sus correspondientes archivos `.meta`.
3. **Pruebas de Conexión de 2 Procesos:** Probar conectividad Host y Cliente remoto en PC/Android o via ParrelSync.

---

## 📂 Estructura del Código C# (32 Scripts)

| Script | Descripción y Función Principal |
| :--- | :--- |
| `BuildTestSceneAndPrefabs.cs` | Generador Editor ejecutable con asignaciones SerializedProperty, KcpTransport, UI Canvas y preservación de escenas. |
| `IInteractable.cs` | Interfaz limpia para objetos interactivos en el mundo 3D. |
| `IgnicitaHarvestNode.cs` | Nodo de recolección de Ignicita validado en servidor por distancia. |
| `ReactorDepositContainer.cs` | Depósito atómico de combustible que valida capacidad y conserva el sobrante del jugador. |
| `CityWallRepairPanel.cs` | Panel de reparación que verifica necesidad de HP antes de cobrar Acero del almacén. |
| `AssetPrefabLinker.cs` | Gestión de carpetas `Assets/Art/` (Models, Textures, Prefabs) y asignación dinámica de modelos 3D. |
| `AndroidPermissionsManager.cs` | Gestión de permisos runtime en Android (`UnityEngine.Android.Permission`) y alerta UI. |
| `TouchScreenHUD.cs` | UI móvil táctil con Joystick virtual y botones para salto, interacción y habilidades. |
| `QualitySettingsManager.cs` | Optimización gráfica adaptativa para PC (60 FPS) y Android (30-60 FPS). |
| `SeasonManager.cs` | Reloj de temporada de 14 días (4 fases), inmunidad de saqueo en Fase 1, impuesto del 5%, persistencia y reinicio. |
| `PlayerController.cs` | Controlador 3D adaptativo PC/Mobile con `cityID`, `isLocalPlayer`, SyncVars, `CmdSelectClass`, reaparición y debuff. |
| `PlayerStatsManager.cs` | Gestión de atributos (`attackPower`, `maxHP`, `harvestSpeed`, `thermalResistance`) y bonificaciones por Ruinas. |
| `RuinsNode.cs` | Ruinas capturables (4 Principales + 8 Secundarias) con buffs globales para la ciudad controladora. |
| `WorldMapManager.cs` | Bucle de presencia continua en servidor para asedio a la Ciudad Presidencial en (0,0,0). |
| `CityAirlock.cs` | Transición al Mapa Mundial exterior y daño HP acumulativo por exposición a la niebla por jugador. |
| `ReactorManager.cs` | Bucle de temperatura en servidor, SyncVar hooks para overlay helado por `cityID`, tasa de congelamiento e impuesto. |
| `CityWallHealthSync.cs` | Sincronización en red de murallas por `cityID`, tratados de Alianza y cambio de fase en Servidor Dedicado. |
| `SharedInventorySync.cs` | Almacén global por `cityID` e interfaz de saqueo (`RaidIgnicita`). |
| `CityLootManager.cs` | Incursión y saqueo `IInteractable` entre ciudades rivales validando brechas/congelación y alianzas. |
| `TerritoryNode.cs` | Nodos neutrales capturables que generan Ignicita pasiva cacheados en intervalos de 1 segundo. |
| `DiplomacyManager.cs` | Sistema diplomático (Alliance, Neutral, War) entre facciones. |
| `MultiChannelChat.cs` | Chat multicanal filtrado en servidor vía `TargetRpc` (Ciudad, Global, Alianza) y pings tácticos rápidos. |
| `GlobalEventManager.cs` | Gestión de eventos climáticos globales (Súper Tormenta Helada acelerando congelamiento del reactor). |
| `PlayerCharacterController.cs` | Movimiento en 3a persona, gancho de agarre del Explorador y debuff de velocidad. |
| `ClassAbilities.cs` | Habilidades únicas activadas vía tecla Q o botón táctil móvil con comandos en Servidor y validación de vida/cooldown. |
| `ReactorHUDUI.cs` | UI del termómetro central, barra de Ignicita e indicador de invernaderos enlazados al local player. |
| `ScreenFrostPostProcessUI.cs` | Efecto visual de bordes helados aislado por `cityID` del jugador local. |
| `ClassSelectionUI.cs` | UI pre-spawn para elegir clase invocando `CmdSelectClass` en PlayerController. |
| `CouncilVotingManager.cs` | Votos de Concejo firmados por `connectionId` validando clase en servidor contra votos duplicados. |
| `EnemyAI.cs` | IA en NavMesh para Sombras Heladas que ataca el muro más debilitado. |
| `NetworkLobbyManager.cs` | Creación y gestión de salas multijugador de 4 a 8 jugadores. |
| `GameManager.cs` | Registro de sistemas multi-ciudad por `cityID`, derrota aislada por ciudad y gestor de fases. |
