using System;
using System.Collections.Generic;
using Lethe.Patches;

namespace CustomVanillaAbility.FroggoCustomScripts
{
    public class SkillAbility_WhenBelowValueHPPercentageChangeSkill : CustomSkillAbility_ChangeSkillAtCondition
    {
        public override void OnBattleStart(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            BattleUnitModel owner = action.Model;
            SkillModel foundSkill = this.GetSelectedSkill(owner);
            if (foundSkill == null) return;

            float hp = owner.GetHpRatio();
            //if (hp <= neededHP)
            action.TryChangeSkill(selectedSkillID);
        }

        public override void OnBeforeTurn(BattleActionModel action)
        {
            action.TryChangeSkill(selectedSkillID);
        }

        public override void Init(SkillModel skill, string scriptName, float jsonValue, int idx, int turnLimit, BuffReferenceData info = null)
        {
            base.Init(skill, scriptName, jsonValue, idx, turnLimit, info);
            neededHP = info.Value;
        }


        protected float neededHP;
    }
}
