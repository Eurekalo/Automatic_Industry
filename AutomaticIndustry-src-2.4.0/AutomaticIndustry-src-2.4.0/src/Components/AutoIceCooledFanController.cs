// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using System;
using AutoMachineRebuilt.Automation;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using HarmonyLib;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Drives the <see cref="IceCooledFan"/> (Ice-E Fan) automatically.
    ///
    /// The vanilla building is a continuous cooling station operated by a Duplicant
    /// with no finite completion progress (workTime = infinity).
    /// This controller performs active cooling directly via <see cref="IceCooledFan.DoCooling(float)"/>
    /// without requiring a Duplicant, suppresses operate chores, hides the meaningless 0% progress bar,
    /// and supports the optional "Ignore Too Cold" mode to continue cooling below 5°C.
    /// </summary>
    public sealed class AutoIceCooledFanController : AutoWorkControllerBase
    {
        private static readonly Action<IceCooledFan, float> DoCoolingMethod =
            AccessTools.MethodDelegate<Action<IceCooledFan, float>>(AccessTools.Method(typeof(IceCooledFan), "DoCooling"));

        private static readonly Action<IceCooledFan> UpdateMeterMethod =
            AccessTools.MethodDelegate<Action<IceCooledFan>>(AccessTools.Method(typeof(IceCooledFan), "UpdateMeter"));

        [MyCmpGet]
        private IceCooledFan fan;

        [MyCmpGet]
        private IceCooledFanWorkable workable;

        protected override void Prepare()
        {
            base.Prepare();
            if (fan == null) fan = GetComponent<IceCooledFan>();
            if (workable == null) workable = GetComponent<IceCooledFanWorkable>();

            if (workable != null)
            {
                workable.ShowProgressBar(false);
            }

            ChoreSuppression.CancelOperateChores(gameObject);
        }

        protected override void Step(float dt)
        {
            if (fan == null) fan = GetComponent<IceCooledFan>();
            if (fan == null) return;

            if (workable != null)
            {
                workable.ShowProgressBar(false);
            }

            // Suppress Duplicant operate chores completely
            ChoreSuppression.CancelOperateChores(gameObject);

            // Check if ice/snow material is stored
            if (!fan.HasMaterial())
            {
                SetActive(false);
                StopAnimation();
                return;
            }

            bool ignoreTooCold = AutoMachineOptions.Instance != null && AutoMachineOptions.Instance.IgnoreTooColdIceCooledFan;

            // Perform cooling if ignoreTooCold is enabled or if environment is not too cold
            if (ignoreTooCold || !IsEnvironmentTooCold(fan))
            {
                if (DoCoolingMethod != null)
                {
                    DoCoolingMethod(fan, dt);
                }
                if (UpdateMeterMethod != null)
                {
                    UpdateMeterMethod(fan);
                }

                SetActive(true);

                if (animController != null && animController.currentAnim != "working_loop")
                {
                    animController.Play("working_loop", KAnim.PlayMode.Loop);
                }
            }
            else
            {
                SetActive(false);

                if (animController != null && animController.currentAnim != "on")
                {
                    animController.Play("on", KAnim.PlayMode.Loop);
                }
            }
        }

        public override void StopAutomation()
        {
            base.StopAutomation();
            if (animController != null)
            {
                animController.Play("off", KAnim.PlayMode.Once);
            }
        }

        /// <summary>
        /// Tests whether the local environment is below the vanilla fan threshold.
        /// </summary>
        private static bool IsEnvironmentTooCold(IceCooledFan fan)
        {
            if (fan == null) return false;
            int cell = Grid.PosToCell(fan.transform.GetPosition());
            if (!Grid.IsValidCell(cell)) return false;

            float temp = Grid.Temperature[cell];
            return temp <= fan.minCooledTemperature;
        }
    }
}
