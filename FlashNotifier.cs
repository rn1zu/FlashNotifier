using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace FlashNotifier;

public class FlashNotifierConfig : BasePluginConfig
{
    [JsonPropertyName("FlashTagColor")]
    public string FlashTagColor { get; set; } = "\u0009";

    [JsonPropertyName("TextColor")]
    public string TextColor { get; set; } = "\u0001";

    [JsonPropertyName("YouColor")]
    public string YouColor { get; set; } = "\u0001";

    [JsonPropertyName("ForColor")]
    public string ForColor { get; set; } = "\u0001";

    [JsonPropertyName("DurationColor")]
    public string DurationColor { get; set; } = "\u0001";

    [JsonPropertyName("DurationDotColor")]
    public string DurationDotColor { get; set; } = "\u0001";

    [JsonPropertyName("UnitColor")]
    public string UnitColor { get; set; } = "\u0001";

    [JsonPropertyName("SentenceDotColor")]
    public string SentenceDotColor { get; set; } = "\u0001";

    [JsonPropertyName("NameColorT")]
    public string NameColorT { get; set; } = "\u0007";

    [JsonPropertyName("NameColorCT")]
    public string NameColorCT { get; set; } = "\u000B";

    [JsonPropertyName("MinDuration")]
    public float MinDuration { get; set; } = 0.5f;

    [JsonPropertyName("DurationDecimals")]
    public int DurationDecimals { get; set; } = 1;

    [JsonPropertyName("SecondsFormat")]
    public string SecondsFormat { get; set; } = "short";

    [JsonPropertyName("NotifyTeamFlashOnly")]
    public bool NotifyTeamFlashOnly { get; set; } = false;

    [JsonPropertyName("EnableNotifications")]
    public bool EnableNotifications { get; set; } = true;

    [JsonPropertyName("TogglePermission")]
    public string TogglePermission { get; set; } = "css/root";

    [JsonPropertyName("EnableBlindToggle")]
    public bool EnableBlindToggle { get; set; } = true;

    [JsonPropertyName("BlindTogglePermission")]
    public string BlindTogglePermission { get; set; } = "css/root";

    [JsonPropertyName("AnnounceBlindToggle")]
    public bool AnnounceBlindToggle { get; set; } = true;

    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 1;
}

public class PlayerSettings
{
    public bool NotificationsEnabled { get; set; }
    public bool BlindDisabled { get; set; }
}

public class FlashNotifierPlugin : BasePlugin, IPluginConfig<FlashNotifierConfig>
{
    public override string ModuleName => "Flash Notifier";
    public override string ModuleVersion => "1.0.1";
    public override string ModuleAuthor => "You";
    public override string ModuleDescription => "Shows flash blindness duration and controls blind effect";

    private FlashNotifierConfig _config = new();
    public FlashNotifierConfig Config { get => _config; set { _config = value; } }

    private readonly Dictionary<int, PlayerSettings> _playerSettings = new();

    public void OnConfigParsed(FlashNotifierConfig config)
    {
        _config = config;
    }

    public override void Load(bool hotReload)
    {
        RegisterEventHandler<EventPlayerBlind>(OnPlayerBlind);
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

        RegisterListener<Listeners.OnTick>(OnTick);

        AddCommand("blind", "Toggle blind effect. Usage: !blind | !blind 1 | !blind 0", OnBlindCommand);
        AddCommand("blindinfo", "Toggle flash notifications. Usage: !blindinfo | !blindinfo 1 | !blindinfo 0", OnBlindInfoCommand);
    }

    // ──────────────────────────────────────────────────────────────
    //  Localization helpers
    // ──────────────────────────────────────────────────────────────

    private string Tr(string key, params object[] args)
        => BuildMessage(key, -1f, args);

    private string TrD(string key, float duration, params object[] args)
        => BuildMessage(key, duration, args);

    private string BuildMessage(string key, float duration, object[] args)
    {
        string message = args.Length > 0 ? Localizer[key, args] : Localizer[key];

        string you = $"{_config.YouColor}{Localizer["flash.you"]}{_config.TextColor}";
        string dot = $"{_config.SentenceDotColor}.";

        string forRaw = Localizer["flash.for"];
        if (forRaw == "flash.for") forRaw = "";
        string forWord = $"{_config.ForColor}{forRaw}{_config.TextColor}";

        string unit = duration >= 0f ? GetSecondsUnit(duration) : string.Empty;

        return message
            .Replace("%you%", you)
            .Replace("%dot%", dot)
            .Replace("%for%", forWord)
            .Replace("%unit%", unit);
    }

    private string GetSecondsUnit(float duration)
    {
        string format = (_config.SecondsFormat ?? "short").Trim().ToLowerInvariant();

        string baseKey;
        bool plural;
        switch (format)
        {
            // Long form — pluralized
            case "long":
            case "l":
            case "full":
            case "word":
                baseKey = "flash.unit.long";
                plural = true;
                break;

            // Medium form — "sec"
            case "medium":
            case "med":
            case "m":
                baseKey = "flash.unit.medium";
                plural = false;
                break;

            // Short form — "s" (also the fallback for anything unknown)
            default:
                baseKey = "flash.unit.short";
                plural = false;
                break;
        }

        string unitText = plural
            ? ResolvePlural(baseKey, duration)
            : Localizer[baseKey];

        if (unitText == baseKey) unitText = "";

        return $"{_config.UnitColor}{unitText}{_config.TextColor}";
    }

    private string ResolvePlural(string baseKey, float duration)
    {
        int decimals = _config.DurationDecimals;
        if (decimals < 0) decimals = 0;
        if (decimals > 3) decimals = 3;
        bool hasVisibleDecimals = decimals > 0;

        double shown = Math.Round(duration, decimals, MidpointRounding.AwayFromZero);

        var candidates = new List<string>(3);
        void Add(string s) { if (!candidates.Contains(s)) candidates.Add(s); }

        Add(RussianCategory(shown, hasVisibleDecimals));
        Add(EnglishCategory(shown, hasVisibleDecimals));
        Add("other");

        foreach (var suffix in candidates)
        {
            string key = $"{baseKey}.{suffix}";
            string val = Localizer[key];
            if (val != key) return val;
        }

        return Localizer[baseKey];
    }

    // Russian CLDR rules for a countable noun like "секунда".
    private static string RussianCategory(double shown, bool hasVisibleDecimals)
    {
        if (hasVisibleDecimals)
            return "other";

        long n = (long)Math.Round(shown);
        n = Math.Abs(n);
        long mod10 = n % 10;
        long mod100 = n % 100;

        if (mod10 == 1 && mod100 != 11) return "one";
        if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return "few";
        return "many";
    }

    // English CLDR rules.
    private static string EnglishCategory(double shown, bool hasVisibleDecimals)
    {
        if (!hasVisibleDecimals && Math.Abs(shown - 1.0) < 0.0001)
            return "one";
        return "other";
    }

    // ──────────────────────────────────────────────────────────────
    //  Tick / blind suppression
    // ──────────────────────────────────────────────────────────────

    private void OnTick()
    {
        foreach (var player in Utilities.GetPlayers())
        {
            if (player == null || !player.IsValid) continue;
            if (!_playerSettings.TryGetValue(player.Slot, out var settings)) continue;
            if (!settings.BlindDisabled) continue;

            var pawn = player.PlayerPawn.Value;
            if (pawn == null) continue;

            if (pawn.FlashDuration > 0f) pawn.FlashDuration = 0f;
            if (pawn.FlashMaxAlpha > 0f) pawn.FlashMaxAlpha = 0f;
        }
    }

    // ──────────────────────────────────────────────────────────────
    //  Connect / disconnect
    // ──────────────────────────────────────────────────────────────

    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid) return HookResult.Continue;

        _playerSettings[player.Slot] = new PlayerSettings
        {
            NotificationsEnabled = _config.EnableNotifications,
            BlindDisabled = false
        };
        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player != null) _playerSettings.Remove(player.Slot);
        return HookResult.Continue;
    }

    private PlayerSettings GetPlayerSettings(CCSPlayerController player)
    {
        if (!_playerSettings.ContainsKey(player.Slot))
        {
            _playerSettings[player.Slot] = new PlayerSettings
            {
                NotificationsEnabled = _config.EnableNotifications,
                BlindDisabled = false
            };
        }
        return _playerSettings[player.Slot];
    }

    // ──────────────────────────────────────────────────────────────
    //  Commands
    // ──────────────────────────────────────────────────────────────

    private int ParseMode(CommandInfo commandInfo)
    {
        if (commandInfo.ArgCount < 2) return -1;
        string arg = commandInfo.GetArg(1).Trim().ToLower();
        if (arg == "1" || arg == "on" || arg == "true" || arg == "yes") return 1;
        if (arg == "0" || arg == "off" || arg == "false" || arg == "no") return 0;
        return -1;
    }

    private void OnBlindCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!_config.EnableBlindToggle) return;

        if (player == null)
        {
            Server.PrintToChatAll(Tr("flash.players_only"));
            return;
        }

        if (!string.IsNullOrEmpty(_config.BlindTogglePermission)
            && !AdminManager.PlayerHasPermissions(player, _config.BlindTogglePermission))
        {
            player.PrintToChat($" {_config.FlashTagColor}[Flash] {_config.TextColor}{Tr("flash.no_permission")}");
            return;
        }

        var settings = GetPlayerSettings(player);
        int mode = ParseMode(commandInfo);
        bool newState = mode == -1 ? !settings.BlindDisabled : (mode == 0);

        if (settings.BlindDisabled == newState)
        {
            if (_config.AnnounceBlindToggle)
            {
                string key = newState ? "flash.blind_already_off" : "flash.blind_already_on";
                player.PrintToChat($" {_config.FlashTagColor}[Flash] {_config.TextColor}{Tr(key)}");
            }
            return;
        }

        settings.BlindDisabled = newState;

        if (_config.AnnounceBlindToggle)
        {
            string key = newState ? "flash.blind_off" : "flash.blind_on";
            player.PrintToChat($" {_config.FlashTagColor}[Flash] {_config.TextColor}{Tr(key)}");
        }
    }

    private void OnBlindInfoCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null)
        {
            Server.PrintToChatAll(Tr("flash.players_only"));
            return;
        }

        if (!string.IsNullOrEmpty(_config.TogglePermission)
            && !AdminManager.PlayerHasPermissions(player, _config.TogglePermission))
        {
            player.PrintToChat($" {_config.FlashTagColor}[Flash] {_config.TextColor}{Tr("flash.no_permission")}");
            return;
        }

        var settings = GetPlayerSettings(player);
        int mode = ParseMode(commandInfo);
        bool newState = mode == -1 ? !settings.NotificationsEnabled : (mode == 1);

        if (settings.NotificationsEnabled == newState)
        {
            string key = newState ? "flash.info_already_on" : "flash.info_already_off";
            player.PrintToChat($" {_config.FlashTagColor}[Flash] {_config.TextColor}{Tr(key)}");
            return;
        }

        settings.NotificationsEnabled = newState;
        string resultKey = newState ? "flash.info_enabled" : "flash.info_disabled";
        player.PrintToChat($" {_config.FlashTagColor}[Flash] {_config.TextColor}{Tr(resultKey)}");
    }

    // ──────────────────────────────────────────────────────────────
    //  Duration formatting
    // ──────────────────────────────────────────────────────────────

    private string GetNameColor(CCSPlayerController player)
    {
        if (player.Team == CsTeam.Terrorist) return _config.NameColorT;
        if (player.Team == CsTeam.CounterTerrorist) return _config.NameColorCT;
        return _config.TextColor;
    }

    private string FormatDuration(float duration)
    {
        int decimals = _config.DurationDecimals;
        if (decimals < 0) decimals = 0;
        decimals = Math.Clamp(decimals, 0, 3);

        // Force invariant culture so the separator is ALWAYS '.' regardless
        // of the server's OS locale (avoids "3,0" on Russian Windows).
        string formatted = duration.ToString($"F{decimals}", CultureInfo.InvariantCulture);
        int dotIndex = formatted.IndexOf('.');

        if (dotIndex < 0)
            return $"{_config.DurationColor}{formatted}{_config.TextColor}";

        string intPart = formatted.Substring(0, dotIndex);
        string fracPart = formatted.Substring(dotIndex + 1);

        return $"{_config.DurationColor}{intPart}" +
               $"{_config.DurationDotColor}." +
               $"{_config.DurationColor}{fracPart}" +
               $"{_config.TextColor}";
    }

    // ──────────────────────────────────────────────────────────────
    //  Blind event
    // ──────────────────────────────────────────────────────────────

    private HookResult OnPlayerBlind(EventPlayerBlind @event, GameEventInfo info)
    {
        var victim = @event.Userid;
        if (victim == null || !victim.IsValid) return HookResult.Continue;

        var settings = GetPlayerSettings(victim);
        if (!settings.NotificationsEnabled) return HookResult.Continue;

        float duration = @event.BlindDuration;
        if (duration < _config.MinDuration) return HookResult.Continue;

        var attacker = @event.Attacker;
        string dur = FormatDuration(duration);
        string flashTag = $"{_config.FlashTagColor}[Flash]";
        string textColor = _config.TextColor;

        // 1. Self-flash
        if (attacker != null && attacker.IsValid && attacker == victim)
        {
            victim.PrintToChat($" {flashTag} {textColor}{TrD("flash.you_were_flashed_by_yourself", duration, dur)}");
            return HookResult.Continue;
        }

        // 2. No attacker
        if (attacker == null || !attacker.IsValid)
        {
            victim.PrintToChat($" {flashTag} {textColor}{TrD("flash.you_were_flashed", duration, dur)}");
            return HookResult.Continue;
        }

        string attackerColored = $"{GetNameColor(attacker)}{attacker.PlayerName}{textColor}";
        string victimColored = $"{GetNameColor(victim)}{victim.PlayerName}{textColor}";

        // 3. Team flash
        if (attacker.Team == victim.Team)
        {
            if (_config.NotifyTeamFlashOnly) return HookResult.Continue;

            victim.PrintToChat($" {flashTag} {textColor}{TrD("flash.teammate_flashed_you", duration, attackerColored, dur)}");
            attacker.PrintToChat($" {flashTag} {textColor}{TrD("flash.you_flashed_teammate", duration, victimColored, dur)}");
            return HookResult.Continue;
        }

        // 4. Enemy flash
        victim.PrintToChat($" {flashTag} {textColor}{TrD("flash.player_flashed_you", duration, attackerColored, dur)}");
        attacker.PrintToChat($" {flashTag} {textColor}{TrD("flash.you_flashed_player", duration, victimColored, dur)}");

        return HookResult.Continue;
    }
}