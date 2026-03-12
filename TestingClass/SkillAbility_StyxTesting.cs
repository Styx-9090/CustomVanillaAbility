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

        public override int GetExpectedCoinScaleAdder(BattleActionModel action, CoinModel coin, SinActionModel targetSinActionOrNull)
        {
            return TEST_BUFF;
        }

        public override int GetCoinScaleAdder(BattleActionModel action, CoinModel coin)
        {
            return TEST_BUFF;
        }

        protected int TEST_BUFF = 5;
    }
}
