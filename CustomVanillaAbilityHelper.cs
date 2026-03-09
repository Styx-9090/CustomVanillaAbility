using CustomVanillaAbility.CustomClasses;
using System;
using System.Collections.Generic;

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

        public static void ProcessPatchBoolLogic(string dictKey, int id, Func<CustomSkillAbilityBase, bool> abilityCheck, out bool __result)
        {
            __result = false;
            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue(dictKey, out CustomAbilityBundle skillBundle) || !skillBundle.affectedLookup.Contains(id)) return;


            foreach (CustomSkillAbilityBase skillAbility in skillBundle.customAbilityDict[id])
            {
                if (abilityCheck(skillAbility))
                {
                    __result = true;
                    return;
                }
            }
        }

        public static void ProcessPatchIntLogic(string dictKey, int id, Func<CustomSkillAbilityBase, int> abilityCheck, out int __result)
        {
            __result = 0;
            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue(dictKey, out CustomAbilityBundle skillBundle) || !skillBundle.affectedLookup.Contains(id)) return;
          
            foreach (CustomSkillAbilityBase skillAbility in skillBundle.customAbilityDict[id]) __result += abilityCheck(skillAbility);
        }
    }
}
