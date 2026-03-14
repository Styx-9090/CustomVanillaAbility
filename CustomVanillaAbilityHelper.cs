using CustomVanillaAbility.CustomClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomVanillaAbility
{
    public static class CustomVanillaAbilityHelper
    {
        public static System.Collections.Generic.List<T> ToSystem<T>(this Il2CppSystem.Collections.Generic.List<T> il2cppList)
        {
            var count = il2cppList.Count;
            var array = new T[count];
            for (int i = 0; i < count; i++) array[i] = il2cppList[i];
            return new System.Collections.Generic.List<T>(array);
        }

        public static Il2CppSystem.Collections.Generic.List<T> ToIl2Cpp<T>(this System.Collections.Generic.List<T> systemList)
        {
            if (systemList == null) return null;
            int count = systemList.Count;
            var il2cppList = new Il2CppSystem.Collections.Generic.List<T>(count);
            for (int i = 0; i < count; i++) il2cppList.Add(systemList[i]);
            return il2cppList;
        }

        public static System.Collections.Generic.Dictionary<TKey, TValue> ConvertDictionary<TKey, TValue>(this Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue> il2cppDict)
        {
            var result = new Dictionary<TKey, TValue>(il2cppDict.Count);
            foreach (var kv in il2cppDict) result.Add(kv.Key, kv.Value);
            return result;
        }

        public static bool IsOverride(this System.Reflection.MethodInfo method)
        {
            return method.GetBaseDefinition() != method;
        }

        public static bool ProcessPatchListLogic(CustomAbilityBundle bundle, int instanceId, object instance,  out System.Collections.Generic.List<CustomAbilityBase> returnList)
        {
            returnList = null;


            if (!bundle.affectedLookup.Contains(instanceId)) return false;
            if (!bundle.customAbilityTable.TryGetValue(instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList) || abilityList.Count <= 0) return false;

            returnList = abilityList;
            return true;
        }

        /*
        public static void ProcessPatchBoolLogic<T>(string dictKey, int instanceId, object instance, string methodName, Func<T, bool> abilityCheck, out bool __result) where T : CustomAbilityBase
        {
            __result = false;
            CustomVanillaAbilityMain.Instance.Log.LogInfo("Processing bool with name = " + methodName);

            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue(dictKey, out CustomAbilityBundle skillBundle) || !skillBundle.affectedLookup.Contains(instanceId)) return;
            if (!skillBundle.customAbilityTable.TryGetValue(instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList) || abilityList.Count <= 0) return;

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not T realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                if (abilityCheck(realAbility))
                {
                    __result = true;
                    return;
                }
            }
        }

        public static void ProcessPatchStringLogic<T>(string dictKey, int instanceId, object instance, string methodName, Func<T, string> abilityCheck, out string __result) where T : CustomAbilityBase
        {
            __result = string.Empty;
            CustomVanillaAbilityMain.Instance.Log.LogInfo("Processing string with name = " + methodName);

            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue(dictKey, out CustomAbilityBundle skillBundle) || !skillBundle.affectedLookup.Contains(instanceId)) return;
            if (!skillBundle.customAbilityTable.TryGetValue(instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList) || abilityList.Count <= 0) return;

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not T realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                string result = abilityCheck(realAbility);
                if (!string.IsNullOrEmpty(result)) __result = result;
            }
        }

        public static void ProcessPatchIntLogic<T>(string dictKey, int instanceId, object instance, string methodName, Func<T, int> abilityCheck, out int __result) where T : CustomAbilityBase
        {
            __result = 0;
            CustomVanillaAbilityMain.Instance.Log.LogInfo("Processing int with name = " + methodName);

            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue(dictKey, out CustomAbilityBundle skillBundle) || !skillBundle.affectedLookup.Contains(instanceId)) return;
            if (!skillBundle.customAbilityTable.TryGetValue(instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList) || abilityList.Count <= 0) return;
            CustomVanillaAbilityMain.Instance.Log.LogInfo("Processing abilities from method with name = " + methodName);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not T realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += abilityCheck(realAbility); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public static void ProcessPatchFloatLogic<T>(string dictKey, int instanceId, object instance, string methodName,  Func<T, float> abilityCheck, out float __result) where T : CustomAbilityBase
        {
            __result = 0;
            CustomVanillaAbilityMain.Instance.Log.LogInfo("Processing float with name = " + methodName);

            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue(dictKey, out CustomAbilityBundle skillBundle) || !skillBundle.affectedLookup.Contains(instanceId)) return;
            if (!skillBundle.customAbilityTable.TryGetValue(instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList) || abilityList.Count <= 0) return;

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not T realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try {__result += abilityCheck(realAbility); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public static void ProcessPatchVoidLogic<T>(string dictKey, int instanceId, object instance, string methodName, Action<T> triggerTiming) where T : CustomAbilityBase
        {
            CustomVanillaAbilityMain.Instance.Log.LogInfo("Processing void with name = " + methodName);

            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue(dictKey, out CustomAbilityBundle skillBundle) || !skillBundle.affectedLookup.Contains(instanceId)) return;
            if (!skillBundle.customAbilityTable.TryGetValue(instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList) || abilityList.Count <= 0) return;

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not T realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                triggerTiming(realAbility);
            }
        }
        */
    }
}
