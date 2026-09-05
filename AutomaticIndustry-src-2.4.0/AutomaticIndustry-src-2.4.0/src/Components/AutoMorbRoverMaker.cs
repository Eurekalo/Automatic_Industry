// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using AutoMachineRebuilt.Automation;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Automates the Morb Rover Maker (Biobot Builder).
    /// When the Biobot is ready (crafting progress == 1 and germs collected >= required),
    /// the building transitions to doctor.needed state for a duplicant release chore.
    /// This controller automatically triggers the release and spawns the Biobot,
    /// cancelling the Duplicant chore.
    /// </summary>
    public sealed class AutoMorbRoverMaker : KMonoBehaviour, ISim1000ms
    {
        private MorbRoverMaker.Instance smi;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            smi = gameObject.GetSMI<MorbRoverMaker.Instance>();
        }

        public void Sim1000ms(float dt)
        {
            if (!AutoMachineOptions.IsEnabledFor(gameObject, "MORBROVERMAKER") &&
                !AutoMachineOptions.IsEnabledFor(gameObject, "MorbRoverMaker"))
            {
                return;
            }

            SafeInvoke.Try("Biobot Builder automation", delegate
            {
                if (smi == null || !smi.IsRunning())
                {
                    return;
                }

                // Check if the rover is ready for release (in doctor states or ready)
                if (smi.RoverDevelopment_Progress >= 1f &&
                    smi.MorbDevelopment_Progress >= 1f)
                {
                    smi.CancelWorkChore_ReleaseRover();
                    smi.SpawnRover();
                    smi.GoTo(smi.sm.operational.idle);
                    Log.Verbose("Automated Biobot release spawned on " + name);
                }

                ChoreSuppression.CancelOperateChores(gameObject);
            });
        }
    }
}
