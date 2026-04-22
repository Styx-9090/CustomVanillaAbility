/*
using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomVanillaAbility;
using CustomVanillaAbility.CustomClasses;

[BepInPlugin("GUID", "Name", "Version"), BepInDependency("Styx9090.CustomVanillaAbility", BepInDependency.DependencyFlags.HardDependency)]
public class MainTemplate : BasePlugin
{
    // Reminder the dependency is "Styx9090.CustomVanillaAbility"

    public override void Load()
    {
        CustomVanillaAbilityMain vanillaAbilityMain = CustomVanillaAbilityMain.Instance;

        vanillaAbilityMain.RegisterCustomAbility<SkillAbility_TemplateSkill>("TemplateSkill");
    }
}

public class SkillAbility_TemplateSkill : CustomSkillAbilityBase
{
    public override void Init()
    {
        this._extractedData = "this string gets everything after the underscore (_), eg. TemplateSkill_Test -> Test";
        this._scriptName = "this string is the whole unmodified scriptName in the json";
        this._extractorRegex = new System.Text.RegularExpressions.Regex("this regex contain the pattern you registed the class with");
        //this._extractedRegexData this is the result of Regex.Match(scriptName)
    }
}
*/