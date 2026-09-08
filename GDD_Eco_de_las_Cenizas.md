# GAME DESIGN DOCUMENT (GDD)
# Eco de las Cenizas - Modo PvPvE Cross-Play (PC y Android)

**Género:** Multijugador PvPvE Cooperativo 3D / Guerra de Clanes / Supervivencia Cross-Play
**Plataforma Objetivo:** PC Windows / Android / iOS / Consolas de última generación
**Perspectiva:** Tercera Persona (3D Action/Survival & Base Management)
**Público Objetivo:** Jugadores de supervivencia cooperativa y competitiva (ej. *Rust*, *Helldivers 2*, *Division 2 Dark Zone*, *Frostpunk*).

---

## 1. Experiencia Multiplataforma Cross-Play (PC & Android)

**Eco de las Cenizas** está diseñado desde su arquitectura base para ser **100% Cross-Play entre PC y dispositivos móviles (Android/iOS)**.

### **1.1. HUD Táctil Móvil y Controles Adaptativos**
- **Soporte Móvil (`TouchScreenHUD`):** Joystick virtual dinámico para movimiento 3D y botones táctiles dedicados para salto, interacción y habilidades.
- **Entrada Adaptativa (`PlayerController`):** El personaje detecta y responde en tiempo real tanto a combinación de Teclado/Mouse como a toques en pantalla táctil.
- **Optimizador de Calidad (`QualitySettingsManager`):** En Android reduce automáticamente las sombras y la distancia de renderizado a 150m para garantizar 30–60 FPS estables durante sesiones Cross-Play.

---

## 2. Calendario de Temporada de 14 Días y Guerra de Calderas

```
 Días 1 - 3              Días 4 - 8             Días 9 - 12             Días 13 - 14
+-----------------------+----------------------+-----------------------+-----------------------+
| FASE 1: SETTLEMENT    | FASE 2: EXPANSION    | FASE 3: PRESIDENTIAL  | FASE 4: OVERLOAD WIPE |
| - Inmunidad de Saqueo | - Saqueo de Almacén  |   SIEGE (0,0,0)       | - Súper Tormenta      |
| - Fortificación       | - Captura de Ruinas  | - Hold 3h = Gobernador| - Entrega Cosméticos  |
|   de la Ciudad        |   y Nodos            | - Impuesto 5% Global  | - Reinicio Servidor   |
+-----------------------+----------------------+-----------------------+-----------------------+
```

---

## 3. Mapa Mundial, Ruinas y Atributos

- **Ciudad Presidencial Capital (0,0,0):** Centro de la región y objetivo de la Fase 3.
- **Ruinas (4 Principales + 8 Secundarias):** Otorgan multiplicadores globales de Ataque (+25%), Cosecha (+30%), Vida (+30%) y Eficiencia Térmica (+40%).
- **Esclusa de Ciudad (`CityAirlock`):** Puerta de enlace hacia la niebla exterior con temporizador de exposición por jugador.
