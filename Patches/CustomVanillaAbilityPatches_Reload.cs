using HarmonyLib;
using Lethe.Patches;
using SimpleJSON;
using System;

namespace CustomVanillaAbility.Patches
{
    public static class CustomVanillaAbilityPatches_Reload
    {
        [HarmonyPatch(typeof(Data), nameof(Data.LoadCustomLocale), new[] { typeof(LOCALIZE_LANGUAGE) })]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void Postfix_Data_LoadCustomLocale(LOCALIZE_LANGUAGE lang)
        {
            var main = CustomVanillaAbilityMain.Instance;

            bool skillFlag = main.customAbilityDict.TryGetValue("skill", out var skillBundle) && skillBundle.availableState;
            bool coinFlag = main.customAbilityDict.TryGetValue("coin", out var coinBundle) && coinBundle.availableState;

            if (!skillFlag && !coinFlag) return;

            skillBundle?.affectedLookup.Clear();
            coinBundle?.affectedLookup.Clear();

            JSONArray skillJsonNodes;
            if (skillFlag && coinFlag)
            {
                var combined = new System.Collections.Generic.HashSet<string>(skillBundle.abilityLookup);
                combined.UnionWith(coinBundle.abilityLookup);
                main.ScanModFiles(main.skillPath, combined, out skillJsonNodes);

                skillBundle.nodes = skillJsonNodes;
                coinBundle.nodes = skillJsonNodes;
            }
            else if (skillFlag)
            {
                main.ScanModFiles(main.skillPath, skillBundle.abilityLookup, out skillJsonNodes);
                skillBundle.nodes = skillJsonNodes;
            }
            else
            {
                main.ScanModFiles(main.skillPath, coinBundle.abilityLookup, out skillJsonNodes);
                coinBundle.nodes = skillJsonNodes;
            }

            foreach (JSONNode node in skillJsonNodes)
            {
                JSONArray skillDataArray = node["skillData"]?.AsArray;
                if (skillDataArray == null) continue;

                int id = node["id"];

                foreach (JSONNode skillData in skillDataArray)
                {
                    if (skillFlag)
                    {
                        JSONArray abilityList = skillData["abilityScriptList"]?.AsArray;
                        if (abilityList != null)
                        {
                            foreach (JSONNode ability in abilityList)
                            {
                                string name = ability["scriptName"];
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
                                string name = ability["scriptName"];
                                if (string.IsNullOrWhiteSpace(name) || !ContainsAny(name, coinBundle.abilityLookup)) continue;

                                coinBundle.affectedLookup.Add(id);
                                break;
                            }
                        }
                    }
                }
            }

            bool passiveFlag = main.customAbilityDict.TryGetValue("passive", out var passiveBundle) && passiveBundle.availableState;

            if (!passiveFlag) return;
            main.ScanModFiles(main.passivePath, passiveBundle.abilityLookup, out JSONArray passiveJsonNodes);
            passiveBundle.nodes = passiveJsonNodes;

            foreach (JSONNode node in passiveJsonNodes)
            {
                JSONArray passiveDataArray = node["requireIDList"]?.AsArray;
                if (passiveDataArray == null) continue;

                int id = node["id"];

                foreach (JSONNode passiveData in passiveDataArray)
                {
                    string name = passiveData.Value;
                    if (string.IsNullOrWhiteSpace(name) || !ContainsAny(name, skillBundle.abilityLookup)) continue;

                    skillBundle.affectedLookup.Add(id);
                    break;
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
