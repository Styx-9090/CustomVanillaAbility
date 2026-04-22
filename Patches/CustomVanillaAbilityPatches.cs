using HarmonyLib;
using Lethe.Patches;
using SimpleJSON;
using System;

namespace CustomVanillaAbility.Patches
{
    public static class CustomVanillaAbilityPatches
    {
        [HarmonyPatch(typeof(Data), nameof(Data.LoadCustomLocale), new[] { typeof(LOCALIZE_LANGUAGE) })]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void Postfix_Data_LoadCustomLocale(LOCALIZE_LANGUAGE lang)
        {
            var main = CustomVanillaAbilityMain.Instance;

            bool skillFlag = main.customAbilityDict.TryGetValue("skill", out var skillBundle) && skillBundle.availableState;
            bool coinFlag = main.customAbilityDict.TryGetValue("coin", out var coinBundle) && coinBundle.availableState;

            if (!skillFlag && !coinFlag) return;

            skillBundle?.SafeClean();
            coinBundle?.SafeClean();

            JSONArray skillJsonNodes;
            if (skillFlag && coinFlag)
            {
                var combined = new System.Collections.Generic.HashSet<string>(skillBundle.abilityLookup);
                combined.UnionWith(coinBundle.abilityLookup);
                main.ScanModFiles(main.skillPath, combined, out skillJsonNodes);
            }
            else if (skillFlag) main.ScanModFiles(main.skillPath, skillBundle.abilityLookup, out skillJsonNodes);
            else main.ScanModFiles(main.skillPath, coinBundle.abilityLookup, out skillJsonNodes);

            foreach (JSONNode node in skillJsonNodes)
            {
                JSONArray skillDataArray = node["skillData"]?.AsArray;
                if (skillDataArray == null) continue;

                var id = node["id"];

                foreach (JSONNode skillData in skillDataArray)
                {
                    if (skillFlag)
                    {
                        JSONArray abilityList = skillData["abilityScriptList"]?.AsArray;
                        if (abilityList != null)
                        {
                            foreach (JSONNode ability in abilityList)
                            {
                                var name = ability["scriptName"];

                                if (string.IsNullOrWhiteSpace(name) || !ContainsAny(name, skillBundle.abilityLookup)) continue;

                                skillBundle.affectedLookup.Add(id);
                                break;
                            }
                        }
                    }

                    if (coinFlag)
                    {
                        JSONArray coinList = skillData["coinList"]?.AsArray;
                        if (coinList == null) continue;

                        foreach (JSONNode coin in coinList)
                        {
                            JSONArray abilityList = coin["abilityScriptList"]?.AsArray;
                            if (abilityList == null) continue;

                            foreach (JSONNode ability in abilityList)
                            {
                                var name = ability["scriptName"];
                                if (string.IsNullOrWhiteSpace(name) || !ContainsAny(name, coinBundle.abilityLookup)) continue;

                                coinBundle.affectedLookup.Add(id);
                                break;
                            }
                        }
                    }
                }
            }

        }

        public static bool ContainsAny(string value, System.Collections.Generic.HashSet<string> lookup)
        {
            foreach (string key in lookup) if (value.Contains(key)) return true;
            return false;
        }
    }
}
