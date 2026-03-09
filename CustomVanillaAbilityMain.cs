using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomVanillaAbility.CustomClasses;
using Lethe;
using SimpleJSON;
using Spine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace CustomVanillaAbility;

[BepInPlugin(GUID, NAME, VERSION)]
public class CustomVanillaAbilityMain : BasePlugin
{
    public const string GUID = $"{AUTHOR}.{NAME}";
    public const string NAME = "CustomVanillaAbility";
    public const string VERSION = "1.0.0";
    public const string AUTHOR = "Styx9090";

    public static CustomVanillaAbilityMain Instance;

    internal System.Collections.Generic.Dictionary<string, CustomAbilityBundle> customAbilityDict;

    internal string skillPath;

    public override void Load()
    {
        Instance = this;
        skillPath = @"custom_limbus_data\skill";

        customAbilityDict = new System.Collections.Generic.Dictionary<string, CustomAbilityBundle>(StringComparer.OrdinalIgnoreCase);
        customAbilityDict.Add("skill", new CustomAbilityBundle());
    }

    public void RegisterCustomAbility(CustomAbilityBase customAbility)
    {
        if (customAbility != null)
        {
            CustomAbilityBundle bundle = null;
            if (customAbility is CustomSkillAbilityBase) bundle = customAbilityDict["skill"];

            if (bundle == null) return;

            bundle.abilityHash.Add(customAbility);
            bundle.abilityLookup.Add(nameof(customAbility));
            if (bundle.availableState == false) bundle.availableState = true;
        }
    }


    public void ScanModFiles(string filesPath, System.Collections.Generic.HashSet<string> lookupHash, out System.Collections.Generic.HashSet<int> outHash)
    {
        outHash = null;
        foreach (string modPath in Directory.GetDirectories(LetheMain.modsPath.FullPath))
        {
            string finalFilePath = Path.Combine(filesPath, modPath);

            if (!Directory.Exists(finalFilePath)) return;

            foreach (string jsonFilePath in Directory.GetFiles(finalFilePath))
            {
                string jsonFile = File.ReadAllText(jsonFilePath);
                if (!lookupHash.Any(jsonFile.Contains)) return;

                if (outHash == null) outHash = new System.Collections.Generic.HashSet<int>();

                JSONNode root = JSON.Parse(jsonFile);
                JSONArray list = root["list"]?.AsArray;
                if (list == null) continue;

                for (int i = 0; i < list.Count; i++)
                {
                    JSONNode data = list[i];
                    outHash.Add(data["id"].AsInt);
                }
            }
        }
    }
}
