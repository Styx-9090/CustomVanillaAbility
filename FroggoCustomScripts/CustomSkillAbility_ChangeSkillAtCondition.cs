using System;
using System.Collections.Generic;
using CustomVanillaAbility.CustomClasses;

namespace CustomVanillaAbility.FroggoCustomScripts
{
    public class CustomSkillAbility_ChangeSkillAtCondition : CustomSkillAbilityBase
    {
        protected SkillModel GetSelectedSkill(BattleUnitModel unit)
        {
            SkillModel foundSkill = null;
            BattleUnitModel owner = unit;
            Il2CppSystem.Collections.Generic.List<SkillModel> skillList = owner.GetSkillList();

            for (int i = 0; i < skillList.Count; i++)
            {
                if (skillList[i].GetID() == selectedSkillID)
                {
                    foundSkill = skillList[i];
                    break;
                }
            }

            return foundSkill;
        }


        public override void Init(SkillModel skill, string scriptName, float jsonValue, int idx, int turnLimit, BuffReferenceData info = null)
        {
            base.Init(skill, scriptName, jsonValue, idx, turnLimit, info);
            if (!int.TryParse(this._extractedData, out selectedSkillID)) selectedSkillID = (int)jsonValue;
        }


        protected int selectedSkillID;
    }
}
