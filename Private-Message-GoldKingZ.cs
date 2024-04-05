using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Localization;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Private_Message_GoldKingZ.Config;
using CounterStrikeSharp.API.Modules.Menu;
using PrivateMessageGoldKingZ;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Timers;

namespace Private_Message_GoldKingZ;

public class PrivateMessageGoldKingZ : BasePlugin
{
    public override string ModuleName => "Direct Private Message Rooms (Players Send Dm Create Private Rooms)";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Gold KingZ";
    public override string ModuleDescription => "https://github.com/oqyh";
    internal static IStringLocalizer? Stringlocalizer;

    public override void Load(bool hotReload)
    {
        Configs.Load(ModuleDirectory);
        Stringlocalizer = Localizer;
        AddCommandListener("say", OnPlayerSayPublic, HookMode.Pre);
        AddCommandListener("say_team", OnPlayerSayTeam, HookMode.Pre);
        RegisterEventHandler<EventPlayerConnectFull>(OnEventPlayerConnectFull);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
    }
    public void OnMapStart(string mapname)
    {
        if(Configs.GetConfigData().Log_Dm_AutoDeleteLogsMoreThanXdaysOld > 0)
        {
            string FpathDM = Path.Combine(ModuleDirectory,"../../plugins/Private-Message-GoldKingZ/logs/Direct Messages");
            Helper.DeleteOldFiles(FpathDM, "*" + ".txt", TimeSpan.FromDays(Configs.GetConfigData().Log_Dm_AutoDeleteLogsMoreThanXdaysOld));
        }
        if(Configs.GetConfigData().Log_Pr_AutoDeleteLogsMoreThanXdaysOld > 0)
        {
            string FpathPR = Path.Combine(ModuleDirectory,"../../plugins/Private-Message-GoldKingZ/logs/Private Room Messages");
            Helper.DeleteOldFiles(FpathPR, "*" + ".txt", TimeSpan.FromDays(Configs.GetConfigData().Log_Pr_AutoDeleteLogsMoreThanXdaysOld));
        }
    }
    private HookResult OnPlayerSayPublic(CCSPlayerController? Caller, CommandInfo info)
    {
        string FpathPR = Path.Combine(ModuleDirectory,"../../plugins/Private-Message-GoldKingZ/logs/Private Room Messages/");
        string FpathDM = Path.Combine(ModuleDirectory,"../../plugins/Private-Message-GoldKingZ/logs/Direct Messages/");
        string Time = DateTime.Now.ToString("HH:mm:ss");
        string Date = DateTime.Now.ToString("MM-dd-yyyy");
        string fileNamedm = DateTime.Now.ToString("[MM-dd-yyyy] ") + "Dm-Messages.txt";
        string TpathDM = Path.Combine(ModuleDirectory,"../../plugins/Private-Message-GoldKingZ/logs/Direct Messages/") + $"{fileNamedm}";
        string fileNamepr = DateTime.Now.ToString("[MM-dd-yyyy] ") + "Pivate-Room-Messages.txt";
        string TpathPR = Path.Combine(ModuleDirectory,"../../plugins/Private-Message-GoldKingZ/logs/Private Room Messages/") + $"{fileNamepr}";

        if (Caller == null || !Caller.IsValid)return HookResult.Continue;
        var message = info.GetArg(1);
        var targetname = info.GetArg(2);
        var target = info.GetArgTargetResult(2);
        var CallerName = Caller.PlayerName;
        var CallerTeam = Caller.TeamNum;
        var CallerSteamID = Caller.SteamID;
        var callPlayerIp = Caller.IpAddress;
        string[] partsip = callPlayerIp!.Split(':');
        string CallerIp = partsip[0];

        if (string.IsNullOrWhiteSpace(message)) return HookResult.Continue;
        string trimmedMessageStart = message.TrimStart();
        string trimmedMessage = trimmedMessageStart.TrimEnd();
        
        
        var parts = message.Split(' ');
        string MESSAGEAFTER = string.Join(" ", parts.Skip(2));

        string DmInGameCommand = Configs.GetConfigData().Dm_InGameCommands;
        string[] DmInGameCommands = DmInGameCommand.Split(',');
        if (DmInGameCommands.Any(command => trimmedMessageStart.StartsWith(command.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            if (!string.IsNullOrEmpty(Configs.GetConfigData().Dm_AllowOnlyForGroups) && !Globals.dm_group.ContainsKey(CallerSteamID))
            {
                if (!string.IsNullOrEmpty(Localizer["dm.not.allowed"]))
                {
                    Helper.AdvancedPrintToChat(Caller, Localizer["dm.not.allowed"]);
                }
                return HookResult.Continue;
            }

            if (parts.Length >= 3)
            {
                string targetName = parts[1];
                TargetResult targetResult = info.GetArgTargetResult(2);
                if (targetResult == null)
                {
                    return HookResult.Continue;
                }

                List<CCSPlayerController> playersToTarget = targetResult.Players.Where(player => player != Caller && player != null && player.IsValid && !player.IsBot && !player.IsHLTV && player.Connected == PlayerConnectedState.PlayerConnected).ToList();

                CCSPlayerController closestMatch = Helper.FindClosestMatch(playersToTarget, targetName);

                if (closestMatch != Caller && closestMatch != null && closestMatch.IsValid)
                {
                    if (!string.IsNullOrEmpty(Localizer["dm.message.format"]))
                    {
                        var recivername = closestMatch.PlayerName;
                        var reciversteamid = closestMatch.SteamID;
                        var recPlayerIp = closestMatch.IpAddress;
                        string[] Rpartsip = recPlayerIp!.Split(':');
                        string reciverIp = Rpartsip[0];
                        Helper.AdvancedPrintToChat(closestMatch, Localizer["dm.message.format"], CallerName, recivername, MESSAGEAFTER);
                        Helper.AdvancedPrintToChat(Caller, Localizer["dm.message.format"], CallerName, recivername, MESSAGEAFTER);

                        if(!string.IsNullOrEmpty(Configs.GetConfigData().AllowTheseGroupsToSpyAllDmsInGame))
                        {
                            var playersall = Helper.GetAllController();
                            playersall.ForEach(player => 
                            {
                                if(player == Caller || player == closestMatch)return;
                                var steamid = player.SteamID;
                                if (Globals.spy_dm.ContainsKey(steamid))
                                {
                                    if (!string.IsNullOrEmpty(Localizer["spy.ingame.dm.message.format"]))
                                    {
                                        Helper.AdvancedPrintToChat(player, Localizer["spy.ingame.dm.message.format"], CallerName, closestMatch.PlayerName, MESSAGEAFTER);
                                    }
                                }
                            });
                        }
                        if(Configs.GetConfigData().Log_SendLogToText)
                        {
                            var replacerlog = Helper.ReplaceMessages(Configs.GetConfigData().Log_Dm_Message_Format, Time, Date, CallerName, closestMatch.PlayerName, MESSAGEAFTER, CallerSteamID.ToString(), reciversteamid.ToString(), CallerIp.ToString(), reciverIp.ToString());
                            
                            Task.Run(() =>
                            {
                                if(!Directory.Exists(FpathDM))
                                {
                                    Directory.CreateDirectory(FpathDM);
                                }

                                if(!File.Exists(TpathDM))
                                {
                                    using (File.Create(TpathDM)) { }
                                }
                                
                                try
                                {
                                    File.AppendAllLines(TpathDM, new[]{replacerlog});
                                }catch
                                {

                                }
                            });
                            
                        }
                        var replacerlogd = Helper.ReplaceMessages(Configs.GetConfigData().Log_DiscordDmMessageFormat, Time, Date, CallerName, closestMatch.PlayerName, MESSAGEAFTER, CallerSteamID.ToString(), reciversteamid.ToString(), CallerIp.ToString(), reciverIp.ToString());
                        
                        if (!string.IsNullOrEmpty(Configs.GetConfigData().Log_DiscordDmMessageFormat))
                        {
                            if(Configs.GetConfigData().Log_SendLogToDiscordOnMode == 1)
                            { 
                                Task.Run(() =>
                                {
                                    _ = Helper.SendToDiscordWebhookNormal(Configs.GetConfigData().Log_DiscordWebHookURL, replacerlogd);
                                });
                            }else if(Configs.GetConfigData().Log_SendLogToDiscordOnMode == 2)
                            {
                                Task.Run(() =>
                                {
                                    _ = Helper.SendToDiscordWebhookNameLink(Configs.GetConfigData().Log_DiscordWebHookURL, replacerlogd, CallerSteamID.ToString(), CallerName);
                                });
                            }else if(Configs.GetConfigData().Log_SendLogToDiscordOnMode == 3)
                            {
                                Task.Run(() =>
                                {
                                    _ = Helper.SendToDiscordWebhookNameLinkWithPicture(Configs.GetConfigData().Log_DiscordWebHookURL, replacerlogd, CallerSteamID.ToString(), CallerName);
                                });
                            }
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Localizer["dm.cannot.find.playername"]))
                    {
                        Helper.AdvancedPrintToChat(Caller, Localizer["dm.cannot.find.playername"], targetName);
                    }
                }
                
            }else if (parts.Length == 2)
            {
                if (!string.IsNullOrEmpty(Localizer["dm.missing.message"]))
                {
                    Helper.AdvancedPrintToChat(Caller, Localizer["dm.missing.message"]);
                }
                
            }else if (parts.Length == 1)
            {
                if (!string.IsNullOrEmpty(Localizer["dm.missing.name"]))
                {
                    Helper.AdvancedPrintToChat(Caller, Localizer["dm.missing.name"]);
                }
            }
            return HookResult.Handled;
        }

        string[] PrInGameCreateRoomCommands = Configs.GetConfigData().Pr_InGameCreateRoomCommands.Split(',');
        if (PrInGameCreateRoomCommands.Any(cmd => cmd.Equals(trimmedMessage, StringComparison.OrdinalIgnoreCase)))
        {
            if (!string.IsNullOrEmpty(Configs.GetConfigData().Pr_AllowOnlyForGroups) && !Globals.pr_group.ContainsKey(CallerSteamID))
            {
                if (!string.IsNullOrEmpty(Localizer["pr.not.allowed"]))
                {
                    Helper.AdvancedPrintToChat(Caller, Localizer["pr.not.allowed"]);
                }
                return HookResult.Continue;
            }

            if(!Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID) )
            {
                if(Globals.CurrentRoomAvailable == 0)
                {
                    Globals.CurrentRoomAvailable++;
                }
                Globals.CreatingPrivateRoom.Add(CallerSteamID, Globals.CurrentRoomAvailable);
                Globals.CurrentRoomAvailable++;
            }

            if (!string.IsNullOrEmpty(Localizer["pr.created"]))
            {
                Helper.AdvancedPrintToChat(Caller, Localizer["pr.created"]);
            }
        }

        string[] PrInGameInviteRoomCommands = Configs.GetConfigData().Pr_InGameInviteRoomCommands.Split(',');
        if (PrInGameInviteRoomCommands.Any(cmd => cmd.Equals(trimmedMessage, StringComparison.OrdinalIgnoreCase)))
        {
            if (!string.IsNullOrEmpty(Configs.GetConfigData().Pr_AllowOnlyForGroups) && !Globals.pr_group.ContainsKey(CallerSteamID))
            {
                if (!string.IsNullOrEmpty(Localizer["pr.not.allowed"]))
                {
                    Helper.AdvancedPrintToChat(Caller, Localizer["pr.not.allowed"]);
                }
                return HookResult.Continue;
            }

            if (!Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID))
            {
                if (!string.IsNullOrEmpty(Localizer["pr.notcreated"]))
                {
                    Helper.AdvancedPrintToChat(Caller, Localizer["pr.notcreated"]);
                }
                return HookResult.Continue;
            }

            var PrivateMenu = new ChatMenu (string.IsNullOrEmpty(Localizer["pr.menu.name"]) ? "Private Message Room Menu" : Localizer["pr.menu.name"]);
            var AllPlayers = Helper.GetAllController();
            foreach (var players in AllPlayers)
            {
                if(Caller == players ||  players.IsHLTV || players.IsBot)continue;
                var TargetPlayersNames = players.PlayerName;
                var TargetPlayersUserID = (int)players.UserId!;
                PrivateMenu.AddMenuOption(TargetPlayersNames, (Caller, option) => HandleMenu(Caller, TargetPlayersUserID));
            }

            if (!string.IsNullOrEmpty(Localizer["pr.menu.exit"]))
            {
                PrivateMenu.AddMenuOption(Localizer["pr.menu.exit"], SelectExit);
            }else if (string.IsNullOrEmpty(Localizer["pr.menu.exit"]))
            {
                PrivateMenu.AddMenuOption("Exit", SelectExit);
            }

            MenuManager.OpenChatMenu(Caller, PrivateMenu);
        }

        if(Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID) && Globals.CreatingPrivateRoom[CallerSteamID] != 0)
        {
            string[] PrInGameLeaveRoomCommands = Configs.GetConfigData().Pr_InGameLeaveRoomCommands.Split(',');
            var playersall = Helper.GetAllController();
            if (PrInGameLeaveRoomCommands.Any(cmd => cmd.Equals(trimmedMessage, StringComparison.OrdinalIgnoreCase)))
            {
                playersall.ForEach(player => 
                {
                    var steamid = player.SteamID;
                    if (Globals.CreatingPrivateRoom.ContainsKey(player.SteamID) && Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID))
                    {
                        if (Globals.CreatingPrivateRoom[player.SteamID] == Globals.CreatingPrivateRoom[CallerSteamID])
                        {
                            if (!string.IsNullOrEmpty(Localizer["pr.left"]))
                            {
                                Helper.AdvancedPrintToChat(player, Localizer["pr.left"], CallerName);
                            }
                        }
                    }
                });
                Globals.CreatingPrivateRoom.Remove(CallerSteamID);
                return HookResult.Handled;
            }
            string[] PrInGameListPlayersInRoomCommands = Configs.GetConfigData().Pr_InGameListPlayersInRoomCommands.Split(',');
            if (PrInGameListPlayersInRoomCommands.Any(cmd => cmd.Equals(trimmedMessage, StringComparison.OrdinalIgnoreCase)))
            {
                if (!string.IsNullOrEmpty(Localizer["pr.listplayers.info"]))
                {
                    Helper.AdvancedPrintToChat(Caller, Localizer["pr.listplayers.info"]);
                }
                
                playersall.ForEach(player => 
                {
                    var steamid = player.SteamID;
                    if (Globals.CreatingPrivateRoom.ContainsKey(player.SteamID) && Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID))
                    {
                        if (Globals.CreatingPrivateRoom[player.SteamID] == Globals.CreatingPrivateRoom[CallerSteamID])
                        {
                            var playersintheroom = player.PlayerName;
                            if (!string.IsNullOrEmpty(Localizer["pr.listplayers"]))
                            {
                                Helper.AdvancedPrintToChat(Caller, Localizer["pr.listplayers"], playersintheroom);
                            }
                        }
                    }
                });
                return HookResult.Handled;
            }
            
            if (Configs.GetConfigData().Pr_OnSayTeamOnly || info.GetArg(1).StartsWith("!") || info.GetArg(1).StartsWith("@") || info.GetArg(1).StartsWith("/") || info.GetArg(1).StartsWith(".") || info.GetArg(1).StartsWith("rtv")) return HookResult.Continue;
            
            playersall.ForEach(player => 
            {
                var steamid = player.SteamID;
                if (Globals.CreatingPrivateRoom.ContainsKey(player.SteamID))
                {
                    if (Globals.CreatingPrivateRoom[player.SteamID] == Globals.CreatingPrivateRoom[CallerSteamID])
                    {
                        if (!string.IsNullOrEmpty(Localizer["pr.message.format"]))
                        {
                            Helper.AdvancedPrintToChat(player, Localizer["pr.message.format"], CallerName, message);
                            
                            if(!string.IsNullOrEmpty(Configs.GetConfigData().AllowTheseGroupsToSpyAllPrmsInGame))
                            {
                                playersall.ForEach(playerS => 
                                {
                                    if(playerS == Caller)return;
                                    var steamidS = playerS.SteamID;
                                    if (Globals.spy_pr.ContainsKey(steamidS))
                                    {
                                        if (!string.IsNullOrEmpty(Localizer["spy.ingame.pr.message.format"]))
                                        {
                                            Helper.AdvancedPrintToChat(playerS, Localizer["spy.ingame.pr.message.format"], CallerName, message);
                                        }
                                    }
                                });
                            }
                            if(Configs.GetConfigData().Log_SendLogToText)
                            {
                                var replacerlog = Helper.ReplaceMessages(Configs.GetConfigData().Log_Pr_Message_Format, Time, Date, CallerName, string.Empty, message, CallerSteamID.ToString(), string.Empty, CallerIp.ToString(), string.Empty);
                                
                                Task.Run(() =>
                                {
                                    if(!Directory.Exists(FpathPR))
                                    {
                                        Directory.CreateDirectory(FpathPR);
                                    }

                                    if(!File.Exists(TpathPR))
                                    {
                                        using (File.Create(TpathPR)) { }
                                    }
                                    
                                    try
                                    {
                                        File.AppendAllLines(TpathPR, new[]{replacerlog});
                                    }catch
                                    {

                                    }
                                });
                                
                            }
                            var replacerlogd = Helper.ReplaceMessages(Configs.GetConfigData().Log_DiscordPrMessageFormat, Time, Date, CallerName, string.Empty, message, CallerSteamID.ToString(), string.Empty, CallerIp.ToString(), string.Empty);
                            
                            if (!string.IsNullOrEmpty(Configs.GetConfigData().Log_DiscordPrMessageFormat))
                            {
                                if(Configs.GetConfigData().Log_SendLogToDiscordOnMode == 1)
                                { 
                                    Task.Run(() =>
                                    {
                                        _ = Helper.SendToDiscordWebhookNormal(Configs.GetConfigData().Log_DiscordWebHookURL, replacerlogd);
                                    });
                                }else if(Configs.GetConfigData().Log_SendLogToDiscordOnMode == 2)
                                {
                                    Task.Run(() =>
                                    {
                                        _ = Helper.SendToDiscordWebhookNameLink(Configs.GetConfigData().Log_DiscordWebHookURL, replacerlogd, CallerSteamID.ToString(), CallerName);
                                    });
                                }else if(Configs.GetConfigData().Log_SendLogToDiscordOnMode == 3)
                                {
                                    Task.Run(() =>
                                    {
                                        _ = Helper.SendToDiscordWebhookNameLinkWithPicture(Configs.GetConfigData().Log_DiscordWebHookURL, replacerlogd, CallerSteamID.ToString(), CallerName);
                                    });
                                }
                            }
                        }
                    }
                }
            });
            return HookResult.Handled;
        }


        if(Globals.Watingforaccept.ContainsKey(CallerSteamID))
        {
            string[] PrInGameJoinRoomCommands = Configs.GetConfigData().Pr_InGameJoinRoomCommands.Split(',');
            var playersall = Helper.GetAllController();
            if (PrInGameJoinRoomCommands.Any(cmd => cmd.Equals(trimmedMessage, StringComparison.OrdinalIgnoreCase)))
            {
                if(!Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID))
                {
                    Globals.CreatingPrivateRoom.Add(CallerSteamID, 0);
                }
                if(Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID))
                {
                    Globals.CreatingPrivateRoom[CallerSteamID] = Globals.Watingforaccept[CallerSteamID];
                    playersall.ForEach(player => 
                    {
                        var steamid = player.SteamID;
                        if (Globals.CreatingPrivateRoom.ContainsKey(player.SteamID) && Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID))
                        {
                            if (Globals.CreatingPrivateRoom[player.SteamID] == Globals.CreatingPrivateRoom[CallerSteamID])
                            {
                                if (!string.IsNullOrEmpty(Localizer["pr.joined"]))
                                {
                                    Helper.AdvancedPrintToChat(player, Localizer["pr.joined"], CallerName);
                                }
                                
                            }
                        }
                    });
                    if (!string.IsNullOrEmpty(Localizer["pr.joined.info"]))
                    {
                        Helper.AdvancedPrintToChat(Caller, Localizer["pr.joined.info"]);
                    }
                }
                Globals.Watingforaccept.Remove(CallerSteamID);
                return HookResult.Handled;
            }
        }

        return HookResult.Continue;
    }
    private HookResult OnPlayerSayTeam(CCSPlayerController? Caller, CommandInfo info)
    {
        string FpathPR = Path.Combine(ModuleDirectory,"../../plugins/Private-Message-GoldKingZ/logs/Private Room Messages/");
        string FpathDM = Path.Combine(ModuleDirectory,"../../plugins/Private-Message-GoldKingZ/logs/Direct Messages/");
        string Time = DateTime.Now.ToString("HH:mm:ss");
        string Date = DateTime.Now.ToString("MM-dd-yyyy");
        string fileNamedm = DateTime.Now.ToString("[MM-dd-yyyy] ") + "Dm-Messages.txt";
        string TpathDM = Path.Combine(ModuleDirectory,"../../plugins/Private-Message-GoldKingZ/logs/Direct Messages/") + $"{fileNamedm}";
        string fileNamepr = DateTime.Now.ToString("[MM-dd-yyyy] ") + "Pivate-Room-Messages.txt";
        string TpathPR = Path.Combine(ModuleDirectory,"../../plugins/Private-Message-GoldKingZ/logs/Private Room Messages/") + $"{fileNamepr}";

        if (Caller == null || !Caller.IsValid)return HookResult.Continue;
        var message = info.GetArg(1);
        var targetname = info.GetArg(2);
        var target = info.GetArgTargetResult(2);
        var CallerName = Caller.PlayerName;
        var CallerTeam = Caller.TeamNum;
        var CallerSteamID = Caller.SteamID;
        var callPlayerIp = Caller.IpAddress;
        string[] partsip = callPlayerIp!.Split(':');
        string CallerIp = partsip[0];

        if (string.IsNullOrWhiteSpace(message)) return HookResult.Continue;
        string trimmedMessageStart = message.TrimStart();
        string trimmedMessage = trimmedMessageStart.TrimEnd();
        
        
        var parts = message.Split(' ');
        string MESSAGEAFTER = string.Join(" ", parts.Skip(2));

        string DmInGameCommand = Configs.GetConfigData().Dm_InGameCommands;
        string[] DmInGameCommands = DmInGameCommand.Split(',');
        if (DmInGameCommands.Any(command => trimmedMessageStart.StartsWith(command.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            if (!string.IsNullOrEmpty(Configs.GetConfigData().Dm_AllowOnlyForGroups) && !Globals.dm_group.ContainsKey(CallerSteamID))
            {
                if (!string.IsNullOrEmpty(Localizer["dm.not.allowed"]))
                {
                    Helper.AdvancedPrintToChat(Caller, Localizer["dm.not.allowed"]);
                }
                return HookResult.Continue;
            }

            if (parts.Length >= 3)
            {
                string targetName = parts[1];
                TargetResult targetResult = info.GetArgTargetResult(2);
                if (targetResult == null)
                {
                    return HookResult.Continue;
                }

                List<CCSPlayerController> playersToTarget = targetResult.Players.Where(player => player != Caller && player != null && player.IsValid && !player.IsBot && !player.IsHLTV && player.Connected == PlayerConnectedState.PlayerConnected).ToList();

                CCSPlayerController closestMatch = Helper.FindClosestMatch(playersToTarget, targetName);

                if (closestMatch != Caller && closestMatch != null && closestMatch.IsValid)
                {
                    if (!string.IsNullOrEmpty(Localizer["dm.message.format"]))
                    {
                        var recivername = closestMatch.PlayerName;
                        var reciversteamid = closestMatch.SteamID;
                        var recPlayerIp = closestMatch.IpAddress;
                        string[] Rpartsip = recPlayerIp!.Split(':');
                        string reciverIp = Rpartsip[0];
                        Helper.AdvancedPrintToChat(closestMatch, Localizer["dm.message.format"], CallerName, recivername, MESSAGEAFTER);
                        Helper.AdvancedPrintToChat(Caller, Localizer["dm.message.format"], CallerName, recivername, MESSAGEAFTER);

                        if(!string.IsNullOrEmpty(Configs.GetConfigData().AllowTheseGroupsToSpyAllDmsInGame))
                        {
                            var playersall = Helper.GetAllController();
                            playersall.ForEach(player => 
                            {
                                if(player == Caller || player == closestMatch)return;
                                var steamid = player.SteamID;
                                if (Globals.spy_dm.ContainsKey(steamid))
                                {
                                    if (!string.IsNullOrEmpty(Localizer["spy.ingame.dm.message.format"]))
                                    {
                                        Helper.AdvancedPrintToChat(player, Localizer["spy.ingame.dm.message.format"], CallerName, closestMatch.PlayerName, MESSAGEAFTER);
                                    }
                                }
                            });
                        }
                        if(Configs.GetConfigData().Log_SendLogToText)
                        {
                            var replacerlog = Helper.ReplaceMessages(Configs.GetConfigData().Log_Dm_Message_Format, Time, Date, CallerName, closestMatch.PlayerName, MESSAGEAFTER, CallerSteamID.ToString(), reciversteamid.ToString(), CallerIp.ToString(), reciverIp.ToString());
                            
                            Task.Run(() =>
                            {
                                if(!Directory.Exists(FpathDM))
                                {
                                    Directory.CreateDirectory(FpathDM);
                                }

                                if(!File.Exists(TpathDM))
                                {
                                    using (File.Create(TpathDM)) { }
                                }
                                
                                try
                                {
                                    File.AppendAllLines(TpathDM, new[]{replacerlog});
                                }catch
                                {

                                }
                            });
                            
                        }
                        var replacerlogd = Helper.ReplaceMessages(Configs.GetConfigData().Log_DiscordDmMessageFormat, Time, Date, CallerName, closestMatch.PlayerName, MESSAGEAFTER, CallerSteamID.ToString(), reciversteamid.ToString(), CallerIp.ToString(), reciverIp.ToString());
                        
                        if (!string.IsNullOrEmpty(Configs.GetConfigData().Log_DiscordDmMessageFormat))
                        {
                            if(Configs.GetConfigData().Log_SendLogToDiscordOnMode == 1)
                            { 
                                Task.Run(() =>
                                {
                                    _ = Helper.SendToDiscordWebhookNormal(Configs.GetConfigData().Log_DiscordWebHookURL, replacerlogd);
                                });
                            }else if(Configs.GetConfigData().Log_SendLogToDiscordOnMode == 2)
                            {
                                Task.Run(() =>
                                {
                                    _ = Helper.SendToDiscordWebhookNameLink(Configs.GetConfigData().Log_DiscordWebHookURL, replacerlogd, CallerSteamID.ToString(), CallerName);
                                });
                            }else if(Configs.GetConfigData().Log_SendLogToDiscordOnMode == 3)
                            {
                                Task.Run(() =>
                                {
                                    _ = Helper.SendToDiscordWebhookNameLinkWithPicture(Configs.GetConfigData().Log_DiscordWebHookURL, replacerlogd, CallerSteamID.ToString(), CallerName);
                                });
                            }
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Localizer["dm.cannot.find.playername"]))
                    {
                        Helper.AdvancedPrintToChat(Caller, Localizer["dm.cannot.find.playername"], targetName);
                    }
                }
                
            }else if (parts.Length == 2)
            {
                if (!string.IsNullOrEmpty(Localizer["dm.missing.message"]))
                {
                    Helper.AdvancedPrintToChat(Caller, Localizer["dm.missing.message"]);
                }
                
            }else if (parts.Length == 1)
            {
                if (!string.IsNullOrEmpty(Localizer["dm.missing.name"]))
                {
                    Helper.AdvancedPrintToChat(Caller, Localizer["dm.missing.name"]);
                }
            }
            return HookResult.Handled;
        }

        string[] PrInGameCreateRoomCommands = Configs.GetConfigData().Pr_InGameCreateRoomCommands.Split(',');
        if (PrInGameCreateRoomCommands.Any(cmd => cmd.Equals(trimmedMessage, StringComparison.OrdinalIgnoreCase)))
        {
            if (!string.IsNullOrEmpty(Configs.GetConfigData().Pr_AllowOnlyForGroups) && !Globals.pr_group.ContainsKey(CallerSteamID))
            {
                if (!string.IsNullOrEmpty(Localizer["pr.not.allowed"]))
                {
                    Helper.AdvancedPrintToChat(Caller, Localizer["pr.not.allowed"]);
                }
                return HookResult.Continue;
            }

            if(!Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID) )
            {
                if(Globals.CurrentRoomAvailable == 0)
                {
                    Globals.CurrentRoomAvailable++;
                }
                Globals.CreatingPrivateRoom.Add(CallerSteamID, Globals.CurrentRoomAvailable);
                Globals.CurrentRoomAvailable++;
            }

            if (!string.IsNullOrEmpty(Localizer["pr.created"]))
            {
                Helper.AdvancedPrintToChat(Caller, Localizer["pr.created"]);
            }
        }

        string[] PrInGameInviteRoomCommands = Configs.GetConfigData().Pr_InGameInviteRoomCommands.Split(',');
        if (PrInGameInviteRoomCommands.Any(cmd => cmd.Equals(trimmedMessage, StringComparison.OrdinalIgnoreCase)))
        {
            if (!string.IsNullOrEmpty(Configs.GetConfigData().Pr_AllowOnlyForGroups) && !Globals.pr_group.ContainsKey(CallerSteamID))
            {
                if (!string.IsNullOrEmpty(Localizer["pr.not.allowed"]))
                {
                    Helper.AdvancedPrintToChat(Caller, Localizer["pr.not.allowed"]);
                }
                return HookResult.Continue;
            }

            if (!Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID))
            {
                if (!string.IsNullOrEmpty(Localizer["pr.notcreated"]))
                {
                    Helper.AdvancedPrintToChat(Caller, Localizer["pr.notcreated"]);
                }
                return HookResult.Continue;
            }

            var PrivateMenu = new ChatMenu (string.IsNullOrEmpty(Localizer["pr.menu.name"]) ? "Private Message Room Menu" : Localizer["pr.menu.name"]);
            var AllPlayers = Helper.GetAllController();
            foreach (var players in AllPlayers)
            {
                if(Caller == players ||  players.IsHLTV || players.IsBot)continue;
                var TargetPlayersNames = players.PlayerName;
                var TargetPlayersUserID = (int)players.UserId!;
                PrivateMenu.AddMenuOption(TargetPlayersNames, (Caller, option) => HandleMenu(Caller, TargetPlayersUserID));
            }

            if (!string.IsNullOrEmpty(Localizer["pr.menu.exit"]))
            {
                PrivateMenu.AddMenuOption(Localizer["pr.menu.exit"], SelectExit);
            }else if (string.IsNullOrEmpty(Localizer["pr.menu.exit"]))
            {
                PrivateMenu.AddMenuOption("Exit", SelectExit);
            }

            MenuManager.OpenChatMenu(Caller, PrivateMenu);
        }

        if(Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID) && Globals.CreatingPrivateRoom[CallerSteamID] != 0)
        {
            string[] PrInGameLeaveRoomCommands = Configs.GetConfigData().Pr_InGameLeaveRoomCommands.Split(',');
            var playersall = Helper.GetAllController();
            if (PrInGameLeaveRoomCommands.Any(cmd => cmd.Equals(trimmedMessage, StringComparison.OrdinalIgnoreCase)))
            {
                playersall.ForEach(player => 
                {
                    var steamid = player.SteamID;
                    if (Globals.CreatingPrivateRoom.ContainsKey(player.SteamID) && Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID))
                    {
                        if (Globals.CreatingPrivateRoom[player.SteamID] == Globals.CreatingPrivateRoom[CallerSteamID])
                        {
                            if (!string.IsNullOrEmpty(Localizer["pr.left"]))
                            {
                                Helper.AdvancedPrintToChat(player, Localizer["pr.left"], CallerName);
                            }
                        }
                    }
                });
                Globals.CreatingPrivateRoom.Remove(CallerSteamID);
                return HookResult.Handled;
            }
            string[] PrInGameListPlayersInRoomCommands = Configs.GetConfigData().Pr_InGameListPlayersInRoomCommands.Split(',');
            if (PrInGameListPlayersInRoomCommands.Any(cmd => cmd.Equals(trimmedMessage, StringComparison.OrdinalIgnoreCase)))
            {
                if (!string.IsNullOrEmpty(Localizer["pr.listplayers.info"]))
                {
                    Helper.AdvancedPrintToChat(Caller, Localizer["pr.listplayers.info"]);
                }
                
                playersall.ForEach(player => 
                {
                    var steamid = player.SteamID;
                    if (Globals.CreatingPrivateRoom.ContainsKey(player.SteamID) && Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID))
                    {
                        if (Globals.CreatingPrivateRoom[player.SteamID] == Globals.CreatingPrivateRoom[CallerSteamID])
                        {
                            var playersintheroom = player.PlayerName;
                            if (!string.IsNullOrEmpty(Localizer["pr.listplayers"]))
                            {
                                Helper.AdvancedPrintToChat(Caller, Localizer["pr.listplayers"], playersintheroom);
                            }
                        }
                    }
                });
                return HookResult.Handled;
            }
            
            if (info.GetArg(1).StartsWith("!") || info.GetArg(1).StartsWith("@") || info.GetArg(1).StartsWith("/") || info.GetArg(1).StartsWith(".") || info.GetArg(1).StartsWith("rtv")) return HookResult.Continue;
            
            playersall.ForEach(player => 
            {
                var steamid = player.SteamID;
                if (Globals.CreatingPrivateRoom.ContainsKey(player.SteamID))
                {
                    if (Globals.CreatingPrivateRoom[player.SteamID] == Globals.CreatingPrivateRoom[CallerSteamID])
                    {
                        if (!string.IsNullOrEmpty(Localizer["pr.message.format"]))
                        {
                            Helper.AdvancedPrintToChat(player, Localizer["pr.message.format"], CallerName, message);
                            
                            if(!string.IsNullOrEmpty(Configs.GetConfigData().AllowTheseGroupsToSpyAllPrmsInGame))
                            {
                                playersall.ForEach(playerS => 
                                {
                                    if(playerS == Caller)return;
                                    var steamidS = playerS.SteamID;
                                    if (Globals.spy_pr.ContainsKey(steamidS))
                                    {
                                        if (!string.IsNullOrEmpty(Localizer["spy.ingame.pr.message.format"]))
                                        {
                                            Helper.AdvancedPrintToChat(playerS, Localizer["spy.ingame.pr.message.format"], CallerName, message);
                                        }
                                    }
                                });
                            }
                            if(Configs.GetConfigData().Log_SendLogToText)
                            {
                                var replacerlog = Helper.ReplaceMessages(Configs.GetConfigData().Log_Pr_Message_Format, Time, Date, CallerName, string.Empty, message, CallerSteamID.ToString(), string.Empty, CallerIp.ToString(), string.Empty);
                                
                                Task.Run(() =>
                                {
                                    if(!Directory.Exists(FpathPR))
                                    {
                                        Directory.CreateDirectory(FpathPR);
                                    }

                                    if(!File.Exists(TpathPR))
                                    {
                                        using (File.Create(TpathPR)) { }
                                    }
                                    
                                    try
                                    {
                                        File.AppendAllLines(TpathPR, new[]{replacerlog});
                                    }catch
                                    {

                                    }
                                });
                                
                            }
                            var replacerlogd = Helper.ReplaceMessages(Configs.GetConfigData().Log_DiscordPrMessageFormat, Time, Date, CallerName, string.Empty, message, CallerSteamID.ToString(), string.Empty, CallerIp.ToString(), string.Empty);
                            
                            if (!string.IsNullOrEmpty(Configs.GetConfigData().Log_DiscordPrMessageFormat))
                            {
                                if(Configs.GetConfigData().Log_SendLogToDiscordOnMode == 1)
                                { 
                                    Task.Run(() =>
                                    {
                                        _ = Helper.SendToDiscordWebhookNormal(Configs.GetConfigData().Log_DiscordWebHookURL, replacerlogd);
                                    });
                                }else if(Configs.GetConfigData().Log_SendLogToDiscordOnMode == 2)
                                {
                                    Task.Run(() =>
                                    {
                                        _ = Helper.SendToDiscordWebhookNameLink(Configs.GetConfigData().Log_DiscordWebHookURL, replacerlogd, CallerSteamID.ToString(), CallerName);
                                    });
                                }else if(Configs.GetConfigData().Log_SendLogToDiscordOnMode == 3)
                                {
                                    Task.Run(() =>
                                    {
                                        _ = Helper.SendToDiscordWebhookNameLinkWithPicture(Configs.GetConfigData().Log_DiscordWebHookURL, replacerlogd, CallerSteamID.ToString(), CallerName);
                                    });
                                }
                            }
                        }
                    }
                }
            });
            return HookResult.Handled;
        }


        if(Globals.Watingforaccept.ContainsKey(CallerSteamID))
        {
            string[] PrInGameJoinRoomCommands = Configs.GetConfigData().Pr_InGameJoinRoomCommands.Split(',');
            var playersall = Helper.GetAllController();
            if (PrInGameJoinRoomCommands.Any(cmd => cmd.Equals(trimmedMessage, StringComparison.OrdinalIgnoreCase)))
            {
                if(!Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID))
                {
                    Globals.CreatingPrivateRoom.Add(CallerSteamID, 0);
                }
                if(Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID))
                {
                    Globals.CreatingPrivateRoom[CallerSteamID] = Globals.Watingforaccept[CallerSteamID];
                    playersall.ForEach(player => 
                    {
                        var steamid = player.SteamID;
                        if (Globals.CreatingPrivateRoom.ContainsKey(player.SteamID) && Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID))
                        {
                            if (Globals.CreatingPrivateRoom[player.SteamID] == Globals.CreatingPrivateRoom[CallerSteamID])
                            {
                                if (!string.IsNullOrEmpty(Localizer["pr.joined"]))
                                {
                                    Helper.AdvancedPrintToChat(player, Localizer["pr.joined"], CallerName);
                                }
                                
                            }
                        }
                    });
                    if (!string.IsNullOrEmpty(Localizer["pr.joined.info"]))
                    {
                        Helper.AdvancedPrintToChat(Caller, Localizer["pr.joined.info"]);
                    }
                }
                Globals.Watingforaccept.Remove(CallerSteamID);
                return HookResult.Handled;
            }
        }

        return HookResult.Continue;
    }
    
    private void HandleMenu(CCSPlayerController Caller, int TargetPlayersUserID)
    {
        var GetTarget = Utilities.GetPlayerFromUserid(TargetPlayersUserID);
        if(GetTarget == null || !GetTarget.IsValid)return;

        var CallerName = Caller.PlayerName;
        var CallerTeam = Caller.TeamNum;
        var CallerSteamID = Caller.SteamID;

        var TargetPlayerName = GetTarget.PlayerName;
        var TargetPlayerSteamID = GetTarget.SteamID;
        var TargetPlayerTeam = GetTarget.TeamNum;

        var GetTargerIP = GetTarget.IpAddress;
        string[] parts = GetTargerIP!.Split(':');
        string TargerIP = parts[0];
        
        

        if (Globals.CreatingPrivateRoom.ContainsKey(TargetPlayerSteamID) && Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID))
        {
            if (Globals.CreatingPrivateRoom[TargetPlayerSteamID] == Globals.CreatingPrivateRoom[CallerSteamID])
            {
                if (!string.IsNullOrEmpty(Localizer["pr.invited.in.your.room"]))
                {
                    Helper.AdvancedPrintToChat(Caller, Localizer["pr.invited.in.your.room"], TargetPlayerName);
                }
                MenuManager.CloseActiveMenu(Caller);
                return;
            }
        }

        if (Globals.CreatingPrivateRoom.ContainsKey(TargetPlayerSteamID))
        {
            if (!string.IsNullOrEmpty(Localizer["pr.already.in.private.room"]))
            {
                Helper.AdvancedPrintToChat(Caller, Localizer["pr.already.in.private.room"], TargetPlayerName);
            }
            MenuManager.CloseActiveMenu(Caller);
            return;
        }

        if(Globals.Watingforaccept.ContainsKey(TargetPlayerSteamID))
        {
            if (!string.IsNullOrEmpty(Localizer["pr.invited.already"]))
            {
                Helper.AdvancedPrintToChat(Caller, Localizer["pr.invited.already"], TargetPlayerName);
            }
            MenuManager.CloseActiveMenu(Caller);
            return;
        }
        
        if(!Globals.Watingforaccept.ContainsKey(TargetPlayerSteamID))
        {
            Globals.Watingforaccept.Add(TargetPlayerSteamID, 0);
        }
        if(Globals.Watingforaccept.ContainsKey(TargetPlayerSteamID))
        {
            Globals.Watingforaccept[TargetPlayerSteamID] = Globals.CreatingPrivateRoom[CallerSteamID];
            if (!string.IsNullOrEmpty(Localizer["pr.invited"]))
            {
                Helper.AdvancedPrintToChat(GetTarget, Localizer["pr.invited"], CallerName);
            }
        }

        if(Globals.CreatingPrivateRoom.ContainsKey(CallerSteamID) && Globals.CreatingPrivateRoom[CallerSteamID] != 0)
        {
            var playersall = Helper.GetAllController();
            playersall.ForEach(player => 
            {
                var steamid = player.SteamID;
                if (Globals.CreatingPrivateRoom.ContainsKey(player.SteamID))
                {
                    if (Globals.CreatingPrivateRoom[player.SteamID] == Globals.CreatingPrivateRoom[CallerSteamID])
                    {
                        if (!string.IsNullOrEmpty(Localizer["pr.annouce.invite"]))
                        {
                            Helper.AdvancedPrintToChat(player, Localizer["pr.annouce.invite"], CallerName, TargetPlayerName);
                        }
                    }
                }
            });
        }

        Server.NextFrame(() =>
        {
            AddTimer(Configs.GetConfigData().Pr_InviteExpiredInSec, () =>
            {
                if(GetTarget == null || !GetTarget.IsValid)return;
                if(Globals.Watingforaccept.ContainsKey(TargetPlayerSteamID))
                {
                    Globals.Watingforaccept.Remove(TargetPlayerSteamID);
                    if (!string.IsNullOrEmpty(Localizer["pr.invited.expired"]))
                    {
                        Helper.AdvancedPrintToChat(GetTarget, Localizer["pr.invited.expired"]);
                    }
                }
            }, TimerFlags.STOP_ON_MAPCHANGE);
        });

        MenuManager.CloseActiveMenu(Caller);
    }
    private void SelectExit(CCSPlayerController Caller, ChatMenuOption option)
    {
        MenuManager.CloseActiveMenu(Caller);
    }
    

    public HookResult OnEventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event == null)return HookResult.Continue;
        var player = @event.Userid;

        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV) return HookResult.Continue;
        var playerid = player.SteamID;

        if(!string.IsNullOrEmpty(Configs.GetConfigData().AllowTheseGroupsToSpyAllDmsInGame) && Helper.IsPlayerInGroupPermission(player, Configs.GetConfigData().AllowTheseGroupsToSpyAllDmsInGame))
        {
            if (!Globals.spy_dm.ContainsKey(playerid))
            {
                Globals.spy_dm.Add(playerid, true);
            }
        }
        if(!string.IsNullOrEmpty(Configs.GetConfigData().AllowTheseGroupsToSpyAllPrmsInGame) && Helper.IsPlayerInGroupPermission(player, Configs.GetConfigData().AllowTheseGroupsToSpyAllPrmsInGame))
        {
            if (!Globals.spy_pr.ContainsKey(playerid))
            {
                Globals.spy_pr.Add(playerid, true);
            }
        }

        if(!string.IsNullOrEmpty(Configs.GetConfigData().Dm_AllowOnlyForGroups) && Helper.IsPlayerInGroupPermission(player, Configs.GetConfigData().Dm_AllowOnlyForGroups))
        {
            if (!Globals.dm_group.ContainsKey(playerid))
            {
                Globals.dm_group.Add(playerid, true);
            }
        }

        if(!string.IsNullOrEmpty(Configs.GetConfigData().Pr_AllowOnlyForGroups) && Helper.IsPlayerInGroupPermission(player, Configs.GetConfigData().Pr_AllowOnlyForGroups))
        {
            if (!Globals.pr_group.ContainsKey(playerid))
            {
                Globals.pr_group.Add(playerid, true);
            }
        }

        return HookResult.Continue;
    }



    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;
        var player = @event.Userid;
        var playerid = player.SteamID;
        
        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV) return HookResult.Continue;

        Globals.spy_dm.Remove(playerid);
        Globals.spy_pr.Remove(playerid);
        Globals.dm_group.Remove(playerid);
        Globals.pr_group.Remove(playerid);
        Globals.CreatingPrivateRoom.Remove(playerid);
        Globals.Watingforaccept.Remove(playerid);
        
        return HookResult.Continue;
    }
    public void OnMapEnd()
    {
        Helper.ClearVariables();
    }
    public override void Unload(bool hotReload)
    {
        Helper.ClearVariables();
    }
}