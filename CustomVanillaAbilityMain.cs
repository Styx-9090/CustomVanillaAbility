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
    public const string VERSION = "1.2.0";
    public const string AUTHOR = "Styx9090";

    public static CustomVanillaAbilityMain Instance;
    public static Harmony modHarmony;


    internal System.Collections.Generic.Dictionary<string, CustomAbilityBundle> customAbilityDict;
    internal System.Collections.Generic.HashSet<int> generalAffectedHash;
    internal string skillPath;
    internal string passivePath;


    public Type archiveSkillType;
    public Type archivePassiveType;

    public override void Load()
    {
        Instance = this;
        skillPath = @"custom_limbus_data\skill";
        passivePath = @"custom_limbus_data\passive";


        customAbilityDict = new System.Collections.Generic.Dictionary<string, CustomAbilityBundle>(StringComparer.OrdinalIgnoreCase);
        generalAffectedHash = [];
        customAbilityDict.Add("skill", new CustomSkillAbilityBundle());
        customAbilityDict.Add("passive", new CustomPassiveAbilityBundle());


        archiveSkillType = typeof(CustomSkillAbilityBase);
        archivePassiveType = typeof(CustomPassiveAbilityBase);


        modHarmony = new Harmony(GUID);
        modHarmony.PatchAll(typeof(CustomVanillaAbilityPatches_Reload));
        //modHarmony.PatchAll(typeof(CustomVanillaAbilityPatches_BattleUnitModel));
        modHarmony.PatchAll(typeof(CustomVanillaAbilityPatches_SkillModel));
        modHarmony.PatchAll(typeof(CustomVanillaAbilityPatches_PassiveModel));

        RegisterCustomAbility<SkillAbility_StyxTesting>("StyxTesting");
        RegisterCustomAbility<SkillAbility_WhenBelowValueHPPercentageChangeSkill>("WhenBelowValueHPPercentageChangeSkill");
        RegisterCustomAbility<SkillAbility_AtLessThanStackMPCheckChangeSkill>("AtLessThanStackMPCheckChangeSkill");
        RegisterCustomAbility<SkillAbility_AttackDmgUpByRatioByLostHpRatioSingleTargetWithMax>("AttackDmgUpByRatioByLostHpRatioSingleTargetWithMax");
        RegisterCustomAbility<SkillAbility_AtLessThanConditionalChangeSkill>("AtLessThanConditionalChangeSkill");
        RegisterCustomAbility<SkillAbility_ChangeDefenseOnConditional>("ChangeDefenseOnConditional");
        RegisterCustomAbility<SkillAbility_AtValueSpeedChangeSkill>("AtValueSpeedChangeSkill");

        RegisterCustomAbility<PassiveAbility_StyxTestingPassive>("StyxTesting");
    }

    public void RegisterCustomAbility<T>(string abilityName) where T : CustomAbilityBase
    {
        System.Type abilityType = typeof(T);
        if (abilityType != null)
        {
            CustomAbilityBundle bundle = null;
            if (abilityType.IsSubclassOf(archiveSkillType) || abilityType == archiveSkillType) customAbilityDict.TryGetValue("skill", out bundle);
            else if (abilityType.IsSubclassOf(archivePassiveType) || abilityType == archivePassiveType) customAbilityDict.TryGetValue("passive", out bundle);

            CustomVanillaAbilityMain.Instance.Log.LogInfo($"{abilityType.Name} -> {(bundle == null ? "NULL BUNDLE" : bundle.GetType().Name)}");
            if (bundle == null || !bundle.abilityTypeHash.Add(abilityType)) return;

            bool isRegex = abilityName.Length > 3 && abilityName.StartsWith("Reg", StringComparison.OrdinalIgnoreCase);

            if (!isRegex)
            {
                if (bundle.abilityClassDict.TryAdd(abilityName, abilityType)) bundle.abilityLookup.Add(abilityName);
            }
            else
            {
               Regex newReg = new(abilityName[3..], RegexOptions.Compiled | RegexOptions.CultureInvariant);
               bundle.abilityClassRegDict.Add(newReg, abilityType);
               bundle.regexLookup.Add(newReg);
            }
            
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
