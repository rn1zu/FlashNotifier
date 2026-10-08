using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FlashNotifier;

public class FlashNotifierConfig : BasePluginConfig
{
    [JsonPropertyName("FlashTagColor")]
    public string FlashTagColor { get; set; } = "\u0009"; // Yellow

    [JsonPropertyName("TextColor")]
    public string TextColor { get; set; } = "\u0001"; // White

    [JsonPropertyName("YouColor")]
    public string YouColor { get; set; } = "\u0001"; // White

    [JsonPropertyName("DurationColor")]
    public string DurationColor { get; set; } = "\u0001"; // Color of the seconds number

    [JsonPropertyName("DurationDotColor")]
    public string DurationDotColor { get; set; } = "\u0001"; // Color of the decimal dot (4.94)

    [JsonPropertyName("SentenceDotColor")]
    public string SentenceDotColor { get; set; } = "\u0001"; // Color of the sentence-ending dot

    [JsonPropertyName("NameColorT")]
    public string NameColorT { get; set; } = "\u0007"; // Red

    [JsonPropertyName("NameColorCT")]
    public string NameColorCT { get; set; } = "\u000B"; // Blue

    [JsonPropertyName("MinDuration")]
    public float MinDuration { get; set; } = 0.5f;

    [JsonPropertyName("DurationDecimals")]
    public int DurationDecimals { get; set; } = 1;

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
    public override string ModuleVersion => "1.0.0";
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

    // Wraps Localizer: replaces %you% and %dot% with colored text
    private string Tr(string key, params object[] args)
    {
        string message = Localizer[key, args];
        string you = $"{_config.YouColor}{Localizer["flash.you"]}{_config.TextColor}";
        string dot = $"{_config.SentenceDotColor}.";
        return message
            .Replace("%you%", you)
            .Replace("%dot%", dot);
    }

    private void OnTick()
    {
        foreach (var player in Utilities.GetPlayers())
        {
            if (player == null || !player.IsValid) continue;
            if (!_playerSettings.TryGetValue(player.Slot, out var settings)) continue;
            if (!settings.BlindDisabled) continue;

            var pawn = player.PlayerPawn.Value;
            if (pawn == null) continue;

            if (pawn.FlashDuration > 0f)
                pawn.FlashDuration = 0f;
            if (pawn.FlashMaxAlpha > 0f)
                pawn.FlashMaxAlpha = 0f;
        }
    }

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

    private int ParseMode(CommandInfo commandInfo)
    {
        if (commandInfo.ArgCount < 2) return -1;

        string arg = commandInfo.GetArg(1).Trim().ToLower();

        if (arg == "1" || arg == "on" || arg == "true" || arg == "yes")
            return 1;

        if (arg == "0" || arg == "off" || arg == "false" || arg == "no")
            return 0;

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

    private string GetNameColor(CCSPlayerController player)
    {
        if (player.Team == CsTeam.Terrorist) return _config.NameColorT;
        if (player.Team == CsTeam.CounterTerrorist) return _config.NameColorCT;
        return _config.TextColor;
    }

    private string FormatDuration(float duration)
    {
        int decimals = _config.DurationDecimals;

        // Any negative value means whole seconds without a dot
        if (decimals < 0)
            decimals = 0;

        decimals = Math.Clamp(decimals, 0, 3);

        // Whole seconds - no dot at all
        if (decimals == 0)
            return $"{_config.DurationColor}{duration.ToString("F0")}{_config.TextColor}";

        string formatted = duration.ToString($"F{decimals}");
        int dotIndex = formatted.IndexOf('.');

        // Safety: if the dot is missing, color the whole thing as DurationColor
        if (dotIndex < 0)
            return $"{_config.DurationColor}{formatted}{_config.TextColor}";

        string intPart = formatted.Substring(0, dotIndex);
        string fracPart = formatted.Substring(dotIndex + 1);

        return $"{_config.DurationColor}{intPart}" +
               $"{_config.DurationDotColor}." +
               $"{_config.DurationColor}{fracPart}" +
               $"{_config.TextColor}";
    }

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
            victim.PrintToChat($" {flashTag} {textColor}{Tr("flash.you_were_flashed_by_yourself", dur)}");
            return HookResult.Continue;
        }

        // 2. No attacker (world / environment)
        if (attacker == null || !attacker.IsValid)
        {
            victim.PrintToChat($" {flashTag} {textColor}{Tr("flash.you_were_flashed", dur)}");
            return HookResult.Continue;
        }

        string attackerColored = $"{GetNameColor(attacker)}{attacker.PlayerName}{textColor}";
        string victimColored = $"{GetNameColor(victim)}{victim.PlayerName}{textColor}";

        // 3. Flashed by a teammate
        if (attacker.Team == victim.Team)
        {
            if (_config.NotifyTeamFlashOnly) return HookResult.Continue;

            victim.PrintToChat($" {flashTag} {textColor}{Tr("flash.teammate_flashed_you", attackerColored, dur)}");
            attacker.PrintToChat($" {flashTag} {textColor}{Tr("flash.you_flashed_teammate", victimColored, dur)}");
            return HookResult.Continue;
        }

        // 4. Flashed by an enemy
        victim.PrintToChat($" {flashTag} {textColor}{Tr("flash.player_flashed_you", attackerColored, dur)}");
        attacker.PrintToChat($" {flashTag} {textColor}{Tr("flash.you_flashed_player", victimColored, dur)}");

        return HookResult.Continue;
    }
}