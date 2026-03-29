using System;
using System.Collections.Generic;
using Lethe.Patches;

namespace CustomVanillaAbility.FroggoCustomScripts
{
    public class SkillAbility_ChangeDefenseOnConditional : CustomSkillAbility_ChangeSkillAtCondition
    {
        public override void OnBeforeDefense(BattleActionModel action)
        {
            if (selectedBuff == BUFF_UNIQUE_KEYWORD.None) return;

            BattleUnitModel owner = action.Model;
            SkillModel foundSkill = this.GetSelectedSkill(owner);
            if (foundSkill == null) return;


            int potency_check = this._info.stack;
            int count_check = this._info.turn;

            if (owner.GetActivatedBuffStack(selectedBuff) <= potency_check && owner.GetActivatedBuffTurn(selectedBuff) <= count_check) action.ChangeSkill(foundSkill);
        }


        public override void Init(SkillModel skill, string scriptName, float jsonValue, int idx, int turnLimit, BuffReferenceData info = null)
        {
            base.Init(skill, scriptName, jsonValue, idx, turnLimit, info);
            selectedBuff = CustomBuffs.ParseBuffUniqueKeyword(this._info.buffKeyword);
        }

        protected BUFF_UNIQUE_KEYWORD selectedBuff;
    }
}
