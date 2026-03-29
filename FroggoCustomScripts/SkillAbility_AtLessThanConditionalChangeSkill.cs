using System;
using System.Collections.Generic;
using Lethe.Patches;

namespace CustomVanillaAbility.FroggoCustomScripts
{
    public class SkillModel_AtValueSpeedChangeSkill : CustomSkillAbility_ChangeSkillAtCondition
    {
        public override void OnBattleStart(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            BattleUnitModel owner = action.Model;
            SkillModel foundSkill = this.GetSelectedSkill(owner);
            if (foundSkill == null) return;

            int speed = owner.GetIntegerOfOriginSpeed();
            if (speed >= neededSpeed) action.ChangeSkill(foundSkill);
        }


        public override void Init(SkillModel skill, string scriptName, float jsonValue, int idx, int turnLimit, BuffReferenceData info = null)
        {
            base.Init(skill, scriptName, jsonValue, idx, turnLimit, info);
            neededSpeed = info.IntegerValue;
        }


        protected int neededSpeed;
    }
}
