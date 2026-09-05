// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Empties the output storage of the Smoker without a Duplicant.
    ///
    /// The Smoker itself already fabricates unattended; only the "awaiting
    /// emptying" step requires a cook. This component performs the very same
    /// drop the vanilla chore performs
    /// (<see cref="FoodSmoker.StatesInstance.OnEmptyComplete"/>), which lets
    /// the state machine return to its working state through the regular
    /// storage change event.
    /// </summary>
    public sealed class AutoFoodSmoker : KMonoBehaviour, ISim1000ms
    {
        private FoodSmoker.StatesInstance smi;

        /// <summary>Caches the state machine instance of the building.</summary>
        protected override void OnSpawn()
        {
            base.OnSpawn();
            smi = gameObject.GetSMI<FoodSmoker.StatesInstance>();
        }

        /// <summary>
        /// Drops the finished products once per simulated second while the
        /// building holds anything in its output storage.
        /// </summary>
        /// <param name="dt">Elapsed simulated seconds.</param>
        public void Sim1000ms(float dt)
        {
            if (!AutoMachineOptions.IsEnabledFor(gameObject, SmokerConfig.ID))
            {
                return;
            }

            SafeInvoke.Try("Smoker automatic emptying", delegate
            {
                if (smi == null || !smi.IsRunning() || !smi.RequiresEmptying())
                {
                    return;
                }

                // Passing null is safe: the vanilla callback ignores the chore.
                smi.OnEmptyComplete(null);
            });
        }
    }
}