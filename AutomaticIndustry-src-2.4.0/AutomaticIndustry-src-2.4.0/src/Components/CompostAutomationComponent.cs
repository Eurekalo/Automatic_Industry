// Copyright (c) 2026 Automatic Industry contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using AutoMachineRebuilt.Automation;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Manages dual progress bars, animations, countdown status items, and automated flipping logic for the Compost building:
    ///
    /// 1. Conversion Progress Bar (0% -> 100%):
    ///    Placed at the bottom of the building. Tracks the ratio of polluted dirt converted into dirt.
    ///    Hidden when storage has no contents.
    ///
    /// 2. Light Blue Flip Progress Bar (0% -> 100%):
    ///    Placed directly below the conversion progress bar in light blue (Sky Blue).
    ///    Active only during the flipping phase while playing the shovel loop animation.
    ///
    /// 3. Status Countdown Item:
    ///    Displays the countdown to next pitchfork flip or flipping state in EN, ZH-CN, ZH-TW, KO, JA.
    /// </summary>
    public class CompostAutomationComponent : KMonoBehaviour, ISim200ms
    {
        private const float AutoFlipDuration = 10.0f;
        private static readonly Color LightBlueColor = new Color(0.35f, 0.75f, 1.0f);

        [MyCmpGet]
        private Compost compost;

        [MyCmpGet]
        private CompostWorkable workable;

        [MyCmpGet]
        private Storage storage;

        [MyCmpGet]
        private KBatchedAnimController animController;

        [MyCmpGet]
        private KSelectable selectable;

        private ProgressBar progressBarConversion;
        private ProgressBar progressBarFlip;

        private float conversionPercent;
        private float flipTimer;
        private float flipPercent;
        private float timeUntilFlip = 120f;
        private bool wasComposting;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            CustomStatusItems.Register();

            if (compost == null) compost = GetComponent<Compost>();
            if (workable == null) workable = GetComponent<CompostWorkable>();
            if (storage == null) storage = GetComponent<Storage>();
            if (animController == null) animController = GetComponent<KBatchedAnimController>();
            if (selectable == null) selectable = GetComponent<KSelectable>();

            if (compost != null && compost.flipInterval > 0f)
            {
                timeUntilFlip = compost.flipInterval;
            }

            if (workable != null)
            {
                workable.ShowProgressBar(false);
            }

            if (AutoMachineOptions.IsEnabledFor(gameObject, "COMPOST"))
            {
                ChoreSuppression.CancelOperateChores(gameObject);
            }
        }

        public void Sim200ms(float dt)
        {
            if (compost == null || storage == null)
            {
                return;
            }

            bool automated = AutoMachineOptions.IsEnabledFor(gameObject, "COMPOST");

            if (automated)
            {
                ChoreSuppression.CancelOperateChores(gameObject);
            }

            UpdateConversionProgressBar();
            UpdateFlipProgressBar(dt, automated);
            UpdateStatusItem(dt, automated);
        }

        /// <summary>
        /// Bar 1: Tracks polluted dirt -> dirt conversion progress (0% -> 100%).
        /// Placed at building base. Hidden when storage contains no compostable material or dirt.
        /// </summary>
        private void UpdateConversionProgressBar()
        {
            float pollutedDirt = storage.GetMassAvailable(CompostConfig.COMPOST_TAG);
            float dirt = storage.GetMassAvailable(SimHashes.Dirt.CreateTag());
            bool hasContents = (pollutedDirt > 0.01f || dirt > 0.01f);

            if (hasContents)
            {
                conversionPercent = Mathf.Clamp01(1f - (pollutedDirt / 300f));
                if (pollutedDirt <= 0.01f && dirt > 0.01f)
                {
                    conversionPercent = 1f;
                }

                if (progressBarConversion == null)
                {
                    progressBarConversion = ProgressBar.CreateProgressBar(gameObject, () => conversionPercent);
                }

                if (progressBarConversion != null)
                {
                    progressBarConversion.SetVisibility(true);
                }
            }
            else if (progressBarConversion != null)
            {
                progressBarConversion.SetVisibility(false);
            }
        }

        /// <summary>
        /// Bar 2: Light blue progress bar tracking flip chore progress (0% -> 100%).
        /// Positioned neatly below Bar 1 at offset (0, -0.25, 0).
        /// Active only during the flipping phase and drives the shovel animation.
        /// </summary>
        private void UpdateFlipProgressBar(float dt, bool automated)
        {
            if (compost.smi == null)
            {
                return;
            }

            if (automated)
            {
                StateMachine.BaseState curState = StateMachineUtil.CurrentState(compost.smi);
                bool needsFlip = (curState == compost.smi.sm.inert);

                if (needsFlip)
                {
                    flipTimer += dt;
                    flipPercent = Mathf.Clamp01(flipTimer / AutoFlipDuration);

                    EnsureFlipProgressBar();
                    if (progressBarFlip != null)
                    {
                        progressBarFlip.SetVisibility(true);
                    }

                    if (animController != null && animController.currentAnim != "working_loop")
                    {
                        animController.Play("working_loop", KAnim.PlayMode.Loop);
                    }

                    if (flipTimer >= AutoFlipDuration)
                    {
                        flipTimer = 0f;
                        if (progressBarFlip != null)
                        {
                            progressBarFlip.SetVisibility(false);
                        }

                        if (animController != null)
                        {
                            animController.Play("on", KAnim.PlayMode.Once);
                        }

                        if (compost.flipInterval > 0f)
                        {
                            timeUntilFlip = compost.flipInterval;
                        }

                        SafeInvoke.Try("transition to composting after auto flip", delegate
                        {
                            compost.smi.GoTo(compost.smi.sm.composting);
                        });
                    }
                }
                else
                {
                    flipTimer = 0f;
                    if (progressBarFlip != null)
                    {
                        progressBarFlip.SetVisibility(false);
                    }
                }
            }
            else
            {
                // Vanilla mode: Track manual duplicant workable progress
                bool dupeWorking = workable != null && workable.worker != null;
                if (dupeWorking)
                {
                    flipPercent = Mathf.Clamp01(workable.GetPercentComplete());
                    EnsureFlipProgressBar();
                    if (progressBarFlip != null)
                    {
                        progressBarFlip.SetVisibility(true);
                    }

                    if (animController != null && animController.currentAnim != "working_loop")
                    {
                        animController.Play("working_loop", KAnim.PlayMode.Loop);
                    }
                }
                else
                {
                    if (progressBarFlip != null)
                    {
                        progressBarFlip.SetVisibility(false);
                    }
                }
            }
        }

        private void EnsureFlipProgressBar()
        {
            if (progressBarFlip == null)
            {
                progressBarFlip = ProgressBar.CreateProgressBar(gameObject, () => flipPercent, new Vector3(0f, -0.25f, 0f));
                if (progressBarFlip != null)
                {
                    progressBarFlip.barColor = LightBlueColor;
                }
            }
        }

        public bool IsFlipping
        {
            get
            {
                if (compost?.smi == null) return false;
                StateMachine.BaseState curState = StateMachineUtil.CurrentState(compost.smi);
                return (curState == compost.smi.sm.inert) || (workable != null && workable.worker != null);
            }
        }

        public float GetRemainingFlipTime()
        {
            return Mathf.Max(0f, timeUntilFlip);
        }

        /// <summary>
        /// Updates the Status panel with the live countdown to next pitchfork flip.
        /// </summary>
        private void UpdateStatusItem(float dt, bool automated)
        {
            if (selectable == null || compost.smi == null)
            {
                return;
            }

            StateMachine.BaseState curState = StateMachineUtil.CurrentState(compost.smi);
            bool isComposting = (curState == compost.smi.sm.composting);
            bool isFlipping = IsFlipping;

            if (isComposting)
            {
                if (!wasComposting && compost.flipInterval > 0f)
                {
                    timeUntilFlip = compost.flipInterval;
                }
                wasComposting = true;
                timeUntilFlip = Mathf.Max(0f, timeUntilFlip - dt);

                selectable.SetStatusItem(
                    Db.Get().StatusItemCategories.Main,
                    CustomStatusItems.CompostFlipCountdown,
                    this);
            }
            else if (isFlipping)
            {
                wasComposting = false;
                selectable.SetStatusItem(
                    Db.Get().StatusItemCategories.Main,
                    CustomStatusItems.CompostFlipCountdown,
                    this);
            }
            else
            {
                wasComposting = false;
                selectable.SetStatusItem(Db.Get().StatusItemCategories.Main, null);
            }
        }

        protected override void OnCleanUp()
        {
            if (progressBarConversion != null)
            {
                progressBarConversion.gameObject.DeleteObject();
                progressBarConversion = null;
            }
            if (progressBarFlip != null)
            {
                progressBarFlip.gameObject.DeleteObject();
                progressBarFlip = null;
            }
            if (selectable != null)
            {
                selectable.SetStatusItem(Db.Get().StatusItemCategories.Main, null);
            }
            base.OnCleanUp();
        }
    }
}
