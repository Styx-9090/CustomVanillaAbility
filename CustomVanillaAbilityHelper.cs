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

        public static void ProcessPatchBoolLogic<T>(string dictKey, int id, string methodName, Func<T, bool> abilityCheck, out bool __result) where T : CustomAbilityBase
        {
            __result = false;
            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue(dictKey, out CustomAbilityBundle skillBundle) || !skillBundle.affectedLookup.Contains(id)) return;

            foreach (CustomSkillAbilityBase skillAbility in skillBundle.customAbilityDict[id])
            {
                if (skillAbility is not T realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                if (abilityCheck(realAbility))
                {
                    __result = true;
                    return;
                }
            }
        }

        public static void ProcessPatchStringLogic<T>(string dictKey, int id, string methodName, Func<T, string> abilityCheck, out string __result) where T : CustomAbilityBase
        {
            __result = string.Empty;
            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue(dictKey, out CustomAbilityBundle skillBundle) || !skillBundle.affectedLookup.Contains(id)) return;

            foreach (CustomSkillAbilityBase skillAbility in skillBundle.customAbilityDict[id])
            {
                if (skillAbility is not T realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                string result = abilityCheck(realAbility);
                if (!string.IsNullOrEmpty(result)) __result = result;
            }
        }

        public static void ProcessPatchIntLogic<T>(string dictKey, int id, string methodName, Func<T, int> abilityCheck, out int __result) where T : CustomAbilityBase
        {
            __result = 0;
            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue(dictKey, out CustomAbilityBundle skillBundle) || !skillBundle.affectedLookup.Contains(id)) return;

            foreach (CustomSkillAbilityBase skillAbility in skillBundle.customAbilityDict[id])
            {
                if (skillAbility is not T realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                __result += abilityCheck(realAbility);
            }
        }

        public static void ProcessPatchFloatLogic<T>(string dictKey, int id, string methodName,  Func<T, float> abilityCheck, out float __result) where T : CustomAbilityBase
        {
            __result = 0;
            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue(dictKey, out CustomAbilityBundle skillBundle) || !skillBundle.affectedLookup.Contains(id)) return;

            foreach (CustomSkillAbilityBase skillAbility in skillBundle.customAbilityDict[id])
            {
                if (skillAbility is not T realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                __result += abilityCheck(realAbility);
            }
        }

        public static void ProcessPatchVoidLogic<T>(string dictKey, int id, string methodName, Action<T> triggerTiming) where T : CustomAbilityBase
        {
            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue(dictKey, out CustomAbilityBundle skillBundle) || !skillBundle.affectedLookup.Contains(id)) return;

            foreach (CustomAbilityBase skillAbility in skillBundle.customAbilityDict[id])
            {
                if (skillAbility is not T realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                triggerTiming(realAbility);
            }
        }
    }
}
