// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace AutoMachineRebuilt.Util
{
    /// <summary>
    /// Small reflection helpers used to observe and drive vanilla state
    /// machines without copying their logic.
    ///
    /// The mod deliberately reuses the states the base game declares instead
    /// of re-implementing the transition conditions: a state jump keeps every
    /// Enter/Exit callback, animation and timer of the vanilla state intact,
    /// which is what makes the automation behave exactly like a Duplicant
    /// interaction.
    /// </summary>
    internal static class StateMachineUtil
    {
        /// <summary>Binding flags covering public and private instance members.</summary>
        private const BindingFlags Instance =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>Reads an instance field, returning <c>null</c> when absent.</summary>
        /// <param name="owner">Object to read from.</param>
        /// <param name="fieldName">Field name.</param>
        internal static object Field(object owner, string fieldName)
        {
            if (owner == null)
            {
                return null;
            }

            FieldInfo field = AccessTools.Field(owner.GetType(), fieldName);
            return field == null ? null : field.GetValue(owner);
        }

        /// <summary>Writes an instance field when it exists.</summary>
        /// <param name="owner">Object to write to.</param>
        /// <param name="fieldName">Field name.</param>
        /// <param name="value">Value to assign.</param>
        internal static void SetField(object owner, string fieldName, object value)
        {
            if (owner == null)
            {
                return;
            }

            FieldInfo field = AccessTools.Field(owner.GetType(), fieldName);
            if (field != null)
            {
                field.SetValue(owner, value);
            }
        }

        /// <summary>Reads a boolean instance field, defaulting to <c>false</c>.</summary>
        /// <param name="owner">Object to read from.</param>
        /// <param name="fieldName">Field name.</param>
        internal static bool BoolField(object owner, string fieldName)
        {
            object value = Field(owner, fieldName);
            return value is bool && (bool)value;
        }

        /// <summary>
        /// Invokes a parameterless instance method and returns its result, or
        /// <c>null</c> when the method does not exist in this game version.
        /// </summary>
        /// <param name="owner">Object to call on.</param>
        /// <param name="methodName">Method name.</param>
        internal static object Call(object owner, string methodName)
        {
            if (owner == null)
            {
                return null;
            }

            MethodInfo method = owner.GetType().GetMethod(methodName, Instance, null, Type.EmptyTypes, null);
            return method == null ? null : method.Invoke(owner, null);
        }

        /// <summary>Invokes a parameterless instance method returning a boolean.</summary>
        /// <param name="owner">Object to call on.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="fallback">Value used when the method is missing.</param>
        internal static bool CallBool(object owner, string methodName, bool fallback)
        {
            object value = Call(owner, methodName);
            return value is bool ? (bool)value : fallback;
        }

        /// <summary>Whether the state machine instance currently sits in a state.</summary>
        /// <param name="smi">State machine instance.</param>
        /// <param name="state">State to compare with.</param>
        internal static bool IsInState(StateMachine.Instance smi, StateMachine.BaseState state)
        {
            if (smi == null || state == null)
            {
                return false;
            }

            StateMachine.BaseState current = CurrentState(smi);
            while (current != null)
            {
                if (current == state)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        /// <summary>Returns the active leaf state of an instance.</summary>
        /// <param name="smi">State machine instance.</param>
        internal static StateMachine.BaseState CurrentState(StateMachine.Instance smi)
        {
            return smi == null ? null : Call(smi, "GetCurrentState") as StateMachine.BaseState;
        }

        /// <summary>
        /// Jumps the instance to a state of its own definition. This is the
        /// same call the vanilla transitions perform.
        /// </summary>
        /// <param name="smi">State machine instance.</param>
        /// <param name="state">Target state.</param>
        /// <returns><c>true</c> when the jump was issued.</returns>
        internal static bool GoTo(StateMachine.Instance smi, StateMachine.BaseState state)
        {
            if (smi == null || state == null)
            {
                return false;
            }

            MethodInfo goTo = AccessTools.Method(smi.GetType(), "GoTo",
                new[] { typeof(StateMachine.BaseState) });
            if (goTo == null)
            {
                return false;
            }

            goTo.Invoke(smi, new object[] { state });
            return true;
        }

        /// <summary>Reads a named state out of a state machine definition graph.</summary>
        /// <param name="root">Object owning the state field (definition or nested group).</param>
        /// <param name="path">Dot separated field path, for example "operational.geyserSelected".</param>
        internal static StateMachine.BaseState StateAt(object root, string path)
        {
            object current = root;
            foreach (string part in path.Split('.'))
            {
                current = Field(current, part);
                if (current == null)
                {
                    return null;
                }
            }

            return current as StateMachine.BaseState;
        }

        /// <summary>Returns the state machine instance of a building, if any.</summary>
        /// <param name="go">Building game object.</param>
        internal static StateMachine.Instance AnyInstance(GameObject go)
        {
            if (go == null)
            {
                return null;
            }

            StateMachineComponent[] components = go.GetComponents<StateMachineComponent>();
            for (int i = 0; i < components.Length; i++)
            {
                StateMachine.Instance smi = components[i].GetSMI();
                if (smi != null)
                {
                    return smi;
                }
            }

            return null;
        }
        /// <summary>
        /// Returns the state machine instance of a building whose state machine
        /// is declared by the given type, for example "IceKettle".
        ///
        /// A building can carry its state machine either through a
        /// <see cref="StateMachineComponent"/> (Desalinator) or through a
        /// definition registered on the <see cref="StateMachineController"/>
        /// (Ice Liquefier, Gleaner), so both sources are scanned.
        /// </summary>
        /// <param name="go">Building game object.</param>
        /// <param name="declaringTypeName">Name of the state machine class.</param>
        internal static StateMachine.Instance InstanceOf(GameObject go, string declaringTypeName)
        {
            if (go == null || string.IsNullOrEmpty(declaringTypeName))
            {
                return null;
            }

            StateMachineComponent[] components = go.GetComponents<StateMachineComponent>();
            for (int i = 0; i < components.Length; i++)
            {
                StateMachine.Instance smi = components[i].GetSMI();
                if (Declares(smi, declaringTypeName))
                {
                    return smi;
                }
            }

            StateMachineController controller = go.GetComponent<StateMachineController>();
            if (controller == null)
            {
                return null;
            }

            using (IEnumerator<StateMachine.Instance> instances = controller.GetEnumerator())
            {
                while (instances.MoveNext())
                {
                    if (Declares(instances.Current, declaringTypeName))
                    {
                        return instances.Current;
                    }
                }
            }

            return null;
        }

        /// <summary>Whether an instance belongs to a named state machine class.</summary>
        /// <param name="smi">State machine instance, may be <c>null</c>.</param>
        /// <param name="declaringTypeName">Name of the state machine class.</param>
        private static bool Declares(StateMachine.Instance smi, string declaringTypeName)
        {
            if (smi == null)
            {
                return false;
            }

            for (Type type = smi.GetType(); type != null; type = type.DeclaringType)
            {
                if (type.Name == declaringTypeName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
