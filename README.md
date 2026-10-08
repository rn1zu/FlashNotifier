# Flash Notifier

A [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) plugin for CS2 that notifies players when they get blinded by a flashbang — showing who flashed them and how long the effect lasts. Also allows players to disable the blind effect and the notifications entirely.

---

## ✨ Features

- 📢 **Blind notifications** — attacker name and duration in seconds
- 🎨 **Full color customization** — tag, main text, "You" word, T/CT names, seconds number, and dots
- 🕹️ **`!blind` command** — toggle the blind effect (screen whiteness) on/off
- 🔔 **`!blindinfo` command** — toggle notifications on/off
- 🌐 **Localization** — English and Russian included, easy to add your own
- 🔢 **Flexible duration format** — choose decimal places, including a "no dot" mode
- 🛡️ **Permission control** — restrict commands to specific admin flags

---

## 📦 Requirements

| Component | Minimum version |
|---|---|
| CounterStrikeSharp | **1.0.305** |
| .NET Runtime (server) | **10.0** |
| CS2 | any current version |

> ⚠️ The plugin uses `Microsoft.Extensions.Localization`, which ships with CounterStrikeSharp. No extra dependencies are required.

---

## 🚀 Installation

1. Download `FlashNotifier-v1.0.0.zip` from the [**Releases**](../../releases) page.
2. Extract the archive into your plugins folder:

   ```
   csgo/addons/counterstrikesharp/plugins/FlashNotifier/
   ```

   The result should look like this:

   ```
   plugins/FlashNotifier/
   ├── FlashNotifier.dll
   └── lang/
       ├── en.json
       └── ru.json
   ```

3. **Fully restart** the server (do **not** just use `css_plugins reload`).
4. On the first launch, a config file will be created at:

   ```
   csgo/addons/counterstrikesharp/configs/plugins/FlashNotifier/FlashNotifier.json
   ```

---

## ⚙️ Configuration

### Full parameter reference

| Parameter | Type | Default | Description |
|---|---|---|---|
| `FlashTagColor` | string | `\u0009` | Color of the `[Flash]` tag |
| `TextColor` | string | `\u0001` | Color of the main message text |
| `YouColor` | string | `\u0001` | Color of the word "You" |
| `DurationColor` | string | `\u0001` | Color of the seconds number |
| `DurationDotColor` | string | `\u0001` | Color of the **dot inside the number** (`4.94`) |
| `SentenceDotColor` | string | `\u0001` | Color of the **sentence-ending dot** (`... seconds.`) |
| `NameColorT` | string | `\u0007` | Color of terrorist names |
| `NameColorCT` | string | `\u000B` | Color of counter-terrorist names |
| `MinDuration` | float | `0.5` | Minimum flash duration (in seconds) required to trigger a notification |
| `DurationDecimals` | int | `1` | Number of decimal places. See the table below |
| `NotifyTeamFlashOnly` | bool | `false` | `true` — only notify about **team flashes** |
| `EnableNotifications` | bool | `true` | Default notification state for newly connected players |
| `TogglePermission` | string | `css/root` | Flag required for `!blindinfo`. Empty string = available to everyone |
| `EnableBlindToggle` | bool | `true` | Enable the `!blind` command |
| `BlindTogglePermission` | string | `css/root` | Flag required for `!blind`. Empty string = available to everyone |
| `AnnounceBlindToggle` | bool | `true` | Reply in chat when the `!blind` command toggles state |
| `ConfigVersion` | int | `1` | Service field. **Do not edit manually** |

### 🎨 Color codes

CS2's chat only supports a **fixed palette of ~10 colors**. HEX colors (`#FF0000`) are **not supported** through `PrintToChat` — this is an engine limitation, not a plugin limitation.

| Code | Color | Appearance |
|---|---|---|
| `\u0001` | White | `FFFFFF` |
| `\u0002` | Dark red | `C81E1E` |
| `\u0003` | Purple | `9632C8` |
| `\u0004` | Green | `32C832` |
| `\u0005` | Light green | `32C8C8` |
| `\u0006` | Lime | `64FF64` |
| `\u0007` | Red | `FF3C3C` |
| `\u0008` | Gray | `B4B4B4` |
| `\u0009` | Yellow | `FFDC32` |
| `\u000B` | Blue | `5082FF` |

### 🔢 `DurationDecimals` — decimal places

| Value | Output (example `4.94`) | Dot shown |
|---|---|---|
| `-1` or any value `< 0` | `5` | no |
| `0` | `5` | no |
| `1` | `4.9` | yes |
| `2` | `4.94` | yes |
| `3` | `4.940` | yes |
| `> 3` | `4.940` (clamped to 3) | yes |

### 🛡️ Permissions

`TogglePermission` and `BlindTogglePermission` accept a **CSS admin flag** (e.g. `css/root`, `@css/ban`, `css/admin`).

- **`"css/root"`** — only root admins (safe default).
- **`""`** — **everyone** can use the command. Useful for testing, but on a public server this is effectively a cheat (players can see through flashbangs).

### 📋 Example config

```json
{
  "FlashTagColor": "\u0009",
  "TextColor": "\u0001",
  "YouColor": "\u0004",
  "DurationColor": "\u0001",
  "DurationDotColor": "\u0006",
  "SentenceDotColor": "\u0008",
  "NameColorT": "\u0007",
  "NameColorCT": "\u000B",
  "MinDuration": 0.5,
  "DurationDecimals": 2,
  "NotifyTeamFlashOnly": false,
  "EnableNotifications": true,
  "TogglePermission": "css/root",
  "EnableBlindToggle": true,
  "BlindTogglePermission": "css/root",
  "AnnounceBlindToggle": true,
  "ConfigVersion": 1
}
```

---

## 🕹️ Commands

| Command | Argument | Action |
|---|---|---|
| `!blind` | — | Toggle the blind effect on/off |
| `!blind 1` | `1`, `on`, `true`, `yes` | Enable the blind effect |
| `!blind 0` | `0`, `off`, `false`, `no` | Disable the blind effect |
| `!blindinfo` | — | Toggle notifications on/off |
| `!blindinfo 1` | `1`, `on`, `true`, `yes` | Enable notifications |
| `!blindinfo 0` | `0`, `off`, `false`, `no` | Disable notifications |

---

## 🌐 Localization

### How the language is selected

CounterStrikeSharp picks the localization file **automatically for each player**, based on their game client language (`cl_language` in CS2 settings). **You do not configure anything** — the framework reads the language from the client when the player connects, then:

1. When the plugin calls `Localizer["key"]`, CounterStrikeSharp checks which language the receiving player uses.
2. It looks for a matching file in the `lang/` folder — for example, `ru.json` for Russian, `de.json` for German, `fr.json` for French.
3. If the file does not exist or the key is missing, it **falls back to English** (`en.json`).

**Important:** the available languages are determined **entirely by the files present in the `lang/` folder**. If you delete `ru.json`, Russian players will see English. If you add `de.json`, German players will automatically see German — no server config needed.

### How to add your own language

1. Identify the language code CS2 uses for the target language. This is **not always ISO 639-1** — always confirm with the actual `cl_language` values reported by CS2 clients. Common ones:

   - `en` — English
   - `ru` — Russian
   - `de` — German
   - `fr` — French
   - `es` — Spanish
   - `pt` — Portuguese
   - `pl` — Polish
   - `tr` — Turkish
   - `zh` — Chinese (Simplified)
   - `ko` — Korean
   - `ja` — Japanese

2. Copy `en.json` to a new file named `<code>.json`, for example `de.json`.

3. Translate only the **values** on the right side of each `:`. Keep the **keys** on the left unchanged.

4. **Do NOT modify the placeholders.** They are substituted by the engine at runtime — the translated string must contain the same placeholders as the English original.

5. Save the file and **fully restart the server**. CounterStrikeSharp scans the `lang/` folder only on plugin load.

### ⚠️ Critical: placeholders in localization files

| Placeholder | What it inserts |
|---|---|
| `{0}`, `{1}`, `{2}` | Player names and the seconds value (depends on the string) |
| `%you%` | The colored "You" word |
| `%dot%` | The colored sentence-ending dot |

**Do not replace `%you%` with `{you}` or `%dot%` with `{dot}`.** CounterStrikeSharp uses `string.Format` internally, which throws a `FormatException` on any non-numeric placeholder inside curly braces. Using `{you}` will crash the plugin on the first flash notification. If you want a placeholder that `string.Format` ignores, always use the `%...%` form.

### Example of adding German (`de.json`)

**Original `en.json` fragment:**

```json
"flash.you": "You",
"flash.you_were_flashed": "%you% were flashed for {0} seconds%dot%",
```

**Translated `de.json` fragment:**

```json
"flash.you": "Du",
"flash.you_were_flashed": "%you% wurdest für {0} Sekunden geblendet%dot%",
```

Note: `%you%`, `{0}` and `%dot%` remain **unchanged**.

---

## 🚨 Important behavior notes

### 1. Notification logic

**Who receives what:**

| Situation | Recipient | Message |
|---|---|---|
| Self-flash | You | "You were flashed by yourself for 4.9 seconds." |
| Team flash | You | "Your teammate **X** flashed you for 4.9 seconds." |
| Team flash | Teammate | "You flashed your teammate **Y** for 4.9 seconds." |
| Enemy flash | You | "Your opponent **X** flashed you for 4.9 seconds." |
| Enemy flash | Enemy | "You flashed your opponent **Y** for 4.9 seconds." |

Your own name is **never shown to you** — it is always replaced with "You".

### 2. `NotifyTeamFlashOnly = true`

When enabled, notifications about **enemy flashes** are completely suppressed. Useful for training servers where only team coordination matters.

### 3. `MinDuration`

Flashes shorter than `MinDuration` **do not trigger notifications**. This is anti-spam protection. Default is `0.5` seconds.

### 4. `EnableNotifications` vs `!blindinfo`

- `EnableNotifications` — the **default** state for newly connected players.
- `!blindinfo` — the player's **personal** setting, stored in memory until they reconnect.
- On reconnect, the player's personal setting **resets** to `EnableNotifications`.

### 5. `!blind` does not block the flash

The `!blind` command **removes the blindness effect** (the white screen), but does **not** block the flashbang itself — it still detonates and produces sound and particles. This is implemented by clearing `FlashDuration` and `FlashMaxAlpha` every server tick.

### 6. `ConfigVersion`

`ConfigVersion` is a **service field**. Do not edit it manually. If CSS detects that the version in the file does not match the version in the code, it may overwrite the config with default values.

### 7. Restarting after installation

CounterStrikeSharp caches loaded plugins. After installing or updating:

- **New plugin**: full server restart.
- **`.dll` update**: `css_plugins reload FlashNotifier` may not pick up new dependencies — prefer a full restart.
- **Editing `lang/*.json`**: `css_plugins reload FlashNotifier` is enough.

---

## 🐛 Troubleshooting

### Plugin does not appear in `css_plugins list`

**Cause:** incorrect folder structure or a failed `.dll` load.

**Fix:**

1. Verify that `.dll` is located at `plugins/FlashNotifier/FlashNotifier.dll`.
2. Open the server console — there will be an error message.
3. Make sure the **.NET 10 runtime** is installed on the server.

### Notifications do not appear in chat

1. Check `EnableNotifications` in the config.
2. Check `!blindinfo` — it may have been disabled by the player.
3. Check `MinDuration` — the flash may be too short.
4. Check `NotifyTeamFlashOnly` — if `true`, enemy flashes are hidden.

### The chat shows `%you%`, `%dot%` or `{0}` literally

**Cause:** localization did not load, or the file contains **invalid placeholders**.

**Fix:**

1. Make sure `lang/en.json` and `lang/ru.json` are located **next to the `.dll`**, not in the root of `plugins/`.
2. Verify that the JSON file uses `%you%` and `%dot%` (Latin letters, lowercase).

### `FormatException` in the console

**Cause:** the localization file uses `{you}` instead of `%you%`.

**Fix:** replace all `{you}` and `{dot}` with `%you%` and `%dot%`. CounterStrikeSharp uses `string.Format`, which crashes on non-numeric placeholders inside curly braces.

### Colors do not apply

1. Check that the config uses `\u0009`, not `"\\u0009"` (double backslash).
2. HEX colors (`#FF0000`) are **not supported** in this version — use the codes from the table above.
3. Make sure the server was restarted after editing the config.

---

## 📜 License

This plugin is released under a **non-commercial license**.

You are permitted to use, modify, and redistribute the plugin **for non-commercial purposes only**. This means:

- ✅ Free use on any non-monetized server or in any non-commercial project.
- ✅ Free modification for your own non-commercial needs.
- ✅ Free redistribution of the original or modified version, provided this license is preserved.
- ❌ Selling the plugin, bundling it with paid products, or using it as part of a commercial service is **not permitted** without prior written permission from the author.

The plugin is provided **as is**, without warranty of any kind. See the [LICENSE](LICENSE) file for the full text.

---

## 🔗 Framework

This plugin is built on top of the [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) framework. All API, event system, localization, and permission handling are provided by that project and its contributors.