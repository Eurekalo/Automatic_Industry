// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using KSerialization;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Workable target for Duplicants to perform the automation toggle / modification
    /// chore on a building using a wrench tool and animation.
    /// </summary>
    [SerializationConfig(MemberSerialization.OptIn)]
    [AddComponentMenu("KMonoBehaviour/Workable/AutomationToggleWorkable")]
    public class AutomationToggleWorkable : Workable
    {
        [MyCmpGet]
        private AutoBuildingCustomizer customizer;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            showProgressBar = true;
            resetProgressOnStop = false;
            faceTargetWhenWorking = true;
            synchronizeAnims = false;
            multitoolContext = "wrench";
            multitoolHitEffectTag = "fx_wrench_splash";

            try
            {
                var anim = Assets.GetAnim("anim_interacts_wrench_kanim");
                if (anim != null)
                {
                    overrideAnims = new KAnimFile[1] { anim };
                }
            }
            catch
            {
            }
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            SetWorkTime(2.0f);
        }

        protected override void OnCompleteWork(WorkerBase worker)
        {
            base.OnCompleteWork(worker);
            if (customizer == null)
            {
                customizer = GetComponent<AutoBuildingCustomizer>();
            }

            if (customizer != null)
            {
                customizer.CompleteToggle();
            }
        }
    }
}
