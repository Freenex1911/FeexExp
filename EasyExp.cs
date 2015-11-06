using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using UnityEngine;

namespace Freenex.EasyExp
{
    public class EasyExp : RocketPlugin
    {
        public static EasyExp Instance;

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList() {
                    {"experience_self_successful","You gave yourself {0} experience."},
                    {"experience_other_caller_successful","You gave {1} {0} experience."},
                    {"experience_other_caller_not_found","Player not found."},
                    {"experience_other_player_msg","{1} gave you {0} experience."},
                    {"experience_onrevive_msg","You received {0} experience."},
                    {"experience_ondeath_msg","You lost all your experience."}
                };
            }
        }

        protected override void Load()
        {
            Instance = this;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerDeath += UnturnedPlayerEvents_OnPlayerDeath;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerRevive += UnturnedPlayerEvents_OnPlayerRevive;
            Logger.Log("Freenex's EasyExp has been loaded!");
        }

        protected override void Unload()
        {
            Instance = this;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerDeath -= UnturnedPlayerEvents_OnPlayerDeath;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerRevive -= UnturnedPlayerEvents_OnPlayerRevive;
            Logger.Log("Freenex's EasyExp has been unloaded!");
        }

        void UnturnedPlayerEvents_OnPlayerRevive(Rocket.Unturned.Player.UnturnedPlayer player, Vector3 position, byte revive)
        {
            foreach (string playerPermission in player.GetPermissions())
            {
                if (playerPermission.ToLower().Contains("exp.onrevive."))
                {
                    uint configExp;
                    bool isNumeric = uint.TryParse(playerPermission.Replace("exp.onrevive.", string.Empty), out configExp);
                    if (isNumeric)
                    {
                        player.Experience = player.Experience + configExp;
                        if (!(EasyExp.Instance.Translations.Instance.Translate("experience_onrevive_msg") == string.Empty))
                        {
                            UnturnedChat.Say(player, EasyExp.Instance.Translations.Instance.Translate("experience_onrevive_msg", configExp), Color.yellow);
                        }
                    }
                    else { Logger.LogError(playerPermission + " is not numeric."); }
                }
            }
        }

        void UnturnedPlayerEvents_OnPlayerDeath(Rocket.Unturned.Player.UnturnedPlayer player, SDG.Unturned.EDeathCause cause, SDG.Unturned.ELimb limb, Steamworks.CSteamID murderer)
        {
            if (player.HasPermission("exp.deleteondeath") && (!(player.IsAdmin)))
            {
                player.Experience = 0;
                if (!(EasyExp.Instance.Translations.Instance.Translate("experience_ondeath_msg") == string.Empty))
                {
                    UnturnedChat.Say(player, EasyExp.Instance.Translations.Instance.Translate("experience_ondeath_msg"), Color.yellow);
                }
            }
        }
    }
}
