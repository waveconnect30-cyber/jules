# ESPECIFICACIONES TÉCNICAS
# Eco de las Cenizas - Technical Specifications

Documento de referencia para el equipo de desarrollo, arquitectura multijugador, balance numérico y especificaciones de rendimiento.

---

## 1. Arquitectura de Red y Sincronización Multijugador

| Parámetro | Especificación Técnica | Detalles de Implementación |
| :--- | :--- | :--- |
| **Arquitectura de Red** | Cliente-Servidor Deduciendo Autoridad (Dedicated Server / Host Authority) | Sincronización mediante Photon Fusion 2 / Mirror Networking sobre Unity. |
| **Tickrate de Servidor** | 30 Hz (Simulación de Estado) / 60 Hz (Movimiento de Jugador) | Interpolación y predicción del lado del cliente activadas para movimiento 3D. |
| **Sincronización de Estado Global** | `NetworkVariable` / `SyncVar` | Sincronización continua de Temperatura Central, Reserva de Ignicita y Estado de Invernaderos. |
| **Sincronización de Murallas** | Matriz de Estado de Muros (4 Secciones: N, S, E, O) | Transmisión de eventos de daño (`OnWallDamaged`) y reparación (`OnWallRepaired`) por RPCs con verificación de servidor. |
| **Capacidad por Sesión** | 4 a 16 Jugadores por Instancia | Escalado dinámico de dificultad y vida de los enemigos según el número de conexiones activas. |

---

## 2. Balance de Recursos y Parámetros del Reactor

| Variable / Recurso | Valor Inicial | Tasa de Consumo / Cambio | Condición Umbral / Impacto en Juego |
| :--- | :--- | :--- | :--- |
| **Reserva Global de Ignicita** | 100 Unidades | -1 Unidad / segundo (Reactor Activo) | Al llegar a 0, el Reactor deja de generar calor. |
| **Temperatura de la Ciudad** | +20.0 °C | -5.0 °C / minuto (Sin Ignicita)<br>+2.0 °C / unidad de Ignicita depositada | - **T > 0°C:** Operación Normal.<br>- **T ≤ -10.0°C:** Estado de Congelación Grave. |
| **Producción de Invernaderos** | 100% Eficiencia | Disminución a 0% cuando T ≤ -10.0°C | Deshabilita la generación de raciones de comida y filtros biológicos. |
| **Penalización Movimiento Jugadores**| 100% Velocidad | -20% Velocidad Global cuando T ≤ -10.0°C | Aplica debuff *Hipotermia* a todos los jugadores en la sesión. |
| **Capacidad Máx. Contenedor** | 1,000 Unidades | N/A | Exceso de Ignicita puede convertirse en impulsos térmicos temporales. |

---

## 3. Atributos y Estadísticas de Clases Interdependientes

| Clase | Rol Principal | HP Máximo | Vel. Movimiento | Habilidad Especial | Impacto en la Comunidad al Morir/Desconectarse |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Ingeniero** | Infraestructura y Mantenimiento | 120 HP | 4.5 m/s | **Soldadura Sobrecargada:** Repara +50% rápido la muralla/reactor. | Las reparaciones consumen 2x materiales y tardan +100% de tiempo. |
| **Explorador** | Extracción y Reconocimiento | 100 HP | 6.0 m/s | **Gancho de Agarre y Extractor Ligero:** Extrae Ignicita +100% rápido. | Las incursiones exteriores pierden un 50% del volumen de Ignicita traída. |
| **Científico** | Botánica y Filtros | 80 HP | 4.8 m/s | **Síntesis Catalítica:** Duplica la eficiencia energética de la Ignicita (+4°C/ud). | Invernaderos sufren -50% de rendimiento base y los filtros duran 30% menos. |
| **Táctico** | Defensa y Comando | 150 HP | 5.0 m/s | **Aura de Bastión:** Otorga +20% defensa a jugadores en la muralla. | Perímetro defensivo sin bonificaciones; torretas pierden puntería automática. |

---

## 4. Parámetros de Secciones de Muralla y Enemigos

| Sección / Entidad | HP Máximo | Resistencia a Hielo | Requisito de Reparación | Comportamiento Defensivo / Ofensivo |
| :--- | :--- | :--- | :--- | :--- |
| **Muralla Norte** | 5,000 HP | 10% | 50 Acero + 10 Ignicita | Punto de impacto de ráfagas directas de la Niebla Helada. |
| **Muralla Sur** | 4,000 HP | 15% | 40 Acero + 10 Ignicita | Sección cercana a los Invernaderos; fallo interrumpe cultivos. |
| **Muralla Este** | 4,000 HP | 15% | 40 Acero + 10 Ignicita | Entrada principal del puerto de exploración exterior. |
| **Muralla Oeste** | 4,500 HP | 20% | 45 Acero + 10 Ignicita | Flanqueada por depósitos de combustible. |
| **Sombra Helada (Sombra)** | 150 HP | Immuno a Frío | Vuln. a Fuego/Térmico (+50%) | Ataque cuerpo a cuerpo; congela estructuras impactadas. |
| **Sombra Helada (Besta Cristalia)**| 600 HP | Immuno a Frío | Vuln. a Explosivos Térmicos | Enmuro que embiste la muralla causando daño en área. |

---

## 5. Rendimiento Técnico y Metas de Plataforma

| Criterio | PC (Recomendado) | Consolas (PS5 / Xbox Series X) |
| :--- | :--- | :--- |
| **Motor de Juego** | Unreal Engine 5.3+ / Unity 2022.3 LTS | Unreal Engine 5.3+ / Unity 2022.3 LTS |
| **Resolución y Target FPS** | 1080p / 4K @ 60 FPS | Dynamic 4K @ 60 FPS (Modo Rendimiento) |
| **Límite de Polígonos (Escena)**| Max 2,500,000 Triángulos visibles | Max 2,000,000 Triángulos visibles |
| **Efectos de Niebla Volumétrica**| High / Ultra Volumetric Fog & Lighting | Medium / High Volumetric Fog |
| **Límite de Instancias VFx** | 1,000 partículas térmicas simultáneas | 800 partículas térmicas simultáneas |
