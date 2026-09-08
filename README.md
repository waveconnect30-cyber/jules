# Eco de las Cenizas - Modo PvPvE Cross-Play (PC y Android)

¡Bienvenido al repositorio de **Eco de las Cenizas**, un videojuego multijugador PvPvE cooperativo y competitivo 3D en un mundo helado con soporte **Cross-Play entre PC y Android**.

---

## 📌 Referencia de Entrega
**CODEX-5AC805E-GENERADOR-COMPLETO**

---

## 📄 Documentación del Proyecto

- 📖 **Game Design Document (GDD PvPvE):** [`GDD_Eco_de_las_Cenizas.md`](./GDD_Eco_de_las_Cenizas.md)
- 📊 **Especificaciones Técnicas:** [`docs/Technical_Specifications.md`](./docs/Technical_Specifications.md)
- ⚙️ **Dependencias Unity (Packages):** [`Packages/manifest.json`](./Packages/manifest.json)
- ⚙️ **Versión de Unity Editor:** [`ProjectSettings/ProjectVersion.txt`](./ProjectSettings/ProjectVersion.txt) (`2022.3.10f1`)
- 🤖 **Manifiesto de Permisos Android:** [`Assets/Plugins/Android/AndroidManifest.xml`](./Assets/Plugins/Android/AndroidManifest.xml)
- 🛠️ **Generador de Escena y Prefabs:** [`Assets/Editor/BuildTestSceneAndPrefabs.cs`](./Assets/Editor/BuildTestSceneAndPrefabs.cs)
- 🎮 **Escena de Prueba Multijugador:** [`Assets/Scenes/TestScene.unity`](./Assets/Scenes/TestScene.unity)

---

## 🛠️ Generador de Escena y Prefabs en Unity 2022.3 LTS

Para generar la escena y los prefabs reales con componentes nativos de Unity/Mirror:

1. Abre el proyecto en **Unity 2022.3 LTS**.
2. En la barra de menú superior, selecciona:
   `EcoDeLasCenizas -> Build Real Test Scene and Prefabs`
3. Se generarán automáticamente:
   - `Assets/Prefabs/PlayerPrefab.prefab` (Capsule + NetworkIdentity + NetworkTransformUnreliable + PlayerController + PlayerStatsManager + ClassAbilities)
   - `Assets/Prefabs/ReactorPrefab.prefab` (Cylinder + NetworkIdentity + ReactorManager + ReactorDepositContainer)
   - `Assets/Prefabs/CityWallPrefab.prefab` (Cube + NetworkIdentity + CityWallHealthSync + CityWallRepairPanel)
   - `Assets/Prefabs/ResourceNodePrefab.prefab` (Sphere + NetworkIdentity + IgnicitaHarvestNode)
   - `Assets/Scenes/TestScene.unity` (Light, Ground Plane, Camera, AudioListener, NetworkManagerHUD, GameManager, Spanish Canvas HUD, SpawnPoints)
4. `Assets/Scenes/TestScene.unity` se registrará en `EditorBuildSettings.scenes`.

---

## 🔒 Conservación Atómica de Recursos y Ejemplos Antes/Después

### **1. Depósito de Ignicita al Reactor (`ReactorDepositContainer.cs`):**
- **Validación Previa:** Se resuelve el `ReactorManager` objetivo, la distancia del jugador y la capacidad disponible (`MaxIgnicita - CurrentIgnicita`) **ANTES** de descontar cualquier recurso.
- **Ejemplo Antes/Después:**
  - *Jugador lleva 50 Ignicita; Reactor tiene capacidad restante de 30 Ignicita.*
  - *Resultado:* Se transfieren 30 Ignicita al reactor y el jugador conserva los 20 Ignicita restantes.
  - *Si el jugador está fuera de distancia o el reactor está lleno:* Se cancela el comando sin descontar nada (conserva 50 Ignicita).

### **2. Reparación de Muralla (`CityWallRepairPanel.cs`):**
- **Validación Previa:** Se resuelve la muralla y se verifica que `currentHP < maxHP` **ANTES** de cobrarse el Acero del almacén.
- **Ejemplo Antes/Después:**
  - *Muralla tiene 4800 / 5000 HP; Almacén posee 50 Acero (Costo: 10 Acero).*
  - *Resultado:* Se descuentan 10 Acero y la muralla se repara a 5000 / 5000 HP.
  - *Si la muralla ya está al 100% (5000/5000 HP) o el almacén no tiene Acero:* La reparación se rechaza y el almacén conserva íntegros sus 50 Acero.

---

## 📝 Lista de Puntos Pendientes Documentados (Próxima Iteración Local)

1. **Ejecución Local de Unity 2022.3 y Weaver:** La generación de metadatos `.meta` binarios finales y la compilación del Mirror Weaver dependen de abrir el proyecto e invocar el MenuItem en Unity 2022.3 LTS local.
2. **Pruebas de Conexión de 2 Procesos (Host / Cliente Remoto):** La verificación de interacción física a distancia y sincronización en ejecutable `.exe` / `.apk` requiere ejecutar dos instancias locales en Unity.

---

## 📂 Estructura del Código C# (32 Scripts en `Assets/Scripts/` y `Assets/Editor/`)

| Script | Descripción y Función Principal |
| :--- | :--- |
| `BuildTestSceneAndPrefabs.cs` | Script Editor ejecutable que construye la escena `TestScene.unity` y los 4 prefabs de red reales. |
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
| `GameManager.cs` | Registro de sistemas multi-ciudad por `cityID`, derrota aislada por ciudad (`OnServerCityReactorFrozen`) y gestor de fases. |
