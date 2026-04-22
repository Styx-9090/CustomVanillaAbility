using System;
using System.Collections.Generic;
using Lethe.Patches;

namespace CustomVanillaAbility.FroggoCustomScripts
{
    public class SkillAbility_AtLessThanStackMPCheckChangeSkill : CustomSkillAbility_ChangeSkillAtCondition
    {
        public override void OnBeforeTurn(BattleActionModel action)
        {
            BattleUnitModel owner = action.Model;
            SkillModel foundSkill = this.GetSelectedSkill(owner);
            if (foundSkill == null) return;

            int mp = owner.Mp;
            if (mp >= neededMP) action.ChangeSkill(foundSkill);
        }


        public override void Init()
        {
            neededMP = this._info.IntegerValue;
        }


        protected int neededMP;
    }
}
