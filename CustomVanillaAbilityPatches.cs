using CustomVanillaAbility.CustomClasses;
using HarmonyLib;
using Lethe.Patches;
using SimpleJSON;
using System;
using System.Linq;

namespace CustomVanillaAbility
{
    public static class CustomVanillaAbilityPatches
    {
        [HarmonyPatch(typeof(Data), nameof(Data.LoadCustomLocale), new[] { typeof(LOCALIZE_LANGUAGE) })]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void Postfix_Data_LoadCustomLocale(LOCALIZE_LANGUAGE lang)
        {
            CustomVanillaAbilityMain main = CustomVanillaAbilityMain.Instance;


            //Skills and Coins
            bool skillFlag = main.customAbilityDict.TryGetValue("skill", out CustomAbilityBundle skillBundle);
            bool coinFlag = main.customAbilityDict.TryGetValue("coin", out CustomAbilityBundle coinBundle);

            if (skillFlag || coinFlag)
            {
                if (skillFlag) skillBundle.SafeClean();
                if (coinFlag) coinBundle.SafeClean();

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
                else main.ScanModFiles(main.skillPath, coinBundle.abilityLookup, out coinBundle.affectedLookup);
            }
        }
    }


    public static class CustomVanillaAbilityPatches_SkillModel
    {
        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.Init))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        private static void Init_Postfix(SkillModel __instance)
        {
            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue("skill", out CustomAbilityBundle bundle)) return;

            int skillId = __instance.GetID();
            if (!bundle.affectedLookup.Contains(skillId)) return;

            int skillKey = __instance.GetID();
            if (bundle.customAbilityDict.ContainsKey(skillKey)) return;

            System.Collections.Generic.List<CustomAbilityBase> newAbilities = new System.Collections.Generic.List<CustomAbilityBase>();
            int baseIndex = __instance.GetAbilityList().Count;

            foreach (AbilityData abilityData in __instance._skillData.abilityScriptList)
            {
                string scriptName = bundle.abilityLookup.FirstOrDefault(x => abilityData.scriptName.Contains(x));
                if (scriptName == null) continue;

                if (!bundle.abilityClassDict.TryGetValue(scriptName, out System.Type template)) continue;
                CustomSkillAbilityBase ability = (CustomSkillAbilityBase)Activator.CreateInstance(template);

                int idx = baseIndex + newAbilities.Count + 1;
                ability.Init(__instance, scriptName, abilityData.Value, idx, abilityData.TurnLimit, abilityData.BuffData);
                if (abilityData.ConditionalData != null) ability.AttachConditionalData(abilityData.ConditionalData);
                if (abilityData.TurnLimit != 0)  ability.InitLimitedActivateCountData(abilityData.TurnLimit);

                newAbilities.Add(ability);
            }
            bundle.customAbilityDict.Add(skillKey, newAbilities);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.CanTeamKillOnStableOverclock))]
        [HarmonyPostfix]
        private static void CanTeamKillOnStableOverclock_Postfix(BattleActionModel action, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "CanTeamKillOnStableOverclock", ability => ability.CanTeamKillOnStableOverclock(action), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsShow))]
        [HarmonyPostfix]
        private static void IsShow_Postfix(SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsShow", ability => !ability.IsShow(), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IgnoreDefenseSkill))]
        [HarmonyPostfix]
        private static void IgnoreDefenseSkill_Postfix(BattleActionModel action, BattleUnitModel target, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IgnoreDefenseSkill", ability => ability.IgnoreDefenseSkill(action, target), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsActionable))]
        [HarmonyPostfix]
        private static void IsActionable_Postfix(BattleActionModel action, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsActionable", ability => ability.IsActionable(action), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsPanicBlock))]
        [HarmonyPostfix]
        private static void IsPanicBlock_Postfix(SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsPanicBlock", ability => ability.IsPanicBlock(), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsSkillAbsorbingThisDamage))]
        [HarmonyPostfix]
        private static void IsSkillAbsorbingThisDamage_Postfix(SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsSkillAbsorbingThisDamage", ability => ability.IsSkillAbsorbingThisDamage(), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.CanUseSkill))]
        [HarmonyPostfix]
        private static void CanUseSkill_Postfix(BattleUnitModel actor, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "CanUseSkill", ability => ability.CanUseSkill(actor), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.CanDealTarget))]
        [HarmonyPostfix]
        private static void CanDealTarget_Postfix(BattleActionModel action, BattleUnitModel target, CoinModel coin, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "CanDealTarget", ability => ability.CanDealTarget(action, target, coin), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetPrimeTargets))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        private static void GetPrimeTargets_Postfix(BattleActionModel action, SkillModel __instance, ref Il2CppSystem.Collections.Generic.List<PrimeTargetData> __result)
        {
            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue("skill", out CustomAbilityBundle skillBundle) || !skillBundle.affectedLookup.Contains(__instance.GetID())) return;

            foreach (CustomAbilityBase skillAbility in skillBundle.customAbilityDict[__instance.GetID()])
            {
                if (skillAbility is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains("GetPrimeTargets")) continue;

                System.Collections.Generic.List<PrimeTargetData> primeTargetData = realAbility.GetPrimeTargets(action);
                if (primeTargetData != null && primeTargetData.Count > 0)
                {
                    __result = primeTargetData.ToIl2Cpp();
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.AttackByMpDmgRatherThanHpDmg))]
        [HarmonyPostfix]
        private static void AttackByMpDmgRatherThanHpDmg_Postfix(BattleActionModel action, CoinModel coin, int resultDmg, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "AttackByMpDmgRatherThanHpDmg", ability => ability.AttackByMpDmgRatherThanHpDmg(action, coin, resultDmg), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.BlockLoseBuffByReactWithAction))]
        [HarmonyPostfix]
        private static void BlockLoseBuffByReactWithAction_Postfix(BattleActionModel action, CoinModel coinOrNull, BUFF_UNIQUE_KEYWORD keyword, bool? isCritical, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "BlockLoseBuffByReactWithAction", ability => ability.BlockLoseBuffByReactWithAction(action, keyword, isCritical), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.BlockGivingBuff))]
        [HarmonyPostfix]
        private static void BlockGivingBuff_Postfix(BattleActionModel action, BattleUnitModel buffTarget, BUFF_UNIQUE_KEYWORD keyword, CoinModel coinOrNull, bool? isCritical, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "BlockGivingBuff", ability => ability.BlockGivingBuff(action, buffTarget, keyword, coinOrNull, isCritical), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.ExpectedBlockGivingBuff))]
        [HarmonyPostfix]
        private static void ExpectedBlockGivingBuff_Postfix(BattleActionModel action, BattleUnitModel buffTarget, BUFF_UNIQUE_KEYWORD keyword, CoinModel coinOrNull, bool? isCritical, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "ExpectedBlockGivingBuff", ability => ability.ExpectedBlockGivingBuff(action, buffTarget, keyword, coinOrNull, isCritical), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetSkillLevelAdder))]
        [HarmonyPostfix]
        private static void GetSkillLevelAdder_Postfix(BattleActionModel action, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetSkillLevelAdder", ability => ability.GetSkillLevelAdder(action), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetSkillPowerAdder))]
        [HarmonyPostfix]
        private static void GetSkillPowerAdder_Postfix(BattleActionModel action, COIN_ROLL_TYPE rollType, Il2CppSystem.Collections.Generic.List<CoinModel> coins, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetSkillPowerAdder", ability => ability.GetSkillPowerAdder(action, rollType), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedSkillPowerAdder))]
        [HarmonyPostfix]
        private static void GetExpectedSkillPowerAdder_Postfix(BattleActionModel action, COIN_ROLL_TYPE rollType, SinActionModel expectedTargetSinActionOrNull, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedSkillPowerAdder", ability => ability.GetExpectedSkillPowerAdder(action, rollType, expectedTargetSinActionOrNull), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetEvadeSkillPowerAdder))]
        [HarmonyPostfix]
        private static void GetEvadeSkillPowerAdder_Postfix(BattleActionModel evadeAction, BattleActionModel attackerAction, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetEvadeSkillPowerAdder", ability => ability.GetEvadeSkillPowerAdder(evadeAction, attackerAction), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedEvadeSkillPowerAdder))]
        [HarmonyPostfix]
        private static void GetExpectedEvadeSkillPowerAdder_Postfix(BattleActionModel evadeAction, BattleActionModel attackerAction, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedEvadeSkillPowerAdder", ability => ability.GetExpectedEvadeSkillPowerAdder(evadeAction, attackerAction), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetCoinScaleAdder))]
        [HarmonyPostfix]
        private static void GetCoinScaleAdder_Postfix(BattleActionModel action, CoinModel coin, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetCoinScaleAdder", ability => ability.GetCoinScaleAdder(action, coin), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedCoinScaleAdder))]
        [HarmonyPostfix]
        private static void GetExpectedCoinScaleAdder_Postfix(BattleActionModel action, CoinModel coin, SinActionModel targetSinActionOrNull, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedCoinScaleAdder", ability => ability.GetExpectedCoinScaleAdder(action, coin, targetSinActionOrNull), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetSkillPowerResultAdder))]
        [HarmonyPostfix]
        private static void GetSkillPowerResultAdder_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, CoinModel coinOrNull, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetSkillPowerResultAdder", ability => ability.GetSkillPowerResultAdder(action, timing), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetEvadeCoinScaleAdder))]
        [HarmonyPostfix]
        private static void GetEvadeCoinScaleAdder_Postfix(BattleActionModel evadeAction, BattleActionModel attackerAction, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetEvadeCoinScaleAdder", ability => ability.GetEvadeCoinScaleAdder(evadeAction, attackerAction), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedEvadeCoinScaleAdder))]
        [HarmonyPostfix]
        private static void GetExpectedEvadeCoinScaleAdder_Postfix(BattleActionModel evadeAction, BattleActionModel attackerAction, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedEvadeCoinScaleAdder", ability => ability.GetExpectedEvadeCoinScaleAdder(evadeAction, attackerAction), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedSkillPowerResultAdder))]
        [HarmonyPostfix]
        private static void GetExpectedSkillPowerResultAdder_Postfix(BattleActionModel action, BattleUnitModel expectedTarget, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedSkillPowerResultAdder", ability => ability.GetExpectedSkillPowerResultAdder(action, expectedTarget), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetParryingResultAdder))]
        [HarmonyPostfix]
        private static void GetParryingResultAdder_Postfix(BattleActionModel actorAction, int actorResult, BattleActionModel oppoAction, int oppoResult, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetParryingResultAdder", ability => ability.GetParryingResultAdder(actorAction, actorResult, oppoAction, oppoResult), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedParryingResultAdder))]
        [HarmonyPostfix]
        private static void GetExpectedParryingResultAdder_Postfix(BattleActionModel actorAction, int actorResult, BattleActionModel oppoActionOrNull, int oppoResult, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedParryingResultAdder", ability => ability.GetExpectedParryingResultAdder(actorAction, actorResult, oppoActionOrNull, oppoResult), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetOpponentParryingResultAdder))]
        [HarmonyPostfix]
        private static void GetOpponentParryingResultAdder_Postfix(BattleActionModel actorAction, int actorResult, BattleActionModel oppoAction, int oppoResult, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetOpponentParryingResultAdder", ability => ability.GetOpponentParryingResultAdder(actorAction, actorResult, oppoAction, oppoResult), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedOpponentParryingResultAdder))]
        [HarmonyPostfix]
        private static void GetExpectedOpponentParryingResultAdder_Postfix(BattleActionModel actorAction, int actorResult, BattleActionModel oppoAction, int oppoResult, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedOpponentParryingResultAdder", ability => ability.GetExpectedOpponentParryingResultAdder(actorAction, actorResult, oppoAction, oppoResult), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetAttackDmgMultiplier))]
        [HarmonyPostfix]
        private static void GetAttackDmgMultiplier_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel target, bool isWinDuel, bool isCritical, SkillModel __instance, ref float __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchFloatLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetAttackDmgMultiplier", ability => ability.GetAttackDmgMultiplier(action, coin, target, isCritical), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedAttackDmgMultiplier))]
        [HarmonyPostfix]
        private static void GetExpectedAttackDmgMultiplier_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel targetOrNull, SkillModel __instance, ref float __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchFloatLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedAttackDmgMultiplier", ability => ability.GetExpectedAttackDmgMultiplier(action, targetOrNull, coin), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetAttackDmgAdder))]
        [HarmonyPostfix]
        private static void GetAttackDmgAdder_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel target, bool isWinDuel, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetAttackDmgAdder", ability => ability.GetAttackDmgAdder(action, target), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedAttackDmgAdder))]
        [HarmonyPostfix]
        private static void GetExpectedAttackDmgAdder_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel targetOrNull, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedAttackDmgAdder", ability => ability.GetExpectedAttackDmgAdder(action, targetOrNull), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetAttackHpDmgAdder))]
        [HarmonyPostfix]
        private static void GetAttackHpDmgAdder_Postfix(BattleUnitModel target, CoinModel coin, bool isWinDuel, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetAttackHpDmgAdder", ability => ability.GetAttackHpDmgAdder(target), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedAttackHpDmgAdder))]
        [HarmonyPostfix]
        private static void GetExpectedAttackHpDmgAdder_Postfix(BattleUnitModel target, CoinModel coin, bool isWinDuel, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedAttackHpDmgAdder", ability => ability.GetExpectedAttackHpDmgAdder(target), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetCriticalChanceMultiplier))]
        [HarmonyPostfix]
        private static void GetCriticalChanceMultiplier_Postfix(BattleActionModel action, SkillModel __instance, ref float __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchFloatLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetCriticalChanceMultiplier", ability => ability.GetCriticalChanceMultiplier(action), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetCriticalChanceAdder))]
        [HarmonyPostfix]
        private static void GetCriticalChanceAdder_Postfix(BattleActionModel action, CoinModel coin, SkillModel __instance, ref float __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchFloatLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetCriticalChanceAdder", ability => ability.GetCriticalChanceAdder(action, coin), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OverwriteCriticalResult))]
        [HarmonyPostfix]
        private static void OverwriteCriticalResult_Postfix(BattleActionModel action, CoinModel coin, bool tempCritical, ref bool? overwirteCriticalResult, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OverwriteCriticalResult", ability => ability.OverwriteCriticalResult(action, coin, tempCritical, out bool? overwirteCriticalResult), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetCoinProb), new Type[] { typeof(UnitModel), typeof(float) })]
        [HarmonyPostfix]
        private static void GetCoinProb_UnitModel_Postfix(UnitModel unit, float defaultProb, SkillModel __instance, ref float __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchFloatLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetCoinProb", ability => ability.GetCoinProb(defaultProb), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetCoinProb), new Type[] { typeof(BattleUnitModel), typeof(float) })]
        [HarmonyPostfix]
        private static void GetCoinProb_BattleUnitModel_Postfix(BattleUnitModel unit, float defaultProb, SkillModel __instance, ref float __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchFloatLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetCoinProb", ability => ability.GetCoinProb(defaultProb), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetGiveBuffStackAdder))]
        [HarmonyPostfix]
        private static void GetGiveBuffStackAdder_Postfix(BattleActionModel action, CoinModel coinOrNull, BattleUnitModel target, BUFF_UNIQUE_KEYWORD keyword, int stack, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetGiveBuffStackAdder", ability => ability.GetBuffStackAdder(action, coinOrNull, target, keyword, stack), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetUseBuffTurnAdder))]
        [HarmonyPostfix]
        private static void GetUseBuffTurnAdder_Postfix(BattleActionModel action, int turn, BUFF_UNIQUE_KEYWORD buf, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetUseBuffTurnAdder", ability => ability.GetUseBuffTurnAdder(action, turn, buf), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetGiveBuffTurnAdder))]
        [HarmonyPostfix]
        private static void GetGiveBuffTurnAdder_Postfix(BattleActionModel action, BattleUnitModel target, BUFF_UNIQUE_KEYWORD keyword, int turn, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetGiveBuffTurnAdder", ability => ability.GetBuffTurnAdder(action, target, keyword, turn), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.BeforeAttack))]
        [HarmonyPostfix]
        private static void BeforeAttack_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "BeforeAttack", ability => ability.BeforeAttack(action, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsRetreatSkill))]
        [HarmonyPostfix]
        private static void IsRetreatSkill_Postfix(BattleActionModel action, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsRetreatSkill", ability => ability.IsRetreatSkill(action), out __result);
        }

        /*
        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OverwriteTargetableList))]
        [HarmonyPostfix]
        private static void OverwriteTargetableList_Postfix(BattleActionModel action, Il2CppSystem.Collections.Generic.List<SinActionModel> targetableSlotListOrNull, Il2CppSystem.Collections.Generic.List<BattleUnitModel> targetableUnitListOrNull, Il2CppSystem.Collections.Generic.List<SinActionModel> addedSlotListOrNull)
        {

        }
        */

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetAdditionalActivateCountForDefenseSkill))]
        [HarmonyPostfix]
        private static void GetAdditionalActivateCountForDefenseSkill_Postfix(BattleUnitModel owner, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetAdditionalActivateCountForDefenseSkill", ability => ability.GetAdditionalActivateCountForDefenseSkill(owner), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.ChangeAttackDamage))]
        [HarmonyPostfix]
        private static void ChangeAttackDamage_Postfix(BattleActionModel action, BattleUnitModel target, CoinModel coin, int resultDmg, bool isCritical, bool? isWinDuel, BATTLE_EVENT_TIMING timing, SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "ChangeAttackDamage", ability => ability.ChangeAttackDamage(action, target, coin, resultDmg, isCritical, timing), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetGiveBsGaugeUpMultiplier))]
        [HarmonyPostfix]
        private static void GetGiveBsGaugeUpMultiplier_Postfix(bool onGiveExplosion, BattleUnitModel target, BattleActionModel action, CoinModel coinOrNull, SkillModel __instance, ref float __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchFloatLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetGiveBsGaugeUpMultiplier", ability => ability.GetGiveBsGaugeUpMultiplier(onGiveExplosion, target, action, coinOrNull), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OverwriteSkillIconID))]
        [HarmonyPostfix]
        private static void OverwriteSkillIconID_Postfix(SkillModel __instance, ref string __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchStringLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OverwriteSkillIconID", ability => ability.OverwriteSkillIconID(), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnUseCoinConsume))]
        [HarmonyPostfix]
        private static void OnUseCoinConsume_Postfix(BattleActionModel action, CoinModel coin, BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnUseCoinConsume", ability => ability.OnUseCoinConsume(action, coin, keyword, stack, turn, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.RightBeforeGiveBuffBySkill))]
        [HarmonyPostfix]
        private static void RightBeforeGiveBuffBySkill_Postfix(BattleActionModel action, BattleUnitModel target, BUFF_UNIQUE_KEYWORD bufKeyword, int originalStack, int originalTurn, int activeRound, BATTLE_EVENT_TIMING timing, bool? isCritical, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "RightBeforeGiveBuffBySkill", ability => ability.RightBeforeGiveBuffBySkill(action, target, bufKeyword, originalStack, originalTurn, activeRound, timing, isCritical));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.RightAfterGiveBuffBySkill))]
        [HarmonyPostfix]
        private static void RightAfterGiveBuffBySkill_Postfix(BattleActionModel action, BattleUnitModel target, BUFF_UNIQUE_KEYWORD bufKeyword, int originalStack, int originalTurn, int resultStack, int resultTurn, int activeRound, BATTLE_EVENT_TIMING timing, bool? isCritical, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "RightAfterGiveBuffBySkill", ability => ability.RightAfterGiveBuffBySkill(action, target, bufKeyword, originalStack, originalTurn, resultStack, resultTurn, activeRound, timing, isCritical));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnBattleStart))]
        [HarmonyPostfix]
        private static void OnBattleStart_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnBattleStart", ability => ability.OnBattleStart(action, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnRoundEnd))]
        [HarmonyPostfix]
        private static void OnRoundEnd_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnRoundEnd", ability => ability.OnRoundEnd(action, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnBeforeTurn))]
        [HarmonyPostfix]
        private static void OnBeforeTurn_Postfix(BattleActionModel action, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnBeforeTurn", ability => ability.OnBeforeTurn(action));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnBeforeDefense))]
        [HarmonyPostfix]
        private static void OnBeforeDefense_Postfix(BattleActionModel action, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnBeforeDefense", ability => ability.OnBeforeDefense(action));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnStartTurn_BeforeLog))]
        [HarmonyPostfix]
        private static void OnStartTurn_BeforeLog_Postfix(BattleActionModel action, Il2CppSystem.Collections.Generic.List<BattleUnitModel> targets, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnStartTurn_BeforeLog", ability => ability.OnStartTurn_BeforeLog(action, targets.ToSystem(), timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnTryEvade))]
        [HarmonyPostfix]
        private static void OnTryEvade_Postfix(BattleActionModel action, BattleActionModel attackerAction, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnTryEvade", ability => ability.OnTryEvade(action, attackerAction, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.AfterRecheckTargetList))]
        [HarmonyPostfix]
        private static void AfterRecheckTargetList_Postfix(BattleActionModel action, bool valid, bool mainTargetAlive, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "AfterRecheckTargetList", ability => ability.AfterRecheckTargetList(action, valid, mainTargetAlive));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnStartDuel))]
        [HarmonyPostfix]
        private static void OnStartDuel_Postfix(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnStartDuel", ability => ability.OnStartDuel(selfAction, oppoAction, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnBeforeParryingOnce))]
        [HarmonyPostfix]
        private static void OnBeforeParryingOnce_Postfix(BattleActionModel ownerAction, BattleActionModel oppoAction, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnBeforeParryingOnce", ability => ability.OnBeforeParryingOnce(ownerAction, oppoAction));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnBeforeParryingOnce_AfterLog))]
        [HarmonyPostfix]
        private static void OnBeforeParryingOnce_AfterLog_Postfix(BattleActionModel ownerAction, BattleActionModel oppoAction, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnBeforeParryingOnce_AfterLog", ability => ability.OnBeforeParryingOnce(ownerAction, oppoAction));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnDuelAfter_BeforeLog))]
        [HarmonyPostfix]
        private static void OnDuelAfter_BeforeLog_Postfix(SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnDuelAfter_BeforeLog", ability => ability.OnDuelAfter_BeforeLog());
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnDuelAfter_AfterLog))]
        [HarmonyPostfix]
        private static void OnDuelAfter_AfterLog_Postfix(SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnDuelAfter_AfterLog", ability => ability.OnDuelAfter_AfterLog());
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnWinParrying))]
        [HarmonyPostfix]
        private static void OnWinParrying_Postfix(BattleActionModel selfAction, BattleActionModel oppoAction, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnWinParrying", ability => ability.OnWinParrying(selfAction, oppoAction));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnLoseParrying))]
        [HarmonyPostfix]
        private static void OnLoseParrying_Postfix(BattleActionModel selfAction, BattleActionModel oppoAction, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnLoseParrying", ability => ability.OnLoseParrying(selfAction, oppoAction));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnWinDuel))]
        [HarmonyPostfix]
        private static void OnWinDuel_Postfix(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing, int parryingCount, BattleLog_Parrying lastLogOrNull, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnWinDuel", ability => ability.OnWinDuel(selfAction, oppoAction, timing, parryingCount, lastLogOrNull));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnLoseDuel))]
        [HarmonyPostfix]
        private static void OnLoseDuel_Postfix(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnLoseDuel", ability => ability.OnLoseDuel(selfAction, oppoAction, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.BeforeBehaviour))]
        [HarmonyPostfix]
        private static void BeforeBehaviour_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "BeforeBehaviour", ability => ability.BeforeBehaviour(action, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnStartBehaviour))]
        [HarmonyPostfix]
        private static void OnStartBehaviour_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnStartBehaviour", ability => ability.OnStartBehaviour(action, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnCriticalIsActivated))]
        [HarmonyPostfix]
        private static void OnCriticalIsActivated_Postfix(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing, Il2CppSystem.Collections.Generic.Dictionary<BUFF_UNIQUE_KEYWORD, float> affectKeywords, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnCriticalIsActivated", ability => ability.OnCriticalIsActivated(action, coin, timing, affectKeywords.ConvertDictionary()));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnAttackConfirmed))]
        [HarmonyPostfix]
        private static void OnAttackConfirmed_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel target, BATTLE_EVENT_TIMING timing, bool isCritical, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnAttackConfirmed", ability => ability.OnAttackConfirmed(action, target, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.BeforeGiveAttackDamage))]
        [HarmonyPostfix]
        private static void BeforeGiveAttackDamage_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel target, bool? isWinDuel, bool isCritical, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "BeforeGiveAttackDamage", ability => ability.BeforeGiveAttackDamage(action, target, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnSucceedAttack))]
        [HarmonyPostfix]
        private static void OnSucceedAttack_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel target, int finalDmg, int realDmg, bool isCritical, bool? isWinDuel, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnSucceedAttack", ability => ability.OnSucceedAttack(action, coin, target, finalDmg, realDmg, isCritical, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnSucceedEvade))]
        [HarmonyPostfix]
        private static void OnSucceedEvade_Postfix(BattleActionModel attackerAction, BattleActionModel evadeAction, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnSucceedEvade", ability => ability.OnSucceedEvade(attackerAction, evadeAction, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnFailedEvade))]
        [HarmonyPostfix]
        private static void OnFailedEvade_Postfix(BattleActionModel attackerAction, BattleActionModel evadeAction, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnFailedEvade", ability => ability.OnFailedEvade(attackerAction, evadeAction, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.BeforeCompleteCommand))]
        [HarmonyPostfix]
        private static void BeforeCompleteCommand_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, ref int newSkillID, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "BeforeCompleteCommand", ability => ability.BeforeCompleteCommand(action, timing, out int newSkillID), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnCompleteCommand))]
        [HarmonyPostfix]
        private static void OnCompleteCommand_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnCompleteCommand", ability => ability.OnCompleteCommand(action, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnStartCoin))]
        [HarmonyPostfix]
        private static void OnStartCoin_Postfix(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnStartCoin", ability => ability.OnStartCoin(action, coin));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnEndCoin_BeforeLog))]
        [HarmonyPostfix]
        private static void OnEndCoin_BeforeLog_Postfix(BattleActionModel action, CoinModel coin, bool isCritical, bool? isWinDuel, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnEndCoin_BeforeLog", ability => ability.OnEndCoin_BeforeLog(action, coin, isCritical, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnEndCoin_AfterLog))]
        [HarmonyPostfix]
        private static void OnEndCoin_AfterLog_Postfix(BattleActionModel action, CoinModel coin, bool isCritical, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnEndCoin_AfterLog", ability => ability.OnEndCoin_AfterLog(action, coin, isCritical, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnEndAttack))]
        [HarmonyPostfix]
        private static void OnEndAttack_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnEndAttack", ability => ability.OnEndAttack(action, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnGiveBsGaugeUp))]
        [HarmonyPostfix]
        private static void OnGiveBsGaugeUp_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel target, int value, BATTLE_EVENT_TIMING timing, bool onExplosion, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnGiveBsGaugeUp", ability => ability.OnGiveBsGaugeUp(action, target, value, timing, onExplosion));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnEndBehaviour))]
        [HarmonyPostfix]
        private static void OnEndBehaviour_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnEndBehaviour", ability => ability.OnEndBehaviour(action, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnEndBehave_Refresh))]
        [HarmonyPostfix]
        private static void OnEndBehave_Refresh_Postfix(BattleActionModel action, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnEndBehave_Refresh", ability => ability.OnEndBehave_Refresh(action));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnEndTurn))]
        [HarmonyPostfix]
        private static void OnEndTurn_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnEndTurn", ability => ability.OnEndTurn(action, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.DoneWithAction))]
        [HarmonyPostfix]
        private static void DoneWithAction_Postfix(BattleActionModel action, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "DoneWithAction", ability => ability.DoneWithAction(action));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnDiscarded))]
        [HarmonyPostfix]
        private static void OnDiscarded_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnDiscarded", ability => ability.OnDiscarded(action, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.StackNextTurnAggroAdder))]
        [HarmonyPostfix]
        private static void StackNextTurnAggroAdder_Postfix(SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "StackNextTurnAggroAdder", ability => ability.StackNextTurnAggroAdder(), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OverrideCanDuel))]
        [HarmonyPostfix]
        private static void OverwriteCanDuel_Postfix(BattleActionModel action, SkillModel __instance, ref bool __result, BattleActionModel opponentActionOrNull = null)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OverwriteCanDuel", ability => ability.OverwriteCanDuel(action, out bool canDuel, opponentActionOrNull), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.CanBeChangedTarget))]
        [HarmonyPostfix]
        private static void CanBeChangedTarget_Postfix(BattleActionModel ownerAction, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "CanBeChangedTarget", ability => ability.CanBeChangedTarget(ownerAction, out bool canChangedTarget), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.CanChangeMainTargetRegardlessSpeed))]
        [HarmonyPostfix]
        private static void CanChangeMainTargetRegardlessSpeed_Postfix(BattleActionModel otherAction, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "CanChangeMainTargetRegardlessSpeed", ability => ability.CanBeChangedTargetIgnoreSpeed(), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.CanBeChangedTargetIgnoreSpeed))]
        [HarmonyPostfix]
        private static void CanBeChangedTargetIgnoreSpeed_Postfix(BattleActionModel action, BattleActionModel otherAction, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "CanBeChangedTargetIgnoreSpeed", ability => ability.CanBeChangedTargetIgnoreSpeed(), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsDefenseSkillForOther))]
        [HarmonyPostfix]
        private static void IsDefenseSkillForOther_Postfix(BattleUnitModel self, BattleUnitModel originTarget, BattleActionModel opponentActionOrNull, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsDefenseSkillForOther", ability => ability.IsDefenseSkillForOther(self, originTarget, opponentActionOrNull), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.CanCheckErode))]
        [HarmonyPostfix]
        private static void CanCheckErode_Postfix(BattleActionModel action, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "CanCheckErode", ability => ability.CanCheckErode(action), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsReusable))]
        [HarmonyPostfix]
        private static void IsReusable_Postfix(BattleActionModel action, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsReusable", ability => ability.IsReusable(action), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsChangeable))]
        [HarmonyPostfix]
        private static void IsChangeable_Postfix(BattleActionModel action, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsChangeable", ability => ability.IsChangeable(action), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.BlockAddSinStock))]
        [HarmonyPostfix]
        private static void BlockAddSinStock_Postfix(BattleActionModel action, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "BlockAddSinStock", ability => ability.BlockAddSinStock(action), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsBlockingTargetBurstBuffEffectReact))]
        [HarmonyPostfix]
        private static void IsBlockingTargetBurstBuffEffectReact_Postfix(BattleUnitModel target, int stack, int turn, BattleActionModel selfAction, CoinModel selfCoin, bool isCritical, BATTLE_EVENT_TIMING timing, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsBlockingTargetBurstBuffEffectReact", ability => ability.IsBlockingTargetBurstBuffEffectReact(target, stack, turn, selfAction, selfCoin, isCritical, timing), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsBlockingTargetSinkingBuffEffectReact))]
        [HarmonyPostfix]
        private static void IsBlockingTargetSinkingBuffEffectReact_Postfix(BattleUnitModel target, int stack, int turn, BattleActionModel selfAction, CoinModel selfCoin, bool isCritical, BATTLE_EVENT_TIMING timing, SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsBlockingTargetSinkingBuffEffectReact", ability => ability.IsBlockingTargetSinkingBuffEffectReact(target, stack, turn, selfAction, selfCoin, isCritical, timing), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.CanRollCoin))]
        [HarmonyPostfix]
        private static void CanRollCoin_Postfix(BattleActionModel action, CoinModel coin, SkillModel __instance, ref bool __result, out bool forceToEndSkill)
        {
            forceToEndSkill = false;
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "CanRollCoin", ability => ability.CanRollCoin(action, coin, out bool forceToEndSkill), out __result);
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnSkillChangedEgo))]
        [HarmonyPostfix]
        private static void OnSkillChangedEgo_Postfix(BattleActionModel action, bool isOverClock, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnSkillChangedEgo", ability => ability.OnSkillChangedEgo(action, isOverClock, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnChangeSkillBeforeCompleteCommand))]
        [HarmonyPostfix]
        private static void OnChangeSkillBeforeCompleteCommand_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnChangeSkillBeforeCompleteCommand", ability => ability.OnChangeSkillBeforeCompleteCommand(action, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnCancelAction))]
        [HarmonyPostfix]
        private static void OnCancelAction_Postfix(BattleActionModel action, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnCancelAction", ability => ability.OnCancelAction(action));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnStartTurn_AfterLog))]
        [HarmonyPostfix]
        private static void OnStartTurn_AfterLog_Postfix(BattleActionModel action, Il2CppSystem.Collections.Generic.List<BattleUnitModel> targets, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnStartTurn_AfterLog", ability => ability.OnStartTurn_AfterLog(action, targets.ToSystem(), timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnAttackCanceledByAbility))]
        [HarmonyPostfix]
        private static void OnAttackCanceledByAbility_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnAttackCanceledByAbility", ability => ability.OnAttackCanceledByAbility(action, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnAfterParryingOnce_BeforeLog))]
        [HarmonyPostfix]
        private static void OnAfterParryingOnce_BeforeLog_Postfix(PARRYING_RESULT reuslt, BattleActionModel ownerAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnAfterParryingOnce_BeforeLog", ability => ability.OnAfterParryingOnce_BeforeLog(reuslt, ownerAction, oppoAction, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnKillTarget))]
        [HarmonyPostfix]
        private static void OnKillTarget_Postfix(BattleActionModel action, CoinModel coinOrNull, BattleUnitModel target, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnKillTarget", ability => ability.OnKillTarget(action, coinOrNull, target, dmgSrcType, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnBreakTarget))]
        [HarmonyPostfix]
        private static void OnBreakTarget_Postfix(BattleActionModel action, CoinModel coinOrNull, BattleUnitModel target, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnBreakTarget", ability => ability.OnBreakTarget(action, coinOrNull, target, dmgSrcType, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnDestroyTargetPart))]
        [HarmonyPostfix]
        private static void OnDestroyTargetPart_Postfix(BattleActionModel action, CoinModel coinOrNull, BattleUnitModel_Abnormality_Part target, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnDestroyTargetPart", ability => ability.OnDestroyTargetPart(action, coinOrNull, target, dmgSrcType, timing));
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnAddCoinByAbility))]
        [HarmonyPostfix]
        private static void OnAddCoinByAbility_Postfix(BattleActionModel action, CoinModel newCoin, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnAddCoinByAbility", ability => ability.OnAddCoinByAbility(action, newCoin, timing));
        }


    }
}
