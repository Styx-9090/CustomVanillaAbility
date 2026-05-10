using CustomVanillaAbility.CustomClasses;
using System;
using System.Collections.Generic;

namespace CustomVanillaAbility.TestingClass
{
    public class PassiveAbility_StyxTestingPassive : CustomPassiveAbilityBase
    {
        public override int GetCoinScaleAdder(BattleActionModel action, BattleActionModel oppoActionOrNull, CoinModel coin)
        {
            return 8;
        }

        public override int GetExpectedCoinScaleAdder(BattleActionModel action, CoinModel coin, COIN_ROLL_TYPE rollType, SinActionModel expectedTargetSinActionOrNull)
        {
            return 8;
        }
    }
}
