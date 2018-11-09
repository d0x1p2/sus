﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SUS.Shared.Utility;
using SUS.Shared.Objects.Mobiles;
using SUS.Shared.Objects;

namespace SUS.Shared.Objects
{
    [Flags, Serializable]
    public enum Types
    {
        Town = 1,
        Dungeon = 2,
        OpenWorld = 4,
        PvP = 8
    };

    [Flags, Serializable]
    public enum Locations
    {
        None          = 0x00000000,
        Moongate      = 0x00000001,

        Unused1       = 0x00000002,

        Britain       = 0x00000004,
        BuccaneersDen = 0x00000008,
        Cove          = 0x00000010,
        Minoc         = 0x00000020,
        SkaraBrae     = 0x00000040,
        Trinsic       = 0x00000080,
        Vesper        = 0x00000100,
        Yew           = 0x00000200,

        Unused2       = 0x00000400,

        Destard       = 0x00000800,
        Despise       = 0x00001000,
        Covetous      = 0x00002000,
        Shame         = 0x00004000,
        Wind          = 0x00008000,
        Wrong         = 0x00010000,

        Unused3       = 0x00020000,
        Unused4       = 0x00040000,

        SolenHive     = 0x00080000,
        OrcCaves      = 0x00100000,

        Unused5       = 0x00200000,
        Unused6       = 0x00400000,
        Unused7       = 0x00800000,

        Graveyard     = 0x01000000,
        Sewers        = 0x02000000,
        Swamp         = 0x04000000,
        Wilderness    = 0x08000000,

        Unused8       = 0x10000000,
        Unused9       = 0x20000000,
        Unused10      = 0x40000000,

        Basic         = Britain | Graveyard | Sewers | Wilderness
    }

    [Serializable]
    public class Node
    {
        public int ID;
        public string Name = string.Empty;
        public string Description = string.Empty;
        public bool isSpawnable { get; private set; } = false;

        public HashSet<Node> Connections = new HashSet<Node>();
        public HashSet<Mobile> Mobiles = new HashSet<Mobile>();

        protected Types Type;
        protected Locations Location;

        #region Constructors
        public Node(Types type, Locations location, string description)
        {
            this.Location = location;
            this.ID = (int)location;
            this.Name = Enum.GetName(typeof(Locations), location);
            this.Type = type;
            this.Description = description;

            if ((type & Types.Dungeon) == Types.Dungeon)
                isSpawnable = true;
            else if ((type & Types.OpenWorld) == Types.OpenWorld)
                isSpawnable = true;
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            Node node = obj as Node;
            return this.ID == node.ID && (int)this.Location == (int)node.Location && (int)this.Type == (int)node.Type;
        }

        public override int GetHashCode()
        {
            int hash = 37;
            hash += this.ID.GetHashCode();
            hash *= 397;
            hash += this.Location.GetHashCode();
            hash *= 397;
            hash += this.Type.GetHashCode();
            return hash *= 397;
        }
        #endregion

        public byte[] ToByte()
        {
            return Utility.Network.Serialize(this);
        }

        #region Updates and Getters
        /// <summary>
        ///     Adds either a Player or NPC. Performs and update if the mobile already exists.
        /// </summary>
        /// <param name="mobile">Mobile to be added.</param>
        /// <returns>Succcess (true), or Failure (false)</returns>
        public bool AddMobile(Mobile mobile)
        {
            if (this.Mobiles.Contains(mobile))
                this.Mobiles.Remove(mobile);
            return this.Mobiles.Add(mobile);
        }

        /// <summary>
        ///     Removes the mobile from the correct list (NPCs or Players)
        /// </summary>
        /// <param name="mobile">Mobile to remove.</param>
        /// <returns>Number of elements removed.</returns>
        public bool RemoveMobile(Mobile mobile)
        {
            return this.Mobiles.Remove(mobile);
        }

        public bool HasMobile(Mobile mobile)
        {
            return this.Mobiles.Contains(mobile);
        }

        /// <summary>
        ///     First attempts to remove the mobile, if it fails- it returns early. If removing succeeds, it re-adds the mobile.
        /// </summary>
        /// <param name="mobile">Mobile to be updated.</param>
        /// <returns>Succcess (true), or Failure (false)</returns>
        public bool UpdateMobile(Mobile mobile)
        {   // Remove the mobile, if there is none that are removed- return early, else just readd the new.
            if (this.RemoveMobile(mobile) == false)
                return false;
            return AddMobile(mobile);
        }

        public void AddConnection(ref Node node)
        {
            node.Connections.Add(this);
            Connections.Add(node);
        }

        /// <summary>
        ///     Retrieves the internal Location.
        /// </summary>
        /// <returns></returns>
        public Locations GetLocation()
        {
            return this.Location;
        }

        /// <summary>
        ///     Nulls out lists to reduce bandwidth when sending data.
        /// </summary>
        public void Clean()
        {
            this.Mobiles = null;
        }
        #endregion
    }
}
