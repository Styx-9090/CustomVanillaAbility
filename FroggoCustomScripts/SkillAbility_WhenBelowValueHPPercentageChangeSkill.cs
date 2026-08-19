using System;
using System.Collections.Generic;

namespace CustomVanillaAbility.FroggoCustomScripts
{
    public class SkillAbility_WhenBelowValueHPPercentageChangeSkill : CustomSkillAbility_ChangeSkillAtCondition
    {
        public override void OnBeforeTurn(BattleActionModel action)
        {
            BattleUnitModel owner = action.Model;
            SkillModel foundSkill = this.GetSelectedSkill(owner);
            if (foundSkill == null) return;

            float hp = owner.GetHpRatio();
            if (hp <= neededHP) action.ChangeSkill(foundSkill);
        }

        public override void Init()
        {
            neededHP = this._info.Value;
            base.Init();
        }

        protected float neededHP;
    }
}
