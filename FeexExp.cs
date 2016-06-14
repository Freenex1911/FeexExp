using Rocket.API;
using Rocket.API.Collections;
using Rocket.API.Serialisation;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using Rocket.Unturned.Skills;
using System;
using UnityEngine;

namespace Freenex.FeexExp
{
    public class FeexExp : RocketPlugin<FeexExpConfiguration>
    {
        public static FeexExp Instance;

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList() {
                    {"general_not_found","Player not found."},
                    {"general_invalid_parameter","Invalid parameter."},
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
            UnturnedPlayerEvents.OnPlayerDeath += UnturnedPlayerEvents_OnPlayerDeath;
            UnturnedPlayerEvents.OnPlayerRevive += UnturnedPlayerEvents_OnPlayerRevive;
            Logger.Log("Freenex's FeexExp has been loaded!");
        }

        protected override void Unload()
        {
            UnturnedPlayerEvents.OnPlayerDeath -= UnturnedPlayerEvents_OnPlayerDeath;
            UnturnedPlayerEvents.OnPlayerRevive -= UnturnedPlayerEvents_OnPlayerRevive;
            Logger.Log("Freenex's FeexExp has been unloaded!");
        }

        void UnturnedPlayerEvents_OnPlayerDeath(UnturnedPlayer player, SDG.Unturned.EDeathCause cause, SDG.Unturned.ELimb limb, Steamworks.CSteamID murderer)
        {
            if (Instance.Configuration.Instance.EnableCustomLosePercentage)
            {
                FeexExpPlayerComponent tmpComponent = player.GetComponent<FeexExpPlayerComponent>();
                tmpComponent.Agriculture = player.GetSkillLevel(UnturnedSkill.Agriculture);
                tmpComponent.Cardio = player.GetSkillLevel(UnturnedSkill.Cardio);
                tmpComponent.Cooking = player.GetSkillLevel(UnturnedSkill.Cooking);
                tmpComponent.Crafting = player.GetSkillLevel(UnturnedSkill.Crafting);
                tmpComponent.Dexerity = player.GetSkillLevel(UnturnedSkill.Dexerity);
                tmpComponent.Diving = player.GetSkillLevel(UnturnedSkill.Diving);
                tmpComponent.Engineer = player.GetSkillLevel(UnturnedSkill.Engineer);
                tmpComponent.Exercise = player.GetSkillLevel(UnturnedSkill.Exercise);
                tmpComponent.Fishing = player.GetSkillLevel(UnturnedSkill.Fishing);
                tmpComponent.Healing = player.GetSkillLevel(UnturnedSkill.Healing);
                tmpComponent.Immunity = player.GetSkillLevel(UnturnedSkill.Immunity);
                tmpComponent.Mechanic = player.GetSkillLevel(UnturnedSkill.Mechanic);
                tmpComponent.Outdoors = player.GetSkillLevel(UnturnedSkill.Outdoors);
                tmpComponent.Overkill = player.GetSkillLevel(UnturnedSkill.Overkill);
                tmpComponent.Parkour = player.GetSkillLevel(UnturnedSkill.Parkour);
                tmpComponent.Sharpshooter = player.GetSkillLevel(UnturnedSkill.Sharpshooter);
                tmpComponent.Sneakybeaky = player.GetSkillLevel(UnturnedSkill.Sneakybeaky);
                tmpComponent.Strength = player.GetSkillLevel(UnturnedSkill.Strength);
                tmpComponent.Survival = player.GetSkillLevel(UnturnedSkill.Survival);
                tmpComponent.Toughness = player.GetSkillLevel(UnturnedSkill.Toughness);
                tmpComponent.Vitality = player.GetSkillLevel(UnturnedSkill.Vitality);
                tmpComponent.Warmblooded = player.GetSkillLevel(UnturnedSkill.Warmblooded);
            }

            UnturnedPlayer UPmurderer = UnturnedPlayer.FromCSteamID(murderer);
            try
            {
                if (player.Id == UPmurderer.Id) { return; }
            }
            catch { return; }

            foreach (Permission playerPermission in UPmurderer.GetPermissions())
            {
                if (playerPermission.Name.ToLower().Contains("exp.onkill."))
                {
                    string[] permissionExpArray = playerPermission.Name.ToLower().Replace("exp.onkill.", string.Empty).Split('.');

                    uint permissionExp;
                    uint permissionPercentage = 100;
                    bool isNumeric = uint.TryParse(permissionExpArray[0], out permissionExp);
                    if (!isNumeric) { Logger.LogError(permissionExpArray[0] + " is not numeric."); return; }

                    if (permissionExpArray.Length == 2)
                    {
                        bool isPercentageNumeric = uint.TryParse(permissionExpArray[1], out permissionPercentage);
                        if (!isPercentageNumeric) { Logger.LogError(permissionExpArray[1] + " is not numeric."); }
                    }

                    System.Random rand = new System.Random();
                    int chance = rand.Next(1, 101);

                    if (chance <= permissionPercentage)
                    {
                        UPmurderer.Experience = UPmurderer.Experience + permissionExp;
                        UnturnedChat.Say(UPmurderer, FeexExp.Instance.Translations.Instance.Translate("exp_onkill_true", player.DisplayName, permissionExp));
                    }
                    else
                    {
                        UnturnedChat.Say(UPmurderer, FeexExp.Instance.Translations.Instance.Translate("exp_onkill_false", player.DisplayName));
                    }
                }
            }
        }

        void UnturnedPlayerEvents_OnPlayerRevive(UnturnedPlayer player, Vector3 position, byte revive)
        {
            if (Instance.Configuration.Instance.EnableCustomLosePercentage)
            {
                FeexExpPlayerComponent tmpComponent = player.GetComponent<FeexExpPlayerComponent>();
                decimal losePercentage = (100 - Instance.Configuration.Instance.CustomLosePercentage) / 100;
                player.SetSkillLevel(UnturnedSkill.Agriculture, Convert.ToByte(Math.Floor(tmpComponent.Agriculture * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Cardio, Convert.ToByte(Math.Floor(tmpComponent.Cardio * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Cooking, Convert.ToByte(Math.Floor(tmpComponent.Cooking * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Crafting, Convert.ToByte(Math.Floor(tmpComponent.Crafting * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Dexerity, Convert.ToByte(Math.Floor(tmpComponent.Dexerity * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Diving, Convert.ToByte(Math.Floor(tmpComponent.Diving * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Engineer, Convert.ToByte(Math.Floor(tmpComponent.Engineer * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Exercise, Convert.ToByte(Math.Floor(tmpComponent.Exercise * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Fishing, Convert.ToByte(Math.Floor(tmpComponent.Fishing * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Healing, Convert.ToByte(Math.Floor(tmpComponent.Healing * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Immunity, Convert.ToByte(Math.Floor(tmpComponent.Immunity * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Mechanic, Convert.ToByte(Math.Floor(tmpComponent.Mechanic * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Outdoors, Convert.ToByte(Math.Floor(tmpComponent.Outdoors * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Overkill, Convert.ToByte(Math.Floor(tmpComponent.Overkill * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Parkour, Convert.ToByte(Math.Floor(tmpComponent.Parkour * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Sharpshooter, Convert.ToByte(Math.Floor(tmpComponent.Sharpshooter * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Sneakybeaky, Convert.ToByte(Math.Floor(tmpComponent.Sneakybeaky * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Strength, Convert.ToByte(Math.Floor(tmpComponent.Strength * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Survival, Convert.ToByte(Math.Floor(tmpComponent.Survival * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Toughness, Convert.ToByte(Math.Floor(tmpComponent.Toughness * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Vitality, Convert.ToByte(Math.Floor(tmpComponent.Vitality * losePercentage)));
                player.SetSkillLevel(UnturnedSkill.Warmblooded, Convert.ToByte(Math.Floor(tmpComponent.Warmblooded * losePercentage)));
            }

            foreach (Permission playerPermission in player.GetPermissions())
            {
                if (playerPermission.Name.ToLower().Contains("exp.onrevive."))
                {
                    uint permissionExp;
                    bool isNumeric = uint.TryParse(playerPermission.Name.ToLower().Replace("exp.onrevive.", string.Empty), out permissionExp);
                    if (isNumeric)
                    {
                        player.Experience = player.Experience + permissionExp;
                        UnturnedChat.Say(player, FeexExp.Instance.Translations.Instance.Translate("exp_onrevive", permissionExp));
                    }
                    else { Logger.LogError(playerPermission + " is not numeric."); }
                }
            }
        }
    }
}
