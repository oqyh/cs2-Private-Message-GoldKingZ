# [CS2] Private-Message-GoldKingZ (1.0.0)

### Private Room Messages, Direct Messages (Vips,Admins,Spy Messages,Discord Log, Text Log)

![cs2pr](https://github.com/oqyh/cs2-Private-Message-GoldKingZ/assets/48490385/aa61606d-7f3f-4fc4-9837-2285f209fdb5)

![cs2dm](https://github.com/oqyh/cs2-Private-Message-GoldKingZ/assets/48490385/5e2598d0-6d7f-4966-9139-1891f32bfb6d)

![logs](https://github.com/oqyh/cs2-Private-Message-GoldKingZ/assets/48490385/fb54e096-cbdf-488c-ba4e-b4977fe95fb2)

![discord](https://github.com/oqyh/cs2-Private-Message-GoldKingZ/assets/48490385/b81719ef-8b9d-4f64-a818-4290ae470e56)


## .:[ Dependencies ]:.
[Metamod:Source (2.x)](https://www.sourcemm.net/downloads.php/?branch=master)

[CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/releases)

[Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json)




## .:[ Configuration ]:.

> [!CAUTION]
> Config Located In ..\addons\counterstrikesharp\plugins\Private-Message-GoldKingZ\config\config.json                                           
>

```json
{
  //Ingame Commands For Direct Messages
  "Dm_InGameCommands": "!dm,!pm",
  //Group To Allow Access Direct Messages (ex:@css/root,@css/admin,@css/vip,#css/admin,#css/vip)
  "Dm_AllowOnlyForGroups": "",
  //Group To Spy InGame Direct Messages (ex:@css/root,@css/admin,@css/vip,#css/admin,#css/vip)
  "AllowTheseGroupsToSpyAllDmsInGame": "",

  //Ingame Commands For Making Private Room Messages
  "Pr_InGameCreateRoomCommands": "!makeprivateroom,!mpr",
  //Ingame Commands To Check List Players In The Room
  "Pr_InGameListPlayersInRoomCommands": "!list,!listplayers,!players",
  //Ingame Commands To Invite Players
  "Pr_InGameInviteRoomCommands": "!inv,!invite",
  //Ingame Commands To Accept/Join Invitation 
  "Pr_InGameJoinRoomCommands": "!join,!j",
  //Ingame Commands To Leave/Quit Room
  "Pr_InGameLeaveRoomCommands": "!leave,!quit,!q",
  //After Invite Time In Sec Expired The Invitation 
  "Pr_InviteExpiredInSec": 60,
  //Group To Allow Access Making Private Room Messages (ex:@css/root,@css/admin,@css/vip,#css/admin,#css/vip)
  "Pr_AllowOnlyForGroups": "",
  //Pr_OnSayTeamOnly: True = Will Make Public Say Normal And Team Say Private Room Messages
  //Pr_OnSayTeamOnly: False = Will Make Public Say Private Room Messages And Team Say Private Room Messages
  "Pr_OnSayTeamOnly": false,
  //Group To Spy InGame Private Room Messages (ex:@css/root,@css/admin,@css/vip,#css/admin,#css/vip)
  "AllowTheseGroupsToSpyAllPrmsInGame": "",

  //==========================
  //        Format
  //==========================
  //{DATE} = Date DD-MM-YYYY
  //{TIME} = Time HH:mm:ss
  //{CALLERNAME} = Player Name Who Write Message
  //{RECEIVERNAME} = Player Name Who Receive Message
  //{MESSAGE} = What {CALLERNAME} Typed
  //{CIPADRESS} = Player SteamID Who Write ex: 76561198206086993
  //{RSTEAMID} = Player SteamID Who Receive ex: 76561198206086993
  //{CIPADRESS} = Player IpAdress Who Write ex: 127.0.0.0
  //{RIPADRESS} = Player IpAdress Who Receive ex: 127.0.0.0
  //==========================

  //Enable Or Disable Log Local
  "Log_SendLogToText": false,
  //If Log_SendLogToText Enabled How Do You Like Message Look Like
  //Direct Message
  "Log_Dm_Message_Format": "[{DATE} - {TIME}] [DM] {CALLERNAME} -> {RECEIVERNAME}: {MESSAGE}  ({CSTEAMID} - Ip: {CIPADRESS} || {RSTEAMID} - Ip: {RIPADRESS})",
  //Private Room Message
  "Log_Pr_Message_Format": "[{DATE} - {TIME}] [PR] {CALLERNAME}: {MESSAGE}  ({CSTEAMID} - Ip: {CIPADRESS})",
  //If Log_SendLogToText Enabled Auto Delete Logs If More Than X (Days) Old
  //Direct Message
  "Log_Dm_AutoDeleteLogsMoreThanXdaysOld": 0,
  //Private Room Message
  "Log_Pr_AutoDeleteLogsMoreThanXdaysOld": 0,


  //Send Log To Discord Via WebHookURL
  //Log_SendLogToDiscordOnMode (0) = Disable
  //Log_SendLogToDiscordOnMode (1) = Text Only
  //Log_SendLogToDiscordOnMode (2) = Text With + Name + Hyperlink To Steam Profile
  //Log_SendLogToDiscordOnMode (3) = Text With + Name + Hyperlink To Steam Profile + Profile Picture
  "Log_SendLogToDiscordOnMode": 0,

  //If Log_SendLogToDiscordOnMode (2) or Log_SendLogToDiscordOnMode (3) How Would You Side Color Message To Be Check (https://www.color-hex.com/) For Colors
  "Log_DiscordSideColor": "00FFFF",
  //Discord WebHook
  "Log_DiscordWebHookURL": "https://discord.com/api/webhooks/XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
  //If Log_SendLogToDiscordOnMode (1) or (2) or (3) How Do You Like Message Look Like
  //Direct Message
  "Log_DiscordDmMessageFormat": "[{DATE} - {TIME}] [DM] {CALLERNAME} -> {RECEIVERNAME}: {MESSAGE}  ({CSTEAMID} - Ip: {CIPADRESS} || {RSTEAMID} - Ip: {RIPADRESS})",
  //Private Room Message
  "Log_DiscordPrMessageFormat": "[{DATE} - {TIME}] [PR] {CALLERNAME}: {MESSAGE}  ({CSTEAMID} - Ip: {CIPADRESS})",
  //If Log_SendLogToDiscordOnMode (3) And Player Doesn't Have Profile Picture Which Picture Do You Like To Be Replaced
  "Log_DiscordUsersWithNoAvatarImage": "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b5/b5bd56c1aa4644a474a2e4972be27ef9e82e517e_full.jpg",

}
```

![colors](https://github.com/oqyh/cs2-Private-Message-GoldKingZ/assets/48490385/07b50124-01b9-454e-9dcd-f3b548304a03)

## .:[ Language ]:.



```json

```

## .:[ Change Log ]:.
```
(1.0.0)
-Initial Release
```

## .:[ Donation ]:.

If this project help you reduce time to develop, you can give me a cup of coffee :)

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://paypal.me/oQYh)
