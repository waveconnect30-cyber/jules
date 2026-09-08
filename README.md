# Eco de las Cenizas - Modo PvPvE Cross-Play (PC y Android)

¡Bienvenido al repositorio de **Eco de las Cenizas**, un videojuego multijugador PvPvE cooperativo y competitivo 3D en un mundo helado con soporte **Cross-Play entre PC y Android**.

---

## 📄 Documentación del Proyecto

- 📖 **Game Design Document (GDD PvPvE):** [`GDD_Eco_de_las_Cenizas.md`](./GDD_Eco_de_las_Cenizas.md)
- 📊 **Especificaciones Técnicas:** [`docs/Technical_Specifications.md`](./docs/Technical_Specifications.md)
- ⚙️ **Dependencias Unity (Packages):** [`Packages/manifest.json`](./Packages/manifest.json)
- ⚙️ **Versión de Unity Editor:** [`ProjectSettings/ProjectVersion.txt`](./ProjectSettings/ProjectVersion.txt) (`2022.3.10f1`)
- 🤖 **Manifiesto de Permisos Android:** [`Assets/Plugins/Android/AndroidManifest.xml`](./Assets/Plugins/Android/AndroidManifest.xml)

---

## 🔒 Autoridad de Servidor, Seguridad y Compatibilidad Dedicated Server

1. **Estructura de Proyecto Unity:**
   - Todos los C# scripts residen en `Assets/Scripts/` y los plugins móviles en `Assets/Plugins/`.
   - `Packages/manifest.json` incluye la dependencia oficial Git de **Mirror Networking** (`com.vis2k.mirror`).
2. **Autoridad y Sincronización del Jugador (`PlayerController.cs`):**
   - Movimiento e interacciones protegidos por `isLocalPlayer` / `hasAuthority`.
   - Sincronización de `currentHP`, `cityID`, clase e inventario mediante `[SyncVar]`.
   - Lógica de muerte y reaparición (`Respawn`) en servidor con punto de reaparición por `cityID`.
3. **Implementaciones Concretas de Interacción (`IInteractable`):**
   - `IgnicitaHarvestNode.cs`: Extracción de Ignicita a inventario.
   - `ReactorDepositContainer.cs`: Depósito de combustible al reactor por `[Command]`.
   - `CityWallRepairPanel.cs`: Reparación de muralla consumiendo materiales por `[Command]`.
4. **Seguridad en Chat y Habilidades en Servidor (`MultiChannelChat.cs`, `ClassAbilities.cs`):**
   - Emisor validado vía `NetworkConnectionToClient`. Mensajes de Ciudad/Alianza enviados por `TargetRpc` a clientes autorizados.
   - Habilidades ejecutadas vía `[Command]` en el Servidor (incluyendo `NetworkServer.Spawn` para la torreta del Ingeniero).
5. **Concejo, Saqueos y Servidor Dedicado (`CouncilVotingManager.cs`, `CityLootManager.cs`, `CityWallHealthSync.cs`):**
   - Votos firmados por `netId` con registro único contra votos duplicados.
   - Bloqueo de daño y saqueos entre ciudades con tratado de `Alliance` activo.
   - Transiciones de fase procesadas directamente en Servidor Dedicado tras brecha de muralla.
6. **Sincronización Climática y Daño Real (`GlobalEventManager.cs`, `CityAirlock.cs`):**
   - Eventos sincronizados por `[SyncVar]` duplicando cooldowns de habilidades durante la Súper Tormenta.
   - Daño real por exposición acumulada al frío tóxico en `CityAirlock.cs`.

---

## 📱📱 Permisos y Soporte Multiplataforma Android

1. **Manifiesto de Permisos (`Assets/Plugins/Android/AndroidManifest.xml`):**
   - Permisos para multijugador y chat de voz: `INTERNET`, `ACCESS_NETWORK_STATE`, `WAKE_LOCK`, `RECORD_AUDIO` y `WRITE_EXTERNAL_STORAGE`.
2. **Gestor de Permisos Runtime (`AndroidPermissionsManager.cs`):**
   - Comprueba y solicita permisos al iniciar la app en Android utilizando `UnityEngine.Android.Permission`. Muestra una advertencia en UI si un permiso crítico es rechazado.
3. **HUD Táctil Móvil (`TouchScreenHUD.cs`):**
   - Interfaz gráfica táctil con Joystick virtual para movimiento 3D y botones dinámicos para salto, interacción y habilidades.
4. **Gestor de Calidad Gráfica (`QualitySettingsManager.cs`):**
   - Ajusta dinámicamente sombras, distancia de dibujado y límites de textura para garantizar 30–60 FPS en móviles.

---

## 🛠️ Instrucciones de Exportación en Unity (.apk / .aab)

### 💻 Compilación para PC Windows (.exe)
1. Abre Unity Editor en **PC, Mac & Linux Standalone**.
2. Ve a `File -> Build Settings`.
3. Selecciona **Standalone Windows (x86_64)** como plataforma objetivo.
4. Haz clic en **Build** y guarda el ejecutable `.exe`.

### 📱 Compilación para Android (.apk / .aab)
1. Ve a `File -> Build Settings`.
2. Selecciona la plataforma **Android** y haz clic en **Switch Platform**.
3. Asegúrate de tener instalado el **Android NDK/SDK** en Unity Hub.
4. **Para archivo ejecutable directo (.apk):**
   - Desmarca la opción *Build App Bundle (.aab)*.
   - Haz clic en **Build** para generar el archivo `.apk` e instalarlo vía APK Installer o ADB (`adb install app.apk`).
5. **Para archivo de publicación Google Play Store (.aab):**
   - Marca la casilla **Build App Bundle (.aab)**.
   - Configura tu KeyStore en *Player Settings -> Publishing Settings*.
   - Haz clic en **Build** para generar el archivo `.aab` listo para subir a Google Play Console.

---

## 📂 Estructura del Código C# (31 Scripts en `Assets/Scripts/`)

| Script | Descripción y Función Principal |
| :--- | :--- |
| `IInteractable.cs` | Interfaz limpia para objetos interactivos en el mundo 3D. |
| `IgnicitaHarvestNode.cs` | Nodo de recolección de Ignicita en el mapa mundo. |
| `ReactorDepositContainer.cs` | Depósito de combustible al reactor vía `[Command]`. |
| `CityWallRepairPanel.cs` | Panel de reparación de murallas vía `[Command]`. |
| `AssetPrefabLinker.cs` | Gestión de carpetas `Assets/Art/` (Models, Textures, Prefabs) y asignación dinámica de modelos 3D. |
| `AndroidPermissionsManager.cs` | Gestión de permisos runtime en Android (`UnityEngine.Android.Permission`) y alerta UI. |
| `TouchScreenHUD.cs` | UI móvil táctil con Joystick virtual y botones para salto, interacción y habilidades. |
| `QualitySettingsManager.cs` | Optimización gráfica adaptativa para PC (60 FPS) y Android (30-60 FPS). |
| `SeasonManager.cs` | Reloj de temporada de 14 días (4 fases), inmunidad de saqueo en Fase 1, impuesto del 5% y reinicio. |
| `PlayerController.cs` | Controlador 3D adaptativo PC/Mobile con `cityID`, `isLocalPlayer`, SyncVars, reaparición y debuff de velocidad. |
| `PlayerStatsManager.cs` | Gestión de atributos (`attackPower`, `maxHP`, `harvestSpeed`, `thermalResistance`) y bonificaciones por Ruinas. |
| `RuinsNode.cs` | Ruinas capturables (4 Principales + 8 Secundarias) con buffs globales para la ciudad controladora. |
| `WorldMapManager.cs` | Gestión del Mapa Mundial y asedio a la Ciudad Presidencial en (0,0,0) desbloqueado en Fase 3. |
| `CityAirlock.cs` | Transición al Mapa Mundial exterior y daño HP acumulativo por exposición a la niebla por jugador. |
| `ReactorManager.cs` | Bucle de temperatura en servidor, SyncVar hooks para overlay helado, consumo e impuesto del Gobernador del 5%. |
| `CityWallHealthSync.cs` | Sincronización en red de murallas por `cityID`, tratados de Alianza y cambio de fase en Servidor Dedicado. |
| `SharedInventorySync.cs` | Almacén global por `cityID` e interfaz de saqueo (`RaidIgnicita`). |
| `CityLootManager.cs` | Sistema de asalto y robo de recursos entre ciudades rivales validando brecha de muro/congelación y alianzas. |
| `TerritoryNode.cs` | Nodos neutrales capturables que generan Ignicita pasiva cacheados en intervalos de 1 segundo. |
| `DiplomacyManager.cs` | Sistema diplomático (Alliance, Neutral, War) entre facciones. |
| `MultiChannelChat.cs` | Chat multicanal filtrado en servidor vía `TargetRpc` (Ciudad, Global, Alianza) y pings tácticos rápidos. |
| `GlobalEventManager.cs` | Gestión de eventos climáticos globales (Súper Tormenta Helada con multiplicador de cooldowns x2 vía SyncVar). |
| `PlayerCharacterController.cs` | Movimiento en 3a persona, gancho de agarre del Explorador y debuff de velocidad. |
| `ClassAbilities.cs` | Habilidades únicas activadas vía tecla Q o botón táctil móvil con comandos en Servidor. |
| `ReactorHUDUI.cs` | UI del termómetro central, barra de Ignicita e indicador de invernaderos. |
| `ScreenFrostPostProcessUI.cs` | Efecto visual de bordes helados en pantalla y banner de advertencia. |
| `ClassSelectionUI.cs` | UI pre-spawn para elegir entre Explorador, Ingeniero, Científico y Táctico. |
| `CouncilVotingManager.cs` | Votos firmados por `netId` con registro único contra votos duplicados. |
| `EnemyAI.cs` | IA en NavMesh para Sombras Heladas que ataca el muro más debilitado. |
| `NetworkLobbyManager.cs` | Creación y gestión de salas multijugador de 4 a 8 jugadores. |
| `GameManager.cs` | Gestor del bucle de fases en servidor (Expedición, Asedio, Concejo) y condiciones de victoria/derrota. |
