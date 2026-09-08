# GAME DESIGN DOCUMENT (GDD)
# Eco de las Cenizas - Modo PvPvE Cross-Play (PC y Android)

**Género:** Multijugador PvPvE Cooperativo 3D / Guerra de Clanes / Supervivencia Cross-Play
**Plataforma Objetivo:** PC Windows / Android (.apk / .aab) / iOS / Consolas de última generación
**Perspectiva:** Tercera Persona (3D Action/Survival & Base Management)
**Público Objetivo:** Jugadores de supervivencia cooperativa y competitiva (ej. *Rust*, *Helldivers 2*, *Division 2 Dark Zone*, *Frostpunk*).

---

## 1. Experiencia Multiplataforma y Red Servidor-Autoritativa

**Eco de las Cenizas** utiliza una arquitectura cliente-servidor autoritativa con seguridad reforzada y soporte para Servidor Dedicado:

### **1.1. Autoridad de Red, Sincronización y Muerte/Respawn**
- **Controlador Local (`PlayerController`):** Lectura de entradas de teclado/mouse y joystick táctil protegidas con comprobaciones `isLocalPlayer` / `hasAuthority`.
- **Sincronización de Vida y Respawn:** Variable `currentHP` sincronizada vía `[SyncVar]`. Al llegar a 0 HP, la muerte se ejecuta en servidor, deshabilita temporales de entrada y reaparece al jugador en el punto de spawn de su `cityID`.
- **Sincronización del Reactor (`ReactorManager`):** El procesamiento de temperatura, consumo de Ignicita y penalizaciones se ejecutan exclusivamente en el Servidor (`[Server]`) y se sincronizan hacia los clientes vía `[SyncVar]`.
- **Seguridad en Chat Multicanal (`MultiChannelChat`):** Los mensajes de canal de Ciudad y Alianza son validados mediante `NetworkConnectionToClient` y distribuidos por `TargetRpc` a clientes autorizados.
- **Registro Único de Votos del Concejo (`CouncilVotingManager`):** Cada votación registra la conexión única de jugador (`netId`), rechazando llamadas de voto duplicadas en la misma sesión.
- **Soporte Servidor Dedicado (`Dedicated Server`):** Las brechas de muralla y cambios de fase se procesan en el Servidor sin depender de llamadas visuales en clientes.

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

## 3. Mapa Mundial, Ruinas e Interacciones Concretas

- **Ciudad Presidencial Capital (0,0,0):** Centro de la región y objetivo de la Fase 3.
- **Ruinas (4 Principales + 8 Secundarias):** Otorgan multiplicadores globales de Ataque (+25%), Cosecha (+30%), Vida (+30%) y Eficiencia Térmica (+40%).
- **Esclusa de Ciudad (`CityAirlock`):** Puerta de enlace hacia la niebla exterior con daño HP acumulativo por exposición por jugador.
- **Interacciones Concretas (`IInteractable`):** Nodos de recolección de Ignicita, contenedores de depósito al reactor y paneles de reparación de murallas.
