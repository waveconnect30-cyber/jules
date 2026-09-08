# Eco de las Cenizas - Modo PvPvE Cross-Play (PC y Android)

¡Bienvenido al repositorio de **Eco de las Cenizas**, un videojuego multijugador PvPvE cooperativo y competitivo 3D en un mundo helado con soporte **Cross-Play entre PC y Android**.

---

## 📄 Documentación del Proyecto

- 📖 **Game Design Document (GDD PvPvE):** [`GDD_Eco_de_las_Cenizas.md`](./GDD_Eco_de_las_Cenizas.md)
- 📊 **Especificaciones Técnicas:** [`docs/Technical_Specifications.md`](./docs/Technical_Specifications.md)

---

## 📱📱 Soporte Multiplataforma Cross-Play (PC & Android)

1. **HUD Táctil Móvil (`TouchScreenHUD.cs`):**
   - Interfaz gráfica táctil con Joystick virtual para movimiento 3D y botones dinámicos para salto, interacción y habilidades.
   - Activación automática en plataformas móviles (`Application.isMobilePlatform`).
2. **Controles Adaptativos (`PlayerController.cs`):**
   - Responde automáticamente a entradas de Teclado/Mouse (PC) y Touch/Virtual Joystick (Android).
3. **Gestor de Calidad Gráfica (`QualitySettingsManager.cs`):**
   - Ajusta dinámicamente sombras, distancia de dibujado y límites de textura para garantizar 30–60 FPS en móviles.

---

## 🛠️ Instrucciones para Compilar la Build de PC (.exe) y Android (.apk)

### 💻 Compilación para PC Windows (.exe)
1. Abre Unity Editor en **PC, Mac & Linux Standalone**.
2. Ve a `File -> Build Settings`.
3. Selecciona **Standalone Windows (x86_64)** como plataforma objetivo.
4. Haz clic en **Build** y guarda el ejecutable `.exe`.

### 📱 Compilación para Android (.apk)
1. Ve a `File -> Build Settings`.
2. Selecciona la plataforma **Android** y haz clic en **Switch Platform**.
3. Asegúrate de tener instalado el **Android NDK/SDK** en Unity Hub.
4. En *Player Settings -> Identification*, configura el **Package Name** (ej: `com.ecodelascenizas.game`).
5. Haz clic en **Build** para generar el archivo `.apk` instalable.

---

## 📂 Estructura del Código C# (25 Scripts)

| Script | Descripción y Función Principal |
| :--- | :--- |
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
