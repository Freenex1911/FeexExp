using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using UnityEngine;

namespace Freenex.EasyExp
{
    public class CommandExp : IRocketCommand
    {
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0 || command.Length > 2)
            {
                return;
            }

            if (command.Length == 1)
            {
                if (!(caller.HasPermission("exp"))) { return; }
                uint commandExp;
                bool isNumeric = uint.TryParse(command[0], out commandExp);
                if (isNumeric)
                {
                    UnturnedPlayer player = (UnturnedPlayer)caller;
                    player.Experience = player.Experience + commandExp;
                    if (!(EasyExp.Instance.Translations.Instance.Translate("experience_self_successful") == string.Empty))
                    {
                        UnturnedChat.Say(caller, EasyExp.Instance.Translations.Instance.Translate("experience_self_successful", commandExp), Color.yellow);
                    }
                }
            }

            if (command.Length == 2)
            {
                if (!(caller.HasPermission("exp.other"))) { return; }
                uint commandExp;
                bool isNumeric = uint.TryParse(command[0], out commandExp);
                if (isNumeric)
                {
                    UnturnedPlayer player = UnturnedPlayer.FromName(command[1]);
                    if (player == null)
                    {
                        if (!(EasyExp.Instance.Translations.Instance.Translate("experience_other_caller_not_found") == string.Empty))
                        {
                            UnturnedChat.Say(caller, EasyExp.Instance.Translations.Instance.Translate("experience_other_caller_not_found"), Color.yellow);
                        }
                        return;
                    }

                    player.Experience = player.Experience + commandExp;
                    if (player.Id == caller.Id)
                    {
                        if (!(EasyExp.Instance.Translations.Instance.Translate("experience_self_successful") == string.Empty))
                        {
                            UnturnedChat.Say(caller, EasyExp.Instance.Translations.Instance.Translate("experience_self_successful", commandExp), Color.yellow);
                        }
                    }
                    else
                    {
                        if (!(EasyExp.Instance.Translations.Instance.Translate("experience_other_player_msg") == string.Empty))
                        {
                            UnturnedChat.Say(player, EasyExp.Instance.Translations.Instance.Translate("experience_other_player_msg", commandExp, caller.DisplayName), Color.yellow);
                        }
                        if (!(EasyExp.Instance.Translations.Instance.Translate("experience_other_caller_successful") == string.Empty))
                        {
                            UnturnedChat.Say(caller, EasyExp.Instance.Translations.Instance.Translate("experience_other_caller_successful", commandExp, player.DisplayName), Color.yellow);
                        }
                    }
                }
            }
        }

        public string Help
        {
            get { return "Give yourself or others Experience"; }
        }

        public string Name
        {
            get { return "exp"; }
        }

        public string Syntax
        {
            get { return "<experience> <player>"; }
        }

        public bool AllowFromConsole
        {
            get { return false; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }
        public List<string> Permissions
        {
            get
            {
                return new List<string>()
                {
                    "exp",
                    "exp.other"
                };
            }
        }
    }
}
