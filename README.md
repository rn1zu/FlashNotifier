# Flash Notifier

A [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) plugin for CS2 that notifies players when they get blinded by a flashbang — showing who flashed them and how long the effect lasts. Also allows players to disable the blind effect and the notifications entirely.

---

## ✨ Features

- 📢 **Blind notifications** — attacker name and duration
- 🎨 **Full color customization** — tag, main text, "You" word, "for" preposition, T/CT names, seconds number, seconds unit, and dots
- 🔢 **Flexible duration format** — choose decimal places, including a "no dot" mode
- 🔤 **Configurable unit form** — `s` / `sec` / `seconds` (and language equivalents)
- 🌍 **Proper pluralization** — `1 second` vs `2 seconds`, `1 секунду` vs `2 секунды` vs `5 секунд`
- 🕹️ **`!blind` command** — toggle the blind effect (screen whiteness) on/off
- 🔔 **`!blindinfo` command** — toggle notifications on/off
- 🌐 **Localization** — English and Russian included, easy to add your own
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

1. Download `FlashNotifier-v1.0.1.zip` from the [**Releases**](../../releases) page.
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

> 📌 **Upgrading from an older version?** The localization templates have changed — see [Migration from pre-1.0.1](#-migration-from-pre-101) below.

---

## ⚙️ Configuration

### Full parameter reference

| Parameter | Type | Default | Description |
|---|---|---|---|
| `FlashTagColor` | string | `\u0009` | Color of the `[Flash]` tag |
| `TextColor` | string | `\u0001` | Color of the main message text |
| `YouColor` | string | `\u0001` | Color of the word "You" |
| `ForColor` | string | `\u0001` | Color of the preposition ("for" / "на") |
| `DurationColor` | string | `\u0001` | Color of the seconds number |
| `DurationDotColor` | string | `\u0001` | Color of the **dot inside the number** (`4.94`) |
| `UnitColor` | string | `\u0001` | Color of the **seconds unit** (`s` / `sec` / `seconds`) |
| `SentenceDotColor` | string | `\u0001` | Color of the **sentence-ending dot** (`... seconds.`) |
| `NameColorT` | string | `\u0007` | Color of terrorist names |
| `NameColorCT` | string | `\u000B` | Color of counter-terrorist names |
| `MinDuration` | float | `0.5` | Minimum flash duration (in seconds) required to trigger a notification |
| `DurationDecimals` | int | `1` | Number of decimal places. See the table below |
| `SecondsFormat` | string | `short` | Unit form. See the table below |
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

> ℹ️ The decimal separator is always a period (`.`), regardless of the server's OS locale. This is enforced via `CultureInfo.InvariantCulture`, so you will never see `3,0` even on a Russian Windows server.

### 🔤 `SecondsFormat` — unit form

Controls which form of the seconds unit is displayed after the number.

| Value | Aliases | EN example (`4.9`) | RU example (`4.9`) |
|---|---|---|---|
| `short` *(default)* | `s` | `4.9 s` | `4.9 с` |
| `medium` | `med`, `m` | `4.9 sec` | `4.9 сек` |
| `long` | `l`, `full`, `word` | `4.9 seconds` | `4.9 секунды` |

> ℹ️ The value is case-insensitive and trimmed of whitespace: `"Med"`, `"  MED  "`, `"med"` — all equivalent. Anything unrecognized falls back to `short`.

**Pluralization** is applied **only** in `long` form. `short` and `medium` are invariant across all languages.

| Number | EN (`long`) | RU (`long`) |
|---|---|---|
| `1` | `1 second` | `1 секунду` |
| `2` | `2 seconds` | `2 секунды` |
| `4` | `4 seconds` | `4 секунды` |
| `5` | `5 seconds` | `5 секунд` |
| `11` | `11 seconds` | `11 секунд` |
| `21` | `21 seconds` | `21 секунду` |
| `1.5` / `4.9` (any decimal) | `1.5 seconds` | `4.9 секунды` |

> ℹ️ Decimal values use the `other` category in English (plural) and the `other` category in Russian (genitive singular, `секунды`). This is the linguistically correct behaviour.

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
  "ForColor": "\u0008",
  "DurationColor": "\u0001",
  "DurationDotColor": "\u0006",
  "UnitColor": "\u0008",
  "SentenceDotColor": "\u0008",
  "NameColorT": "\u0007",
  "NameColorCT": "\u000B",
  "MinDuration": 0.5,
  "DurationDecimals": 2,
  "SecondsFormat": "short",
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
| `%for%` | The colored preposition ("for" / "на") |
| `%unit%` | The colored seconds unit, pluralized according to language rules |
| `%dot%` | The colored sentence-ending dot |

**Do not replace `%you%` with `{you}` or `%dot%` with `{dot}`.** CounterStrikeSharp uses `string.Format` internally, which throws a `FormatException` on any non-numeric placeholder inside curly braces. Using `{you}` will crash the plugin on the first flash notification. If you want a placeholder that `string.Format` ignores, always use the `%...%` form.

### 🔤 Unit keys and pluralization

The unit word is looked up by one of these keys, depending on `SecondsFormat`:

| `SecondsFormat` | Key |
|---|---|
| `short` | `flash.unit.short` |
| `medium` | `flash.unit.medium` |
| `long` | `flash.unit.long.<category>` — see below |

For `long`, the plugin tries several CLDR plural categories in order:

1. Russian rules (`one` / `few` / `many` / `other`)
2. English rules (`one` / `other`)
3. `other`

The first matching key found in the language file wins. This means **you only need to define the categories your language actually uses**:

- **English** — define `flash.unit.long.one` and `flash.unit.long.other`.
- **Russian** — define `flash.unit.long.one`, `.few`, `.many`, `.other`.
- **Japanese / Chinese / Korean** — only `flash.unit.long.other`.

If a key is missing, the plugin falls through to the next candidate without crashing.

### ⚠️ Grammatical case — important for translators

CLDR plural categories (`one` / `few` / `many` / `other`) describe how a language counts nouns — but they know **nothing** about grammatical case. A language that declines its nouns (Russian, Polish, Czech, Serbian, Ukrainian, …) needs the form that matches the specific sentence structure.

In this plugin all six message templates use the same pattern:

```
... %for% {N} %unit% ...
```

The preposition before the unit dictates the case:

- **Russian** — `на` → accusative → `"секунду"` (not `"секунда"`)
- **English** — `for` → no case system → `"second"` works everywhere
- **German** — `für` → accusative → `"Sekunde"` (same form as nominative)

When translating, do **NOT** copy the nominative/dictionary form from a CLDR reference — use the form required by the sentence in the template. If your language has cases, check each of the six templates individually.

> 📌 **Why this matters:** In the bundled `ru.json`, the unit for the `one` category is `"секунду"` (accusative), not the nominative `"секунда"`, because all six templates place the unit after the preposition «на». A naïve translation that copies the CLDR nominative form would produce the grammatically wrong `"на 1 секунда"` instead of the correct `"на 1 секунду"`.

### 🚫 Languages without a preposition

Some languages (Japanese, Chinese, Korean) don't use a preposition in this construction. Just **remove `%for%` from the template** — and remember to also remove the space that followed it, otherwise you'll get a double space:

```json
// Instead of:
"%you% were flashed %for% {0} %unit%%dot%"

// Write:
"%you% были ослеплены {0} %unit%%dot%"
```

`.Replace("%for%", ...)` on a string that doesn't contain the placeholder is a safe no-op — nothing will break. Alternatively, keep `%for%` and set `"flash.for": ""` in the language file; the placeholder becomes an empty string.

### Example of adding German (`de.json`)

**Original `en.json` fragment:**

```json
"flash.you": "You",
"flash.for": "for",
"flash.unit.short": "s",
"flash.unit.medium": "sec",
"flash.unit.long.one": "second",
"flash.unit.long.other": "seconds",

"flash.you_were_flashed": "%you% were flashed %for% {0} %unit%%dot%",
```

**Translated `de.json` fragment:**

```json
"flash.you": "Du",
"flash.for": "für",
"flash.unit.short": "s",
"flash.unit.medium": "Sek",
"flash.unit.long.one": "Sekunde",
"flash.unit.long.other": "Sekunden",

"flash.you_were_flashed": "%you% wurdest %for% {0} %unit% geblendet%dot%",
```

Note: `%you%`, `%for%`, `{0}`, `%unit%` and `%dot%` remain **unchanged**.

---

## 🔄 Migration from pre-1.0.1

If you are upgrading from an older version **and you only use the bundled `en.json` / `ru.json`**, no action is needed — just replace the files.

If you have **custom language files** (`de.json`, `fr.json`, …), you must update them:

1. Add the new keys:

   ```json
   "flash.for": "for",
   "flash.unit.short": "s",
   "flash.unit.medium": "sec",
   "flash.unit.long.one": "second",
   "flash.unit.long.other": "seconds"
   ```

   (Adjust for your language; use the appropriate plural categories — see the section above.)

2. Replace the hardcoded `for {N} seconds` pattern in **all six message templates** with `%for% {N} %unit%`.

   - Old: `"%you% were flashed for {0} seconds%dot%"`
   - New: `"%you% were flashed %for% {0} %unit%%dot%"`

   Repeat for: `flash.you_were_flashed_by_yourself`, `flash.player_flashed_you`, `flash.you_flashed_player`, `flash.teammate_flashed_you`, `flash.you_flashed_teammate`.

3. Save and **fully restart** the server.

4. **For case-inflected languages (Russian, Polish, Czech, …):** make sure the `flash.unit.long.*` values use the grammatical case required by the sentence template. For the bundled `ru.json` this means `flash.unit.long.one` must be `"секунду"` (accusative), not the nominative `"секунда"`.

5. **`SecondsFormat` now accepts aliases** (`med`, `m`, `l`, `full`, `word`). The canonical values are still `short` / `medium` / `long`, but any of the aliases works and is case-insensitive.

> ⚠️ If you skip steps 1–2, your custom templates will still contain the literal words `for` and `seconds`, and the plugin will simply **not substitute** `%for%` / `%unit%` — the message will still render, just without the new configurable coloring and pluralization.

---

## 🚨 Important behavior notes

### 1. Notification logic

**Who receives what:**

| Situation | Recipient | Message |
|---|---|---|
| Self-flash | You | "You were flashed by yourself for 4.9 s." |
| Team flash | You | "Your teammate **X** flashed you for 4.9 s." |
| Team flash | Teammate | "You flashed your teammate **Y** for 4.9 s." |
| Enemy flash | You | "Your opponent **X** flashed you for 4.9 s." |
| Enemy flash | Enemy | "You flashed your opponent **Y** for 4.9 s." |

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

### The chat shows `%you%`, `%for%`, `%unit%`, `%dot%` or `{0}` literally

**Cause:** localization did not load, or the file contains **invalid placeholders**.

**Fix:**

1. Make sure `lang/en.json` and `lang/ru.json` are located **next to the `.dll`**, not in the root of `plugins/`.
2. Verify that the JSON file uses `%you%`, `%for%`, `%unit%` and `%dot%` (Latin letters, lowercase).
3. If you're upgrading from an older version with custom lang files, see [Migration from pre-1.0.1](#-migration-from-pre-101).

### Unit word is missing or empty

**Cause:** a `flash.unit.*` key is missing from your language file, or the plural category is not defined.

**Fix:**

1. Make sure your language file defines the correct key for the chosen `SecondsFormat`.
2. For `long`, ensure at least `flash.unit.long.other` exists — it is the final fallback.
3. Check that `SecondsFormat` in the config is one of `short`, `medium`, `long` (or one of their aliases — see the "`SecondsFormat` seems ignored" troubleshooting entry below).

### `SecondsFormat` seems ignored

**Cause:** an unrecognized value in the config, so the plugin silently fell back to `short`.

**Fix:** make sure `SecondsFormat` is one of:

- `short`, `s`
- `medium`, `med`, `m`
- `long`, `l`, `full`, `word`

The value is case-insensitive and trimmed. Anything else falls back to `short`.

### Wrong grammatical form in the unit word

**Cause:** the language file uses the dictionary (nominative) form of the noun, but the sentence template requires a specific case.

**Fix:** check each of the six `flash.*` templates in your language file, identify which case the preposition requires, and set `flash.unit.long.*` accordingly. See [Grammatical case — important for translators](#️-grammatical-case--important-for-translators) above.

### `FormatException` in the console

**Cause:** the localization file uses `{you}` instead of `%you%`.

**Fix:** replace all `{you}`, `{for}`, `{unit}` and `{dot}` with `%you%`, `%for%`, `%unit%` and `%dot%`. CounterStrikeSharp uses `string.Format`, which crashes on non-numeric placeholders inside curly braces.

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