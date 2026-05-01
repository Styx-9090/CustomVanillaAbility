using System;
using System.Collections.Generic;
using CustomVanillaAbility.CustomClasses;

namespace CustomVanillaAbility.FroggoCustomScripts
{
    public class SkillAbility_AttackDmgUpByRatioByLostHpRatioSingleTargetWithMax : CustomSkillAbilityBase
    {

        public override float GetAttackDmgMultiplier(BattleActionModel action, CoinModel coin, BattleUnitModel target, bool isCritical)
        {
            return ProcessDamage(CustomVanillaAbilityHelper.GetTargetModel(TARGET_STRING, action));
        }

        public override float GetExpectedAttackDmgMultiplier(BattleActionModel action, BattleUnitModel target, CoinModel coin, SinActionModel targetSinActionOrNull)
        {
            return ProcessDamage(CustomVanillaAbilityHelper.GetTargetModel(TARGET_STRING, action));
        }

        protected float ProcessDamage(BattleUnitModel target)
        {
            float ratio = target.GetHpRatio();

            float finalRatio = 100 - ratio;
            if (RATIO_MAX > 0 && finalRatio > RATIO_MAX) finalRatio = RATIO_MAX;

            finalRatio *= DAMAGE_RATIO;
            return finalRatio;
        }

        public override void Init()
        {
            if (this._extractedSeparatedData.Length == 1) return;

            if (this._extractedSeparatedData.Length < 3) TARGET_STRING = this._extractedSeparatedData[1];
            else
            {
                TARGET_STRING = this._extractedSeparatedData[1];
                RATIO_MAX = int.Parse(this._extractedSeparatedData[2]);
            }

            DAMAGE_RATIO = (int)this._jsonValue; 
        }

        protected string TARGET_STRING = "Target";
        protected float DAMAGE_RATIO;
        protected int RATIO_MAX = 100;
    }
}
