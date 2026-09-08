# Eco de las Cenizas - Modo PvPvE Cross-Play (PC y Android)

¡Bienvenido al repositorio de **Eco de las Cenizas**, un videojuego multijugador PvPvE cooperativo y competitivo 3D en un mundo helado con soporte **Cross-Play entre PC y Android**.

---

## 📌 Referencia de Entrega
**CODEX-EE3F79C-BASE-JUGABLE**

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
   - `Assets/Prefabs/PlayerPrefab.prefab` (Capsule + NetworkIdentity + NetworkTransform + PlayerController + PlayerStatsManager + ClassAbilities)
   - `Assets/Prefabs/ReactorPrefab.prefab` (Cylinder + NetworkIdentity + ReactorManager + ReactorDepositContainer)
   - `Assets/Prefabs/CityWallPrefab.prefab` (Cube + NetworkIdentity + CityWallHealthSync + CityWallRepairPanel)
   - `Assets/Prefabs/ResourceNodePrefab.prefab` (Sphere + NetworkIdentity + IgnicitaHarvestNode)
   - `Assets/Scenes/TestScene.unity` (Light, Ground Plane, NetworkManagerHUD, GameManager, SpawnPoints)

---

## 🔒 Autoridad de Servidor, Transacciones Atómicas y Seguridad

1. **Autoridad y Sincronización del Jugador (`PlayerController.cs`):**
   - Comandos de selección de clase trasladados a `PlayerController.cs` (`CmdSelectClass`).
   - Movimiento, salto e interacciones protegidos por `isLocalPlayer` / `hasAuthority`.
   - Sincronización de `currentHP`, `cityID`, clase e inventario mediante `[SyncVar]`.
2. **Conservación Atómica de Recursos:**
   - **Reactor Deposit (`ReactorDepositContainer.cs`):** Valida proximidad física y estado antes de descontar Ignicita del jugador.
   - **Wall Repair (`CityWallRepairPanel.cs`):** Valida distancia y verifica que el almacén compartido de la ciudad tenga suficiente Acero antes de reparar. Si falla, conserva todos los materiales sin pérdidas.
   - **Raiding (`CityLootManager.cs`):** Requiere brecha de muro o reactor congelado y valida distancia física antes de transferir Ignicita.
3. **Seguridad en Chat y Filtro de Conexiones (`MultiChannelChat.cs`):**
   - Identifica al emisor en el servidor vía `NetworkConnectionToClient`. Filtra los mensajes de canal de Ciudad y Alianza entregándolos vía `TargetRpc` a las conexiones autorizadas.
4. **Asedio Continuo a la Capital (`WorldMapManager.cs`):**
   - Bucle por tiempo en el servidor (`Update()`) comprobando la presencia física continua de los miembros del clan dentro del radio de la Ciudad Presidencial (0,0,0).

---

## 📝 Lista de Puntos Pendientes Documentados (Próxima Iteración)

1. **Compilación y Pruebas Binarias en Editor Unity:** Ejecutar MenuItem `Build Real Test Scene and Prefabs` en Unity 2022.3.10f1 para generar metadatos binarios finales `.meta` y realizar prueba de 2 ejecutable (.exe / .apk).
2. **Asedio a la Capital - Distancia en Interacción:** `CmdInitiateCapitalSiege` utiliza presencia física continua en área (0,0,0); se agregará resolución explícita de disputas si coinciden dos clanes en el área.
3. **Incursiones de Saqueo - Proximidad a Caldera:** `CmdInitiateCityRaid` mide proximidad física al gestor; se ajustará a la collider específica del almacén de la ciudad objetivo.
4. **Persistencia Avanzada de Temporada:** `SeasonManager` guarda `SeasonCompleted` y `LastGovernorCityID` en `PlayerPrefs`; se migrará a base de datos remota JSON/SQL.

---

## 📂 Estructura del Código C# (31 Scripts en `Assets/Scripts/`)

| Script | Descripción y Función Principal |
| :--- | :--- |
| `IInteractable.cs` | Interfaz limpia para objetos interactivos en el mundo 3D. |
| `IgnicitaHarvestNode.cs` | Nodo de recolección de Ignicita validado en servidor por distancia. |
| `ReactorDepositContainer.cs` | Depósito atómico de combustible al reactor por `[Command]` en servidor. |
| `CityWallRepairPanel.cs` | Panel de reparación de murallas que verifica y descuenta Acero en servidor. |
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
| `GameManager.cs` | Registro de sistemas multi-ciudad por `cityID`, gestor de fases en servidor y condiciones de derrota por congelamiento. |
