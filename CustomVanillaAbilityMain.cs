using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomVanillaAbility.CustomClasses;
using CustomVanillaAbility.FroggoCustomScripts;
using CustomVanillaAbility.Patches;
using CustomVanillaAbility.TestingClass;
using HarmonyLib;
using Lethe;
using SimpleJSON;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
    internal System.Collections.Generic.HashSet<int> generalAffectedHash;
    internal string skillPath;

    //private Regex classNameRegex;

    public Type archiveSkillType;

    public override void Load()
    {
        Instance = this;
        skillPath = @"custom_limbus_data\skill";
        //classNameRegex = new Regex(@"^(\w+Ability_)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        customAbilityDict = new System.Collections.Generic.Dictionary<string, CustomAbilityBundle>(StringComparer.OrdinalIgnoreCase);
        generalAffectedHash = [];
        customAbilityDict.Add("skill", new CustomAbilityBundle());
        archiveSkillType = typeof(CustomSkillAbilityBase);


        modHarmony = new Harmony(GUID);
        modHarmony.PatchAll(typeof(CustomVanillaAbilityPatches));
        modHarmony.PatchAll(typeof(CustomVanillaAbilityPatches_SkillModel));

        RegisterCustomAbility<SkillAbility_StyxTesting>("StyxTesting");
        RegisterCustomAbility<SkillAbility_WhenBelowValueHPPercentageChangeSkill>("WhenBelowValueHPPercentageChangeSkill");
    }

    public void RegisterCustomAbility<T>(string abilityName) where T : CustomAbilityBase
    {
        System.Type abilityType = typeof(T);
        if (abilityType != null)
        {
            CustomAbilityBundle bundle = null;
            if (abilityType.IsSubclassOf(archiveSkillType) || abilityType == archiveSkillType) bundle = customAbilityDict["skill"];

            if (bundle == null || bundle.abilityClassDict.ContainsKey(abilityName)) return;
            bundle.abilityClassDict.Add(abilityName, abilityType);
            bundle.abilityLookup.Add(abilityName);
            if (bundle.availableState == false) bundle.availableState = true;
        }
    }


    public void ScanModFiles(string filesPath, System.Collections.Generic.HashSet<string> lookupHash, out JSONArray outArray)
    {
        outArray = new JSONArray();
        foreach (string modPath in Directory.GetDirectories(LetheMain.modsPath.FullPath))
        {
            string finalFilePath = Path.Combine(modPath, filesPath);
            Log.LogInfo(finalFilePath);

            if (!Directory.Exists(finalFilePath)) continue;

            foreach (string jsonFilePath in Directory.GetFiles(finalFilePath))
            {
                Log.LogInfo(jsonFilePath);
                string jsonFile = File.ReadAllText(jsonFilePath);
                if (!lookupHash.Any(jsonFile.Contains)) continue;

                JSONNode root = JSON.Parse(jsonFile);
                JSONArray list = root["list"]?.AsArray;
                if (list == null) continue;

                foreach (JSONNode item in list) outArray.Add(item);
            }
        }
    }
}
