# Eco de las Cenizas - Modo PvPvE: Guerra de Calderas y Ruinas Mundiales

¡Bienvenido al repositorio de **Eco de las Cenizas**, un videojuego multijugador PvPvE cooperativo y competitivo 3D en un mundo helado de megaciudades rivales, ruinas mundiales y la gran Ciudad Presidencial en (0,0,0).

---

## 📄 Documentación del Proyecto

- 📖 **Game Design Document (GDD PvPvE):** [`GDD_Eco_de_las_Cenizas.md`](./GDD_Eco_de_las_Cenizas.md)
- 📊 **Especificaciones Técnicas:** [`docs/Technical_Specifications.md`](./docs/Technical_Specifications.md)

---

## 🚀 Mapa Mundial, Ruinas y Atributos de Jugador

1. **Ciudad Presidencial Capital (0,0,0) (`WorldMapManager.cs`):**
   - La capital mundial en el centro del mapa. Controlable mediante un asedio de 5 minutos, duplicando todas las bonificaciones de clan.

2. **4 Ruinas Principales y 8 Ruinas Secundarias (`RuinsNode.cs`):**
   - **Militar Principal:** Otorga +25% de Poder de Ataque (`attackPower`).
   - **Industrial Principal:** Otorga +30% de Velocidad de Recolección (`harvestSpeed`).
   - **Salud Principal:** Otorga +30% de Vida Máxima (`maxHP`).
   - **Energía Principal:** Otorga +40% de Eficiencia Térmica al Reactor.
   - **8 Ruinas Secundarias:** Fortalezas Alpha/Beta, Talleres Este/Oeste, BioLabs Norte/Sur y Subestaciones.

3. **Exclusión y Esclusas de Ciudad (`CityAirlock.cs`):**
   - Transición de la zona segura de la cúpula hacia la Niebla Helada exterior con temporizadores de exposición tóxica.

4. **Sistema Dinámico de Atributos (`PlayerStatsManager.cs`):**
   - Calcula estadísticas efectivas en tiempo real segun el número de Ruinas capturadas por el `cityID` del jugador.

---

## 📂 Estructura del Código C# (22 Scripts)

| Script | Descripción y Función Principal |
| :--- | :--- |
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
