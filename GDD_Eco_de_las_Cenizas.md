# GAME DESIGN DOCUMENT (GDD)
# Eco de las Cenizas - Modo PvPvE: Guerra de Calderas

**Género:** Multijugador PvPvE Cooperativo 3D / Guerra de Clanes / Supervivencia en Megaciudad
**Plataforma Objetivo:** PC / Consolas de última generación (Unreal Engine 5 / Unity)
**Perspectiva:** Tercera Persona (3D Action/Survival & Base Management)
**Público Objetivo:** Jugadores de supervivencia cooperativa y competitiva (ej. *Rust*, *Helldivers 2*, *Division 2 Dark Zone*, *Frostpunk*).

---

## 1. Premisa y Visión General (Modelo PvPvE Multiciudad)

En un mundo consumido por un invierno cataclísmico y la **'Niebla Helada'**, múltiples megaciudades industriales rivales (*La Caldera Alpha*, *La Caldera Beta*, etc.) compiten ferozmente por los escasos yacimientos geotérmicos del planeta.

### **Identificador de Facción (`cityID`)**
Cada jugador pertenece a una megaciudad/clan representada por una variable `cityID`. Los jugadores cooperan internamente para mantener la temperatura y defensas de su propia ciudad, mientras compiten directamente en la niebla contra jugadores de ciudades enemigas (`cityID` distinto) y contra las hordas de criaturas **Sombras Heladas**.

### **Condición de Victoria / Derrota y Saqueo**
- **Derrota Compartida por Ciudad:** Si el reactor de tu ciudad se apaga o su muralla es destruida, tu ciudad entra en estado de vulnerabilidad crítica.
- **Saqueo Enemigo (Raid/Looting):** Jugadores de ciudades enemigas pueden asaltar el almacén de tu ciudad destruida y saquear hasta un 25-37.5% de tus reservas de **Ignicita** para sumarlas a su propia caldera.

---

## 2. Core Loop PvPvE (Bucle Principal de Juego)

```
       +-------------------------------------------------------+
       |   FASE 1: Exploración y Puntos de Control Neutrales   |
       |   - Incursiones por Ignicita y combate PvPvE          |
       |   - Captura de Nodos Geotérmicos en la Niebla (`TerritoryNode`) |
       +---------------------------+---------------------------+
                                   |
                                   v
       +-------------------------------------------------------+
       |   FASE 2: Mantenimiento, Red y Saqueo (PvP Raiding)    |
       |   - Depósito en Contenedor Global (`cityID`)           |
       |   - Asalto/Saqueo de Almacenes Enemigos (`CityLootManager`) |
       |   - Diplomacia y Alianzas entre Clanes (`DiplomacyManager`) |
       +---------------------------+---------------------------+
                                   |
                                   v
       +-------------------------------------------------------+
       |   FASE 3: Defensa Tripartita y Eventos Climatológicos |
       |   - Repeler Sombras Heladas y Súper Tormentas Heladas  |
       |   - Coordinación por Chat Multicanal y Pings Tácticos |
       +-------------------------------------------------------+
```

---

## 3. Clases Interdependientes y Facciones (`cityID`)

Las 4 clases especializadas (**Ingeniero, Explorador, Científico, Táctico**) mantienen sus roles interdependientes dentro de la misma ciudad, con interacciones PvP frente a ciudades rivales:

| Clase | Función Aliada (Mismo `cityID`) | Acción Competitiva PvP (Distinto `cityID`) |
| :--- | :--- | :--- |
| **Ingeniero** | Repara murallas y reactor de su ciudad. | Coloca torretas ofensivas para asaltar el reactor enemigo. |
| **Explorador** | Extrae Ignicita y usa el Gancho de Agarre. | Realiza incursiones de reconocimiento y captura `TerritoryNodes` lejanos. |
| **Científico** | Optimiza producción de invernaderos y sueros. | Neutraliza con toxinas las cúpulas de invernaderos enemigas. |
| **Táctico** | Otorga +20% defensa en la muralla aliada. | Lidera escuadrones de asedio contra la ciudad rival. |

---

## 4. Diplomacia, Chat Multicanal y Eventos Climáticos

- **Diplomacia (`DiplomacyManager`):** Estados diplomáticos (War, Neutral, Alliance) que bloquean daño aliado.
- **Chat Multicanal (`MultiChannelChat`):** Canales filtrados por Ciudad, Global y Alianza con pings rápidos.
- **Eventos Globales (`GlobalEventManager`):** Eventos climatológicos como la **Súper Tormenta Helada** que afectan el servidor completo.

---

## 5. Guion y Narrativa Ambiental (Tutorial PvPvE)

**Personaje:** *Maelo*, el anciano operador de La Caldera Nivel 1.

> *(Maelo limpia la sangre congelada de una válvula con su guante gastado)*
>
> "Escúchame bien, novato. Ya no solo luchamos contra la Niebla Helada o contra esas malditas Sombras de hielo... Allá afuera hay otras Calderas, otras ciudades deseperadas con supervivientes que no dudarán en congelarnos con tal de robar nuestro mineral.
>
> Tu `cityID` es tu vida. Usa el **Chat de Ciudad** para coordinarte con tu clan, respeta las **Alianzas** diplomáticas y vigila los cielos cuando suene la alarma de la **Súper Tormenta Helada**. ¡Por La Caldera!"
