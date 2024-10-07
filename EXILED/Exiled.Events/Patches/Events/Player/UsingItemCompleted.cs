// -----------------------------------------------------------------------
// <copyright file="UsingItemCompleted.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Player
{
#pragma warning disable SA1402
#pragma warning disable SA1600 // Elements should be documented
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection.Emit;

    using API.Features;
    using API.Features.Pools;

    using Exiled.API.Extensions;
    using Exiled.Events.EventArgs.Player;

    using HarmonyLib;

    using InventorySystem.Items.Usables;
    using Mirror;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches <see cref="UsableItemsController.Update" />
    /// Adds the <see cref="Handlers.Player.UsingItemCompleted" /> event.
    /// </summary>
    [HarmonyPatch(typeof(UsableItemsController), nameof(UsableItemsController.Update))]
    internal static class UsingItemCompleted
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            LocalBuilder consumable = generator.DeclareLocal(typeof(Consumable));

            Label continueLabel = generator.DefineLabel();
            Label skipConsumable = generator.DefineLabel();
            Label retLabel = generator.DefineLabel();

            int offset = -1;
            int index = newInstructions.FindIndex(i => i.Calls(Method(typeof(Dictionary<ReferenceHub, PlayerHandler>.Enumerator), nameof(Dictionary<ReferenceHub, PlayerHandler>.Enumerator.MoveNext)))) + offset;

            newInstructions[index].labels.Add(retLabel);

            offset = -2;
            index = newInstructions.FindIndex(x => x.Calls(Method(typeof(UsableItem), nameof(UsableItem.ServerOnUsingCompleted)))) + offset;

            newInstructions.InsertRange(index, new[]
            {
                // if (currentUsable.Item is Consumable consumable && consumable._alreadyActivated)
                //     goto continueLabel;
                new CodeInstruction(OpCodes.Ldloc_2).MoveLabelsFrom(newInstructions[index]),
                new(OpCodes.Ldfld, Field(typeof(CurrentlyUsedItem), nameof(CurrentlyUsedItem.Item))),
                new(OpCodes.Isinst, typeof(Consumable)),

                new(OpCodes.Dup),
                new(OpCodes.Stloc_S, consumable.LocalIndex),
                new(OpCodes.Brfalse_S, skipConsumable),

                new(OpCodes.Ldloc_S, consumable.LocalIndex),
                new(OpCodes.Ldfld, Field(typeof(Consumable), nameof(Consumable._alreadyActivated))),
                new(OpCodes.Brtrue_S, continueLabel),

                // Player.Get(keyValuePair.Key)
                new CodeInstruction(OpCodes.Ldloca_S, 1).WithLabels(skipConsumable),
                new(OpCodes.Call, PropertyGetter(typeof(KeyValuePair<ReferenceHub, PlayerHandler>), nameof(KeyValuePair<ReferenceHub, PlayerHandler>.Key))),
                new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),

                // currentUsable.Item
                new(OpCodes.Ldloc_2),
                new(OpCodes.Ldfld, Field(typeof(CurrentlyUsedItem), nameof(CurrentlyUsedItem.Item))),

                // UsingItemCompletedEventArgs ev = new(Player, UsableItem)
                // Handlers.Player.OnUsingItemCompleted(ev)
                new(OpCodes.Newobj, GetDeclaredConstructors(typeof(UsingItemCompletedEventArgs))[0]),
                new(OpCodes.Dup),
                new(OpCodes.Call, Method(typeof(Handlers.Player), nameof(Handlers.Player.OnUsingItemCompleted))),

                // if (ev.IsAllowed) goto continueLabel;
                new(OpCodes.Callvirt, PropertyGetter(typeof(UsingItemCompletedEventArgs), nameof(UsingItemCompletedEventArgs.IsAllowed))),
                new(OpCodes.Brtrue_S, continueLabel),

                new(OpCodes.Ldloc_S, 1),
                new(OpCodes.Call, Method(typeof(UsingItemCompleted), nameof(UnUseItem))),

                // goto ret;
                new(OpCodes.Br_S, retLabel),
                new CodeInstruction(OpCodes.Nop).WithLabels(continueLabel),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }

        internal static void UnUseItem(KeyValuePair<ReferenceHub, PlayerHandler> kvp)
        {
            if (kvp.Value.CurrentUsable.Item is not UsableItem usable)
                return;
            usable.OnUsingCancelled();
            kvp.Value.CurrentUsable = CurrentlyUsedItem.None;
            kvp.Key.inventory.connectionToClient.Send(new StatusMessage(StatusMessage.StatusType.Cancel, usable.ItemSerial), 0);
        }
    }

    /// <summary>
    /// Patches <see cref="Consumable.EquipUpdate" />
    /// Adds the <see cref="Handlers.Player.UsingItemCompleted" /> event.
    /// </summary>
    [HarmonyPatch(typeof(Consumable), nameof(Consumable.EquipUpdate))]
    internal static class ConsumableUsing
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            Label continueLabel = generator.DefineLabel();

            int offset = -1;
            int index = newInstructions.FindIndex(x => x.Calls(Method(typeof(Consumable), nameof(Consumable.ActivateEffects)))) + offset;

            newInstructions.InsertRange(index, new[]
            {
                // this.Owner
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Consumable), nameof(Consumable.Owner))),
                new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),

                // this
                new(OpCodes.Ldarg_0),

                // UsingItemCompletedEventArgs ev = new(this.Owner, this)
                // Handlers.Player.OnUsingItemCompleted(ev)
                new(OpCodes.Newobj, GetDeclaredConstructors(typeof(UsingItemCompletedEventArgs))[0]),
                new(OpCodes.Dup),
                new(OpCodes.Call, Method(typeof(Handlers.Player), nameof(Handlers.Player.OnUsingItemCompleted))),

                // if (ev.IsAllowed) goto continueLabel;
                new(OpCodes.Callvirt, PropertyGetter(typeof(UsingItemCompletedEventArgs), nameof(UsingItemCompletedEventArgs.IsAllowed))),
                new(OpCodes.Brtrue_S, continueLabel),

                // UnUseItem(this);
                // return;
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, Method(typeof(ConsumableUsing), nameof(ConsumableUsing.UnUseItem))),
                new(OpCodes.Ret),

                new CodeInstruction(OpCodes.Nop).WithLabels(continueLabel),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }

        internal static void UnUseItem(Consumable item)
        {
            if (item.Owner == null || UsableItemsController.GetHandler(item.Owner) is not PlayerHandler handler)
                return;
            item.OnUsingCancelled();
            handler.CurrentUsable = CurrentlyUsedItem.None;
            item.Owner.inventory.connectionToClient.Send(new StatusMessage(StatusMessage.StatusType.Cancel, item.ItemSerial), 0);
        }
    }
}