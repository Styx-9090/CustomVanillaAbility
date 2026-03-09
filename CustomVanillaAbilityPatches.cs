using BepInEx.Unity.IL2CPP.Utils.Collections;
using CustomVanillaAbility.CustomClasses;
using HarmonyLib;
using Lethe.Patches;
using System;
using static Il2CppSystem.Net.NetEventSource;

namespace CustomVanillaAbility
{
    public static class CustomVanillaAbilityPatches
    {
        [HarmonyPatch(typeof(Data), nameof(Data.LoadCustomLocale), new[] { typeof(LOCALIZE_LANGUAGE) })]
        [HarmonyPostfix, HarmonyPriority(Priority.Low)]
        public static void Postfix_Data_LoadCustomLocale(LOCALIZE_LANGUAGE lang)
        {
            CustomVanillaAbilityMain main = CustomVanillaAbilityMain.Instance;

            bool skillFlag = main.customAbilityDict.TryGetValue("skill", out CustomAbilityBundle skillBundle);
            bool coinFlag = main.customAbilityDict.TryGetValue("coin", out CustomAbilityBundle coinBundle);

            if (skillFlag || coinFlag)
            {
                if (skillFlag && coinFlag)
                {
                    System.Collections.Generic.HashSet<string> skillSpecialCheck = new();
                    skillSpecialCheck.UnionWith(skillBundle.abilityLookup);
                    skillSpecialCheck.UnionWith(coinBundle.abilityLookup);

                    main.ScanModFiles(main.skillPath, skillSpecialCheck, out System.Collections.Generic.HashSet<int> outSkillHash);

                    skillBundle.affectedLookup = outSkillHash;
                    coinBundle.affectedLookup = outSkillHash;
                }
                else if (skillFlag && !coinFlag) main.ScanModFiles(main.skillPath, skillBundle.abilityLookup, out skillBundle.affectedLookup);
                else main.ScanModFiles(main.skillPath, coinBundle.abilityLookup, out skillBundle.affectedLookup);
            }
        }
    }

    public static class CustomVanillaAbilityPatches_SkillModel
    {
        [HarmonyPatch(nameof(SkillModel.CanTeamKillOnStableOverclock))]
        [HarmonyPostfix]
        private static void CanTeamKillOnStableOverclock_Postfix(SkillModel __instance, BattleActionModel action, ref bool __result)
        {
            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue("skill", out CustomAbilityBundle skillBundle) || !skillBundle.affectedLookup.Contains(__instance.GetID())) return;

            bool flag = false;
            foreach (CustomSkillAbilityBase skillAbility in skillBundle.customAbilityDict[__instance.GetID()])
            {
                if (skillAbility.CanTeamKillOnStableOverclock(action))
                {
                    flag = true;
                    break;
                }
            }
            __result = flag;
        }

        [HarmonyPatch(nameof(SkillModel.IsShow))]
        [HarmonyPostfix]
        private static void IsShow_Postfix(SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic("skill", __instance.GetID(), ability => ability.IsShow(), out __result);
        }


        [HarmonyPatch(nameof(SkillModel.IgnoreDefenseSkill))]
        [HarmonyPostfix]
        private static void IgnoreDefenseSkill_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel target, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic("skill", __instance.GetID(), ability => ability.IgnoreDefenseSkill(action, target), out __result);
        }

        // IsActionable
        [HarmonyPatch(nameof(SkillModel.IsActionable))]
        [HarmonyPostfix]
        private static void IsActionable_Postfix(SkillModel __instance, BattleActionModel action, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic("skill", __instance.GetID(), ability => ability.IsActionable(action), out __result);
        }

        // IsPanicBlock
        [HarmonyPatch(nameof(SkillModel.IsPanicBlock))]
        [HarmonyPostfix]
        private static void IsPanicBlock_Postfix(SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic("skill", __instance.GetID(), ability => ability.IsPanicBlock(), out __result);
        }

        // IsSkillAbsorbingThisDamage
        [HarmonyPatch(nameof(SkillModel.IsSkillAbsorbingThisDamage))]
        [HarmonyPostfix]
        private static void IsSkillAbsorbingThisDamage_Postfix(SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic("skill", __instance.GetID(), ability => ability.IsSkillAbsorbingThisDamage(), out __result);
        }

        // CanUseSkill
        [HarmonyPatch(nameof(SkillModel.CanUseSkill))]
        [HarmonyPostfix]
        private static void CanUseSkill_Postfix(SkillModel __instance, BattleUnitModel actor, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic("skill", __instance.GetID(), ability => ability.CanUseSkill(actor), out __result);
        }

        // CanDealTarget
        [HarmonyPatch(nameof(SkillModel.CanDealTarget))]
        [HarmonyPostfix]
        private static void CanDealTarget_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel target, CoinModel coin, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic("skill", __instance.GetID(), ability => ability.CanDealTarget(action, target, coin), out __result);
        }

        // GetPrimeTargets
        [HarmonyPatch(nameof(SkillModel.GetPrimeTargets))]
        [HarmonyPostfix]
        private static void GetPrimeTargets_Postfix(SkillModel __instance, BattleActionModel action, ref Il2CppSystem.Collections.Generic.List<PrimeTargetData> __result)
        {
            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue("skill", out CustomAbilityBundle skillBundle) || !skillBundle.affectedLookup.Contains(__instance.GetID())) return;


            foreach (CustomSkillAbilityBase skillAbility in skillBundle.customAbilityDict[__instance.GetID()])
            {
                System.Collections.Generic.List<PrimeTargetData> primeTargetData = skillAbility.GetPrimeTargets(action);
                if (primeTargetData != null && primeTargetData.Count > 0)
                {
                    __result = primeTargetData.ToIl2Cpp();
                    return;
                }
            }
        }

        [HarmonyPatch(nameof(SkillModel.AttackByMpDmgRatherThanHpDmg))]
        [HarmonyPostfix]
        private static void AttackByMpDmgRatherThanHpDmg_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, int resultDmg, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic("skill", __instance.GetID(), ability => ability.AttackByMpDmgRatherThanHpDmg(action, coin, resultDmg), out __result);
        }

        // BlockLoseBuffByReactWithAction
        [HarmonyPatch(nameof(SkillModel.BlockLoseBuffByReactWithAction))]
        [HarmonyPostfix]
        private static void BlockLoseBuffByReactWithAction_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coinOrNull, BUFF_UNIQUE_KEYWORD keyword, bool? isCritical, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic("skill", __instance.GetID(), ability => ability.BlockLoseBuffByReactWithAction(action, keyword, isCritical), out __result);
        }

        // BlockGivingBuff
        [HarmonyPatch(nameof(SkillModel.BlockGivingBuff))]
        [HarmonyPostfix]
        private static void BlockGivingBuff_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel buffTarget, BUFF_UNIQUE_KEYWORD keyword, CoinModel coinOrNull, bool? isCritical, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic("skill", __instance.GetID(), ability => ability.BlockGivingBuff(action, buffTarget, keyword, coinOrNull, isCritical), out __result);
        }

        // ExpectedBlockGivingBuff
        [HarmonyPatch(nameof(SkillModel.ExpectedBlockGivingBuff))]
        [HarmonyPostfix]
        private static void ExpectedBlockGivingBuff_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel buffTarget, BUFF_UNIQUE_KEYWORD keyword, CoinModel coinOrNull, bool? isCritical, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic("skill", __instance.GetID(), ability => ability.ExpectedBlockGivingBuff(action, buffTarget, keyword, coinOrNull, isCritical), out __result);
        }

        // GetSkillLevelAdder
        [HarmonyPatch(nameof(SkillModel.GetSkillLevelAdder))]
        [HarmonyPostfix]
        private static void GetSkillLevelAdder_Postfix(SkillModel __instance, BattleActionModel action, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic("skill", __instance.GetID(), ability => ability.GetSkillLevelAdder(action), out __result);
        }

        // GetSkillPowerAdder
        [HarmonyPatch(nameof(SkillModel.GetSkillPowerAdder))]
        [HarmonyPostfix]
        private static void GetSkillPowerAdder_Postfix(SkillModel __instance, BattleActionModel action, COIN_ROLL_TYPE rollType, Il2CppSystem.Collections.Generic.List<CoinModel> coins, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic("skill", __instance.GetID(), ability => ability.GetSkillPowerAdder(action, rollType), out __result);
        }

        // GetExpectedSkillPowerAdder
        [HarmonyPatch(nameof(SkillModel.GetExpectedSkillPowerAdder))]
        [HarmonyPostfix]
        private static void GetExpectedSkillPowerAdder_Postfix(SkillModel __instance, BattleActionModel action, COIN_ROLL_TYPE rollType, SinActionModel expectedTargetSinActionOrNull, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic("skill", __instance.GetID(), ability => ability.GetExpectedSkillPowerAdder(action, rollType, expectedTargetSinActionOrNull), out __result);
        }

        // GetEvadeSkillPowerAdder
        [HarmonyPatch(nameof(SkillModel.GetEvadeSkillPowerAdder))]
        [HarmonyPostfix]
        private static void GetEvadeSkillPowerAdder_Postfix(SkillModel __instance, BattleActionModel evadeAction, BattleActionModel attackerAction, ref int __result)
        {
            // Empty postfix patch
        }

        // GetExpectedEvadeSkillPowerAdder
        [HarmonyPatch(nameof(SkillModel.GetExpectedEvadeSkillPowerAdder))]
        [HarmonyPostfix]
        private static void GetExpectedEvadeSkillPowerAdder_Postfix(SkillModel __instance, BattleActionModel evadeAction, BattleActionModel attackerAction, ref int __result)
        {
            // Empty postfix patch
        }

        // GetCoinScaleAdder
        [HarmonyPatch(nameof(SkillModel.GetCoinScaleAdder))]
        [HarmonyPostfix]
        private static void GetCoinScaleAdder_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, ref int __result)
        {
            // Empty postfix patch
        }

        // GetExpectedCoinScaleAdder
        [HarmonyPatch(nameof(SkillModel.GetExpectedCoinScaleAdder))]
        [HarmonyPostfix]
        private static void GetExpectedCoinScaleAdder_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, SinActionModel targetSinActionOrNull, ref int __result)
        {
            // Empty postfix patch
        }

        // GetSkillPowerResultAdder
        [HarmonyPatch(nameof(SkillModel.GetSkillPowerResultAdder))]
        [HarmonyPostfix]
        private static void GetSkillPowerResultAdder_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing, CoinModel coinOrNull, ref int __result)
        {
            // Empty postfix patch
        }

        // GetEvadeCoinScaleAdder
        [HarmonyPatch(nameof(SkillModel.GetEvadeCoinScaleAdder))]
        [HarmonyPostfix]
        private static void GetEvadeCoinScaleAdder_Postfix(SkillModel __instance, BattleActionModel evadeAction, BattleActionModel attackerAction, ref int __result)
        {
            // Empty postfix patch
        }

        // GetExpectedEvadeCoinScaleAdder
        [HarmonyPatch(nameof(SkillModel.GetExpectedEvadeCoinScaleAdder))]
        [HarmonyPostfix]
        private static void GetExpectedEvadeCoinScaleAdder_Postfix(SkillModel __instance, BattleActionModel evadeAction, BattleActionModel attackerAction, ref int __result)
        {
            // Empty postfix patch
        }

        // GetExpectedSkillPowerResultAdder
        [HarmonyPatch(nameof(SkillModel.GetExpectedSkillPowerResultAdder))]
        [HarmonyPostfix]
        private static void GetExpectedSkillPowerResultAdder_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel expectedTarget, ref int __result)
        {
            // Empty postfix patch
        }

        // GetParryingResultAdder
        [HarmonyPatch(nameof(SkillModel.GetParryingResultAdder))]
        [HarmonyPostfix]
        private static void GetParryingResultAdder_Postfix(SkillModel __instance, BattleActionModel actorAction, int actorResult, BattleActionModel oppoAction, int oppoResult, ref int __result)
        {
            // Empty postfix patch
        }

        // GetExpectedParryingResultAdder
        [HarmonyPatch(nameof(SkillModel.GetExpectedParryingResultAdder))]
        [HarmonyPostfix]
        private static void GetExpectedParryingResultAdder_Postfix(SkillModel __instance, BattleActionModel actorAction, int actorResult, BattleActionModel oppoActionOrNull, int oppoResult, ref int __result)
        {
            // Empty postfix patch
        }

        // GetOpponentParryingResultAdder
        [HarmonyPatch(nameof(SkillModel.GetOpponentParryingResultAdder))]
        [HarmonyPostfix]
        private static void GetOpponentParryingResultAdder_Postfix(SkillModel __instance, BattleActionModel actorAction, int actorResult, BattleActionModel oppoAction, int oppoResult, ref int __result)
        {
            // Empty postfix patch
        }

        // GetExpectedOpponentParryingResultAdder
        [HarmonyPatch(nameof(SkillModel.GetExpectedOpponentParryingResultAdder))]
        [HarmonyPostfix]
        private static void GetExpectedOpponentParryingResultAdder_Postfix(SkillModel __instance, BattleActionModel actorAction, int actorResult, BattleActionModel oppoAction, int oppoResult, ref int __result)
        {
            // Empty postfix patch
        }

        // GetAttackDmgMultiplier
        [HarmonyPatch(nameof(SkillModel.GetAttackDmgMultiplier))]
        [HarmonyPostfix]
        private static void GetAttackDmgMultiplier_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BattleUnitModel target, bool isWinDuel, bool isCritical, ref float __result)
        {
            // Empty postfix patch
        }

        // GetExpectedAttackDmgMultiplier
        [HarmonyPatch(nameof(SkillModel.GetExpectedAttackDmgMultiplier))]
        [HarmonyPostfix]
        private static void GetExpectedAttackDmgMultiplier_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BattleUnitModel targetOrNull, ref float __result)
        {
            // Empty postfix patch
        }

        // GetAttackDmgAdder
        [HarmonyPatch(nameof(SkillModel.GetAttackDmgAdder))]
        [HarmonyPostfix]
        private static void GetAttackDmgAdder_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BattleUnitModel target, bool isWinDuel, ref int __result)
        {
            // Empty postfix patch
        }

        // GetExpectedAttackDmgAdder
        [HarmonyPatch(nameof(SkillModel.GetExpectedAttackDmgAdder))]
        [HarmonyPostfix]
        private static void GetExpectedAttackDmgAdder_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BattleUnitModel targetOrNull, ref int __result)
        {
            // Empty postfix patch
        }

        // GetAttackHpDmgAdder
        [HarmonyPatch(nameof(SkillModel.GetAttackHpDmgAdder))]
        [HarmonyPostfix]
        private static void GetAttackHpDmgAdder_Postfix(SkillModel __instance, BattleUnitModel target, CoinModel coin, bool isWinDuel, ref int __result)
        {
            // Empty postfix patch
        }

        // GetExpectedAttackHpDmgAdder
        [HarmonyPatch(nameof(SkillModel.GetExpectedAttackHpDmgAdder))]
        [HarmonyPostfix]
        private static void GetExpectedAttackHpDmgAdder_Postfix(SkillModel __instance, BattleUnitModel target, CoinModel coin, bool isWinDuel, ref int __result)
        {
            // Empty postfix patch
        }

        // GetCriticalChanceMultiplier
        [HarmonyPatch(nameof(SkillModel.GetCriticalChanceMultiplier))]
        [HarmonyPostfix]
        private static void GetCriticalChanceMultiplier_Postfix(SkillModel __instance, BattleActionModel action, ref float __result)
        {
            // Empty postfix patch
        }

        // GetCriticalChanceAdder
        [HarmonyPatch(nameof(SkillModel.GetCriticalChanceAdder))]
        [HarmonyPostfix]
        private static void GetCriticalChanceAdder_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, ref float __result)
        {
            // Empty postfix patch
        }

        // OverwriteCriticalResult
        [HarmonyPatch(nameof(SkillModel.OverwriteCriticalResult))]
        [HarmonyPostfix]
        private static void OverwriteCriticalResult_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, bool tempCritical, ref bool? overwirteCriticalResult, ref bool __result)
        {
            // Empty postfix patch
        }

        // GetCoinProb (UnitModel overload)
        [HarmonyPatch(nameof(SkillModel.GetCoinProb), new Type[] { typeof(UnitModel), typeof(float) })]
        [HarmonyPostfix]
        private static void GetCoinProb_UnitModel_Postfix(SkillModel __instance, UnitModel unit, float defaultProb, ref float __result)
        {
            // Empty postfix patch
        }

        // GetCoinProb (BattleUnitModel overload)
        [HarmonyPatch(nameof(SkillModel.GetCoinProb), new Type[] { typeof(BattleUnitModel), typeof(float) })]
        [HarmonyPostfix]
        private static void GetCoinProb_BattleUnitModel_Postfix(SkillModel __instance, BattleUnitModel unit, float defaultProb, ref float __result)
        {
            // Empty postfix patch
        }

        // GetGiveBuffStackAdder
        [HarmonyPatch(nameof(SkillModel.GetGiveBuffStackAdder))]
        [HarmonyPostfix]
        private static void GetGiveBuffStackAdder_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coinOrNull, BattleUnitModel target, BUFF_UNIQUE_KEYWORD keyword, int stack, ref int __result)
        {
            // Empty postfix patch
        }

        // GetUseBuffTurnAdder
        [HarmonyPatch(nameof(SkillModel.GetUseBuffTurnAdder))]
        [HarmonyPostfix]
        private static void GetUseBuffTurnAdder_Postfix(SkillModel __instance, BattleActionModel action, int turn, BUFF_UNIQUE_KEYWORD buf, ref int __result)
        {
            // Empty postfix patch
        }

        // GetGiveBuffTurnAdder
        [HarmonyPatch(nameof(SkillModel.GetGiveBuffTurnAdder))]
        [HarmonyPostfix]
        private static void GetGiveBuffTurnAdder_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel target, BUFF_UNIQUE_KEYWORD keyword, int turn, ref int __result)
        {
            // Empty postfix patch
        }

        // BeforeAttack
        [HarmonyPatch(nameof(SkillModel.BeforeAttack))]
        [HarmonyPostfix]
        private static void BeforeAttack_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // IsRetreatSkill
        [HarmonyPatch(nameof(SkillModel.IsRetreatSkill))]
        [HarmonyPostfix]
        private static void IsRetreatSkill_Postfix(SkillModel __instance, BattleActionModel action, ref bool __result)
        {
            // Empty postfix patch
        }

        // OverwriteTargetableList
        [HarmonyPatch(nameof(SkillModel.OverwriteTargetableList))]
        [HarmonyPostfix]
        private static void OverwriteTargetableList_Postfix(SkillModel __instance, BattleActionModel action, Il2CppSystem.Collections.Generic.List<SinActionModel> targetableSlotListOrNull, Il2CppSystem.Collections.Generic.List<BattleUnitModel> targetableUnitListOrNull, Il2CppSystem.Collections.Generic.List<SinActionModel> addedSlotListOrNull)
        {
            // Empty postfix patch
        }

        // GetAdditionalActivateCountForDefenseSkill
        [HarmonyPatch(nameof(SkillModel.GetAdditionalActivateCountForDefenseSkill))]
        [HarmonyPostfix]
        private static void GetAdditionalActivateCountForDefenseSkill_Postfix(SkillModel __instance, BattleUnitModel owner, ref int __result)
        {
            // Empty postfix patch
        }

        // ChangeAttackDamage
        [HarmonyPatch(nameof(SkillModel.ChangeAttackDamage))]
        [HarmonyPostfix]
        private static void ChangeAttackDamage_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel target, CoinModel coin, int resultDmg, bool isCritical, bool? isWinDuel, BATTLE_EVENT_TIMING timing, ref int __result)
        {
            // Empty postfix patch
        }

        // GetGiveBsGaugeUpMultiplier
        [HarmonyPatch(nameof(SkillModel.GetGiveBsGaugeUpMultiplier))]
        [HarmonyPostfix]
        private static void GetGiveBsGaugeUpMultiplier_Postfix(SkillModel __instance, bool onGiveExplosion, BattleUnitModel target, BattleActionModel action, CoinModel coinOrNull, ref float __result)
        {
            // Empty postfix patch
        }

        // OnUseCoinConsume
        [HarmonyPatch(nameof(SkillModel.OnUseCoinConsume))]
        [HarmonyPostfix]
        private static void OnUseCoinConsume_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // RightBeforeGiveBuffBySkill
        [HarmonyPatch(nameof(SkillModel.RightBeforeGiveBuffBySkill))]
        [HarmonyPostfix]
        private static void RightBeforeGiveBuffBySkill_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel target, BUFF_UNIQUE_KEYWORD bufKeyword, int originalStack, int originalTurn, int activeRound, BATTLE_EVENT_TIMING timing, bool? isCritical)
        {
            // Empty postfix patch
        }

        // RightAfterGiveBuffBySkill
        [HarmonyPatch(nameof(SkillModel.RightAfterGiveBuffBySkill))]
        [HarmonyPostfix]
        private static void RightAfterGiveBuffBySkill_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel target, BUFF_UNIQUE_KEYWORD bufKeyword, int originalStack, int originalTurn, int resultStack, int resultTurn, int activeRound, BATTLE_EVENT_TIMING timing, bool? isCritical)
        {
            // Empty postfix patch
        }

        // OnBattleStart
        [HarmonyPatch(nameof(SkillModel.OnBattleStart))]
        [HarmonyPostfix]
        private static void OnBattleStart_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // OnRoundEnd
        [HarmonyPatch(nameof(SkillModel.OnRoundEnd))]
        [HarmonyPostfix]
        private static void OnRoundEnd_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // OnBeforeTurn
        [HarmonyPatch(nameof(SkillModel.OnBeforeTurn))]
        [HarmonyPostfix]
        private static void OnBeforeTurn_Postfix(SkillModel __instance, BattleActionModel action)
        {
            // Empty postfix patch
        }

        // OnBeforeDefense
        [HarmonyPatch(nameof(SkillModel.OnBeforeDefense))]
        [HarmonyPostfix]
        private static void OnBeforeDefense_Postfix(SkillModel __instance, BattleActionModel action)
        {
            // Empty postfix patch
        }

        // OnStartTurn_BeforeLog
        [HarmonyPatch(nameof(SkillModel.OnStartTurn_BeforeLog))]
        [HarmonyPostfix]
        private static void OnStartTurn_BeforeLog_Postfix(SkillModel __instance, BattleActionModel action, Il2CppSystem.Collections.Generic.List<BattleUnitModel> targets, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // OnTryEvade
        [HarmonyPatch(nameof(SkillModel.OnTryEvade))]
        [HarmonyPostfix]
        private static void OnTryEvade_Postfix(SkillModel __instance, BattleActionModel action, BattleActionModel attackerAction, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // AfterRecheckTargetList
        [HarmonyPatch(nameof(SkillModel.AfterRecheckTargetList))]
        [HarmonyPostfix]
        private static void AfterRecheckTargetList_Postfix(SkillModel __instance, BattleActionModel action, bool valid, bool mainTargetAlive)
        {
            // Empty postfix patch
        }

        // OnStartDuel
        [HarmonyPatch(nameof(SkillModel.OnStartDuel))]
        [HarmonyPostfix]
        private static void OnStartDuel_Postfix(SkillModel __instance, BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // OnBeforeParryingOnce
        [HarmonyPatch(nameof(SkillModel.OnBeforeParryingOnce))]
        [HarmonyPostfix]
        private static void OnBeforeParryingOnce_Postfix(SkillModel __instance, BattleActionModel ownerAction, BattleActionModel oppoAction)
        {
            // Empty postfix patch
        }

        // OnBeforeParryingOnce_AfterLog
        [HarmonyPatch(nameof(SkillModel.OnBeforeParryingOnce_AfterLog))]
        [HarmonyPostfix]
        private static void OnBeforeParryingOnce_AfterLog_Postfix(SkillModel __instance, BattleActionModel ownerAction, BattleActionModel oppoAction)
        {
            // Empty postfix patch
        }

        // OnDuelAfter_BeforeLog
        [HarmonyPatch(nameof(SkillModel.OnDuelAfter_BeforeLog))]
        [HarmonyPostfix]
        private static void OnDuelAfter_BeforeLog_Postfix(SkillModel __instance)
        {
            // Empty postfix patch
        }

        // OnDuelAfter_AfterLog
        [HarmonyPatch(nameof(SkillModel.OnDuelAfter_AfterLog))]
        [HarmonyPostfix]
        private static void OnDuelAfter_AfterLog_Postfix(SkillModel __instance)
        {
            // Empty postfix patch
        }

        // OnWinParrying
        [HarmonyPatch(nameof(SkillModel.OnWinParrying))]
        [HarmonyPostfix]
        private static void OnWinParrying_Postfix(SkillModel __instance, BattleActionModel selfAction, BattleActionModel oppoAction)
        {
            // Empty postfix patch
        }

        // OnLoseParrying
        [HarmonyPatch(nameof(SkillModel.OnLoseParrying))]
        [HarmonyPostfix]
        private static void OnLoseParrying_Postfix(SkillModel __instance, BattleActionModel selfAction, BattleActionModel oppoAction)
        {
            // Empty postfix patch
        }

        // OnWinDuel
        [HarmonyPatch(nameof(SkillModel.OnWinDuel))]
        [HarmonyPostfix]
        private static void OnWinDuel_Postfix(SkillModel __instance, BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing, int parryingCount, BattleLog_Parrying lastLogOrNull)
        {
            // Empty postfix patch
        }

        // OnLoseDuel
        [HarmonyPatch(nameof(SkillModel.OnLoseDuel))]
        [HarmonyPostfix]
        private static void OnLoseDuel_Postfix(SkillModel __instance, BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // BeforeBehaviour
        [HarmonyPatch(nameof(SkillModel.BeforeBehaviour))]
        [HarmonyPostfix]
        private static void BeforeBehaviour_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // OnStartBehaviour
        [HarmonyPatch(nameof(SkillModel.OnStartBehaviour))]
        [HarmonyPostfix]
        private static void OnStartBehaviour_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // OnCriticalIsActivated
        [HarmonyPatch(nameof(SkillModel.OnCriticalIsActivated))]
        [HarmonyPostfix]
        private static void OnCriticalIsActivated_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing, Il2CppSystem.Collections.Generic.Dictionary<BUFF_UNIQUE_KEYWORD, float> affectKeywords)
        {
            // Empty postfix patch
        }

        // OnAttackConfirmed
        [HarmonyPatch(nameof(SkillModel.OnAttackConfirmed))]
        [HarmonyPostfix]
        private static void OnAttackConfirmed_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BattleUnitModel target, BATTLE_EVENT_TIMING timing, bool isCritical)
        {
            // Empty postfix patch
        }

        // BeforeGiveAttackDamage
        [HarmonyPatch(nameof(SkillModel.BeforeGiveAttackDamage))]
        [HarmonyPostfix]
        private static void BeforeGiveAttackDamage_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BattleUnitModel target, bool? isWinDuel, bool isCritical, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // OnSucceedAttack
        [HarmonyPatch(nameof(SkillModel.OnSucceedAttack))]
        [HarmonyPostfix]
        private static void OnSucceedAttack_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BattleUnitModel target, int finalDmg, int realDmg, bool isCritical, bool? isWinDuel, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // OnSucceedEvade
        [HarmonyPatch(nameof(SkillModel.OnSucceedEvade))]
        [HarmonyPostfix]
        private static void OnSucceedEvade_Postfix(SkillModel __instance, BattleActionModel attackerAction, BattleActionModel evadeAction, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // OnFailedEvade
        [HarmonyPatch(nameof(SkillModel.OnFailedEvade))]
        [HarmonyPostfix]
        private static void OnFailedEvade_Postfix(SkillModel __instance, BattleActionModel attackerAction, BattleActionModel evadeAction, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // BeforeCompleteCommand
        [HarmonyPatch(nameof(SkillModel.BeforeCompleteCommand))]
        [HarmonyPostfix]
        private static void BeforeCompleteCommand_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing, ref int newSkillID, ref bool __result)
        {
            // Empty postfix patch
        }

        // OnCompleteCommand
        [HarmonyPatch(nameof(SkillModel.OnCompleteCommand))]
        [HarmonyPostfix]
        private static void OnCompleteCommand_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // OnStartCoin
        [HarmonyPatch(nameof(SkillModel.OnStartCoin))]
        [HarmonyPostfix]
        private static void OnStartCoin_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // OnEndCoin_BeforeLog
        [HarmonyPatch(nameof(SkillModel.OnEndCoin_BeforeLog))]
        [HarmonyPostfix]
        private static void OnEndCoin_BeforeLog_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, bool isCritical, bool? isWinDuel, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // OnEndCoin_AfterLog
        [HarmonyPatch(nameof(SkillModel.OnEndCoin_AfterLog))]
        [HarmonyPostfix]
        private static void OnEndCoin_AfterLog_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, bool isCritical, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // OnEndAttack
        [HarmonyPatch(nameof(SkillModel.OnEndAttack))]
        [HarmonyPostfix]
        private static void OnEndAttack_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // OnGiveBsGaugeUp
        [HarmonyPatch(nameof(SkillModel.OnGiveBsGaugeUp))]
        [HarmonyPostfix]
        private static void OnGiveBsGaugeUp_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BattleUnitModel target, int value, BATTLE_EVENT_TIMING timing, bool onExplosion)
        {
            // Empty postfix patch
        }

        // OnEndBehaviour
        [HarmonyPatch(nameof(SkillModel.OnEndBehaviour))]
        [HarmonyPostfix]
        private static void OnEndBehaviour_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // OnEndBehave_Refresh
        [HarmonyPatch(nameof(SkillModel.OnEndBehave_Refresh))]
        [HarmonyPostfix]
        private static void OnEndBehave_Refresh_Postfix(SkillModel __instance, BattleActionModel action)
        {
            // Empty postfix patch
        }

        // OnEndTurn
        [HarmonyPatch(nameof(SkillModel.OnEndTurn))]
        [HarmonyPostfix]
        private static void OnEndTurn_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // DoneWithAction
        [HarmonyPatch(nameof(SkillModel.DoneWithAction))]
        [HarmonyPostfix]
        private static void DoneWithAction_Postfix(SkillModel __instance, BattleActionModel action)
        {
            // Empty postfix patch
        }

        // OnDiscarded
        [HarmonyPatch(nameof(SkillModel.OnDiscarded))]
        [HarmonyPostfix]
        private static void OnDiscarded_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            // Empty postfix patch
        }

        // StackNextTurnAggroAdder
        [HarmonyPatch(nameof(SkillModel.StackNextTurnAggroAdder))]
        [HarmonyPostfix]
        private static void StackNextTurnAggroAdder_Postfix(SkillModel __instance, ref int __result)
        {
            // Empty postfix patch
        }
    }
}
