# GAME DESIGN DOCUMENT (GDD)
# Eco de las Cenizas - Modo PvPvE: Guerra de Calderas y Ruinas Mundiales

**Género:** Multijugador PvPvE Cooperativo 3D / Guerra de Clanes / Supervivencia en Megaciudad
**Plataforma Objetivo:** PC / Consolas de última generación (Unreal Engine 5 / Unity)
**Perspectiva:** Tercera Persona (3D Action/Survival & Base Management)
**Público Objetivo:** Jugadores de supervivencia cooperativa y competitiva (ej. *Rust*, *Helldivers 2*, *Division 2 Dark Zone*, *Frostpunk*).

---

## 1. Premisa y Visión General (Modelo PvPvE Multiciudad)

En un mundo consumido por un invierno cataclísmico y la **'Niebla Helada'**, múltiples megaciudades industriales rivales (*La Caldera Alpha*, *La Caldera Beta*, etc.) compiten ferozmente por el control de la **Ciudad Presidencial** en el centro del mapa (0,0,0) y las **Ruinas Industriales y Militares** esparcidas por el territorio.

---

## 2. Mapa Mundial, Ruinas Centrales y Atributos de Jugador

### **2.1. Ciudad Presidencial Capital (0,0,0)**
- **Ubicación:** Centro absoluto del mapa de juego.
- **Mecánica de Asedio (`WorldMapManager`):** Requiere sostener un asedio de 5 minutos en el perímetro central. El clan victorioso toma la capital y duplica todas las bonificaciones pasivas de sus ruinas.

### **2.2. Sistema de Ruinas (`RuinsNode`)**
El mapa contiene **4 Ruinas Principales** y **8 Ruinas Secundarias** de apoyo:
- **Ruina Militar Principal:** Otorgar +25% Poder de Ataque (`attackPower`).
- **Ruina Industrial Principal:** Otorgar +30% Velocidad de Recolección (`harvestSpeed`).
- **Ruina de Salud Principal:** Otorgar +30% Vida Máxima (`maxHP`).
- **Ruina de Energía Principal:** Otorgar +40% Eficiencia de Combustible al Reactor Central.
- **8 Ruinas Secundarias:** Fortalezas Alpha/Beta, Talleres Este/Oeste, BioLabs Norte/Sur y Subestaciones Eléctricas.

### **2.3. Esclusas de Ciudad (`CityAirlock`) y Atributos (`PlayerStatsManager`)**
- **Esclusa de Salida:** Los jugadores cruzan la esclusa sellada de su ciudad para adentrarse en la niebla. Al salir de la cúpula térmica, se inicia el contador de exposición al frío tóxico.
- **Gestión Dinámica de Stats:** El script `PlayerStatsManager` calcula en tiempo real `attackPower`, `maxHP`, `harvestSpeed` y `thermalResistance` según las Ruinas activas controladas por la `cityID` del jugador.

---

## 3. Core Loop PvPvE (Bucle Principal de Juego)

```
       +-------------------------------------------------------+
       |   FASE 1: Salida por Esclusa y Captura de Ruinas      |
       |   - Transición por `CityAirlock` hacia la Niebla     |
       |   - Captura de Ruinas Principales y Secundarias      |
       +---------------------------+---------------------------+
                                   |
                                   v
       +-------------------------------------------------------+
       |   FASE 2: Asedio a la Ciudad Presidencial (0,0,0)     |
       |   - Batallas masivas por el control del centro (0,0,0) |
       |   - Saqueo de Almacenes Enemigos (`CityLootManager`)   |
       +---------------------------+---------------------------+
                                   |
                                   v
       +-------------------------------------------------------+
       |   FASE 3: Defensa, Diplomacia y Tormentas Heladas     |
       |   - Repeler Sombras Heladas y Súper Tormentas Heladas  |
       |   - Chat Multicanal y Alianzas por `DiplomacyManager` |
       +-------------------------------------------------------+
```

---

## 4. Guion y Narrativa Ambiental (Tutorial de Ruinas y Mapa Mundial)

**Personaje:** *Maelo*, el anciano operador de La Caldera Nivel 1.

> *(Maelo señala el gran mapa táctico de metal en la pared del reactor)*
>
> "Atento, novato. Allá afuera en el centro del mapa está la vieja **Ciudad Presidencial**. Quien controle esa aguja de acero dominará toda la región. Pero no podrás llegar hasta ella sin antes tomar las **Ruinas Industriales y Militares** que la rodean.
>
> Toma la **Esclusa de Salida**. Cada Ruina que nuestro clan capture aumentará la fuerza de tus armas, tu salud y la velocidad con la que extraes Ignicita. Pero ten cuidado: la niebla envenena si te alejas demasiado de los domos térmicos. ¡Cruza la puerta y reclama esas ruinas para La Caldera!"
