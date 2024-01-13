namespace Exiled.CustomRoles.Commands.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.CustomRoles.API;
    using Exiled.CustomRoles.API.Features;

    /// <summary>
    /// Handles the using of custom role abilities.
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public class UseAbility : ICommand
    {
        /// <inheritdoc/>
        public string Command => "ability";

        /// <inheritdoc/>
        public string[] Aliases { get; } = { "a" };

        /// <inheritdoc/>
        public string Description => "Использует спецспособность";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get((CommandSender)sender);

            var role = player.GetCustomRoles().FirstOrDefault();
            if (role == null)
            {
                response = "У вас нет спецролей со спецспособностями";
                return false;
            }

            if (arguments.Count == 0)
            {
                response = string.Format(CustomRoles.Instance!.Config.UseAbilityResponse, RoleInfo.GetAbilitiesInfo(player, role));
                return false;
            }

            var activeAbilities = new List<ActiveAbility>();
            foreach (var ability in role.CustomAbilities)
            {
                if (ability is Scp079ActiveAbility scp079ActiveAbility)
                {
                    if (!scp079ActiveAbility.IsAvailable(player))
                        continue;
                    activeAbilities.Add(scp079ActiveAbility);
                }
                else if (ability is ActiveAbility activeAbility)
                {
                    activeAbilities.Add(activeAbility);
                }
            }

            if (activeAbilities.IsEmpty())
            {
                response = "У вашей спецроли нет спецспособностей!";
                return false;
            }

            int abilityNumber = 0;
            if (arguments.Count > 0)
                if (!int.TryParse(arguments.At(0), out abilityNumber))
                {
                    response = $"{arguments.At(0)} не является числом!";
                    return false;
                }

            if (activeAbilities.Count < abilityNumber)
            {
                response = "Такая способность не существует!";
                return false;
            }

            if (!activeAbilities[abilityNumber - 1].CanUseAbility(player, out string res))
            {
                response = res;
                player.ShowHint(response, 5);
                return false;
            }

            activeAbilities[abilityNumber - 1].UseAbility(player);
            response = $"Способность {activeAbilities[abilityNumber - 1].Name} успешно использована!";
            return false;
        }
    }
}