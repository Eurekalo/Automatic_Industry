// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Reflection;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using HarmonyLib;
using UnityEngine;

namespace AutoMachineRebuilt.Components
{
    /// <summary>
    /// Automates the Spice Grinder by driving the vanilla
    /// <see cref="SpiceGrinder.StatesInstance"/> lifecycle without a Duplicant.
    ///
    /// The Spice Grinder is NOT a <see cref="ComplexFabricator"/>. It has its
    /// own <c>GameStateMachine</c> with three states: <c>inoperational</c>,
    /// <c>operational</c> and <c>ready</c>. The vanilla flow in the
    /// <c>ready</c> state is:
    ///
    /// <list type="number">
    /// <item><c>ToggleRecurringChore(WorkChore&lt;SpiceGrinderWorkable&gt;)</c>
    /// — a Duplicant walks over and works the station.</item>
    /// <item><c>SpiceGrinderWorkable.OnCompleteWork</c> calls
    /// <c>smi.SpiceFood()</c> which applies the spice, drops the food and
    /// consumes ingredients.</item>
    /// </list>
    ///
    /// This controller bypasses the WorkChore entirely and calls
    /// <c>SpiceFood()</c> directly once the vanilla work time elapses. All
    /// vanilla guards are checked first: recipe selected, food present,
    /// ingredients sufficient, building operational, no real worker active.
    ///
    /// The completed food is dropped to the ground as a normal Pickupable by
    /// the vanilla <c>foodStorage.Drop()</c> inside <c>SpiceFood()</c>, so
    /// Auto-Sweepers can pick it up to refrigerators via standard logistics.
    /// </summary>
    public sealed class AutoSpiceGrinderController : AutoWorkControllerBase
    {
        /// <summary>Vanilla work time constant: 5 seconds per 1000 kcal.</summary>
        private const float WorkTimePer1000kcal = 5f;

        private SpiceGrinder.StatesInstance smi;
        private SpiceGrinderWorkable workable;

        /// <summary>Seconds of work already accumulated.</summary>
        private float elapsed;

        /// <summary>Whether a spicing session is in progress.</summary>
        private bool working;

        /// <summary>Guard against double SpiceFood calls.</summary>
        private bool completing;

        /// <summary>
        /// The reflected <c>foodStorageFilter</c> field of the
        /// <c>StatesInstance</c>, used to adjust the food delivery chore type
        /// for Auto-Sweeper compatibility.
        /// </summary>
        private static readonly FieldInfo FoodStorageFilterField =
            AccessTools.Field(typeof(SpiceGrinder.StatesInstance), "foodStorageFilter");

        /// <summary>
        /// The reflected <c>seedStorage</c> field of the <c>StatesInstance</c>,
        /// containing spice ingredients (seeds, salt, etc.).
        /// </summary>
        private static readonly FieldInfo SeedStorageField =
            AccessTools.Field(typeof(SpiceGrinder.StatesInstance), "seedStorage");

        /// <summary>
        /// The reflected <c>SpiceFetches</c> field of the <c>StatesInstance</c>,
        /// tracking active ingredient fetch chores.
        /// </summary>
        private static readonly FieldInfo SpiceFetchesField =
            AccessTools.Field(typeof(SpiceGrinder.StatesInstance), "SpiceFetches");

        /// <summary>
        /// The reflected <c>ClearFetchChore</c> method of the <c>StatesInstance</c>.
        /// </summary>
        private static readonly MethodInfo ClearFetchChoreMethod =
            AccessTools.Method(typeof(SpiceGrinder.StatesInstance), "ClearFetchChore", new Type[] { typeof(Chore) });

        /// <summary>Gets the reflected seed storage from a SpiceGrinder SMI.</summary>
        public static Storage GetSeedStorageStatic(SpiceGrinder.StatesInstance smi)
        {
            if (smi == null || SeedStorageField == null) return null;
            return SeedStorageField.GetValue(smi) as Storage;
        }

        /// <summary>Gets the reflected fetch chore array from a SpiceGrinder SMI.</summary>
        public static FetchChore[] GetSpiceFetchesStatic(SpiceGrinder.StatesInstance smi)
        {
            if (smi == null || SpiceFetchesField == null) return null;
            return SpiceFetchesField.GetValue(smi) as FetchChore[];
        }

        /// <summary>Sets the reflected fetch chore array on a SpiceGrinder SMI.</summary>
        public static void SetSpiceFetchesStatic(SpiceGrinder.StatesInstance smi, FetchChore[] fetches)
        {
            if (smi == null || SpiceFetchesField == null) return;
            SpiceFetchesField.SetValue(smi, fetches);
        }

        /// <summary>
        /// Creates a <c>FabricateFetch</c> chore for delivering spice ingredients to
        /// <c>seedStorage</c>, attaching safe completion and interruption callbacks.
        /// </summary>
        public static FetchChore CreateIngredientFetchChore(SpiceGrinder.StatesInstance smi, System.Collections.Generic.HashSet<Tag> ingredients, float amount)
        {
            if (smi == null) return null;
            Storage seedStorage = GetSeedStorageStatic(smi);
            if (seedStorage == null) return null;

            float num = UnityEngine.Mathf.Max(amount, 1f);
            ChoreType choreType = Db.Get().ChoreTypes.FabricateFetch;
            Tag[] forbidden = smi.AllowMutantSeeds ? null : new Tag[] { GameTags.MutatedSeed };

            return new FetchChore(
                choreType,
                seedStorage,
                num,
                ingredients,
                FetchChore.MatchCriteria.MatchID,
                Tag.Invalid,
                forbidden_tags: forbidden,
                run_until_complete: true,
                on_complete: (Chore c) => ClearFetchChoreSafe(smi, c),
                on_end: (Chore c) => OnFetchEndedSafe(smi, c)
            );
        }

        /// <summary>
        /// Expands the storage filters of the ingredient storage to include all
        /// possible spice ingredients (seeds, salt, sucrose, iron, slime mold)
        /// so that Auto-Sweepers and delivery routines never reject salt or other minerals.
        /// </summary>
        public static void EnsureStorageFilters(Storage storage)
        {
            if (storage == null) return;
            if (storage.storageFilters == null)
            {
                storage.storageFilters = new System.Collections.Generic.List<Tag>();
            }

            void AddTag(Tag tag)
            {
                if (tag.IsValid && !storage.storageFilters.Contains(tag))
                {
                    storage.storageFilters.Add(tag);
                }
            }

            AddTag(GameTags.Seed);
            AddTag(GameTags.CropSeed);
            AddTag(SimHashes.Salt.CreateTag());
            AddTag(SimHashes.Sucrose.CreateTag());
            AddTag(SimHashes.Iron.CreateTag());
            AddTag(SimHashes.SlimeMold.CreateTag());
            AddTag(GameTags.IndustrialIngredient);
            AddTag(GameTags.Solid);

            try
            {
                if (Db.Get()?.Spices?.resources != null)
                {
                    foreach (Database.Spice spice in Db.Get().Spices.resources)
                    {
                        if (spice?.Ingredients != null)
                        {
                            foreach (Database.Spice.Ingredient ing in spice.Ingredients)
                            {
                                if (ing.IngredientSet != null)
                                {
                                    foreach (Tag t in ing.IngredientSet)
                                    {
                                        AddTag(t);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fallback for isolated test environments where Db is uninitialized
            }
        }

        /// <summary>
        /// Configures the storage filters and headroom on the SpiceGrinder prefab
        /// during registration.
        /// </summary>
        public static void ConfigurePrefabStorage(GameObject prefab)
        {
            if (prefab == null) return;
            Storage[] storages = prefab.GetComponents<Storage>();
            if (storages != null)
            {
                foreach (Storage s in storages)
                {
                    if (s != null && s.storageFilters != null && s.storageFilters.Contains(GameTags.Seed))
                    {
                        EnsureStorageFilters(s);
                        s.capacityKg = Mathf.Max(s.capacityKg, 100f);
                    }
                }
            }
        }

        /// <summary>
        /// Safely clears a completed or cancelled fetch chore from the SMI
        /// and resets the corresponding slot in <c>SpiceFetches</c> to prevent
        /// the vanilla permanent deadlock where <c>HasOpenFetches</c> stays true.
        /// </summary>
        public static void ClearFetchChoreSafe(SpiceGrinder.StatesInstance smi, Chore chore)
        {
            if (smi == null) return;
            SafeInvoke.Try("SpiceGrinder ClearFetchChoreSafe", () =>
            {
                try
                {
                    if (ClearFetchChoreMethod != null)
                    {
                        ClearFetchChoreMethod.Invoke(smi, new object[] { chore });
                    }
                }
                catch (Exception ex)
                {
                    Log.Verbose("smi.ClearFetchChore caught: " + ex.Message);
                }

                FetchChore[] fetches = GetSpiceFetchesStatic(smi);
                if (fetches != null && chore != null)
                {
                    for (int i = 0; i < fetches.Length; i++)
                    {
                        if (fetches[i] == chore)
                        {
                            fetches[i] = null;
                            break;
                        }
                    }
                }

                AutoSpiceGrinderController controller = smi.GetComponent<AutoSpiceGrinderController>();
                if (controller != null)
                {
                    controller.EnsureIngredientFetches();
                }
            });
        }

        /// <summary>
        /// Handles the end of a fetch chore (including interruptions and cancellations),
        /// cleaning the slot in <c>SpiceFetches</c> so that the machine can recover.
        /// </summary>
        public static void OnFetchEndedSafe(SpiceGrinder.StatesInstance smi, Chore chore)
        {
            if (smi == null) return;
            SafeInvoke.Try("SpiceGrinder OnFetchEndedSafe", () =>
            {
                FetchChore[] fetches = GetSpiceFetchesStatic(smi);
                if (fetches != null && chore != null)
                {
                    for (int i = 0; i < fetches.Length; i++)
                    {
                        if (fetches[i] == chore)
                        {
                            fetches[i] = null;
                            break;
                        }
                    }
                }

                AutoSpiceGrinderController controller = smi.GetComponent<AutoSpiceGrinderController>();
                if (controller != null)
                {
                    controller.EnsureIngredientFetches();
                }
            });
        }

        /// <summary>
        /// Called when the selected spice option changes to immediately configure
        /// storage headroom and start advance ingredient delivery.
        /// </summary>
        public void OnOptionChanged(SpiceGrinder.Option spiceOption)
        {
            if (smi == null) return;
            Storage seedStorage = GetSeedStorageStatic(smi);
            if (seedStorage != null)
            {
                EnsureStorageFilters(seedStorage);
                float totalKg = spiceOption?.Spice?.TotalKG ?? 3.1f;
                seedStorage.capacityKg = Mathf.Max(totalKg * 20f, 100f);
            }
            EnsureIngredientFetches();
        }

        /// <summary>
        /// Continuously ensures that all required recipe ingredients are pre-stocked
        /// in storage up to target capacity (10 batches) using <c>FabricateFetch</c>
        /// chores, enabling Auto-Sweepers and Duplicants to deliver in advance.
        /// </summary>
        public void EnsureIngredientFetches()
        {
            if (smi == null) return;
            SpiceGrinder.Option opt = smi.SelectedOption;
            if (opt == null || opt.Spice == null || opt.Spice.Ingredients == null) return;

            Storage seedStorage = GetSeedStorageStatic(smi);
            if (seedStorage == null) return;

            EnsureStorageFilters(seedStorage);

            // Expand capacity headroom to eliminate zero-margin deadlocks
            float totalKg = opt.Spice.TotalKG;
            if (seedStorage.capacityKg < Mathf.Max(totalKg * 20f, 100f))
            {
                seedStorage.capacityKg = Mathf.Max(totalKg * 20f, 100f);
            }

            FetchChore[] fetches = GetSpiceFetchesStatic(smi);
            if (fetches == null || fetches.Length != opt.Spice.Ingredients.Length)
            {
                fetches = new FetchChore[opt.Spice.Ingredients.Length];
                SetSpiceFetchesStatic(smi, fetches);
            }

            // Self-heal: clear dead or cancelled chores
            for (int i = 0; i < fetches.Length; i++)
            {
                FetchChore chore = fetches[i];
                if (chore != null)
                {
                    if (chore.isComplete || (!chore.InProgress() && chore.driver == null))
                    {
                        fetches[i] = null;
                    }
                }
            }

            // Stock each ingredient up to 10 batches
            for (int i = 0; i < opt.Spice.Ingredients.Length; i++)
            {
                Database.Spice.Ingredient ingredient = opt.Spice.Ingredients[i];
                if (ingredient.IngredientSet == null || ingredient.IngredientSet.Length == 0) continue;

                System.Collections.Generic.HashSet<Tag> tagSet = new System.Collections.Generic.HashSet<Tag>(ingredient.IngredientSet);
                float targetStock = ingredient.AmountKG * 10f;
                float currentStock = 0f;
                for (int tIdx = 0; tIdx < ingredient.IngredientSet.Length; tIdx++)
                {
                    currentStock += seedStorage.GetAmountAvailable(ingredient.IngredientSet[tIdx]);
                }
                float needed = targetStock - currentStock;

                FetchChore existingChore = fetches[i];
                if (existingChore != null)
                {
                    if (existingChore.isComplete)
                    {
                        fetches[i] = null;
                        existingChore = null;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (needed > 0.05f)
                {
                    float amountToFetch = Mathf.Max(needed, 1f);
                    fetches[i] = CreateIngredientFetchChore(smi, tagSet, amountToFetch);
                }
            }
        }

        /// <summary>
        /// Whether the fetch chore type has been adjusted for this instance.
        /// </summary>
        private bool fetchAdjusted;

        /// <summary>Caches the state machine instance and workable.</summary>
        protected override void Prepare()
        {
            base.Prepare();
            smi = gameObject.GetSMI<SpiceGrinder.StatesInstance>();
            workable = GetComponent<SpiceGrinderWorkable>();
            ApplyKitchenRoomOverride();
            AdjustFetchType();

            if (smi != null)
            {
                Storage seedStorage = GetSeedStorageStatic(smi);
                if (seedStorage != null)
                {
                    EnsureStorageFilters(seedStorage);
                    if (seedStorage.capacityKg < 100f)
                    {
                        seedStorage.capacityKg = 100f;
                    }
                }
                EnsureIngredientFetches();
            }
        }

        /// <summary>
        /// Sets the inKitchen operational flag when room requirement is waived.
        /// </summary>
        private void ApplyKitchenRoomOverride()
        {
            if (operational != null && AutoMachineOptions.IsRoomRequirementIgnored(optionKey))
            {
                operational.SetFlag(SpiceGrinder.inKitchen, true);
            }
        }

        /// <summary>
        /// Adjusts the food delivery fetch chore type from CookFetch to
        /// FabricateFetch so that Auto-Sweepers can deliver food to the grinder.
        /// </summary>
        private void AdjustFetchType()
        {
            if (fetchAdjusted || smi == null || FoodStorageFilterField == null)
            {
                return;
            }

            SafeInvoke.Try("Adjusting SpiceGrinder fetch type", delegate
            {
                object filter = FoodStorageFilterField.GetValue(smi);
                if (filter == null)
                {
                    return;
                }

                FieldInfo choreTypeField = AccessTools.Field(filter.GetType(), "choreType");
                if (choreTypeField != null)
                {
                    choreTypeField.SetValue(filter, Db.Get().ChoreTypes.FabricateFetch);
                    MethodInfo filterChangedMethod = AccessTools.Method(filter.GetType(), "FilterChanged");
                    if (filterChangedMethod != null)
                    {
                        filterChangedMethod.Invoke(filter, null);
                    }
                    fetchAdjusted = true;
                    Log.Verbose("SpiceGrinder food delivery adjusted for Auto-Sweeper");
                }
            });
        }

        /// <summary>Runs one step of the automated spicing cycle.</summary>
        /// <param name="dt">Seconds since the previous step.</param>
        protected override void Step(float dt)
        {
            ApplyKitchenRoomOverride();

            if (smi == null || !smi.IsRunning())
            {
                ResetWork();
                return;
            }

            // Never run alongside a real Duplicant.
            if (workable != null && workable.worker != null)
            {
                ResetWork();
                return;
            }

            if (!IsOperational())
            {
                ResetWork();
                return;
            }

            // Vanilla guards: recipe selected, food present, ingredients ok.
            if (smi.SelectedOption == null)
            {
                ResetWork();
                return;
            }

            // Continuous advance pre-stocking: ensure ingredient fetches exist up to 10 batches
            EnsureIngredientFetches();

            Edible food = smi.CurrentFood;
            if (food == null)
            {
                ResetWork();
                return;
            }

            if (!smi.CanSpice(food.Calories))
            {
                ResetWork();
                return;
            }

            // Start or continue working.
            if (!working)
            {
                working = true;
                elapsed = 0f;
                SetActive(true);
                StartAnimation();
                Log.Verbose("Started automated spicing on " + name);
            }

            // Calculate vanilla work time: Calories * 0.001 / 1000 * 5
            float workTime = food.Calories * 0.001f / 1000f * WorkTimePer1000kcal;
            workTime = Mathf.Max(1f, workTime);

            if (AutoMachineOptions.Instance.ProgressBarSpiceGrinder)
            {
                if (progressBar == null)
                {
                    progressBar = ProgressBar.CreateProgressBar(gameObject, () => workTime > 0f ? Mathf.Clamp01(elapsed / workTime) : 0f);
                }
                progressBar.SetVisibility(true);
            }
            else if (progressBar != null)
            {
                progressBar.gameObject.DeleteObject();
                progressBar = null;
            }

            elapsed += dt;
            if (elapsed < workTime)
            {
                return;
            }

            // Complete the spicing exactly once.
            CompleteSpicing();
        }

        /// <summary>
        /// Calls the vanilla <c>SpiceFood()</c> which applies the spice
        /// effect, drops the food from storage and consumes ingredients.
        /// </summary>
        private void CompleteSpicing()
        {
            if (completing)
            {
                return;
            }

            completing = true;
            try
            {
                // Verify food is still present (it could have been removed
                // between the Step guard and this call).
                if (smi.CurrentFood != null)
                {
                    smi.SpiceFood();
                    Log.Verbose("Automated spicing completed on " + name);
                }
            }
            finally
            {
                completing = false;
                ResetWork();
            }
        }

        /// <summary>Stops work and resets progress.</summary>
        private void ResetWork()
        {
            if (working)
            {
                working = false;
                elapsed = 0f;
                SetActive(false);
                StopAnimation();
                if (progressBar != null)
                {
                    progressBar.gameObject.DeleteObject();
                    progressBar = null;
                }
            }
        }

        private ProgressBar progressBar;

        protected override void OnCleanUp()
        {
            if (progressBar != null)
            {
                progressBar.gameObject.DeleteObject();
                progressBar = null;
            }
            base.OnCleanUp();
        }

        /// <summary>Cleans up when automation is disabled.</summary>
        public override void StopAutomation()
        {
            base.StopAutomation();
            SafeInvoke.Try("stopping SpiceGrinder automation", ResetWork);
        }
    }
}
