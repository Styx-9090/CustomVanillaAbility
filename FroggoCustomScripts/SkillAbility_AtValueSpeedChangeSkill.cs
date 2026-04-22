using System;
using System.Collections.Generic;
using Lethe.Patches;

namespace CustomVanillaAbility.FroggoCustomScripts
{
    public class SkillAbility_AtValueSpeedChangeSkill : CustomSkillAbility_ChangeSkillAtCondition
    {
        public override void OnBeforeTurn(BattleActionModel action)
        {
            BattleUnitModel owner = action.Model;
            SkillModel foundSkill = this.GetSelectedSkill(owner);
            if (foundSkill == null) return;

            int speed = owner.GetIntegerOfOriginSpeed();
            if (speed >= neededSpeed) action.ChangeSkill(foundSkill);
        }


        public override void Init()
        { 
            neededSpeed = this._info.IntegerValue;
        }


        protected int neededSpeed;
    }
}
