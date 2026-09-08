# Eco de las Cenizas - Modo PvPvE: Guerra de Calderas

¡Bienvenido al repositorio de **Eco de las Cenizas**, un videojuego multijugador PvPvE cooperativo y competitivo 3D en un mundo helado de megaciudades rivales.

---

## 📄 Documentación del Proyecto

- 📖 **Game Design Document (GDD PvPvE):** [`GDD_Eco_de_las_Cenizas.md`](./GDD_Eco_de_las_Cenizas.md)
- 📊 **Especificaciones Técnicas:** [`docs/Technical_Specifications.md`](./docs/Technical_Specifications.md)

---

## 🚀 Nuevas Mecánicas PvPvE e Integración de Facciones (`cityID`)

1. **Facciones y Propiedad (`cityID`):**
   - Todos los jugadores poseen una propiedad `public int cityID`.
   - `ReactorManager.cs`, `CityWallHealthSync.cs` y `SharedInventorySync.cs` verifican el `cityID` para permitir reparaciones/depósitos aliados o bloquear interacciones enemigas.

2. **Sistema de Saqueo de Ciudades (`CityLootManager.cs`):**
   - Cuando las defensas o temperatura de una ciudad caen, los jugadores de ciudades rivales pueden ejecutar un asalto (`ExecuteCityRaid`) para saquear el 25-37.5% de la Ignicita del almacén enemigo e importarla a su propia caldera.

3. **Puntos de Control Neutrales (`TerritoryNode.cs`):**
   - Puntos capturables en la Niebla Helada que otorgan flujo pasivo continuo de Ignicita al almacén de la ciudad controladora.

---

## 📂 Estructura del Código C#

| Script | Descripción y Función Principal |
| :--- | :--- |
| `PlayerController.cs` | Controlador 3D con `cityID`, salto, interacciones `IInteractable` y debuff de velocidad. |
| `ReactorManager.cs` | Bucle de temperatura, consumo de Ignicita, filtrado por `cityID` y alerta a -10°C. |
| `CityWallHealthSync.cs` | Sincronización en red de murallas por `cityID` (reparación aliada vs daño enemigo). |
| `SharedInventorySync.cs` | Almacén global por `cityID` e interfaz de saqueo (`RaidIgnicita`). |
| `CityLootManager.cs` | Sistema de asalto y robo de recursos entre ciudades rivales. |
| `TerritoryNode.cs` | Nodos neutrales capturables que generan Ignicita pasiva para la `cityID` dominante. |
| `PlayerCharacterController.cs` | Movimiento en 3a persona, gancho de agarre del Explorador y debuff de velocidad. |
| `ClassAbilities.cs` | Habilidades únicas (Escáner de mineral, torreta de reparación, etc.). |
| `ReactorHUDUI.cs` | UI del termómetro central, barra de Ignicita e indicador de invernaderos. |
| `ScreenFrostPostProcessUI.cs` | Efecto visual de bordes helados en pantalla y banner de advertencia. |
| `ClassSelectionUI.cs` | UI pre-spawn para elegir entre Explorador, Ingeniero, Científico y Táctico. |
| `CouncilVotingManager.cs` | Sesiones de votación ponderadas por clase para distribuir recursos. |
| `EnemyAI.cs` | IA en NavMesh para Sombras Heladas que ataca el muro más debilitado. |
| `NetworkLobbyManager.cs` | Creación y gestión de salas multijugador de 4 a 8 jugadores. |
| `GameManager.cs` | Gestor del bucle de fases (Expedición, Asedio, Concejo) y condiciones de victoria/derrota. |
