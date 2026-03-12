using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Lethe;
using SimpleJSON;
using System;
using System.IO;
using System.Linq;
using CustomVanillaAbility.CustomClasses;
using CustomVanillaAbility.TestingClass;

namespace CustomVanillaAbility;

[BepInPlugin(GUID, NAME, VERSION), BepInDependency("Lethe")]
public class CustomVanillaAbilityMain : BasePlugin
{
    public const string GUID = $"{AUTHOR}.{NAME}";
    public const string NAME = "CustomVanillaAbility";
    public const string VERSION = "1.0.0";
    public const string AUTHOR = "Styx9090";

    public static CustomVanillaAbilityMain Instance;
    public static Harmony modHarmony;

    internal System.Collections.Generic.Dictionary<string, CustomAbilityBundle> customAbilityDict;

    internal string skillPath;

    public override void Load()
    {
        Instance = this;
        skillPath = @"custom_limbus_data\skill";

        modHarmony = new Harmony(GUID);
        modHarmony.PatchAll(typeof(CustomVanillaAbilityPatches));
        modHarmony.PatchAll(typeof(CustomVanillaAbilityPatches_SkillModel));

        customAbilityDict = new System.Collections.Generic.Dictionary<string, CustomAbilityBundle>(StringComparer.OrdinalIgnoreCase);
        customAbilityDict.Add("skill", new CustomAbilityBundle());
        RegisterCustomAbility(new SkillAbility_StyxTesting());
    }

    public void RegisterCustomAbility(CustomAbilityBase customAbility)
    {
        if (customAbility != null)
        {
            CustomAbilityBundle bundle = null;
            if (customAbility is CustomSkillAbilityBase) bundle = customAbilityDict["skill"];

            if (bundle == null) return;

            string abilityName = nameof(customAbility);
            if (abilityName.StartsWith("SkillAbility_", StringComparison.OrdinalIgnoreCase)) abilityName = abilityName.Substring(13);
            bundle.abilityClassDict.Add(abilityName, customAbility);
            bundle.abilityLookup.Add(abilityName);
            if (bundle.availableState == false) bundle.availableState = true;
        }
    }


    public JSONArray ScanModFiles(string filesPath, System.Collections.Generic.HashSet<string> lookupHash, out System.Collections.Generic.HashSet<int> outHash)
    {
        outHash = null;
        JSONArray finalList = null;
        foreach (string modPath in Directory.GetDirectories(LetheMain.modsPath.FullPath))
        {
            string finalFilePath = Path.Combine(filesPath, modPath);

            if (!Directory.Exists(finalFilePath)) return null;

            foreach (string jsonFilePath in Directory.GetFiles(finalFilePath))
            {
                string jsonFile = File.ReadAllText(jsonFilePath);
                if (!lookupHash.Any(jsonFile.Contains)) return null;

                if (outHash == null) outHash = new System.Collections.Generic.HashSet<int>();

                JSONNode root = JSON.Parse(jsonFile);
                JSONArray list = root["list"]?.AsArray;
                if (list == null) continue;

                for (int i = 0; i < list.Count; i++)
                {
                    JSONNode data = list[i];
                    outHash.Add(data["id"].AsInt);
                    finalList.Add(data);
                }
            }
        }

        return finalList;
    }
}
