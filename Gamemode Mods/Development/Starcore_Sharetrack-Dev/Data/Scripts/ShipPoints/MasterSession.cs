﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using klime.PointCheck;
using Math0424.Networking;
using Sandbox.ModAPI;
using Scripts.ShipPoints.HeartNetwork;
using SENetworkAPI;
using ShipPoints.Commands;
using VRage.Game.Components;

namespace SCModRepository_Dev.Gamemode_Mods.Development.Starcore_Sharetrack_Dev.Data.Scripts.ShipPoints
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    internal class MasterSession : MySessionComponentBase
    {
        public static MasterSession I;

        public const ushort ComId = 42511;
        public const string Keyword = "/debug";
        public const string DisplayName = "Debug";

        private readonly PointCheck _pointCheck = new PointCheck();

        public override void LoadData()
        {
            I = this;

            try
            {
                MyNetworkHandler.Init();
                if (!NetworkApi.IsInitialized)
                    NetworkApi.Init(ComId, DisplayName, Keyword);
                HeartNetwork.I = new HeartNetwork();
                HeartNetwork.I.LoadData(42521);
                CommandHandler.Init();
                TrackingManager.Init();
                _pointCheck.Init();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        protected override void UnloadData()
        {
            try
            {
                _pointCheck.Close();
                TrackingManager.Close();
                CommandHandler.Close();
                HeartNetwork.I.UnloadData();
                MyNetworkHandler.Static?.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            I = null;
        }

        public override void UpdateAfterSimulation()
        {
            try
            {
                HeartNetwork.I.Update();
                TrackingManager.UpdateAfterSimulation();
                _pointCheck.UpdateAfterSimulation();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public override void Draw()
        {
            try
            {
                _pointCheck.Draw();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public override void HandleInput()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}
