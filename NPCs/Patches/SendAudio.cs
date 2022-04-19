﻿// -----------------------------------------------------------------------
// <copyright file="SendAudio.cs" company="Build">
// Copyright (c) Build. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace NPCs.Patches
{
#pragma warning disable SA1118
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using HarmonyLib;
    using InventorySystem.Items.Firearms;
    using Mirror;
    using NorthwoodLib.Pools;
    using UnityEngine;
    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches <see cref="FirearmExtensions.ServerSendAudioMessage"/> to prevent the sending of messages to npcs.
    /// </summary>
    [HarmonyPatch(typeof(FirearmExtensions), nameof(FirearmExtensions.ServerSendAudioMessage))]
    internal static class SendAudio
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex(instruction => instruction.OperandIs(Method(typeof(NetworkConnection), nameof(NetworkConnection.Send)))) + 1;
            Label continueLabel = generator.DefineLabel();
            newInstructions[index].labels.Add(continueLabel);

            index = newInstructions.FindIndex(instruction => instruction.opcode == OpCodes.Ldloc_S);
            newInstructions.InsertRange(index, new[]
            {
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(Npc), nameof(Npc.Dictionary))).MoveLabelsFrom(newInstructions[index]),
                new CodeInstruction(OpCodes.Ldloc_S, 5),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(ReferenceHub), nameof(ReferenceHub.gameObject))),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(Dictionary<GameObject, Npc>), nameof(Dictionary<GameObject, Npc>.ContainsKey))),
                new CodeInstruction(OpCodes.Brtrue_S, continueLabel),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}