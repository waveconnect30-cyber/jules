# Eco de las Cenizas - Modo PvPvE Cross-Play (PC y Android)

¡Bienvenido al repositorio de **Eco de las Cenizas**, un videojuego multijugador PvPvE cooperativo y competitivo 3D en un mundo helado con soporte **Cross-Play entre PC y Android**.

---

## 📄 Documentación del Proyecto

- 📖 **Game Design Document (GDD PvPvE):** [`GDD_Eco_de_las_Cenizas.md`](./GDD_Eco_de_las_Cenizas.md)
- 📊 **Especificaciones Técnicas:** [`docs/Technical_Specifications.md`](./docs/Technical_Specifications.md)
- ⚙️ **Dependencias Unity (Packages):** [`Packages/manifest.json`](./Packages/manifest.json)
- 🤖 **Manifiesto de Permisos Android:** [`Plugins/Android/AndroidManifest.xml`](./Plugins/Android/AndroidManifest.xml)

---

## 🔒 Autoridad de Servidor, Seguridad y Optimización de Red

1. **Autoridad de Jugador (`PlayerController.cs`):**
   - Movimiento, salto e interacciones protegidos por comprobación `isLocalPlayer` / `hasAuthority`.
2. **Sincronización de Estado Servidor (`ReactorManager.cs`, `GameManager.cs`):**
   - Transiciones de temperatura y fases de juego procesadas exclusivamente en `[Server]` y sincronizadas vía `[SyncVar]`.
3. **Seguridad y Filtrado de Chat (`MultiChannelChat.cs`):**
   - Validación de emisor en el servidor. Mensajes de canal de Ciudad y Alianza filtrados según `cityID` y tratados diplomáticos activos.
4. **Registro Único de Votos del Concejo (`CouncilVotingManager.cs`):**
   - Registro de ID único de jugador (`PlayerID`). Rechaza intentos de votación duplicada en la misma sesión.
5. **Optimizaciones de Rendimiento (`TerritoryNode.cs`):**
   - Almacenes cacheados por `cityID` con entregas de recursos agrupadas en intervalos de 1 segundo (eliminando búsquedas por fotograma en `Update()`).

---

## 📱📱 Permisos y Soporte Multiplataforma Android

1. **Manifiesto de Permisos (`Plugins/Android/AndroidManifest.xml`):**
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

## 📂 Estructura del Código C# (28 Scripts)

| Script | Descripción y Función Principal |
| :--- | :--- |
| `IInteractable.cs` | Interfaz limpia para objetos interactivos en el mundo 3D. |
| `AssetPrefabLinker.cs` | Gestión de carpetas `Assets/Art/` (Models, Textures, Prefabs) y asignación dinámica de modelos 3D. |
| `AndroidPermissionsManager.cs` | Gestión de permisos runtime en Android (`UnityEngine.Android.Permission`) y alerta UI. |
| `TouchScreenHUD.cs` | UI móvil táctil con Joystick virtual y botones para salto, interacción y habilidades. |
| `QualitySettingsManager.cs` | Optimización gráfica adaptativa para PC (60 FPS) y Android (30-60 FPS). |
| `SeasonManager.cs` | Reloj de temporada de 14 días (4 fases), inmunidad de saqueo en Fase 1, impuesto del 5% y reinicio. |
| `PlayerController.cs` | Controlador 3D adaptativo PC/Mobile con `cityID`, `isLocalPlayer`, salto, interacciones y debuff de velocidad. |
| `PlayerStatsManager.cs` | Gestión de atributos (`attackPower`, `maxHP`, `harvestSpeed`, `thermalResistance`) y bonificaciones por Ruinas. |
| `RuinsNode.cs` | Ruinas capturables (4 Principales + 8 Secundarias) con buffs globales para la ciudad controladora. |
| `WorldMapManager.cs` | Gestión del Mapa Mundial y asedio a la Ciudad Presidencial en (0,0,0). |
| `CityAirlock.cs` | Transición al Mapa Mundial exterior y daño HP por exposición acumulada por jugador. |
| `ReactorManager.cs` | Bucle de temperatura en servidor, consumo de Ignicita, filtrado por `cityID` e impuesto del Gobernador del 5%. |
| `CityWallHealthSync.cs` | Sincronización en red de murallas por `cityID` (reparación aliada vs daño enemigo). |
| `SharedInventorySync.cs` | Almacén global por `cityID` e interfaz de saqueo (`RaidIgnicita`). |
| `CityLootManager.cs` | Sistema de asalto y robo de recursos entre ciudades rivales. |
| `TerritoryNode.cs` | Nodos neutrales capturables que generan Ignicita pasiva cacheados cada 1 segundo. |
| `DiplomacyManager.cs` | Sistema diplomático (Alliance, Neutral, War) entre facciones. |
| `MultiChannelChat.cs` | Chat multicanal filtrado en cliente/servidor (Ciudad, Global, Alianza) y pings tácticos rápidos. |
| `GlobalEventManager.cs` | Gestión de eventos climáticos globales (Súper Tormenta Helada con multiplicador de cooldowns x2). |
| `PlayerCharacterController.cs` | Movimiento en 3a persona, gancho de agarre del Explorador y debuff de velocidad. |
| `ClassAbilities.cs` | Habilidades únicas activadas vía tecla Q o botón táctil móvil. |
| `ReactorHUDUI.cs` | UI del termómetro central, barra de Ignicita e indicador de invernaderos. |
| `ScreenFrostPostProcessUI.cs` | Efecto visual de bordes helados en pantalla y banner de advertencia. |
| `ClassSelectionUI.cs` | UI pre-spawn para elegir entre Explorador, Ingeniero, Científico y Táctico. |
| `CouncilVotingManager.cs` | Votos ponderados por clase con registro único por PlayerID contra votos duplicados. |
| `EnemyAI.cs` | IA en NavMesh para Sombras Heladas que ataca el muro más debilitado. |
| `NetworkLobbyManager.cs` | Creación y gestión de salas multijugador de 4 a 8 jugadores. |
| `GameManager.cs` | Gestor del bucle de fases en servidor (Expedición, Asedio, Concejo) y condiciones de victoria/derrota. |
