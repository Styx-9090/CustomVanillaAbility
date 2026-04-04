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
    public override void Init(SkillModel skill, string scriptName, float jsonValue, int idx, int turnLimit, BuffReferenceData info = null)
    {
        base.Init(skill, scriptName, jsonValue, idx, turnLimit, info);
        this._extractedData = "this string gets everything after the underscore (_), eg. TemplateSkill_Test -> Test";
    }
}
*/