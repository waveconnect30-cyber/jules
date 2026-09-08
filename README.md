# Eco de las Cenizas - Modo PvPvE Cross-Play (PC y Android)

¡Bienvenido al repositorio de **Eco de las Cenizas**, un videojuego multijugador PvPvE cooperativo y competitivo 3D en un mundo helado con soporte **Cross-Play entre PC y Android**.

---

## 📄 Documentación del Proyecto

- 📖 **Game Design Document (GDD PvPvE):** [`GDD_Eco_de_las_Cenizas.md`](./GDD_Eco_de_las_Cenizas.md)
- 📊 **Especificaciones Técnicas:** [`docs/Technical_Specifications.md`](./docs/Technical_Specifications.md)
- 🤖 **Manifiesto de Permisos Android:** [`Plugins/Android/AndroidManifest.xml`](./Plugins/Android/AndroidManifest.xml)

---

## 🎨 Organización de Assets 3D y Vinculación de Prefabs (`Assets/Art/`)

Los modelos 3D (.fbx / .glb) y texturas del proyecto están organizados en las siguientes subcarpetas:

```
Assets/
  Art/
    Models/     -> Archivos 3D (.fbx, .glb) para supervivientes, reactor, murallas y monstruos
    Textures/   -> Texturas PBR (Albedo, Normal, Roughness, Emission)
    Prefabs/    -> Prefabs configurados con scripts y componentes de red
```

### **Vinculación Dinámica en Código (`AssetPrefabLinker.cs`):**
- **Explorador / Clases:** El script `AssetPrefabLinker.cs` asigna dinámicamente el modelo 3D del Explorador (traje ártico, máscara de gas con visor naranja) y demás clases al `PlayerController.cs` según la clase seleccionada.
- **Reactor:** Vincula la malla 3D industrial al script `ReactorManager.cs`.
- **Murallas:** Vincula los tramos de muralla al script `CityWallHealthSync.cs`.
- **Enemigos:** Vincula el modelo de cristal de hielo e ignorancia sombría a `EnemyAI.cs`.

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

## 📂 Estructura del Código C# (27 Scripts)

| Script | Descripción y Función Principal |
| :--- | :--- |
| `AssetPrefabLinker.cs` | Gestión de carpetas `Assets/Art/` (Models, Textures, Prefabs) y asignación dinámica de modelos 3D. |
| `AndroidPermissionsManager.cs` | Gestión de permisos runtime en Android (`UnityEngine.Android.Permission`) y alerta UI. |
| `TouchScreenHUD.cs` | UI móvil táctil con Joystick virtual y botones para salto, interacción y habilidades. |
| `QualitySettingsManager.cs` | Optimización gráfica adaptativa para PC (60 FPS) y Android (30-60 FPS). |
| `SeasonManager.cs` | Reloj de temporada de 14 días (4 fases), inmunidad de saqueo en Fase 1, impuesto del 5% y reinicio. |
| `PlayerController.cs` | Controlador 3D adaptativo PC/Mobile con `cityID`, salto, interacciones y debuff de velocidad. |
| `PlayerStatsManager.cs` | Gestión de atributos (`attackPower`, `maxHP`, `harvestSpeed`, `thermalResistance`) y bonificaciones por Ruinas. |
| `RuinsNode.cs` | Ruinas capturables (4 Principales + 8 Secundarias) con buffs globales para la ciudad controladora. |
| `WorldMapManager.cs` | Gestión del Mapa Mundial y asedio a la Ciudad Presidencial en (0,0,0). |
| `CityAirlock.cs` | Transición al Mapa Mundial exterior y temporizadores de exposición a la niebla por jugador. |
| `ReactorManager.cs` | Bucle de temperatura, consumo de Ignicita, filtrado por `cityID` e impuesto del Gobernador del 5%. |
| `CityWallHealthSync.cs` | Sincronización en red de murallas por `cityID` (reparación aliada vs daño enemigo). |
| `SharedInventorySync.cs` | Almacén global por `cityID` e interfaz de saqueo (`RaidIgnicita`). |
| `CityLootManager.cs` | Sistema de asalto y robo de recursos entre ciudades rivales. |
| `TerritoryNode.cs` | Nodos neutrales capturables que generan Ignicita pasiva para la `cityID` dominante. |
| `DiplomacyManager.cs` | Sistema diplomático (Alliance, Neutral, War) entre facciones. |
| `MultiChannelChat.cs` | Chat multicanal (Ciudad, Global, Alianza) y pings tácticos rápidos. |
| `GlobalEventManager.cs` | Gestión de eventos climáticos globales (Súper Tormenta Helada). |
| `PlayerCharacterController.cs` | Movimiento en 3a persona, gancho de agarre del Explorador y debuff de velocidad. |
| `ClassAbilities.cs` | Habilidades únicas (Escáner de mineral, torreta de reparación, etc.). |
| `ReactorHUDUI.cs` | UI del termómetro central, barra de Ignicita e indicador de invernaderos. |
| `ScreenFrostPostProcessUI.cs` | Efecto visual de bordes helados en pantalla y banner de advertencia. |
| `ClassSelectionUI.cs` | UI pre-spawn para elegir entre Explorador, Ingeniero, Científico y Táctico. |
| `CouncilVotingManager.cs` | Sesiones de votación ponderadas por clase para distribuir recursos. |
| `EnemyAI.cs` | IA en NavMesh para Sombras Heladas que ataca el muro más debilitado. |
| `NetworkLobbyManager.cs` | Creación y gestión de salas multijugador de 4 a 8 jugadores. |
| `GameManager.cs` | Gestor del bucle de fases (Expedición, Asedio, Concejo) y condiciones de victoria/derrota. |
