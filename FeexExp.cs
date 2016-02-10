using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Rocket.Unturned.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Freenex.FeexExp
{
    public class FeexExp : RocketPlugin<FeexExpConfiguration>
    {
        public static FeexExp Instance;
        List<objExpSaveData> listExpSaveData = new List<objExpSaveData>();

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
            if (Instance.Configuration.Instance.EnableCustomLosePercentage)
            {
                listExpSaveData.Clear();
            }
            Logger.Log("Freenex's FeexExp has been unloaded!");
        }

        void UnturnedPlayerEvents_OnPlayerDeath(Rocket.Unturned.Player.UnturnedPlayer player, SDG.Unturned.EDeathCause cause, SDG.Unturned.ELimb limb, Steamworks.CSteamID murderer)
        {
            if (Instance.Configuration.Instance.EnableCustomLosePercentage)
            {
                objExpSaveData objExpSaveDataItem = new objExpSaveData();
                objExpSaveDataItem.steamID = player.CSteamID;
                objExpSaveDataItem.Agriculture = player.GetSkillLevel(UnturnedSkill.Agriculture);
                objExpSaveDataItem.Cardio = player.GetSkillLevel(UnturnedSkill.Cardio);
                objExpSaveDataItem.Cooking = player.GetSkillLevel(UnturnedSkill.Cooking);
                objExpSaveDataItem.Crafting = player.GetSkillLevel(UnturnedSkill.Crafting);
                objExpSaveDataItem.Dexerity = player.GetSkillLevel(UnturnedSkill.Dexerity);
                objExpSaveDataItem.Diving = player.GetSkillLevel(UnturnedSkill.Diving);
                objExpSaveDataItem.Engineer = player.GetSkillLevel(UnturnedSkill.Engineer);
                objExpSaveDataItem.Exercise = player.GetSkillLevel(UnturnedSkill.Exercise);
                objExpSaveDataItem.Fishing = player.GetSkillLevel(UnturnedSkill.Fishing);
                objExpSaveDataItem.Healing = player.GetSkillLevel(UnturnedSkill.Healing);
                objExpSaveDataItem.Immunity = player.GetSkillLevel(UnturnedSkill.Immunity);
                objExpSaveDataItem.Mechanic = player.GetSkillLevel(UnturnedSkill.Mechanic);
                objExpSaveDataItem.Outdoors = player.GetSkillLevel(UnturnedSkill.Outdoors);
                objExpSaveDataItem.Overkill = player.GetSkillLevel(UnturnedSkill.Overkill);
                objExpSaveDataItem.Parkour = player.GetSkillLevel(UnturnedSkill.Parkour);
                objExpSaveDataItem.Sharpshooter = player.GetSkillLevel(UnturnedSkill.Sharpshooter);
                objExpSaveDataItem.Sneakybeaky = player.GetSkillLevel(UnturnedSkill.Sneakybeaky);
                objExpSaveDataItem.Strength = player.GetSkillLevel(UnturnedSkill.Strength);
                objExpSaveDataItem.Survival = player.GetSkillLevel(UnturnedSkill.Survival);
                objExpSaveDataItem.Toughness = player.GetSkillLevel(UnturnedSkill.Toughness);
                objExpSaveDataItem.Vitality = player.GetSkillLevel(UnturnedSkill.Vitality);
                objExpSaveDataItem.Warmblooded = player.GetSkillLevel(UnturnedSkill.Warmblooded);
                listExpSaveData.Add(objExpSaveDataItem);
            }

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

        void UnturnedPlayerEvents_OnPlayerRevive(Rocket.Unturned.Player.UnturnedPlayer player, Vector3 position, byte revive)
        {
            if (Instance.Configuration.Instance.EnableCustomLosePercentage)
            {
                objExpSaveData objExpSaveDataItem = listExpSaveData.Find(x => (x.steamID == player.CSteamID));
                if (objExpSaveDataItem != null)
                {
                    decimal losePercentage = (100 - Instance.Configuration.Instance.CustomLosePercentage) / 100;
                    player.SetSkillLevel(UnturnedSkill.Agriculture, Convert.ToByte(Math.Floor(objExpSaveDataItem.Agriculture * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Cardio, Convert.ToByte(Math.Floor(objExpSaveDataItem.Cardio * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Cooking, Convert.ToByte(Math.Floor(objExpSaveDataItem.Cooking * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Crafting, Convert.ToByte(Math.Floor(objExpSaveDataItem.Crafting * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Dexerity, Convert.ToByte(Math.Floor(objExpSaveDataItem.Dexerity * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Diving, Convert.ToByte(Math.Floor(objExpSaveDataItem.Diving * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Engineer, Convert.ToByte(Math.Floor(objExpSaveDataItem.Engineer * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Exercise, Convert.ToByte(Math.Floor(objExpSaveDataItem.Exercise * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Fishing, Convert.ToByte(Math.Floor(objExpSaveDataItem.Fishing * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Healing, Convert.ToByte(Math.Floor(objExpSaveDataItem.Healing * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Immunity, Convert.ToByte(Math.Floor(objExpSaveDataItem.Immunity * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Mechanic, Convert.ToByte(Math.Floor(objExpSaveDataItem.Mechanic * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Outdoors, Convert.ToByte(Math.Floor(objExpSaveDataItem.Outdoors * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Overkill, Convert.ToByte(Math.Floor(objExpSaveDataItem.Overkill * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Parkour, Convert.ToByte(Math.Floor(objExpSaveDataItem.Parkour * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Sharpshooter, Convert.ToByte(Math.Floor(objExpSaveDataItem.Sharpshooter * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Sneakybeaky, Convert.ToByte(Math.Floor(objExpSaveDataItem.Sneakybeaky * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Strength, Convert.ToByte(Math.Floor(objExpSaveDataItem.Strength * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Survival, Convert.ToByte(Math.Floor(objExpSaveDataItem.Survival * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Toughness, Convert.ToByte(Math.Floor(objExpSaveDataItem.Toughness * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Vitality, Convert.ToByte(Math.Floor(objExpSaveDataItem.Vitality * losePercentage)));
                    player.SetSkillLevel(UnturnedSkill.Warmblooded, Convert.ToByte(Math.Floor(objExpSaveDataItem.Warmblooded * losePercentage)));
                    listExpSaveData.Remove(objExpSaveDataItem);
                }
            }

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

    }

    class objExpSaveData
    {
        public Steamworks.CSteamID steamID;

        public byte Agriculture;
        public byte Cardio;
        public byte Cooking;
        public byte Crafting;
        public byte Dexerity;
        public byte Diving;
        public byte Engineer;
        public byte Exercise;
        public byte Fishing;
        public byte Healing;
        public byte Immunity;
        public byte Mechanic;
        public byte Outdoors;
        public byte Overkill;
        public byte Parkour;
        public byte Sharpshooter;
        public byte Sneakybeaky;
        public byte Strength;
        public byte Survival;
        public byte Toughness;
        public byte Vitality;
        public byte Warmblooded;
    }
}
