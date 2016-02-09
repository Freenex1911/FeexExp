using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using UnityEngine;

namespace Freenex.FeexExp
{
    public class FeexExp : RocketPlugin
    {
        public static FeexExp Instance;

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList() {
                    {"exp_general_not_found","Player not found."},
                    {"exp_general_invalid_parameter","Invalid parameter."},
                    {"exp_self","You gave yourself {0} experience."},
                    {"exp_give_player","{1} gave you {0} experience."},
                    {"exp_give_caller","You gave {1} {0} experience."},
                    {"exp_give_player_console","You've got {0} experience."},
                    {"exp_transfer_player","{1} gave you {0} experience."},
                    {"exp_transfer_caller","You gave {1} {0} experience."},
                    {"exp_transfer_not_enough","You don't have {0} experience."},
                    {"exp_onrevive","You received {0} experience."},
                    {"exp_onkill_true","You killed {0} and received {1} experience."},
                    {"exp_onkill_false","You killed {0} but received no experience."}
                };
            }
        }

        protected override void Load()
        {
            Instance = this;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerDeath += UnturnedPlayerEvents_OnPlayerDeath;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerRevive += UnturnedPlayerEvents_OnPlayerRevive;
            Logger.Log("Freenex's FeexExp has been loaded!");
        }

        protected override void Unload()
        {
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerDeath -= UnturnedPlayerEvents_OnPlayerDeath;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerRevive -= UnturnedPlayerEvents_OnPlayerRevive;
            Logger.Log("Freenex's FeexExp has been unloaded!");
        }

        void UnturnedPlayerEvents_OnPlayerRevive(Rocket.Unturned.Player.UnturnedPlayer player, Vector3 position, byte revive)
        {
            foreach (Rocket.API.Serialisation.Permission playerPermission in player.GetPermissions())
            {
                if (playerPermission.Name.ToLower().Contains("exp.onrevive."))
                {
                    uint permissionExp;
                    bool isNumeric = uint.TryParse(playerPermission.Name.ToLower().Replace("exp.onrevive.", string.Empty), out permissionExp);
                    if (isNumeric)
                    {
                        player.Experience = player.Experience + permissionExp;
                        if (FeexExp.Instance.Translations.Instance.Translate("exp_onrevive") != "exp_onrevive")
                        {
                            UnturnedChat.Say(player, FeexExp.Instance.Translations.Instance.Translate("exp_onrevive", permissionExp));
                        }
                    }
                    else { Logger.LogError(playerPermission + " is not numeric."); }
                }
            }
        }

        void UnturnedPlayerEvents_OnPlayerDeath(Rocket.Unturned.Player.UnturnedPlayer player, SDG.Unturned.EDeathCause cause, SDG.Unturned.ELimb limb, Steamworks.CSteamID murderer)
        {
            UnturnedPlayer UPmurderer = UnturnedPlayer.FromCSteamID(murderer);
            try
            {
                if (player.Id == UPmurderer.Id) { return; }
            }
            catch { return; }

            foreach (Rocket.API.Serialisation.Permission playerPermission in UPmurderer.GetPermissions())
            {
                if (playerPermission.Name.ToLower().Contains("exp.onkill."))
                {
                    string[] OnKillPermission = playerPermission.Name.ToLower().Replace("exp.onkill.", string.Empty).Split('.');

                    uint OnKillExperience;
                    uint OnKillPercentage = 100;
                    bool isExperienceNumeric = uint.TryParse(OnKillPermission[0], out OnKillExperience);
                    if (!isExperienceNumeric) { Logger.LogError(OnKillPermission[0] + " is not numeric."); return; }

                    if (OnKillPermission.Length == 2)
                    {
                        bool isPercentageNumeric = uint.TryParse(OnKillPermission[1], out OnKillPercentage);
                        if (!isPercentageNumeric) { Logger.LogError(OnKillPermission[1] + " is not numeric."); }
                    }

                    System.Random rand = new System.Random();
                    int chance = rand.Next(1, 101);

                    if (chance <= OnKillPercentage)
                    {
                        UPmurderer.Experience = UPmurderer.Experience + OnKillExperience;
                        if (FeexExp.Instance.Translations.Instance.Translate("exp_onkill_true") != "exp_onkill_true")
                        {
                            UnturnedChat.Say(UPmurderer, FeexExp.Instance.Translations.Instance.Translate("exp_onkill_true", player.DisplayName, OnKillExperience));
                        }
                    }
                    else
                    {
                        if (FeexExp.Instance.Translations.Instance.Translate("exp_onkill_false") != "exp_onkill_false")
                        {
                            UnturnedChat.Say(UPmurderer, FeexExp.Instance.Translations.Instance.Translate("exp_onkill_false", player.DisplayName));
                        }
                    }
                }
            }
        }
    }
}
