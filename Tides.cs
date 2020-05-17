//#define DEBUG
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Tides", "RFC1920", "1.0.3")]
    [Description("Standard tidal event during the Rust day")]
    class Tides : RustPlugin
    {
        #region vars
        [PluginReference]
        Plugin GUIAnnouncements;

        private bool globalToggle = true;
        private bool oceanUp = false;
        private bool oceanDn = false;
        private bool startup = true;
        private Timer timeCheck;
        private float currentLevel = 0f;

        string BannerColor = "Blue";
        string TextColor = "Yellow";

        private ConfigData configData;
        private const string permOceanLevel = "tides.use";
        #endregion

        #region Message
        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);
        private void Message(IPlayer player, string key, params object[] args) => player.Reply(Lang(key, player.Id, args));
        #endregion

        #region init
        void Init()
        {
            AddCovalenceCommand("ocean", "cmdOceanLevel");
            permission.RegisterPermission(permOceanLevel, this);

            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["notauthorized"] = "You don't have permission to use this command.",
                ["setlevel"] = "Set ocean level to {0}!",
                ["current"] = "Ocean level currently {0}.",
                ["hightide"] = "High tide...",
                ["lowtide"]  = "Low tide..."
            }, this);
            LoadConfig();
        }

        void OnServerInitialized()
        {
            CheckCurrentTime();
        }
        #endregion

        #region Main
        private void CheckCurrentTime()
        {
            if(globalToggle)
            {
                try
                {
                    float time = TOD_Sky.Instance.Cycle.Hour;
                    if(time >= configData.Sunset || (time >= 0 && time < configData.Sunrise))
                    {
                        if(!oceanUp)
                        {
                            SetOceanLevel(true);
                        }
                    }
                    else if(time >= configData.Sunrise && time < configData.Sunset)
                    {
                        if(!oceanDn)
                        {
                            SetOceanLevel(false);
                        }
                    }
                }
                catch {}
            }
            startup = false;
            timeCheck = timer.Once(configData.speed, CheckCurrentTime);
        }

        [Command("ocean")]
        void cmdOceanLevel(IPlayer player, string command, string[] args)
        {
            if(!player.HasPermission(permOceanLevel)) { Message(player, "notauthorized"); return; }
            if(args.Length > 0)
            {
                if(args.Length > 1)
                {
                    if(args[1] == "force" || args[1] == "fixed")
                    {
                        globalToggle = false;
                    }
                    else if(args[1] == "auto")
                    {
                        globalToggle = true;
                    }
                }
                if(args[0] == "reset")
                {
                    globalToggle = true;
                }
                else if(args[0] == "check")
                {
                    Message(player, "current", currentLevel.ToString());
                }
                else if(float.Parse(args[0]) > -1)
                {
                    ConsoleSystem.Run(ConsoleSystem.Option.Server.Quiet(), "env.oceanlevel " + args[0]);
                    Message(player, "setlevel", args[0]);
                }
            }
        }

        private void SetOceanLevel(bool up = true)
        {
            if(up) currentLevel += configData.increment;
            else   currentLevel -= configData.increment;

            if(currentLevel >= configData.maxLevel)
            {
                oceanUp = true;
                currentLevel = configData.maxLevel;
                if(!startup) MessageToAll("hightide");
                return;
            }
            else if(currentLevel <= configData.minLevel)
            {
                oceanDn = true;
                currentLevel = configData.minLevel;
                if(!startup) MessageToAll("lowtide");
                return;
            }
            else
            {
                oceanUp = false;
                oceanDn = false;
            }
#if DEBUG
            Puts($"Setting ocean level to {currentLevel.ToString()}");
#endif
            ConsoleSystem.Run(ConsoleSystem.Option.Server.Quiet(), "env.oceanlevel " + currentLevel.ToString());
        }
        #endregion

        #region config
        class ConfigData
        {
            [JsonProperty(PropertyName = "Sunrise")]
            public float Sunrise { get; set; }

            [JsonProperty(PropertyName = "Sunset")]
            public float Sunset { get; set; }

            [JsonProperty(PropertyName = "increment")]
            public float increment { get; set; }

            [JsonProperty(PropertyName = "speed")]
            public float speed { get; set; }

            [JsonProperty(PropertyName = "maxLevel")]
            public float maxLevel { get; set; }

            [JsonProperty(PropertyName = "minLevel")]
            public float minLevel { get; set; }

            [JsonProperty(PropertyName = "UseMessageBroadcast")]
            public bool UseMessageBroadcast { get; set; }

            [JsonProperty(PropertyName = "UseGUIAnnouncements")]
            public bool UseGUIAnnouncements { get; set; }

            public VersionNumber Version { get; set; }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            configData = Config.ReadObject<ConfigData>();

            if(configData.Version < Version)
            {
                configData.Version = Version;
            }

            Config.WriteObject(configData, true);
        }

        protected override void LoadDefaultConfig()
        {
            configData = new ConfigData
            {
                Sunrise = 7f,
                Sunset = 18f,
                increment = 0.005f,
                speed = 1f,
                maxLevel = 3f,
                minLevel = 0f,
                UseMessageBroadcast = false,
                UseGUIAnnouncements = false,
                Version = Version
            };
        }

        protected override void SaveConfig() => Config.WriteObject(configData, true);

        void MessageToAll(string key)
        {
            if(!configData.UseMessageBroadcast && !configData.UseGUIAnnouncements) return;
            foreach(var player in BasePlayer.activePlayerList)
            {
                if(configData.UseMessageBroadcast)
                {
                    SendReply(player, lang.GetMessage(key, this, player.UserIDString));
                }
                if(GUIAnnouncements && configData.UseGUIAnnouncements)
                {
                    GUIAnnouncements?.Call("CreateAnnouncement", lang.GetMessage(key, this, player.UserIDString), BannerColor, TextColor, player);
                }
            }
        }
        #endregion
    }
}
