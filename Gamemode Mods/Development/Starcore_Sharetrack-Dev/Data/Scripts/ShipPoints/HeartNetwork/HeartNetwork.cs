﻿using Sandbox.ModAPI;
using SCModRepository.Gamemode_Mods.Stable.Starcore_Sharetrack.Data.Scripts.ShipPoints.MatchTimer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using klime.PointCheck;
using VRage.Game.ModAPI;
using VRageMath;

namespace Scripts.ShipPoints.HeartNetwork
{
    public class HeartNetwork
    {
        public static HeartNetwork I;

        public ushort NetworkId { get; private set; }

        public int NetworkLoadTicks = 240;
        public int TotalNetworkLoad { get; private set; } = 0;
        public Dictionary<Type, int> TypeNetworkLoad = new Dictionary<Type, int>();

        private int networkLoadUpdate = 0;

        public void LoadData(ushort networkId)
        {
            I = this;

            NetworkId = networkId;
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(NetworkId, ReceivedPacket);

            foreach (var type in PacketBase.Types)
            {
                TypeNetworkLoad.Add(type, 0);
            }
        }

        public void UnloadData()
        {
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(NetworkId, ReceivedPacket);
            I = null;
        }

        public void Update()
        {
            networkLoadUpdate--;
            if (networkLoadUpdate <= 0)
            {
                networkLoadUpdate = NetworkLoadTicks;
                TotalNetworkLoad = 0;
                foreach (var networkLoadArray in TypeNetworkLoad.Keys.ToArray())
                {
                    TotalNetworkLoad += TypeNetworkLoad[networkLoadArray];
                    TypeNetworkLoad[networkLoadArray] = 0;
                }

                TotalNetworkLoad /= (NetworkLoadTicks / 60); // Average per-second
            }
        }

        void ReceivedPacket(ushort channelId, byte[] serialized, ulong senderSteamId, bool isSenderServer)
        {
            try
            {
                PacketBase packet = MyAPIGateway.Utilities.SerializeFromBinary<PacketBase>(serialized);
                TypeNetworkLoad[packet.GetType()] += serialized.Length;
                HandlePacket(packet, senderSteamId);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        void HandlePacket(PacketBase packet, ulong senderSteamId)
        {
            packet.Received(senderSteamId);
        }





        public KeyValuePair<Type, int> HighestNetworkLoad()
        {
            Type highest = null;

            foreach (var networkLoadArray in TypeNetworkLoad)
            {
                if (highest == null || networkLoadArray.Value > TypeNetworkLoad[highest])
                {
                    highest = networkLoadArray.Key;
                }
            }

            return new KeyValuePair<Type, int>(highest, TypeNetworkLoad[highest]);
        }

        public void SendToPlayer(PacketBase packet, ulong playerSteamId, byte[] serialized = null)
        {
            RelayToClient(packet, playerSteamId, MyAPIGateway.Session?.Player?.SteamUserId ?? 0, serialized);
        }

        public void SendToEveryone(PacketBase packet, byte[] serialized = null)
        {
            RelayToClients(packet, MyAPIGateway.Session?.Player?.SteamUserId ?? 0, serialized);
        }

        public void SendToServer(PacketBase packet, byte[] serialized = null)
        {
            RelayToServer(packet, MyAPIGateway.Session?.Player?.SteamUserId ?? 0, serialized);
        }


        List<IMyPlayer> TempPlayers = new List<IMyPlayer>();
        void RelayToClients(PacketBase packet, ulong senderSteamId = 0, byte[] serialized = null)
        {
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;

            TempPlayers.Clear();
            MyAPIGateway.Players.GetPlayers(TempPlayers);

            foreach (IMyPlayer p in TempPlayers)
            {
                // skip sending to self (server player) or back to sender
                if (p.SteamUserId == MyAPIGateway.Multiplayer.ServerId || p.SteamUserId == senderSteamId)
                    continue;

                if (serialized == null) // only serialize if necessary, and only once.
                    serialized = MyAPIGateway.Utilities.SerializeToBinary(packet);

                MyAPIGateway.Multiplayer.SendMessageTo(NetworkId, serialized, p.SteamUserId);
            }

            TempPlayers.Clear();
        }

        void RelayToClient(PacketBase packet, ulong playerSteamId, ulong senderSteamId, byte[] serialized = null)
        {
            if (playerSteamId == MyAPIGateway.Multiplayer.ServerId || playerSteamId == senderSteamId)
                return;

            if (serialized == null) // only serialize if necessary, and only once.
                serialized = MyAPIGateway.Utilities.SerializeToBinary(packet);

            MyAPIGateway.Multiplayer.SendMessageTo(NetworkId, serialized, playerSteamId);
        }

        void RelayToServer(PacketBase packet, ulong senderSteamId = 0, byte[] serialized = null)
        {
            if (senderSteamId == MyAPIGateway.Multiplayer.ServerId)
                return;

            if (serialized == null) // only serialize if necessary, and only once.
                serialized = MyAPIGateway.Utilities.SerializeToBinary(packet);

            MyAPIGateway.Multiplayer.SendMessageToServer(NetworkId, serialized);
        }
    }
}
