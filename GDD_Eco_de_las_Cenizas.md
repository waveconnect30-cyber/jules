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
       +---------------------------+---------------------------+
                                   |
                                   v
       +-------------------------------------------------------+
       |   FASE 3: Defensa Tripartita y Asedio                 |
       |   - Repeler Sombras Heladas y Jugadores Invasores      |
       |   - Reparación de Murallas propias vs Ataques a Rivales |
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

## 4. Sistema de Nodos Territoriales Neutrales (`TerritoryNode`)

Esparcidos por la Niebla Helada existen **Nodos Geotérmicos Neutrales**.
- **Mecánica de Captura:** Mantener presencia en el radio del nodo durante 10 segundos.
- **Beneficio de Clan:** Una vez capturado por una `cityID`, genera un flujo pasivo continuo de +2.0 unidades de Ignicita/segundo directamente al almacén global de esa ciudad.

---

## 5. Guion y Narrativa Ambiental (Tutorial PvPvE)

**Personaje:** *Maelo*, el anciano operador de La Caldera Nivel 1.

> *(Maelo limpia la sangre congelada de una válvula con su guante gastado)*
>
> "Escúchame bien, novato. Ya no solo luchamos contra la Niebla Helada o contra esas malditas Sombras de hielo... Allá afuera hay otras Calderas, otras ciudades deseperadas con supervivientes que no dudarán en congelarnos con tal de robar nuestro mineral.
>
> Tu `cityID` es tu vida. Si ves a alguien con una marca de caldera distinta en el casco, no es tu amigo: viene a por nuestra **Ignicita**. Si dejan caer nuestra muralla, saquearán nuestros almacenes hasta dejarnos en cero.
>
> Mantén caliente nuestro reactor, defiende nuestro muro y si ves un **Nodo Geotérmico** en la niebla, tómalo para nuestro clan antes de que ellos lo hagan. ¡Por La Caldera!"
