# GAME DESIGN DOCUMENT (GDD)
# Eco de las Cenizas

**Género:** Multijugador Cooperativo 3D / Supervivencia en Megaciudad / Gestión Social y Defensa
**Plataforma Objetivo:** PC / Consolas de última generación (Unreal Engine 5 / Unity)
**Perspectiva:** Tercera Persona (3D Action/Survival & Base Management)
**Público Objetivo:** Jugadores de supervivencia cooperativa (ej. *Helldivers 2*, *Deep Rock Galactic*, *Frostpunk*, *Don't Starve Together*).

---

## 1. Premisa y Visión General

En un mundo consumido por un invierno cataclísmico y una niebla azul tóxica conocida como la **'Niebla Helada'**, el último reducto de la humanidad habita dentro de **'La Caldera'**: una impresionante megaciudad vertical de arquitectura industrial *dieselpunk* construida alrededor de un reactor geotérmico monumental.

### **Condición de Victoria / Derrota Compartida**
Todos los jugadores comparten el mismo espacio físico y social en la ciudad. **No hay ganadores individuales.** Si el reactor geotérmico se apaga por falta de mineral **Ignicita**, si las murallas defensivas son brechadas por las criaturas de la niebla (**Sombras Heladas**), o si los invernaderos colapsan causando hambruna, **la ciudad entera cae y todos los jugadores pierden la partida simultáneamente.**

---

## 2. Core Loop (Bucle Principal de Juego de 3 Fases)

El bucle de juego está estructurado en tres fases continuas e interconectadas en tiempo real que obligan a los jugadores a dividir sus fuerzas y coordinarse.

```
       +-------------------------------------------------------+
       |   FASE 1: Exploración 3D Exterior (Niebla Helada)    |
       |   - Incursiones por Ignicita y cristales térmicos     |
       |   - Combate con Sombras Heladas y retorno a la ciudad  |
       +---------------------------+---------------------------+
                                   |
                                   v
       +-------------------------------------------------------+
       |   FASE 2: Gestión del Reactor e Invernaderos          |
       |   - Depósito de Ignicita en el Contenedor Global      |
       |   - Mantenimiento térmico y producción agrícola       |
       +---------------------------+---------------------------+
                                   |
                                   v
       +-------------------------------------------------------+
       |   FASE 3: Defensa de Murallas en Tiempo Real          |
       |   - Repeler oleadas de Sombras Heladas                 |
       |   - Reparación de murallas y gestión de torretas      |
       +-------------------------------------------------------+
```

### **Fase 1: Exploración 3D Exterior (Incursiones por 'Ignicita')**
- **Mecánica:** Los jugadores salen del perímetro seguro de La Caldera hacia la Niebla Helada.
- **Objetivos:** Localizar y extraer vetas de **Ignicita** (mineral geotérmico altamente reactivo), recuperar componentes mecánicos de ruinas exteriores y cartografiar rutas seguras.
- **Peligros:** Exposición al frío extremo (barra de congelación), toxicidad de la niebla (requiere filtros de gas) y ataques de criaturas *Sombras Heladas*.

### **Fase 2: Gestión del Reactor y Cúpulas de Invernaderos**
- **Mecánica:** En el núcleo urbano, los jugadores deben procesar y depositar los recursos extraídos.
- **Objetivos:**
  - **Reactor Central:** Alimentar el horno geotérmico con Ignicita para mantener la temperatura por encima de los -10°C óptimos.
  - **Invernaderos Hydro-Térmicos:** Mantener la temperatura de las cúpulas para producir biomasa/raciones. Si la temperatura cae por debajo de -10°C, la producción agrícola se deshabilita automáticamente.
  - **Eficiencia Energética:** Redirigir energía entre niveles habitacionales, defensas y purificadores de aire.

### **Fase 3: Defensa de Murallas en Tiempo Real**
- **Mecánica:** La Niebla Helada se densifica periódicamente traendo consigo hordas de *Sombras Heladas*.
- **Objetivos:**
  - Defender las 4 secciones principales de la muralla (Norte, Sur, Este, Oeste).
  - Operar torretas térmicas, sellar brechas estructurales y repeler enemigos voladores e hiper-cristalizados.
  - Mantener la integridad de los generadores de barrera térmica instalados en las murallas.

---

## 3. Clases Interdependientes y Matriz de Cascada de Fallas

El sistema de juego prohíbe el éxito de jugadores autosuficientes. Se requieren 4 clases especializadas que dependen estrictamente unas de otras:

### **Descripción de las 4 Clases**
1. **Ingeniero:** Especialista en mantenimiento del reactor, fortificación de murallas y reparación de maquinaria industrial. Posee herramientas de soldadura térmica y torretas desplegables.
2. **Explorador:** Rápido, equipado con traje con aislamiento reforzado, visores térmicos y gancho de agarre. Es el único capaz de extraer Ignicita a alta velocidad y transportar cargas pesadas en la niebla.
3. **Científico:** Encargado de la botánica en los invernaderos, síntesis de filtros para gas, elaboración de sueros térmicos y optimización biológica de la Ignicita en el reactor.
4. **Táctico:** Lidera la defensa de las murallas, opera radares de niebla para detectar oleadas, marca objetivos prioritarios y proporciona bonificaciones de moral y resistencia a los aliados.

---

### **Matriz de Cascada de Fallas (Efecto Dominó)**

Si una sola clase falla en sus responsabilidades, se desencadena un colapso sistémico en cadena que afecta inmediatamente a todas las demás clases:

| Clase que Falla | Causa de la Falla | Impacto Inmediato | Cascada de Efecto Dominó en Otras Clases |
| :--- | :--- | :--- | :--- |
| **Explorador** | No regresa con suficientes núcleos de Ignicita. | El Reactor agota su combustible. Temperatura cae a -5°C/min. | - **Científico:** Se congelan las cúpulas de los invernaderos (producción reducida al 0%).<br>- **Ingeniero:** El reactor entra en choque térmico y las tuberías sufren rupturas.<br>- **Táctico:** Los jugadores sufren una penalización del -20% en velocidad de movimiento al defender las murallas debido al congelamiento. |
| **Ingeniero** | Descuida la reparación de tuberías del reactor o las murallas. | Pérdida masiva de eficiencia térmica y brechas mecánicas en los muros. | - **Explorador:** Las compuertas de salida a la niebla se bloquean o se congelan sin energía.<br>- **Científico:** Fugas de gas tóxico penetran los laboratorios e invernaderos.<br>- **Táctico:** Las torretas automáticas de las murallas quedan fuera de servicio por corte eléctrico. |
| **Científico** | Falla en producir filtros de respiración y raciones agrícolas. | Hambruna general y envenenamiento por niebla azul en la población. | - **Explorador:** No puede salir a la niebla sin filtros funcionales (daño constante por toxinas).<br>- **Ingeniero:** Sufre debilidad física por falta de raciones (reparaciones 50% más lentas).<br>- **Táctico:** Reducción severa de la barra de estamina y vida máxima durante el combate defensivo. |
| **Táctico** | Permite que las Sombras Heladas brechen una sección de la muralla. | Hordas de Sombras Heladas invaden los niveles inferiores de La Caldera. | - **Ingeniero:** Invasión directa en la sala de máquinas del reactor; imposible reparar bajo ataque.<br>- **Científico:** Invernaderos destruidos por cristalización de hielo Sombrío.<br>- **Explorador:** Cortadas las rutas de retorno seguro a los contenedores de depósito. |

---

## 4. Sistema de Votación del 'Concejo' (Distribución de Recursos Escasos)

Durante momentos clave de la partida o entre oleadas de la niebla, se activa la interfaz del **Concejo de La Caldera**. Este sistema obliga a los jugadores a votar democráticamente sobre la asignación de recursos limitados comunitarios.

### **Mecánica de Votación:**
1. **Fase de Propuesta:** El sistema presenta dilemas de supervivencia crítica con presupuesto limitado (ej. *Energía disponible: 100 Unidades Térmicas*).
2. **Papeleta de Opciones:**
   - **Opción A (Énfasis en Producción):** Asignar 70% de energía a Invernaderos (Evita hambruna, pero deja las Murallas Norte y Este con torretas apagadas).
   - **Opción B (Énfasis en Defensa):** Asignar 70% de energía a los Escudos Térmicos de la Muralla (Detiene invasión de Sombras Heladas, pero congela el Nivel 3 residencial).
   - **Opción C (Énfasis en Sobrealimentar Reactor):** Convertir la Ignicita en un pulso de calor masivo (Aumenta la temperatura +15°C por 10 min, pero consume el 80% de la reserva global).
3. **Ponderación de Votos:** Cada clase tiene mayor peso de voto según el área en discusión (ej. El Ingeniero tiene un voto de valor x2 en decisiones de Infraestructura/Reactor; el Científico x2 en Alimento/Salubridad).
4. **Consecuencias Sociales:** Votar repetidamente en contra de las necesidades de un sector habitacional provoca disturbios o penalizaciones de eficiencia en ese distrito.

---

## 5. Especificaciones para Generación de Arte e IA Prompts

### **5.1. Concept Art General del Juego**
> **Prompt:** "Concept art for a 3D cooperative survival video game, Unreal Engine 5 render. A massive vertical dieselpunk city named 'La Caldera' surrounded by an icy toxic blue fog. At the center, a gigantic geothermal reactor glowing with intense orange heat. Industrial steel structures, layered tier levels, greenhouse domes, high-contrast volumetric lighting, apocalyptic survival mood, hyper-detailed --ar 16:9"

### **5.2. Personaje 3D (Asset para Modelado - Clase Explorador)**
> **Prompt:** "3D video game character design, T-pose, full body. A rugged 'Explorer' class survivor wearing an insulated Arctic suit, heavy gas mask with orange glowing visor, grappling hook on belt, and a rusted backpack for gathering crystals. Low-poly stylized 3D model, clean topology for rigging, PBR materials --ar 1:1"

### **5.3. Enemigo 3D (Asset de Criatura - Sombra Helada)**
> **Prompt:** "3D monster game asset, low-poly creature made of sharp crystalline ice and dark shadowy smoke, glowing blue core, aggressive stance, stylized dark fantasy, Unreal Engine material showcase --ar 1:1"

---

## 6. Guion y Narrativa Ambiental (Tutorial Introductorio)

**Personaje:** *Maelo*, el anciano operador principal del Reactor Central (PNJ con voz rasposa, cansada pero firme).
**Escenario:** El jugador despierta en la plataforma del Reactor Nivel 1. El metal cruje por el frío exterior y a través de los ventanales se aprecia la Niebla Helada golpeando las cristaleras.

### **Monólogo en Primera Persona del Tutorial:**

> *(Sonido de engranajes pesados y aire a presión liberándose. Maelo escupe a un lado y ajusta una válvula ardiente con una llave inglesa oxidada antes de mirarte).*
>
> "Abre los ojos, novato... Bienvenido a **La Caldera**. Si esperabas un recibimiento caluroso, te equivocaste de siglo. Lo único caluroso aquí es este viejo reactor sobre el que estamos parados, y como puedes oír por sus tirones, se está muriendo de hambre.
>
> Escúchame bien y grábate esto en el casco: allá afuera, en la Niebla Helada, los lobos solitarios no duran ni cinco minutos. Si intentas jugar al héroe egoísta, si guardas la Ignicita para ti solo o te olvidas de reparar la muralla de tus compañeros... el frío nos matará a todos por igual. Aquí no hay ganadores individuales. O salvamos La Caldera juntos, o esta tumba de metal será nuestro congelador comunitario.
>
> Mira ese termómetro en el muro central. ¿Ves cómo la aguja vibra cerca del cero? Sin **Ignicita**, el calor cae cinco grados cada minuto. Si baja de menos diez, las cosechas de las cúpulas morirán congeladas y tus propios pulmones se volverán rígidos como el cristal.
>
> Así que basta de contemplaciones. Ajusta tu máscara de gas, coge tu pico térmico y cruza la compuerta del Nivel 2. Tienes tu primera misión: entra en la niebla, localiza un núcleo de Ignicita pura y tráelo de vuelta antes de que el sector residencial se vuelva un bloque de hielo. ¡Muévete, que la niebla no espera a nadie!"
