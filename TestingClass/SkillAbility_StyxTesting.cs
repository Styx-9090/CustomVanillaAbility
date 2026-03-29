using System;
using System.Collections.Generic;

namespace CustomVanillaAbility.TestingClass
{
    public class SkillAbility_StyxTesting : CustomSkillAbilityBase
    {
        public override void OnRoundEnd(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            base.OnRoundEnd(action, timing);
        }

        public override int GetExpectedCoinScaleAdder(BattleActionModel action, CoinModel coin, COIN_ROLL_TYPE rollType, SinActionModel targetSinActionOrNull)
        {
            return TEST_BUFF;
        }

        public override int GetCoinScaleAdder(BattleActionModel action, CoinModel coin, BattleActionModel oppoActionOrNull)
        {
            return TEST_BUFF;
        }

        public override void Init(SkillModel skill, string scriptName, float jsonValue, int idx, int turnLimit, BuffReferenceData info = null)
        {
            base.Init(skill, scriptName, jsonValue, idx, turnLimit, info);
            if (jsonValue != 0) TEST_BUFF = (int)jsonValue;
        }

        protected int TEST_BUFF = 5;
    }
}
