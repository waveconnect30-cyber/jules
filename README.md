# Eco de las Cenizas - Multijugador Cooperativo 3D

¡Bienvenido al repositorio de **Eco de las Cenizas**, un videojuego multijugador cooperativo 3D de supervivencia en una megaciudad vertical *dieselpunk* llamada **'La Caldera'**.

---

## 📄 Documentación del Proyecto

- 📖 **Game Design Document (GDD):** [`GDD_Eco_de_las_Cenizas.md`](./GDD_Eco_de_las_Cenizas.md)
- 📊 **Especificaciones Técnicas:** [`docs/Technical_Specifications.md`](./docs/Technical_Specifications.md)

---

## 🚀 Guía Paso a Paso para Configurar y Probar la Escena Multijugador en Unity

Sigue estas instrucciones paso a paso para configurar el prototipo funcional en Unity:

### 1. Requisitos Previos
- **Unity 2022.3 LTS** (o superior) con soporte para Build de PC / Mac.
- **Paquete Mirror Networking:** Disponible gratuitamente en la Unity Asset Store o [GitHub de Mirror](https://github.com/vis2k/Mirror).
- **TextMeshPro:** Paquete incluido por defecto en Unity.

### 2. Importación e Instalación
1. Abre Unity Hub y crea un nuevo proyecto en **3D (URP o Built-in Render Pipeline)**.
2. Descarga e importa el paquete **Mirror** vía *Window -> Package Manager* o importando el archivo `.unitypackage`.
3. Copia el contenido de la carpeta `scripts/` de este repositorio en `Assets/Scripts/EcoDeLasCenizas/`.

### 3. Configuración de la Escena Principal (`LaCaldera_Scene.unity`)

1. **Crear el NetworkManager:**
   - En la jerarquía de la escena, crea un GameObject vacío llamado `NetworkManager`.
   - Añade los componentes `NetworkLobbyManager` y `NetworkManagerHUD`.

2. **Crear el GameManager y Sistemas Core:**
   - Crea un GameObject llamado `GameManager` y añádele el script `GameManager.cs`.
   - Crea un GameObject llamado `ReactorSystem` y añádele `ReactorManager.cs`.
   - Crea un GameObject llamado `WallHealthSystem` y añádele `CityWallHealthSync.cs`.
   - Crea un GameObject llamado `CouncilSystem` y añádele `CouncilVotingManager.cs`.
   - Crea un GameObject llamado `WarehouseSystem` y añádele `SharedInventorySync.cs`.

3. **Configurar la Interfaz de Usuario (UI/HUD):**
   - Crea un `Canvas` (UI -> Canvas) y asegúrate de importar TextMeshPro si te lo solicita.
   - Adjunta el script `ReactorHUDUI.cs` a tu Canvas principal y vincula los textos/sliders correspondientes.
   - Adjunta el script `ScreenFrostPostProcessUI.cs` a una imagen overlay para la alerta congelada (-10°C).
   - Adjunta `ClassSelectionUI.cs` a la pantalla de selección de clase.

4. **Configurar el Jugador y Prefab Multijugador:**
   - Crea un GameObject 3D (Cápsula o Modelo de Explorador) con un `CharacterController`.
   - Añade los scripts `PlayerController.cs`, `PlayerCharacterController.cs`, `ClassAbilities.cs` y `NetworkIdentity`.
   - Guarda el objeto como Prefab en la carpeta `Assets/Prefabs/` y asígnalo en el campo *Player Prefab* del `NetworkLobbyManager`.

5. **Configurar el Enemigo (Sombra Helada):**
   - Crea un prefab para el enemigo con los componentes `NavMeshAgent` y `EnemyAI.cs`.

---

## 🎮 Instrucciones para la Prueba Multijugador (Host & Client)

1. En Unity, ve a `File -> Build Settings`.
2. Añade la escena `LaCaldera_Scene` a las escenas del Build.
3. Haz clic en **Build and Run** para ejecutar una instancia ejecutable del juego (Cliente 1).
4. Vuelve al Editor de Unity y presiona **Play** (Host / Servidor).
5. En la ventana del ejecutable:
   - Haz clic en **Host (Server + Client)** en la primera ventana para crear La Caldera.
   - En la segunda ventana, introduce `localhost` y haz clic en **Client** para unirte al mismo servidor.
6. **Pruebas en tiempo real:**
   - **Reactor:** Presiona la tecla **F** sobre el contenedor del reactor para depositar Ignicita y subir la temperatura.
   - **Alerta de Frío:** Deja caer la Ignicita a 0 y observa cómo la temperatura cae a -5°C/min. Al cruzar los **-10°C**, aparecerá el borde congelado en pantalla y la velocidad de movimiento se reducirá un 20%.
   - **Defensa de Murallas:** En la Fase de Asedio, la IA de las *Sombras Heladas* avanzará automáticamente hacia la muralla con menor HP.
   - **Concejo:** Al finalizar el asedio, se abrirá la sesión de votación democrática del Concejo para repartir energía comunitaria.

---

## 📂 Estructura del Código C#

| Script | Descripción y Función Principal |
| :--- | :--- |
| `ReactorManager.cs` | Bucle de temperatura, consumo de Ignicita y penalizaciones (-10°C). |
| `CityWallHealthSync.cs` | Sincronización en red de los HP de las 4 secciones de muralla (Mirror). |
| `PlayerController.cs` | Controlador 3D, salto e interacción raycast (`IInteractable`). |
| `PlayerCharacterController.cs` | Movimiento en 3a persona, gancho de agarre del Explorador y debuff de velocidad. |
| `ClassAbilities.cs` | Habilidades únicas (Escáner de mineral, torreta de reparación, etc.). |
| `ReactorHUDUI.cs` | UI del termómetro central, barra de Ignicita e indicador de invernaderos. |
| `ScreenFrostPostProcessUI.cs` | Efecto visual de bordes helados en pantalla y banner de advertencia. |
| `ClassSelectionUI.cs` | UI pre-spawn para elegir entre Explorador, Ingeniero, Científico y Táctico. |
| `CouncilVotingManager.cs` | Sesiones de votación ponderadas por clase para distribuir recursos. |
| `EnemyAI.cs` | IA en NavMesh para Sombras Heladas que ataca el muro más debilitado. |
| `NetworkLobbyManager.cs` | Creación y gestión de salas multijugador de 4 a 8 jugadores. |
| `SharedInventorySync.cs` | Inventario compartido en red para almacén global de la ciudad. |
| `GameManager.cs` | Gestor del bucle de fases (Expedición, Asedio, Concejo) y condiciones de victoria/derrota. |
