﻿// -----------------------------------------------------------------------
// <copyright file="Npc.cs" company="Build">
// Copyright (c) Build. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace NPCs
{
    using System.Collections.Generic;
    using Exiled.API.Features;
    using Mirror;
    using UnityEngine;

    /// <summary>
    /// Represents the core of an npc.
    /// </summary>
    public class Npc
    {
        private ItemType heldItem;
        private bool isSpawned;

        /// <summary>
        /// Initializes a new instance of the <see cref="Npc"/> class.
        /// </summary>
        /// <param name="roleType">The role of the npc.</param>
        /// <param name="name">The name of the npc.</param>
        /// <param name="scale">The size of the npc.</param>
        public Npc(RoleType roleType, string name, Vector3 scale)
        {
            GameObject = Object.Instantiate(NetworkManager.singleton.playerPrefab);
            Dictionary.Add(GameObject, this);

            ReferenceHub = GameObject.GetComponent<ReferenceHub>();

            GameObject.transform.localScale = scale;

            ReferenceHub.queryProcessor.PlayerId = 9999;
            ReferenceHub.queryProcessor.NetworkPlayerId = 9999;
            ReferenceHub.queryProcessor._ipAddress = "127.0.0.WAN";

            ReferenceHub.characterClassManager.CurClass = roleType;
            ReferenceHub.playerStats.StatModules[0].CurValue = 100;
            ReferenceHub.nicknameSync.Network_myNickSync = name;

            PlayerManager.AddPlayer(GameObject, CustomNetworkManager.slots);

            Player = new Player(GameObject);
            Player.SessionVariables.Add("IsNPC", true);
            Player.Dictionary.Add(GameObject, Player);
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> containing all <see cref="Npc"/>'s on the server.
        /// </summary>
        public static Dictionary<GameObject, Npc> Dictionary { get; } = new();

        /// <summary>
        /// Gets the attached <see cref="UnityEngine.GameObject"/>.
        /// </summary>
        public GameObject GameObject { get; }

        /// <summary>
        /// Gets the attached ReferenceHub component.
        /// </summary>
        public ReferenceHub ReferenceHub { get; }

        /// <summary>
        /// Gets the created <see cref="Exiled.API.Features.Player"/> to represent the npc.
        /// </summary>
        public Player Player { get; }

        /// <summary>
        /// Gets or sets the npc's position.
        /// </summary>
        public Vector3 Position
        {
            get => Player.Position;
            set => ReferenceHub.playerMovementSync.OverridePosition(value, 0f, true);
        }

        /// <summary>
        /// Gets or sets the NPC's held item.
        /// </summary>
        public ItemType HeldItem
        {
            get => heldItem;
            set
            {
                ReferenceHub.inventory.NetworkCurItem = new InventorySystem.Items.ItemIdentifier(value, 0);
                heldItem = value;
            }
        }

        /// <summary>
        /// Gets or sets the npc's name.
        /// </summary>
        public string Name
        {
            get => ReferenceHub.nicknameSync.Network_myNickSync;
            set
            {
                ReferenceHub.nicknameSync.Network_myNickSync = value;
                Respawn();
            }
        }

        /// <summary>
        /// Gets or sets the NPC's role.
        /// </summary>
        public RoleType Role
        {
            get => Player.Role.Type;
            set
            {
                ReferenceHub.characterClassManager.CurClass = value;
                Respawn();
            }
        }

        /// <summary>
        /// Gets or sets the NPC's scale.
        /// </summary>
        public Vector3 Scale
        {
            get => Player.Scale;
            set
            {
                GameObject.transform.localScale = value;
                Respawn();
            }
        }

        /// <summary>
        /// Spawns the NPC.
        /// </summary>
        public void Spawn()
        {
            if (isSpawned)
                return;

            NetworkServer.Spawn(GameObject);
            isSpawned = true;
        }

        /// <summary>
        /// Despawns the NPC.
        /// </summary>
        public void Despawn()
        {
            if (!isSpawned)
                return;

            NetworkServer.UnSpawn(GameObject);
            isSpawned = false;
        }

        /// <summary>
        /// Destroys the fake player.
        /// </summary>
        public void Destroy()
        {
            Dictionary.Remove(GameObject);
            PlayerManager.RemovePlayer(GameObject, CustomNetworkManager.slots);
            NetworkServer.UnSpawn(GameObject);
            Object.Destroy(GameObject);
        }

        private void Respawn()
        {
            Despawn();
            Spawn();
        }
    }
}