using System.Text.Json;
using System.Text.Json.Serialization;

namespace Private_Message_GoldKingZ.Config
{
    public static class Configs
    {
        public static class Shared {
            public static string? CookiesFolderPath { get; set; }
        }
        
        private static readonly string ConfigDirectoryName = "config";
        private static readonly string ConfigFileName = "config.json";
        private static string? _configFilePath;
        private static ConfigData? _configData;

        private static readonly JsonSerializerOptions SerializationOptions = new()
        {
            Converters =
            {
                new JsonStringEnumConverter()
            },
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };

        public static bool IsLoaded()
        {
            return _configData is not null;
        }

        public static ConfigData GetConfigData()
        {
            if (_configData is null)
            {
                throw new Exception("Config not yet loaded.");
            }
            
            return _configData;
        }

        public static ConfigData Load(string modulePath)
        {
            var configFileDirectory = Path.Combine(modulePath, ConfigDirectoryName);
            if(!Directory.Exists(configFileDirectory))
            {
                Directory.CreateDirectory(configFileDirectory);
            }

            _configFilePath = Path.Combine(configFileDirectory, ConfigFileName);
            if (File.Exists(_configFilePath))
            {
                _configData = JsonSerializer.Deserialize<ConfigData>(File.ReadAllText(_configFilePath), SerializationOptions);
            }
            else
            {
                _configData = new ConfigData();
            }

            if (_configData is null)
            {
                throw new Exception("Failed to load configs.");
            }

            SaveConfigData(_configData);

            return _configData;
        }

        private static void SaveConfigData(ConfigData configData)
        {
            if (_configFilePath is null)
            {
                throw new Exception("Config not yet loaded.");
            }

            File.WriteAllText(_configFilePath, JsonSerializer.Serialize(configData, SerializationOptions));
        }

        public class ConfigData
        {
            public string Dm_InGameCommands { get; set; }
            public string Dm_AllowOnlyForGroups { get; set; }
            public string AllowTheseGroupsToSpyAllDmsInGame { get; set; }
            public string empty { get; set; }

            public string Pr_InGameCreateRoomCommands { get; set; }
            public string Pr_InGameListPlayersInRoomCommands { get; set; }
            public string Pr_InGameInviteRoomCommands { get; set; }
            public string Pr_InGameJoinRoomCommands { get; set; }
            public string Pr_InGameLeaveRoomCommands { get; set; }
            public float Pr_InviteExpiredInSec { get; set; }
            public string Pr_AllowOnlyForGroups { get; set; }
            public bool Pr_OnSayTeamOnly { get; set; }
            public string AllowTheseGroupsToSpyAllPrmsInGame { get; set; }
            public string empty2 { get; set; }
            public bool Log_SendLogToText { get; set; }
            public string Log_Dm_Message_Format { get; set; }
            public string Log_Pr_Message_Format { get; set; }
            public int Log_Dm_AutoDeleteLogsMoreThanXdaysOld { get; set; }
            public int Log_Pr_AutoDeleteLogsMoreThanXdaysOld { get; set; }
            private int _Log_SendLogToDiscordOnMode;
            public int Log_SendLogToDiscordOnMode
            {
                get => _Log_SendLogToDiscordOnMode;
                set
                {
                    _Log_SendLogToDiscordOnMode = value;
                    if (_Log_SendLogToDiscordOnMode < 0 || _Log_SendLogToDiscordOnMode > 3)
                    {
                        Log_SendLogToDiscordOnMode = 0;
                        Console.WriteLine("|||||||||||||||||||||||||||||||||||||||||||||||| I N V A L I D ||||||||||||||||||||||||||||||||||||||||||||||||");
                        Console.WriteLine("[Private-Message-GoldKingZ] Log_SendLogToDiscordOnMode: is invalid, setting to default value (0) Please Choose 0 or 1 or 2 or 3.");
                        Console.WriteLine("[Private-Message-GoldKingZ] Log_SendLogToDiscordOnMode (0) = Disable");
                        Console.WriteLine("[Private-Message-GoldKingZ] Log_SendLogToDiscordOnMode (1) = Text Only");
                        Console.WriteLine("[Private-Message-GoldKingZ] Log_SendLogToDiscordOnMode (2) = Text With + Name + Hyperlink To Steam Profile");
                        Console.WriteLine("[Private-Message-GoldKingZ] Log_SendLogToDiscordOnMode (3) = Text With + Name + Hyperlink To Steam Profile + Profile Picture");
                        Console.WriteLine("|||||||||||||||||||||||||||||||||||||||||||||||| I N V A L I D ||||||||||||||||||||||||||||||||||||||||||||||||");
                    }
                }
            }
            private string? _Log_DiscordSideColor;
            public string Log_DiscordSideColor
            {
                get => _Log_DiscordSideColor!;
                set
                {
                    _Log_DiscordSideColor = value;
                    if (_Log_DiscordSideColor.StartsWith("#"))
                    {
                        Log_DiscordSideColor = _Log_DiscordSideColor.Substring(1);
                    }
                }
            }
            public string Log_DiscordWebHookURL { get; set; }
            public string Log_DiscordDmMessageFormat { get; set; }
            public string Log_DiscordPrMessageFormat { get; set; }
            public string Log_DiscordUsersWithNoAvatarImage { get; set; }
            public string empty3 { get; set; }
            public string Information_For_You_Dont_Delete_it { get; set; }
            public ConfigData()
            {
                Dm_InGameCommands = "!dm,!pm";
                Dm_AllowOnlyForGroups = "";
                AllowTheseGroupsToSpyAllDmsInGame = "";
                empty = "-----------------------------------------------------------------------------------";
                Pr_InGameCreateRoomCommands = "!makeprivateroom,!mpr";
                Pr_InGameListPlayersInRoomCommands = "!list,!listplayers,!players";
                Pr_InGameInviteRoomCommands = "!inv,!invite";
                Pr_InGameJoinRoomCommands = "!join,!j";
                Pr_InGameLeaveRoomCommands = "!leave,!quit,!q";
                Pr_InviteExpiredInSec = 60;
                Pr_AllowOnlyForGroups = "";
                Pr_OnSayTeamOnly = false;
                AllowTheseGroupsToSpyAllPrmsInGame = "";
                empty2 = "-----------------------------------------------------------------------------------";
                Log_SendLogToText = false;
                Log_Dm_Message_Format = "[{DATE} - {TIME}] [DM] {CALLERNAME} -> {RECEIVERNAME}: {MESSAGE}  ({CSTEAMID} - Ip: {CIPADRESS} || {RSTEAMID} - Ip: {RIPADRESS})";
                Log_Pr_Message_Format = "[{DATE} - {TIME}] [PR] {CALLERNAME}: {MESSAGE}  ({CSTEAMID} - Ip: {CIPADRESS})";
                Log_Dm_AutoDeleteLogsMoreThanXdaysOld = 0;
                Log_Pr_AutoDeleteLogsMoreThanXdaysOld = 0;
                Log_SendLogToDiscordOnMode = 0;
                Log_DiscordSideColor = "00FFFF";
                Log_DiscordWebHookURL = "https://discord.com/api/webhooks/XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
                Log_DiscordDmMessageFormat = "[{DATE} - {TIME}] [DM] {CALLERNAME} -> {RECEIVERNAME}: {MESSAGE}  ({CSTEAMID} - Ip: {CIPADRESS} || {RSTEAMID} - Ip: {RIPADRESS})";
                Log_DiscordPrMessageFormat = "[{DATE} - {TIME}] [PR] {CALLERNAME}: {MESSAGE}  ({CSTEAMID} - Ip: {CIPADRESS})";
                Log_DiscordUsersWithNoAvatarImage = "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b5/b5bd56c1aa4644a474a2e4972be27ef9e82e517e_full.jpg";
                empty3 = "-----------------------------------------------------------------------------------";
                Information_For_You_Dont_Delete_it = " Vist  [https://github.com/oqyh/cs2-Private-Message-GoldKingZ/tree/main?tab=readme-ov-file#-configuration-] To Understand All Above ";
            }
        }
    }
}