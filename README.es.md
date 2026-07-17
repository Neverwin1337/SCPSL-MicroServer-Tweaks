# SCPSL-MicroServer-Tweaks

> Un plugin ligero para **SCP: Secret Laboratory 14.x** basado en **LabAPI**, ajustado para **servidores con pocos jugadores** (≈ 3–12 jugadores).

[![Game](https://img.shields.io/badge/SCP%3ASL-14.x-blue.svg)](#)
[![LabAPI](https://img.shields.io/badge/LabAPI-required-green.svg)](#)
[![.NET](https://img.shields.io/badge/.NET-Framework%204.8-purple.svg)](#)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)](#)
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](#)
[![Downloads](https://img.shields.io/github/downloads/Neverwin1337/SCPSL-MicroServer-Tweaks/total.svg)](https://github.com/Neverwin1337/SCPSL-MicroServer-Tweaks/releases)

Cuando un servidor tiene solo unos pocos jugadores, el ritmo de SCP:SL vanilla se rompe: las rondas terminan en segundos, el SCP no tiene tiempo ni de moverse, y las oleadas de refuerzos llegan demasiado pronto o demasiado tarde. **SCPSL-MicroServer-Tweaks** suaviza el inicio de la partida en servidores pequeños con cuatro funciones enfocadas y configurables, y **cero florituras**.

## 🌐 Idiomas / Languages / 言語

| Idioma | README |
|--------|--------|
| 🇬🇧 English (default) | [README.md](./README.md) |
| 🇨🇳 简体中文 | [README.zh-CN.md](./README.zh-CN.md) |
| 🇹🇼 繁體中文 | [README.zh-TW.md](./README.zh-TW.md) |
| 🇯🇵 日本語 | [README.ja.md](./README.ja.md) |
| 🇪🇸 Español (actual) | [README.es.md](./README.es.md) |
| 🇧🇷 Português (Brasil) | [README.pt-BR.md](./README.pt-BR.md) |

> Consejo: GitHub muestra automáticamente un selector de idioma en la cabecera del repositorio cuando detecta varios archivos `README.*.md`.

---

## 📖 Tutorial de inicio rápido (≈ 5 minutos)

Sigue estos cinco pasos. El tutorial cubre **Windows y Linux**: elige el bloque que se adapte a tu sistema.

### Paso 1 — Requisitos previos

| Requisito | Windows | Linux / macOS |
|-----------|---------|---------------|
| Servidor dedicado de SCP:SL 14.x | ✅ | ✅ |
| LabAPI instalado y funcionando | ✅ | ✅ |
| SDK de .NET con destino **.NET Framework 4.8** | ✅ | ✅ (necesita los ensamblados de referencia `net48`) |
| Shell para el script de compilación | **PowerShell** | **bash** (o zsh) |

Si solo quieres **ejecutar** el plugin (no compilar desde el código fuente), puedes saltarte el SDK de .NET y descargar el DLL ya compilado desde [Releases](../../releases).

### Paso 2 — Obtén el plugin

- **Compílalo tú mismo** — consulta [Build](#-compilar) más abajo
- **O** descarga el DLL ya compilado desde la página [Releases](../../releases)

### Paso 3 — Instálalo

- [RueI](https://github.com/pawslee/RueI/releases/latest) — framework de hints. Copia `RueI.dll` en la carpeta de plugins `global` de LabAPI.
- Copia `SCPSL_MicroServer_Tweaks.dll` en la carpeta de plugins **global** de LabAPI:

| SO | Ruta |
|----|------|
| Windows | `%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\` |
| Linux   | `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/` |
| macOS   | `~/Library/Application Support/SCP Secret Laboratory/LabAPI/plugins/global/` |

> 💡 En un servidor dedicado sin interfaz gráfica, el «usuario» suele ser la cuenta que ejecuta el servicio SCPSL (por ejemplo, `steam`).

### Paso 4 — Configura la cola de roles iniciales

Abre el `config_gameplay.txt` (o `config.txt`) de tu servidor y añade:

```yaml
team_respawn_queue: 40144443
```

Esto coincide con la tabla de composiciones iniciales recomendadas más abajo.

### Paso 5 — Arranca y ajusta

1. Inicia el servidor. En el primer arranque, LabAPI generará automáticamente un `config.yml` junto al DLL del plugin.
2. Abre ese `config.yml` y revisa los valores por defecto (ya funcionan bien para servidores de 5–8 jugadores).
3. ¿Quieres un congelamiento más largo en partidas de 3 jugadores, o más tokens para NTF? Ajusta `scp_freeze_seconds_by_player_count` y `ntf_starting_tokens` — consulta [Configuración](#-configuración).
4. **Reinicia el servidor** para que la nueva configuración surta efecto.

Listo. 🎉 Invita a algunos amigos, observa cómo los SCP permanecen congelados un momento en su punto de aparición y notarás la diferencia.

---

## ✨ Características

- 🧊 **Congelamiento de SCP en el spawn según el número de jugadores**
  - Todos los SCP móviles iniciales se mantienen en su posición de spawn durante un tiempo configurable.
  - Los SCP pueden seguir mirando a su alrededor y usando el chat de voz mientras están congelados.
  - SCP-079 se excluye automáticamente (no tiene posición de spawn normal).
  - Tabla de anulación por número de jugadores (p. ej. congelamiento más largo en rondas de 4, más corto en 10+).
  - Aviso opcional en pantalla con cuenta atrás para los SCP congelados.

- 🎟️ **Tokens de oleada de refuerzo**
  - Configura los **Respawn Tokens** iniciales de la **oleada primaria** para **Nine-Tailed Fox** y **Chaos Insurgency** / Foundation Enemy.
  - Dos modos:
    - `Set` — reemplaza el valor inicial vanilla.
    - `Add` — suma sobre el valor vanilla.
  - Control por facción, con `-1` para no modificar esa facción.

- 🗳️ **Votación de roles en el lobby**
  - Durante la espera en el lobby, escribe `.1` `.2` `.3` `.4` o `.vote scp/sci/d/guard` para votar.
  - Aviso en pantalla en tiempo real: tiempo restante, conteo de votos, instrucciones.
  - Los que no votan son asignados aleatoriamente.

- 🎲 **Eventos aleatorios**
  - Cada 3–5 minutos se dispara un evento aleatorio. Todos los avisos usan **C.A.S.S.I.E.** con tono y efectos de glitch personalizados.
  - **Fallo de ascensores** — Todos los ascensores bloqueados 60s, con cuenta atrás
  - **Apertura de todas las puertas** — Todas las puertas de la instalación se abren (permanente)
  - **Protocolo sigilo** — Todos los humanos reciben invisibilidad real (efecto SCP-268) 30s, con cuenta atrás
  - **Apagón intermitente** — Las luces de las 3 zonas parpadean (5s off / 1s on) durante 3 min, con cuenta atrás
  - **Alerta de ojiva nuclear** — 50% falsa alarma / 50% detonación real, cuenta atrás de 60s (mín. 10 min de ronda)
  - **Teletransporte aleatorio** — Todos los humanos teletransportados aleatoriamente; LCZ excluida si descontaminada
  - Comando de prueba: `mst_event <elevator|doors|stealth|blackout|nuke|scramble>`

- 🪶 **Diseño ligero**
  - Solo depende de [RueI](https://github.com/pawslee/RueI) para mostrar hints. Sin EXILED, MEC o NWPluginAPI.
  - El bloqueo de posición corre en el hilo principal de Unity.
  - Solo referencia los DLL que vienen con **tu propio servidor actual** — sin paquetes de DLL antiguos.

---

## 📋 Cola de roles iniciales recomendada

Una composición sensata para servidores con pocos jugadores:

| Jugadores | SCPs | Guardias | Clase-D | Científicos |
|-----------|:----:|:--------:|:-------:|:-----------:|
| 3         | 1    | 1        | 1       | 0           |
| 4         | 1    | 1        | 2       | 0           |
| 5         | 1    | 1        | 3       | 0           |
| 6         | 1    | 1        | 4       | 0           |
| 7         | 1    | 1        | 5       | 0           |
| 8         | 1    | 1        | 5       | 1           |
| 10        | 1    | 2        | 6       | 1           |
| 12        | 1    | 2        | 7       | 1           |

Establece esto en tu servidor:

```yaml
team_respawn_queue: 40144443
```

---

## 📦 Requisitos

- Servidor dedicado de **SCP: Secret Laboratory** — 14.x actual
- **LabAPI** instalado
- SDK de **.NET** con destino **.NET Framework 4.8**
- Shell para el script de compilación: **PowerShell** (Windows) **o** **bash** (Linux / macOS)

> ⚠️ Nota sobre Linux: compilar contra `net48` en Linux requiere el paquete `Microsoft.NETFramework.ReferenceAssemblies.net48`. `dotnet` lo restaura automáticamente. Si `dotnet build` se queja, ejecuta `dotnet restore` una vez y vuelve a intentarlo.

---

## 🔧 Compilar

### 🪟 Windows (PowerShell)

Abre PowerShell en la carpeta del repositorio:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\build.ps1 `
  -ServerPath "C:\Path\To\SCP Secret Laboratory Dedicated Server"
```

El script solo copia estos tres ensamblados del servidor:

- `Assembly-CSharp.dll`
- `LabApi.dll` / `LabAPI.dll`
- `UnityEngine.CoreModule.dll`

**Deliberadamente** no busca `MEC.dll` ni `Assembly-CSharp-firstpass.dll`.

Salida:

```text
SCPSL-MicroServer-Tweaks/bin/Release/net48/SCPSL_MicroServer_Tweaks.dll
```

#### Compilar e implementar en un solo paso (Windows)

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\build.ps1 `
  -ServerPath "C:\Path\To\SCP Secret Laboratory Dedicated Server" `
  -Deploy
```

Copia el DLL a `%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\`.

---

### 🐧 Linux / macOS (bash)

Primero, da permisos de ejecución al script:

```bash
chmod +x ./build.sh
```

Luego compila:

```bash
./build.sh \
  --server-path "/home/<user>/SCP Secret Laboratory Dedicated Server"
```

Ruta típica en una instalación con SteamCMD: `/home/steam/SCP Secret Laboratory Dedicated Server`.

#### Compilar e implementar en un solo paso (Linux / macOS)

```bash
./build.sh \
  --server-path "/home/steam/SCP Secret Laboratory Dedicated Server" \
  --deploy
```

Copia el DLL a `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/`.

#### Personalizar la carpeta de plugins

```bash
./build.sh \
  --server-path "/opt/scpsl" \
  --deploy \
  --plugin-dir "/opt/scpsl/LabAPI/plugins/global"
```

Ejecuta `./build.sh --help` para ver todas las opciones.

---

## 🚀 Instalación (manual)

Si no usas `-Deploy` / `--deploy`:

1. Compila el plugin (ver arriba) **o** descarga un DLL de Releases.
2. Copia el DLL a la carpeta de plugins global de LabAPI:

   | SO | Ruta |
   |----|------|
   | Windows | `%AppData%\SCP Secret Laboratory\LabAPI\plugins\global\` |
   | Linux   | `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/` |
   | macOS   | `~/Library/Application Support/SCP Secret Laboratory/LabAPI/plugins/global/` |
3. Inicia el servidor una vez — LabAPI generará `config.yml` junto al DLL.
4. Edita `config.yml` a tu gusto y reinicia el servidor.

### Ejecutar el servidor de SCP:SL en Linux

El servidor dedicado oficial de **SCP:SL** solo se publica para Windows. En Linux se suele usar alguna de estas opciones:

- **Wine** + `winetricks` (lo más sencillo para entornos casuales)
- **Proton** (a través de Steam Play)
- Una imagen **Docker** de la comunidad, por ejemplo [`ghcr.io/serverbp/scp-sl-server`](https://github.com/ServerBp/scp-sl-server)

Sea cual sea el método:

1. Instala LabAPI **dentro del prefijo de Wine/Proton/Docker** igual que en Windows.
2. Coloca el DLL del plugin en la carpeta `…/SCP Secret Laboratory/LabAPI/plugins/global/` de ese prefijo.
3. Compila el plugin **en Linux** con `build.sh` — la salida es un DLL .NET gestionado independiente de la plataforma.

---

## ⚙️ Configuración

Encontrarás un ejemplo listo para copiar en [`config.example.yml`](./config.example.yml).

Valores por defecto:

```yaml
enable_scp_freeze: true
default_scp_freeze_seconds: 60
use_player_count_overrides: true

scp_freeze_seconds_by_player_count:
  5: 75
  6: 60
  7: 45
  8: 40

freeze_capture_delay_seconds: 0.5
position_tolerance: 0.03

show_countdown_hint: true
countdown_hint_interval_seconds: 1
countdown_hint_text: |-
  <size=32><color=#ff5555>Procedimiento de contención en curso</color>
  <color=white>Fallo de contención en: {seconds} s</color></size>
release_hint_text: |-
  <size=32><color=#ff5555>Contención fallida</color>
  <color=white>Ya puedes moverte.</color></size>
release_hint_duration_seconds: 4

enable_starting_tokens: true
starting_token_mode: Set    # Set = reemplaza, Add = suma al vanilla
ntf_starting_tokens: 2      # -1 = no modificar esta facción
chaos_starting_tokens: 2
maximum_token_value: 20

enable_debug_logging: false

enable_role_voting: true
voting_time_seconds: 45
vote_hint_interval_seconds: 1

enable_random_events: true
random_event_min_interval_seconds: 180
random_event_max_interval_seconds: 300
random_event_min_players: 1
random_event_elevator_lock_duration: 60
random_event_stealth_duration: 30
random_event_blackout_duration: 180
random_event_nuke_countdown_seconds: 60
random_event_nuke_false_alarm_chance: 0.5
random_event_nuke_min_minutes: 10
```

### ¿Qué es un "Respawn Token"?

Un Respawn Token representa **una oleada de refuerzo primaria**, no a un jugador individual. Las mini-oleadas no se modifican.

### Test inicial sugerido

```yaml
starting_token_mode: Set
ntf_starting_tokens: 2
chaos_starting_tokens: 2
```

Si la ronda sigue terminando antes de un ciclo de refuerzos útil, sube ambos valores a `3`. Evita valores muy altos: el DMS (Dynamic Map Selection) se retrasará hasta que se agoten los recursos de refuerzo.

---

## 📝 Notas

- Los tokens se aplican durante `WaitingForPlayers`, alineándose con el ciclo de inicialización de oleadas de la 14.x.
- El plugin actualiza tanto el valor base/configurado como el restante, y luego envía la actualización de tokens a los clientes conectados.
- Si otro plugin también sobrescribe los tokens iniciales, el **orden de carga** determina el valor final.
- Este proyecto referencia directamente los ensamblados de LabAPI de **tu** servidor actual: reconstruye tras una actualización mayor de SCP:SL o LabAPI.

---

## 🤝 Contribuir

Se aceptan PRs. Por favor:

1. Mantén las dependencias al mínimo — sin EXILED / Harmony / MEC.
2. No incluyas DLLs del servidor en el repositorio.
3. Reconstruye contra tu propio servidor 14.x antes de abrir un PR.
4. Explica **por qué** el cambio ayuda a servidores con pocos jugadores, no solo **qué** hace.

---

## 📄 Licencia

[MIT](./LICENSE)
