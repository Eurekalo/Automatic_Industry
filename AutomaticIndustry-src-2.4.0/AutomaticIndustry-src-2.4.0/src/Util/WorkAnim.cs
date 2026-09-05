// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using UnityEngine;

namespace AutoMachineRebuilt.Util
{
    /// <summary>
    /// Plays the "building is being worked on" animation while a building
    /// runs without a Duplicant.
    ///
    /// Vanilla starts those animations from the Duplicant's work chore, so an
    /// automated building would otherwise sit completely still while it
    /// produces. This helper picks the first animation the building's kanim
    /// actually contains, which keeps it safe for every building and DLC: if
    /// none of the candidates exist nothing is played and the building simply
    /// keeps its idle animation.
    /// </summary>
    internal static class WorkAnim
    {
        /// <summary>Animation names used by vanilla work loops, best first.</summary>
        private static readonly string[] WorkingCandidates =
        {
            "working_loop", "work_loop", "working", "on"
        };

        /// <summary>Animation names used when a building goes back to idle.</summary>
        private static readonly string[] IdleCandidates =
        {
            "off", "idle", "idle_loop"
        };

        /// <summary>
        /// Starts the looping work animation of a building.
        /// </summary>
        /// <param name="controller">Animation controller of the building.</param>
        /// <returns><c>true</c> when an animation was started.</returns>
        internal static bool PlayWorking(KAnimControllerBase controller)
        {
            return Play(controller, WorkingCandidates, KAnim.PlayMode.Loop);
        }

        /// <summary>
        /// Returns a building to its idle animation.
        /// </summary>
        /// <param name="controller">Animation controller of the building.</param>
        /// <returns><c>true</c> when an animation was started.</returns>
        internal static bool PlayIdle(KAnimControllerBase controller)
        {
            return Play(controller, IdleCandidates, KAnim.PlayMode.Loop);
        }


        /// <summary>
        /// Plays the first animation of a candidate list that the building's
        /// kanim actually contains.
        ///
        /// Ranch stations do not use the generic "working_loop" name: the
        /// shearing stations ship a dedicated shearing animation and the
        /// Duplicant overlay ("anim_interacts_*") cannot be played without a
        /// worker. Exposing the candidate lookup lets a controller ask for its
        /// own animation names and silently fall back to the generic ones.
        /// </summary>
        /// <param name="controller">Animation controller of the building.</param>
        /// <param name="candidates">Animation names, best match first.</param>
        /// <returns><c>true</c> when an animation was started.</returns>
        internal static bool PlayFirst(KAnimControllerBase controller, string[] candidates)
        {
            return Play(controller, candidates, KAnim.PlayMode.Loop);
        }

        /// <summary>
        /// Reads the animation controller of a building.
        /// </summary>
        /// <param name="target">Building game object.</param>
        internal static KAnimControllerBase ControllerOf(GameObject target)
        {
            return target == null ? null : target.GetComponent<KAnimControllerBase>();
        }

        /// <summary>
        /// Safely checks if the animation controller has a specific animation without throwing.
        /// </summary>
        private static bool HasAnimationSafe(KAnimControllerBase controller, HashedString anim)
        {
            if (controller == null) return false;
            try
            {
                return controller.HasAnimation(anim);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Plays the first candidate animation that exists.</summary>
        /// <param name="controller">Animation controller of the building.</param>
        /// <param name="candidates">Animation names, best match first.</param>
        /// <param name="mode">Play mode used for the animation.</param>
        private static bool Play(KAnimControllerBase controller, string[] candidates, KAnim.PlayMode mode)
        {
            if (controller == null || controller.gameObject == null || candidates == null)
            {
                return false;
            }

            try
            {
                foreach (string candidate in candidates)
                {
                    if (string.IsNullOrEmpty(candidate)) continue;
                    HashedString anim = new HashedString(candidate);
                    if (!HasAnimationSafe(controller, anim))
                    {
                        continue;
                    }

                    if (controller.CurrentAnim == null ||
                        controller.CurrentAnim.name != candidate)
                    {
                        try
                        {
                            controller.Play(anim, mode);
                        }
                        catch (System.Exception)
                        {
                            continue;
                        }
                    }

                    return true;
                }
            }
            catch
            {
                // Suppress non-fatal animation lookup/playback glitches (e.g. FastTrack or batch pooling)
            }

            return false;
        }
    }
}
