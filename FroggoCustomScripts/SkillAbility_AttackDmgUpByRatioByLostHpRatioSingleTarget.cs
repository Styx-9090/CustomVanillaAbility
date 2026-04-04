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

        public override float GetExpectedAttackDmgMultiplier(BattleActionModel action, BattleUnitModel target, CoinModel coin)
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

        public override void Init(SkillModel skill, string scriptName, float jsonValue, int idx, int turnLimit, BuffReferenceData info = null)
        {
            base.Init(skill, scriptName, jsonValue, idx, turnLimit, info);
            int CUT_DATA = this._extractedData.IndexOf('_');
            if (CUT_DATA < 0) TARGET_STRING = this._extractedData[(CUT_DATA + 1)..];
            else
            {
                TARGET_STRING = this._extractedData[(CUT_DATA + 1)..];
                RATIO_MAX = int.Parse(this._extractedData[..CUT_DATA]);
            }

            DAMAGE_RATIO = (int)jsonValue; 
        }

        protected string TARGET_STRING;
        protected float DAMAGE_RATIO;
        protected int RATIO_MAX = 0;
    }
}
