using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        IMyTextPanel LCD;
        bool flag = false;
        Vector3D target = new Vector3D(0, 0, 0);

        public Program()
        {
            LCD = GridTerminalSystem.GetBlockWithName("Display") as IMyTextPanel;
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

        }


        public void Debag(string text, object prog = null, bool empty = true)
        {
            IMyTextSurface debag_lcd = ((IMyTextSurface)GridTerminalSystem.GetBlockWithName("deb"));
            if (debag_lcd != null)
            {
                debag_lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                if (empty)
                {
                    debag_lcd.WriteText($"{text}: {prog}  \n", true);
                }
                else
                {
                    debag_lcd.WriteText(text);
                }
            }
        }


        public void aiming(Vector3D target, string argument)
        {
            if (argument == "start")
            {
                flag = true;
            }
            if (argument == "end")
            {
                flag = false;
            }
            Debag("", empty: false);
            Vector3D target_world = target;
            Debag("target_world", target_world);
            IMyShipController cockpit = (IMyShipController)GridTerminalSystem.GetBlockWithName("coc");
            IMyGyro gyro = (IMyGyro)GridTerminalSystem.GetBlockWithName("gyro");

            // переводим мировой вектор цели в СО кокпита
            MatrixD M_WorldToCockpit = MatrixD.Invert(cockpit.WorldMatrix);
            Debag("M_WorldToCockpit", M_WorldToCockpit);
            Vector3D target_cockpit = Vector3D.Transform(target_world, M_WorldToCockpit);
            target_cockpit = Vector3D.Normalize(target_cockpit);
            Debag("target_cockpit", target_cockpit);
            // определяем углы, на которые нужно развернуть кокпит
            double az, el;
            Vector3D.GetAzimuthAndElevation(target_cockpit, out az, out el);
            Debag("az", az);
            Debag("el", el);
            Vector3D cockpit_angles = new Vector3D(-el, -az, 0);
            Debag("cockpit_angles", cockpit_angles);
            // переводим вектор из СО кокпита в СО гироскопа
            MatrixD M_RotateCockpit2World = cockpit.WorldMatrix;
            Debag("M_RotateCockpit2World", M_RotateCockpit2World);
            MatrixD M_RotateWorld2Gyro = MatrixD.Invert(gyro.WorldMatrix);
            Debag("M_RotateWorld2Gyro", M_RotateWorld2Gyro);
            MatrixD M_Cockpit2Gyro = M_RotateCockpit2World * M_RotateWorld2Gyro;
            Debag("M_Cockpit2Gyro", M_Cockpit2Gyro);

            // вращаем без учёта смещений
            Vector3D gyro_angles = Vector3D.Rotate(cockpit_angles, M_Cockpit2Gyro);
            Debag("gyro_angles", gyro_angles);
            // применяем в порядке (Pitch, Yaw, Roll) = (Z, Y, Z)

            if (flag)
            {
                gyro.GyroOverride = true;
                gyro.Pitch = (float)gyro_angles.X;
                gyro.Yaw = (float)gyro_angles.Y;
                gyro.Roll = (float)gyro_angles.Z;
            }
            else
            {
                gyro.GyroOverride = false;

            }

        }


        public void Main(string argument, UpdateType updateSource)
        {
            aiming(target, argument);

        }
    }
}
