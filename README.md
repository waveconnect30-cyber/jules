# Eco de las Cenizas - Modo PvPvE: Guerra de Calderas y Temporadas de 14 Días

¡Bienvenido al repositorio de **Eco de las Cenizas**, un videojuego multijugador PvPvE cooperativo y competitivo 3D en un mundo helado de megaciudades rivales, ruinas mundiales, el título de Gobernador y temporadas de 14 días.

---

## 📄 Documentación del Proyecto

- 📖 **Game Design Document (GDD PvPvE):** [`GDD_Eco_de_las_Cenizas.md`](./GDD_Eco_de_las_Cenizas.md)
- 📊 **Especificaciones Técnicas:** [`docs/Technical_Specifications.md`](./docs/Technical_Specifications.md)

---

## 🗓️ Calendario de Temporada de 14 Días (`SeasonManager.cs`)

| Fase de Temporada | Días | Mecánicas y Reglas de Juego |
| :--- | :--- | :--- |
| **Fase 1: Settlement** | Días 1–3 | Inmunidad de Saqueo activa. Construcción y fortificación segura de la ciudad. |
| **Fase 2: Expansion** | Días 4–8 | Inmunidad desactivada. Incursiones de saqueo (`CityLootManager`) y captura de Ruinas. |
| **Fase 3: PresidentialSiege**| Días 9–12 | Captura del nodo Presidencial (0,0,0). Mantener control por 3 hrs corona al **Gobernador**. |
| **Fase 4: OverloadWipe** | Días 13–14 | Súper Tormenta Helada global, entrega de cosméticos trofeo y reinicio de servidor. |

---

## 🚀 Mecánicas Clave del Sistema de Temporadas y Gobernador

1. **Impuesto del Gobernador (5% Global):**
   - El clan que ostente el título de Gobernador recibe automáticamente un **5% de impuesto pasivo** sobre toda la Ignicita procesada en el servidor.
2. **Condición de Victoria de la Capital (0,0,0):**
   - El clan que mantenga el control ininterrumpido de la Ciudad Presidencial durante **3 horas consecutivas** obtiene el título de Gobernador de La Caldera.

---

## 📂 Estructura del Código C# (23 Scripts)

| Script | Descripción y Función Principal |
| :--- | :--- |
| `SeasonManager.cs` | Reloj de temporada de 14 días (4 fases), inmunidad de saqueo en Fase 1, impuesto del 5% y reinicio. |
| `PlayerController.cs` | Controlador 3D con `cityID`, salto, interacciones `IInteractable` y debuff de velocidad. |
| `PlayerStatsManager.cs` | Gestión de atributos (`attackPower`, `maxHP`, `harvestSpeed`, `thermalResistance`) y bonificaciones por Ruinas. |
| `RuinsNode.cs` | Ruinas capturables (4 Principales + 8 Secundarias) con buffs globales para la ciudad controladora. |
| `WorldMapManager.cs` | Gestión del Mapa Mundial y asedio a la Ciudad Presidencial en (0,0,0). |
| `CityAirlock.cs` | Transición al Mapa Mundial exterior y temporizadores de exposición a la niebla. |
| `ReactorManager.cs` | Bucle de temperatura, consumo de Ignicita, filtrado por `cityID` y alerta a -10°C. |
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
