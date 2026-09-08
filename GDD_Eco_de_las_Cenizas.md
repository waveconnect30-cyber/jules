# GAME DESIGN DOCUMENT (GDD)
# Eco de las Cenizas - Modo PvPvE: Guerra de Calderas y Temporadas de 14 Días

**Género:** Multijugador PvPvE Cooperativo 3D / Guerra de Clanes / Supervivencia en Megaciudad
**Plataforma Objetivo:** PC / Consolas de última generación (Unreal Engine 5 / Unity)
**Perspectiva:** Tercera Persona (3D Action/Survival & Base Management)
**Público Objetivo:** Jugadores de supervivencia cooperativa y competitiva (ej. *Rust*, *Helldivers 2*, *Division 2 Dark Zone*, *Frostpunk*).

---

## 1. Calendario de Temporada de 14 Días (`SeasonManager`)

El juego se organiza en **Temporadas Servidor de 14 días** con reglas progresivas:

```
 Días 1 - 3              Días 4 - 8             Días 9 - 12             Días 13 - 14
+-----------------------+----------------------+-----------------------+-----------------------+
| FASE 1: SETTLEMENT    | FASE 2: EXPANSION    | FASE 3: PRESIDENTIAL  | FASE 4: OVERLOAD WIPE |
| - Inmunidad de Saqueo | - Saqueo de Almacén  |   SIEGE (0,0,0)       | - Súper Tormenta      |
| - Fortificación       | - Captura de Ruinas  | - Hold 3h = Gobernador| - Entrega Cosméticos  |
|   de la Ciudad        |   y Nodos            | - Impuesto 5% Global  | - Reinicio Servidor   |
+-----------------------+----------------------+-----------------------+-----------------------+
```

### **1.1. Inmunidad de Fase 1 (Settlement)**
Durante los primeros 3 días de servidor, la función `ExecuteCityRaid` permanece desactivada. Las ciudades no pueden saquearse mutuamente, permitiendo a los clanes construir su infraestructura básica.

### **1.2. Ciudad Presidencial y Título de Gobernador (Fase 3)**
En los días 9 a 12, se desbloquea la captura de la **Ciudad Presidencial (0,0,0)**.
- **Victoria del Gobernador:** Si un clan sostiene el control ininterrumpido durante **3 horas consecutivas**, se le proclama clan **Gobernador**.
- **Impuesto del 5%:** El clan Gobernador recibe de forma automática un **5% de impuesto pasivo** sobre toda la Ignicita procesada en el mundo.

### **1.3. Cierre y Overload Wipe (Fase 4)**
En los días 13 y 14, se activa el evento global de **Súper Tormenta Helada**. Tras finalizar, los jugadores reciben trofeos cosméticos permanentes y el servidor se reinicia para una nueva temporada.

---

## 2. Mapa Mundial, Ruinas y Atributos

- **Ciudad Presidencial Capital (0,0,0):** Centro de la región y objetivo de la Fase 3.
- **Ruinas (4 Principales + 8 Secundarias):** Otorgan multiplicadores globales de Ataque (+25%), Cosecha (+30%), Vida (+30%) y Eficiencia Térmica (+40%).
- **Esclusa de Ciudad (`CityAirlock`):** Puerta de enlace hacia la niebla exterior con temporizador de exposición.

---

## 3. Core Loop PvPvE de Temporada

1. **Semana 1:** Extracción de mineral, fortificación de la caldera e incursiones PvPvE por ruinas secundarias.
2. **Semana 2:** Asedio masivo a la Ciudad Presidencial en (0,0,0), coronación del Gobernador con impuesto del 5% y sobrevivencia al Overload Wipe final.
