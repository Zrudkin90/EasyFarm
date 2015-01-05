﻿using EasyFarm.Components;
using EasyFarm.UserSettings;
using FFACETools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroLimits.XITool.Classes;
using ZeroLimits.XITool.Enums;

namespace EasyFarm.Classes
{
    public class Executor
    {
        private static MovingUnit Player;

        private FFACE FFACE;

        /// <summary>
        /// Must be set by caller. 
        /// </summary>
        public Unit Target { get; set; }

        public Executor(FFACE fface)
        {
            this.FFACE = fface;
            Player = Player ?? new MovingUnit(fface.Player.ID);
        }

        /// <summary>
        /// Executes moves without the need for a target. 
        /// </summary>
        /// <param name="actions"></param>
        public void ExecuteBuffs(IEnumerable<BattleAbility> actions)
        {
            foreach (var action in actions.ToList())
            {
                // Stop bot from running. 
                if (Player.IsMoving)
                {
                    Thread.Sleep(500);
                    FFACE.Navigator.Reset();
                }

                // Sleep for the server latency. 
                Thread.Sleep(Config.Instance.MiscSettings.CastLatency);

                // Fire the spell off. 
                FFACE.Windower.SendString(action.Ability.ToString());

                // Sleep until a spell is recastable. 
                Thread.Sleep(Config.Instance.MiscSettings.GlobalCooldown);
            }
        }

        /// <summary>
        /// Execute actions by 
        /// </summary>
        /// <param name="actions"></param>
        public void ExecuteActions(IEnumerable<BattleAbility> actions)
        {
            // Gaurd against null targets. 
            if (this.Target == null) return;

            foreach (var action in actions.ToList())
            {
                // Move to target if out of distance. 
                if (Target.Distance > action.Distance)
                {
                    // Move to unit at max buff distance. 
                    var oldTolerance = FFACE.Navigator.DistanceTolerance;
                    FFACE.Navigator.DistanceTolerance = action.Distance;
                    FFACE.Navigator.GotoNPC(Target.ID);
                    FFACE.Navigator.DistanceTolerance = action.Distance;
                }

                // Face unit
                FFACE.Navigator.FaceHeading(Target.Position);

                // Target mob if not currently targeted. 
                if (Target.ID != FFACE.Target.ID)
                {
                    FFACE.Target.SetNPCTarget(Target.ID);
                    FFACE.Windower.SendString("/ta <t>");
                }

                // Stop bot from running. 
                if (Player.IsMoving)
                {
                    Thread.Sleep(500);
                    FFACE.Navigator.Reset();
                }


                // Sleep for the server latency.                 
                Thread.Sleep(Config.Instance.MiscSettings.CastLatency);

                // Fire the spell off. 
                FFACE.Windower.SendString(action.Ability.ToString());

                // Sleep until a spell is recastable; no sleep for abilities. 
                if (!action.Ability.ActionType.Equals(ActionType.Ability))
                    Thread.Sleep(Config.Instance.MiscSettings.GlobalCooldown);
            }
        }
    }
}
