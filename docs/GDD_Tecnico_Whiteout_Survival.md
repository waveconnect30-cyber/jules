# 📖 GDD TÉCNICO COMPLETO: WHITEOUT SURVIVAL (GÉNESIS A REVELACIÓN)
## Biblia de Desarrollo Técnico, Lógica de Servidor/Cliente, Fórmulas y Arquitectura de Datos

---

## CAPÍTULO 1: GÉNESIS — El Núcleo de Simulación y Economía Base

El bucle inicial (Game Loop) se basa en la supervivencia de agentes (Pobladores/Supervivientes) condicionados por el entorno ambiental helado y la temperatura de la caldera.

```
 [ Clima / Ventisca ] ---> [ Temperatura Ambiental ] ---> [ Consumo de Carbón (Horno) ]
                                   |
                                   v
 [ Comida / Camas ] -------> [ Salud & Felicidad ] -------> [ Estado Agente ]
                                   |
                                   +--> IDLE / WORKING / SICK / DEAD
```

### 1.1. Sistema de Agentes (Supervivientes) y Necesidades

#### Atributos del Agente
- **Health (Salud):** $[0 - 100]$
- **Happiness (Felicidad):** $[0 - 100]$
- **State (Estado de Agente):** `[IDLE, WORKING, EATING, SLEEPING, SICK, DEAD]`

#### Consumo Diario / Horario
- Cada habitante consume $X$ unidades de **Comida/hora** de la Cocina/Comedor y requiere una **Cama** asignada en el Refugio.

#### Mecánica de Enfermedad
$$\text{Riesgo de Enfermedad} = f(\text{Temperatura}, \text{Comida Disponible}, \text{Camas Disponibles})$$

$$\text{Riesgo de Enfermedad} = \alpha \cdot \max(0, T_{\text{confort}} - T_{\text{actual}}) + \beta \cdot \left(1 - \frac{\text{Comida}_{\text{disp}}}{\text{Comida}_{\text{req}}}\right) + \gamma \cdot \left(1 - \frac{\text{Camas}_{\text{disp}}}{\text{Población}}\right)$$

#### Lógica de Ticks y Muerte
- Si $\text{Temperatura} < \text{Umbral Crítico}$ (ej. $-10^\circ\text{C}$), el atributo `Health` disminuye progresivamente por tick de simulación.
- Cuando `Health` llega a $0$, el agente pasa al estado `DEAD`. Se detiene inmediatamente la producción asociada a su puesto de trabajo asignado.

---

### 1.2. El Bucle Térmico (El Horno / Furnace)

#### Variable Global de Entorno
- **Ambient_Temperature:** Sigue un ciclo diurno/nocturno dinámico suplementado por eventos climáticos de ventisca programados en el servidor.

#### Especificación del Horno (Furnace)
- **Furnace_Level:** Controla el radio de área caliente (`Heat_Radius`), desbloquea nuevas tecnologías y establece el nivel límite máximo de los demás edificios de la base.
- **Max_Mode (Booleano):** Multiplica por $2\times$ la salida de calor a cambio de duplicar el consumo de Carbón (`Coal_Rate`).

#### Condición de Victoria / Derrota en Base
$$\text{Net\_Heat} = \text{Furnace\_Heat\_Output} - \text{Ambient\_Cold\_Loss}$$
- **Condición de Estabilidad:** $\text{Net\_Heat} \ge \text{Heat\_Required}$.
- Si $\text{Net\_Heat} < \text{Heat\_Required}$, se activa la caída de temperatura interna del refugio, acelerando el riesgo de enfermedad e hipotermia masiva.

---

### 1.3. Árbol de Recursos Primarios

#### Recursos Básicos
- **Madera (Wood):** Requerida para construcción básica y combustible inicial.
- **Carne / Comida (Meat / Food):** Mantiene la salud de los agentes en la Cocina.
- **Carbón (Coal):** Recurso térmico principal para alimentar el Horno.
- **Hierro (Iron):** Utilizado en estructuras avanzadas, armamento e investigaciones.

#### Recursos Especiales / Endgame
- **Cristal de Fuego (Fire Crystal / FC):** Recurso de nivel alto para evolucionar edificios por encima de Furnace Lvl 30.
- **Gemas:** Divisa premium/recompensa de eventos para aceleraciones y gacha.
- **Engranajes y Materiales de Jefes:** Utilizados en el taller de equipamiento de héroes y tecnología de alianza.

---

## CAPÍTULO 2: ÉXODO — El Mapa Global y Marchas (World Map)

Una vez que el Horno alcanza el nivel 7, se habilita la vista del Mapa del Mundo (Overworld).

```
 [ Base Jugador ] ------ (Marcha: Pathfinding / Lerp) ------> [ Nodo / Enemigo ]
        |                                                            |
        <--- (Reporte de Batalla + Carga de Recursos) ----------------+
```

### 2.1. Sistema de Coordenadas y Grid Espacial

- **Estructura del Mapa:** Cuadrícula $2D$ de $(X, Y)$ coordenadas (ej. $1200 \times 1200$ casillas).
- **Entidades en Mapa:**
  - Banderas y Territorio de Alianza (Alliance Banners / HQ).
  - Nodos de Recolección (Tile Nodes L1 a L8: Madera, Carbón, Hierro, Comida).
  - Bestias / Monstruos (Beasts / Beast Camps).
  - Bases de Jugadores (Cities).
  - Castillo Presidencial Capital en $(600, 600)$ / $(0, 0)$ y Fuertes Periféricos.

---

### 2.2. Sistema de Marchas (Troop Marches)

#### Propiedades de Marcha
- `Origin_Coord`: $(X_0, Y_0)$
- `Target_Coord`: $(X_1, Y_1)$
- `Troop_Composition`: Matriz de tropas por clase y tier `[Infantry_T1...TN, Lancers_T1...TN, Marksmen_T1...TN]`
- `Start_Time`: Timestamp Unix de inicio
- `Arrival_Time`: Timestamp Unix de llegada
- `State`: `[MARCHING, GATHERING, RETURNING, COMBAT]`

#### Cálculo de Tiempo de Viaje
$$\text{Distancia} = \sqrt{(X_1 - X_0)^2 + (Y_1 - Y_0)^2}$$

$$\text{Travel\_Time} = \frac{\text{Distancia}}{\text{Base\_Speed} \times (1 + \sum \text{Speed\_Buffs})}$$

---

### 2.3. Asincronía y Determinismo en Servidor

Las marchas y combates en el mapa no corren en tiempo real continuo mediante físicas en el cliente. Se procesan de forma determinista en el servidor mediante eventos diferidos programados en base a **Timestamps UNIX**.

1. El cliente envía la petición de marcha con `Origin`, `Target` y `Troops`.
2. El servidor valida disponibilidad de tropas, calcula `Arrival_Time` y guarda el registro en la base de datos.
3. El cliente efectúa una interpolación visual suave (Visual Lerp) de la tropa sobre la cuadrícula.
4. Al cumplirse `Timestamp >= Arrival_Time`, el servidor ejecuta el cálculo del evento (combate/recolección) e inmuta el estado en la base de datos.

---

## CAPÍTULO 3: LEVÍTICO — Tropa, Hospitales y Triángulo de Combate

### 3.1. Clases de Tropas y Roles

El combate terrestre se basa en la sinergia posicional de tres tipos de tropas:

| Tipo de Tropa | Posición en Línea | Target Prioritario | Rol Principal |
| :--- | :--- | :--- | :--- |
| **Infantería (Infantry)** | Línea Frontal | Infantería Enemiga | Absorción de daño (Tanque principal) |
| **Lanzadores (Lancers)** | Línea Trasera | Flanqueo / Daño a distancia | DPS Rápido y perforante |
| **Tiradores (Marksmen)** | Línea Trasera | Línea Frontal / Daño Sostenido | DPS Constante a distancia |

---

### 3.2. Mecánica de Bajas, Hospitales y Flujo de Bajas

Cuando una base o tropa entra en combate, las pérdidas se clasifican en tres estados según la gravedad:

```
                  [ Daño Recibido en Combate ]
                               |
            +------------------+------------------+
            |                                     |
    [ Heridos Leves ]                     [ Heridos Graves ]
  (Auto-curación al volver)                       |
                                                  v
                                       [ Hospital / Infirmary ]
                                                  |
                                    +-------------+-------------+
                                    | (Si capacidad satisfecha) | (Si desborda)
                                    v                           v
                           [ Curación con Raciones ]    [ Oficina de Alistamiento ]
                                                                |
                                                +---------------+---------------+
                                                | (Si supera límite de oficina) |
                                                v                               v
                                     [ Tropas Recuperables ]           [ MUERTOS ]
```

1. **Heridos Leves:** Se recuperan automáticamente al regresar la marcha a la base.
2. **Heridos Graves:** Van al Hospital (`Infirmary`). Requieren recursos y tiempo de curación.
3. **Desborde (Enlistment Office / Oficina de Alistamiento):** Si el Hospital excede su capacidad máxima (`Infirmary_Capacity`), un porcentaje de las tropas sobrantes ingresa a la Oficina de Alistamiento para ser recuperadas lentamente con el tiempo.
4. **Muertos Definitivos:** Tropas destruidas irreversiblemente si la Oficina de Alistamiento se colapsa por completo durante grandes guerras o ataques directos a ciudades.

---

## CAPÍTULO 4: NÚMEROS — Sistema de Héroes y Combate 2.5D (Arena/Exploración)

### 4.1. Máquina de Estados Finitos (FSM) de Unidades en Batalla

En combates de exploración/arena, las unidades operan mediante una FSM táctica en rejilla:

```
      +--------+
      |  IDLE  |
      +---+----+
          |
          v
      +--------+     Rango de Ataque == False
      |  MOVE  | --------------------------------+
      +---+----+                                 |
          | Rango de Ataque == True              |
          v                                      |
     +----+-----+                                |
     |  ATTACK  | <------------------------------+
     +----+-----+
          | Energy == 100
          v
     +----+-----+
     |  SKILL   |
     +----+-----+
```

- **IDLE:** Estado inicial de búsqueda de objetivo prioritario.
- **MOVE:** Avance hacia el objetivo hasta que `Distancia(Unidad, Target) <= Rango_Ataque`.
- **ATTACK:** Ejecución de ataques básicos que otorgan acumulación de energía (`Energy += Delta_Energy`).
- **SKILL:** Al alcanzar `Energy == 100`, se interrumpe el ciclo normal para lanzar la habilidad definitiva del héroe, reiniciando la energía a $0$.

---

### 4.2. Fórmula Simplificada de Daño

$$\text{Damage} = \left( \text{Attacker\_ATK} \times \frac{100}{100 + \text{Defender\_DEF}} \right) \times \text{Skill\_Multiplier} \times \text{Crit\_Factor}$$

Donde:
- $\text{Crit\_Factor} = 1.5 \text{ o } 2.0$ si $\text{Random}(0, 1) \le \text{Crit\_Chance}$, de lo contrario $1.0$.
- $\text{Skill\_Multiplier} = 1.0$ para ataques normales, o valor de escala (ej. $2.5$) durante la ejecución de `SKILL`.

---

### 4.3. Generaciones de Héroes (Gacha System)

Para mantener el balance y el ciclo de retención monetizable, los héroes se clasifican por Generaciones:
- **Gen_1:** Héroes iniciales del servidor (Solis, Sergey, Molly).
- **Gen_2 a Gen_N:** Desbloqueados progresivamente según la edad del servidor (`Server_Age_Days`).
- **Mecánica Gacha:** Sistema de invocación con garantías (Pity System) y fragmentos de héroe para evolución de estrellas y habilidades.

---

## CAPÍTULO 5: DEUTERONOMIO — Alianzas, Eventos y Meta-Juego

### 5.1. Sistema de Alianzas (Clans / Alliances)

- **Territorio de Alianza:** Definido por el Cuartel General (HQ) y Banderas (`Banners`). Otorga bonificaciones pasivas de producción de recursos y protección de escudo territorial.
- **Rallies (Ataques Conjuntos):**
  1. Un jugador abre una ventana de convocatoria (ej. 5 min).
  2. Los aliados envían tropas a la base del convocante.
  3. Al expirar el temporizador, se forma una super-marcha unificada con la suma de las tropas y los buffs del convocante para atacar objetivos de alto nivel o fortalezas enemigas.

---

### 5.2. Motor de Eventos ("The Event Loop")

El meta-juego late a través de ciclos rotativos sostenidos por consumibles acumulables:

```
+-----------------------------------------------------------------------+
| FASE 1: PREPARACIÓN                                                   |
| Consumo de Aceleradores, Subida de Poder, Reclutamiento de Tropas.   |
+-----------------------------------------------------------------------+
                                   |
                                   v
+-----------------------------------------------------------------------+
| FASE 2: EJECUCIÓN Y COMBATE                                           |
| Batalla de Castillos, SvS (State vs State), Incursiones PvP.          |
+-----------------------------------------------------------------------+
                                   |
                                   v
+-----------------------------------------------------------------------+
| FASE 3: RECOMPENSA Y ENFRIAMIENTO                                    |
| Distribución de Cofres de Gobernador, Títulos y Cooldown Térmico.     |
+-----------------------------------------------------------------------+
```

---

## CAPÍTULO 6: REVELACIÓN — Arquitectura de Datos y Backend (Esquema PostgreSQL / Supabase)

El modelo relacional completo para el motor de juego requiere las siguientes tablas optimizadas con claves foráneas e índices de rendimiento:

### 6.1. Definición de Tablas SQL Principal

```sql
-- TABLA: Facciones / Alianzas
CREATE TABLE alliances (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name TEXT NOT NULL UNIQUE,
    tag VARCHAR(4) NOT NULL UNIQUE,
    leader_id UUID,
    hq_x INT,
    hq_y INT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- TABLA: Jugadores
CREATE TABLE players (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username TEXT NOT NULL,
    furnace_level INT DEFAULT 1,
    fire_crystals INT DEFAULT 0,
    alliance_id UUID REFERENCES alliances(id) ON DELETE SET NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Vincular clave foránea de líder de alianza
ALTER TABLE alliances ADD CONSTRAINT fk_alliance_leader FOREIGN KEY (leader_id) REFERENCES players(id);

-- TABLA: Edificios y Timers del Jugador
CREATE TABLE player_buildings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id UUID NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    building_type TEXT NOT NULL, -- 'furnace', 'sawmill', 'infirmary', 'kitchen', 'barracks'
    level INT DEFAULT 1,
    upgrade_ends_at TIMESTAMP WITH TIME ZONE, -- NULL si no está en construcción
    CONSTRAINT unique_player_building UNIQUE (player_id, building_type)
);

-- TABLA: Tropas del Jugador
CREATE TABLE player_troops (
    player_id UUID NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    troop_type TEXT NOT NULL, -- 'infantry', 'lancer', 'marksman'
    tier INT NOT NULL, -- 1, 2, 3...
    amount INT DEFAULT 0,
    wounded_amount INT DEFAULT 0,
    PRIMARY KEY (player_id, troop_type, tier)
);

-- TABLA: Héroes del Jugador
CREATE TABLE player_heroes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id UUID NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    hero_code TEXT NOT NULL, -- 'solis', 'sergey', 'molly'
    level INT DEFAULT 1,
    stars INT DEFAULT 1,
    skill_level INT DEFAULT 1,
    CONSTRAINT unique_player_hero UNIQUE (player_id, hero_code)
);

-- TABLA: Marchas del Mapa Mundial
CREATE TABLE marches (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id UUID NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    alliance_id UUID REFERENCES alliances(id),
    origin_x INT NOT NULL,
    origin_y INT NOT NULL,
    target_x INT NOT NULL,
    target_y INT NOT NULL,
    start_time TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    arrival_time TIMESTAMP WITH TIME ZONE NOT NULL,
    state TEXT NOT NULL, -- 'MARCHING', 'GATHERING', 'RETURNING', 'COMBAT'
    troop_data JSONB NOT NULL -- Estructura JSONB con tropas enviadas
);

-- TABLA: Eventos Globales de Servidor
CREATE TABLE server_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    event_type TEXT NOT NULL, -- 'CASTLE_BATTLE', 'SVS_PREPARATION', 'BLIZZARD'
    start_time TIMESTAMP WITH TIME ZONE NOT NULL,
    end_time TIMESTAMP WITH TIME ZONE NOT NULL,
    phase TEXT NOT NULL DEFAULT 'PREPARATION'
);
```

---

## 7. RESUMEN DE INTEGRACIÓN Y COMPATIBILIDAD

Este documento técnico consolida la totalidad de mecánicas, modelos matemáticos, arquitecturas de servidor autoritativo y esquemas relacionales necesarios para simular y clonar el bucle de juego de Whiteout Survival en plataformas móviles y PC.
