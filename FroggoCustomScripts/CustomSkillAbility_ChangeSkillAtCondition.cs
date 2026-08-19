using System;
using System.Collections.Generic;
using CustomVanillaAbility.CustomClasses;

namespace CustomVanillaAbility.FroggoCustomScripts
{
    public class CustomSkillAbility_ChangeSkillAtCondition : CustomSkillAbilityBase
    {
        protected SkillModel GetSelectedSkill(BattleUnitModel unit)
        {
            if (selectedSkillID == 0) CustomVanillaAbilityMain.Instance.Log.LogWarning($"selectedSkillID is 0 in ChangeSkillAtCondition from skill with ID/Name {this._skillModel.GetID()}/{this._skillModel.GetSkillName()}");

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


        public override void Init()
        {
            if (!int.TryParse(this._extractedData, out selectedSkillID)) selectedSkillID = (int)this._jsonValue;
        }


        protected int selectedSkillID;
    }
}
