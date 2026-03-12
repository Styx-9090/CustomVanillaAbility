using CustomVanillaAbility.CustomClasses;
using HarmonyLib;
using Lethe.Patches;
using SimpleJSON;
using System;
using System.Linq;
using static BattleUI.Abnormality.AbnormalityPartSkills;

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

            JSONArray skillAbilityArray = null;
            if (skillFlag || coinFlag)
            {
                if (skillFlag && coinFlag)
                {
                    System.Collections.Generic.HashSet<string> skillSpecialCheck = new();
                    skillSpecialCheck.UnionWith(skillBundle.abilityLookup);
                    skillSpecialCheck.UnionWith(coinBundle.abilityLookup);

                    skillAbilityArray = main.ScanModFiles(main.skillPath, skillSpecialCheck, out System.Collections.Generic.HashSet<int> outSkillHash);

                    skillBundle.affectedLookup = outSkillHash;
                    coinBundle.affectedLookup = outSkillHash;
                }
                else if (skillFlag && !coinFlag) skillAbilityArray = main.ScanModFiles(main.skillPath, skillBundle.abilityLookup, out skillBundle.affectedLookup);
                else skillAbilityArray = main.ScanModFiles(main.skillPath, coinBundle.abilityLookup, out coinBundle.affectedLookup);
            }
        }
    }


    public static class CustomVanillaAbilityPatches_SkillModel
    {
        [HarmonyPatch(nameof(SkillModel.Init))]
        [HarmonyPostfix]
        private static void Init_Postfix(SkillModel __instance)
        {
            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue("skill", out CustomAbilityBundle bundle)) return;

            int skillId = __instance.GetID();
            if (!bundle.affectedLookup.Contains(skillId)) return;

            if (bundle.customAbilityDict.TryGetValue(skillId, out System.Collections.Generic.List<CustomAbilityBase> abilityList))
            {
                CustomVanillaAbilityHelper.RefreshAbilities<CustomSkillAbilityBase>(__instance, abilityList);
                return;
            }

            System.Collections.Generic.List<CustomAbilityBase> newAbilities = new System.Collections.Generic.List<CustomAbilityBase>();
            int baseIndex = __instance.GetAbilityList().Count;

            foreach (AbilityData abilityData in __instance._skillData.abilityScriptList)
            {
                string scriptName = bundle.abilityLookup.FirstOrDefault(x => abilityData.scriptName.Contains(x));
                if (scriptName == null) continue;

                if (!bundle.abilityClassDict.TryGetValue(scriptName, out CustomAbilityBase template)) continue;
                CustomSkillAbilityBase ability = Activator.CreateInstance(template.GetType()) as CustomSkillAbilityBase;

                int idx = baseIndex + newAbilities.Count + 1;
                ability.Init(__instance, scriptName, abilityData.Value, idx, abilityData.TurnLimit, abilityData.BuffData);
                if (abilityData.ConditionalData != null) ability.AttachConditionalData(abilityData.ConditionalData);
                if (abilityData.TurnLimit != 0)  ability.InitLimitedActivateCountData(abilityData.TurnLimit);

                newAbilities.Add(ability);
            }
            bundle.customAbilityDict.Add(skillId, newAbilities);
        }

        [HarmonyPatch(nameof(SkillModel.CanTeamKillOnStableOverclock))]
        [HarmonyPostfix]
        private static void CanTeamKillOnStableOverclock_Postfix(SkillModel __instance, BattleActionModel action, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "CanTeamKillOnStableOverclock", ability => ability.CanTeamKillOnStableOverclock(action), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.IsShow))]
        [HarmonyPostfix]
        private static void IsShow_Postfix(SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsShow", ability => !ability.IsShow(), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.IgnoreDefenseSkill))]
        [HarmonyPostfix]
        private static void IgnoreDefenseSkill_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel target, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IgnoreDefenseSkill", ability => ability.IgnoreDefenseSkill(action, target), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.IsActionable))]
        [HarmonyPostfix]
        private static void IsActionable_Postfix(SkillModel __instance, BattleActionModel action, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsActionable", ability => ability.IsActionable(action), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.IsPanicBlock))]
        [HarmonyPostfix]
        private static void IsPanicBlock_Postfix(SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsPanicBlock", ability => ability.IsPanicBlock(), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.IsSkillAbsorbingThisDamage))]
        [HarmonyPostfix]
        private static void IsSkillAbsorbingThisDamage_Postfix(SkillModel __instance, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsSkillAbsorbingThisDamage", ability => ability.IsSkillAbsorbingThisDamage(), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.CanUseSkill))]
        [HarmonyPostfix]
        private static void CanUseSkill_Postfix(SkillModel __instance, BattleUnitModel actor, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "CanUseSkill", ability => ability.CanUseSkill(actor), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.CanDealTarget))]
        [HarmonyPostfix]
        private static void CanDealTarget_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel target, CoinModel coin, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "CanDealTarget", ability => ability.CanDealTarget(action, target, coin), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetPrimeTargets))]
        [HarmonyPostfix]
        private static void GetPrimeTargets_Postfix(SkillModel __instance, BattleActionModel action, ref Il2CppSystem.Collections.Generic.List<PrimeTargetData> __result)
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

        [HarmonyPatch(nameof(SkillModel.AttackByMpDmgRatherThanHpDmg))]
        [HarmonyPostfix]
        private static void AttackByMpDmgRatherThanHpDmg_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, int resultDmg, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "AttackByMpDmgRatherThanHpDmg", ability => ability.AttackByMpDmgRatherThanHpDmg(action, coin, resultDmg), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.BlockLoseBuffByReactWithAction))]
        [HarmonyPostfix]
        private static void BlockLoseBuffByReactWithAction_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coinOrNull, BUFF_UNIQUE_KEYWORD keyword, bool? isCritical, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "BlockLoseBuffByReactWithAction", ability => ability.BlockLoseBuffByReactWithAction(action, keyword, isCritical), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.BlockGivingBuff))]
        [HarmonyPostfix]
        private static void BlockGivingBuff_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel buffTarget, BUFF_UNIQUE_KEYWORD keyword, CoinModel coinOrNull, bool? isCritical, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "BlockGivingBuff", ability => ability.BlockGivingBuff(action, buffTarget, keyword, coinOrNull, isCritical), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.ExpectedBlockGivingBuff))]
        [HarmonyPostfix]
        private static void ExpectedBlockGivingBuff_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel buffTarget, BUFF_UNIQUE_KEYWORD keyword, CoinModel coinOrNull, bool? isCritical, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "ExpectedBlockGivingBuff", ability => ability.ExpectedBlockGivingBuff(action, buffTarget, keyword, coinOrNull, isCritical), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetSkillLevelAdder))]
        [HarmonyPostfix]
        private static void GetSkillLevelAdder_Postfix(SkillModel __instance, BattleActionModel action, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetSkillLevelAdder", ability => ability.GetSkillLevelAdder(action), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetSkillPowerAdder))]
        [HarmonyPostfix]
        private static void GetSkillPowerAdder_Postfix(SkillModel __instance, BattleActionModel action, COIN_ROLL_TYPE rollType, Il2CppSystem.Collections.Generic.List<CoinModel> coins, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetSkillPowerAdder", ability => ability.GetSkillPowerAdder(action, rollType), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetExpectedSkillPowerAdder))]
        [HarmonyPostfix]
        private static void GetExpectedSkillPowerAdder_Postfix(SkillModel __instance, BattleActionModel action, COIN_ROLL_TYPE rollType, SinActionModel expectedTargetSinActionOrNull, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedSkillPowerAdder", ability => ability.GetExpectedSkillPowerAdder(action, rollType, expectedTargetSinActionOrNull), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetEvadeSkillPowerAdder))]
        [HarmonyPostfix]
        private static void GetEvadeSkillPowerAdder_Postfix(SkillModel __instance, BattleActionModel evadeAction, BattleActionModel attackerAction, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetEvadeSkillPowerAdder", ability => ability.GetEvadeSkillPowerAdder(evadeAction, attackerAction), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetExpectedEvadeSkillPowerAdder))]
        [HarmonyPostfix]
        private static void GetExpectedEvadeSkillPowerAdder_Postfix(SkillModel __instance, BattleActionModel evadeAction, BattleActionModel attackerAction, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedEvadeSkillPowerAdder", ability => ability.GetExpectedEvadeSkillPowerAdder(evadeAction, attackerAction), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetCoinScaleAdder))]
        [HarmonyPostfix]
        private static void GetCoinScaleAdder_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetCoinScaleAdder", ability => ability.GetCoinScaleAdder(action, coin), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetExpectedCoinScaleAdder))]
        [HarmonyPostfix]
        private static void GetExpectedCoinScaleAdder_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, SinActionModel targetSinActionOrNull, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedCoinScaleAdder", ability => ability.GetExpectedCoinScaleAdder(action, coin, targetSinActionOrNull), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetSkillPowerResultAdder))]
        [HarmonyPostfix]
        private static void GetSkillPowerResultAdder_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing, CoinModel coinOrNull, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetSkillPowerResultAdder", ability => ability.GetSkillPowerResultAdder(action, timing), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetEvadeCoinScaleAdder))]
        [HarmonyPostfix]
        private static void GetEvadeCoinScaleAdder_Postfix(SkillModel __instance, BattleActionModel evadeAction, BattleActionModel attackerAction, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetEvadeCoinScaleAdder", ability => ability.GetEvadeCoinScaleAdder(evadeAction, attackerAction), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetExpectedEvadeCoinScaleAdder))]
        [HarmonyPostfix]
        private static void GetExpectedEvadeCoinScaleAdder_Postfix(SkillModel __instance, BattleActionModel evadeAction, BattleActionModel attackerAction, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedEvadeCoinScaleAdder", ability => ability.GetExpectedEvadeCoinScaleAdder(evadeAction, attackerAction), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetExpectedSkillPowerResultAdder))]
        [HarmonyPostfix]
        private static void GetExpectedSkillPowerResultAdder_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel expectedTarget, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedSkillPowerResultAdder", ability => ability.GetExpectedSkillPowerResultAdder(action, expectedTarget), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetParryingResultAdder))]
        [HarmonyPostfix]
        private static void GetParryingResultAdder_Postfix(SkillModel __instance, BattleActionModel actorAction, int actorResult, BattleActionModel oppoAction, int oppoResult, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetParryingResultAdder", ability => ability.GetParryingResultAdder(actorAction, actorResult, oppoAction, oppoResult), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetExpectedParryingResultAdder))]
        [HarmonyPostfix]
        private static void GetExpectedParryingResultAdder_Postfix(SkillModel __instance, BattleActionModel actorAction, int actorResult, BattleActionModel oppoActionOrNull, int oppoResult, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedParryingResultAdder", ability => ability.GetExpectedParryingResultAdder(actorAction, actorResult, oppoActionOrNull, oppoResult), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetOpponentParryingResultAdder))]
        [HarmonyPostfix]
        private static void GetOpponentParryingResultAdder_Postfix(SkillModel __instance, BattleActionModel actorAction, int actorResult, BattleActionModel oppoAction, int oppoResult, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetOpponentParryingResultAdder", ability => ability.GetOpponentParryingResultAdder(actorAction, actorResult, oppoAction, oppoResult), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetExpectedOpponentParryingResultAdder))]
        [HarmonyPostfix]
        private static void GetExpectedOpponentParryingResultAdder_Postfix(SkillModel __instance, BattleActionModel actorAction, int actorResult, BattleActionModel oppoAction, int oppoResult, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedOpponentParryingResultAdder", ability => ability.GetExpectedOpponentParryingResultAdder(actorAction, actorResult, oppoAction, oppoResult), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetAttackDmgMultiplier))]
        [HarmonyPostfix]
        private static void GetAttackDmgMultiplier_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BattleUnitModel target, bool isWinDuel, bool isCritical, ref float __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchFloatLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetAttackDmgMultiplier", ability => ability.GetAttackDmgMultiplier(action, coin, target, isCritical), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetExpectedAttackDmgMultiplier))]
        [HarmonyPostfix]
        private static void GetExpectedAttackDmgMultiplier_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BattleUnitModel targetOrNull, ref float __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchFloatLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedAttackDmgMultiplier", ability => ability.GetExpectedAttackDmgMultiplier(action, targetOrNull, coin), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetAttackDmgAdder))]
        [HarmonyPostfix]
        private static void GetAttackDmgAdder_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BattleUnitModel target, bool isWinDuel, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetAttackDmgAdder", ability => ability.GetAttackDmgAdder(action, target), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetExpectedAttackDmgAdder))]
        [HarmonyPostfix]
        private static void GetExpectedAttackDmgAdder_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BattleUnitModel targetOrNull, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedAttackDmgAdder", ability => ability.GetExpectedAttackDmgAdder(action, targetOrNull), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetAttackHpDmgAdder))]
        [HarmonyPostfix]
        private static void GetAttackHpDmgAdder_Postfix(SkillModel __instance, BattleUnitModel target, CoinModel coin, bool isWinDuel, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetAttackHpDmgAdder", ability => ability.GetAttackHpDmgAdder(target), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetExpectedAttackHpDmgAdder))]
        [HarmonyPostfix]
        private static void GetExpectedAttackHpDmgAdder_Postfix(SkillModel __instance, BattleUnitModel target, CoinModel coin, bool isWinDuel, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetExpectedAttackHpDmgAdder", ability => ability.GetExpectedAttackHpDmgAdder(target), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetCriticalChanceMultiplier))]
        [HarmonyPostfix]
        private static void GetCriticalChanceMultiplier_Postfix(SkillModel __instance, BattleActionModel action, ref float __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchFloatLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetCriticalChanceMultiplier", ability => ability.GetCriticalChanceMultiplier(action), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetCriticalChanceAdder))]
        [HarmonyPostfix]
        private static void GetCriticalChanceAdder_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, ref float __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchFloatLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetCriticalChanceAdder", ability => ability.GetCriticalChanceAdder(action, coin), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.OverwriteCriticalResult))]
        [HarmonyPostfix]
        private static void OverwriteCriticalResult_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, bool tempCritical, ref bool? overwirteCriticalResult, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OverwriteCriticalResult", ability => ability.OverwriteCriticalResult(action, coin, tempCritical, out bool? overwirteCriticalResult), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetCoinProb), new Type[] { typeof(UnitModel), typeof(float) })]
        [HarmonyPostfix]
        private static void GetCoinProb_UnitModel_Postfix(SkillModel __instance, UnitModel unit, float defaultProb, ref float __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchFloatLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetCoinProb", ability => ability.GetCoinProb(defaultProb), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetCoinProb), new Type[] { typeof(BattleUnitModel), typeof(float) })]
        [HarmonyPostfix]
        private static void GetCoinProb_BattleUnitModel_Postfix(SkillModel __instance, BattleUnitModel unit, float defaultProb, ref float __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchFloatLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetCoinProb", ability => ability.GetCoinProb(defaultProb), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetGiveBuffStackAdder))]
        [HarmonyPostfix]
        private static void GetGiveBuffStackAdder_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coinOrNull, BattleUnitModel target, BUFF_UNIQUE_KEYWORD keyword, int stack, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetGiveBuffStackAdder", ability => ability.GetBuffStackAdder(action, coinOrNull, target, keyword, stack), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetUseBuffTurnAdder))]
        [HarmonyPostfix]
        private static void GetUseBuffTurnAdder_Postfix(SkillModel __instance, BattleActionModel action, int turn, BUFF_UNIQUE_KEYWORD buf, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetUseBuffTurnAdder", ability => ability.GetUseBuffTurnAdder(action, turn, buf), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetGiveBuffTurnAdder))]
        [HarmonyPostfix]
        private static void GetGiveBuffTurnAdder_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel target, BUFF_UNIQUE_KEYWORD keyword, int turn, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetGiveBuffTurnAdder", ability => ability.GetBuffTurnAdder(action, target, keyword, turn), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.BeforeAttack))]
        [HarmonyPostfix]
        private static void BeforeAttack_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "BeforeAttack", ability => ability.BeforeAttack(action, timing));
        }

        [HarmonyPatch(nameof(SkillModel.IsRetreatSkill))]
        [HarmonyPostfix]
        private static void IsRetreatSkill_Postfix(SkillModel __instance, BattleActionModel action, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsRetreatSkill", ability => ability.IsRetreatSkill(action), out __result);
        }

        /*
        [HarmonyPatch(nameof(SkillModel.OverwriteTargetableList))]
        [HarmonyPostfix]
        private static void OverwriteTargetableList_Postfix(SkillModel __instance, BattleActionModel action, Il2CppSystem.Collections.Generic.List<SinActionModel> targetableSlotListOrNull, Il2CppSystem.Collections.Generic.List<BattleUnitModel> targetableUnitListOrNull, Il2CppSystem.Collections.Generic.List<SinActionModel> addedSlotListOrNull)
        {

        }
        */

        [HarmonyPatch(nameof(SkillModel.GetAdditionalActivateCountForDefenseSkill))]
        [HarmonyPostfix]
        private static void GetAdditionalActivateCountForDefenseSkill_Postfix(SkillModel __instance, BattleUnitModel owner, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetAdditionalActivateCountForDefenseSkill", ability => ability.GetAdditionalActivateCountForDefenseSkill(owner), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.ChangeAttackDamage))]
        [HarmonyPostfix]
        private static void ChangeAttackDamage_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel target, CoinModel coin, int resultDmg, bool isCritical, bool? isWinDuel, BATTLE_EVENT_TIMING timing, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "ChangeAttackDamage", ability => ability.ChangeAttackDamage(action, target, coin, resultDmg, isCritical, timing), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.GetGiveBsGaugeUpMultiplier))]
        [HarmonyPostfix]
        private static void GetGiveBsGaugeUpMultiplier_Postfix(SkillModel __instance, bool onGiveExplosion, BattleUnitModel target, BattleActionModel action, CoinModel coinOrNull, ref float __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchFloatLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "GetGiveBsGaugeUpMultiplier", ability => ability.GetGiveBsGaugeUpMultiplier(onGiveExplosion, target, action, coinOrNull), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.OverwriteSkillIconID))]
        [HarmonyPostfix]
        private static void OverwriteSkillIconID_Postfix(SkillModel __instance, ref string __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchStringLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OverwriteSkillIconID", ability => ability.OverwriteSkillIconID(), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.OnUseCoinConsume))]
        [HarmonyPostfix]
        private static void OnUseCoinConsume_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnUseCoinConsume", ability => ability.OnUseCoinConsume(action, coin, keyword, stack, turn, timing));
        }

        [HarmonyPatch(nameof(SkillModel.RightBeforeGiveBuffBySkill))]
        [HarmonyPostfix]
        private static void RightBeforeGiveBuffBySkill_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel target, BUFF_UNIQUE_KEYWORD bufKeyword, int originalStack, int originalTurn, int activeRound, BATTLE_EVENT_TIMING timing, bool? isCritical)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "RightBeforeGiveBuffBySkill", ability => ability.RightBeforeGiveBuffBySkill(action, target, bufKeyword, originalStack, originalTurn, activeRound, timing, isCritical));
        }

        [HarmonyPatch(nameof(SkillModel.RightAfterGiveBuffBySkill))]
        [HarmonyPostfix]
        private static void RightAfterGiveBuffBySkill_Postfix(SkillModel __instance, BattleActionModel action, BattleUnitModel target, BUFF_UNIQUE_KEYWORD bufKeyword, int originalStack, int originalTurn, int resultStack, int resultTurn, int activeRound, BATTLE_EVENT_TIMING timing, bool? isCritical)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "RightAfterGiveBuffBySkill", ability => ability.RightAfterGiveBuffBySkill(action, target, bufKeyword, originalStack, originalTurn, resultStack, resultTurn, activeRound, timing, isCritical));
        }

        [HarmonyPatch(nameof(SkillModel.OnBattleStart))]
        [HarmonyPostfix]
        private static void OnBattleStart_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnBattleStart", ability => ability.OnBattleStart(action, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnRoundEnd))]
        [HarmonyPostfix]
        private static void OnRoundEnd_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnRoundEnd", ability => ability.OnRoundEnd(action, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnBeforeTurn))]
        [HarmonyPostfix]
        private static void OnBeforeTurn_Postfix(SkillModel __instance, BattleActionModel action)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnBeforeTurn", ability => ability.OnBeforeTurn(action));
        }

        [HarmonyPatch(nameof(SkillModel.OnBeforeDefense))]
        [HarmonyPostfix]
        private static void OnBeforeDefense_Postfix(SkillModel __instance, BattleActionModel action)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnBeforeDefense", ability => ability.OnBeforeDefense(action));
        }

        [HarmonyPatch(nameof(SkillModel.OnStartTurn_BeforeLog))]
        [HarmonyPostfix]
        private static void OnStartTurn_BeforeLog_Postfix(SkillModel __instance, BattleActionModel action, Il2CppSystem.Collections.Generic.List<BattleUnitModel> targets, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnStartTurn_BeforeLog", ability => ability.OnStartTurn_BeforeLog(action, targets.ToSystem(), timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnTryEvade))]
        [HarmonyPostfix]
        private static void OnTryEvade_Postfix(SkillModel __instance, BattleActionModel action, BattleActionModel attackerAction, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnTryEvade", ability => ability.OnTryEvade(action, attackerAction, timing));
        }

        [HarmonyPatch(nameof(SkillModel.AfterRecheckTargetList))]
        [HarmonyPostfix]
        private static void AfterRecheckTargetList_Postfix(SkillModel __instance, BattleActionModel action, bool valid, bool mainTargetAlive)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "AfterRecheckTargetList", ability => ability.AfterRecheckTargetList(action, valid, mainTargetAlive));
        }

        [HarmonyPatch(nameof(SkillModel.OnStartDuel))]
        [HarmonyPostfix]
        private static void OnStartDuel_Postfix(SkillModel __instance, BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnStartDuel", ability => ability.OnStartDuel(selfAction, oppoAction, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnBeforeParryingOnce))]
        [HarmonyPostfix]
        private static void OnBeforeParryingOnce_Postfix(SkillModel __instance, BattleActionModel ownerAction, BattleActionModel oppoAction)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnBeforeParryingOnce", ability => ability.OnBeforeParryingOnce(ownerAction, oppoAction));
        }

        [HarmonyPatch(nameof(SkillModel.OnBeforeParryingOnce_AfterLog))]
        [HarmonyPostfix]
        private static void OnBeforeParryingOnce_AfterLog_Postfix(SkillModel __instance, BattleActionModel ownerAction, BattleActionModel oppoAction)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnBeforeParryingOnce_AfterLog", ability => ability.OnBeforeParryingOnce(ownerAction, oppoAction));
        }

        [HarmonyPatch(nameof(SkillModel.OnDuelAfter_BeforeLog))]
        [HarmonyPostfix]
        private static void OnDuelAfter_BeforeLog_Postfix(SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnDuelAfter_BeforeLog", ability => ability.OnDuelAfter_BeforeLog());
        }

        [HarmonyPatch(nameof(SkillModel.OnDuelAfter_AfterLog))]
        [HarmonyPostfix]
        private static void OnDuelAfter_AfterLog_Postfix(SkillModel __instance)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnDuelAfter_AfterLog", ability => ability.OnDuelAfter_AfterLog());
        }

        [HarmonyPatch(nameof(SkillModel.OnWinParrying))]
        [HarmonyPostfix]
        private static void OnWinParrying_Postfix(SkillModel __instance, BattleActionModel selfAction, BattleActionModel oppoAction)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnWinParrying", ability => ability.OnWinParrying(selfAction, oppoAction));
        }

        [HarmonyPatch(nameof(SkillModel.OnLoseParrying))]
        [HarmonyPostfix]
        private static void OnLoseParrying_Postfix(SkillModel __instance, BattleActionModel selfAction, BattleActionModel oppoAction)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnLoseParrying", ability => ability.OnLoseParrying(selfAction, oppoAction));
        }

        [HarmonyPatch(nameof(SkillModel.OnWinDuel))]
        [HarmonyPostfix]
        private static void OnWinDuel_Postfix(SkillModel __instance, BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing, int parryingCount, BattleLog_Parrying lastLogOrNull)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnWinDuel", ability => ability.OnWinDuel(selfAction, oppoAction, timing, parryingCount, lastLogOrNull));
        }

        [HarmonyPatch(nameof(SkillModel.OnLoseDuel))]
        [HarmonyPostfix]
        private static void OnLoseDuel_Postfix(SkillModel __instance, BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnLoseDuel", ability => ability.OnLoseDuel(selfAction, oppoAction, timing));
        }

        [HarmonyPatch(nameof(SkillModel.BeforeBehaviour))]
        [HarmonyPostfix]
        private static void BeforeBehaviour_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "BeforeBehaviour", ability => ability.BeforeBehaviour(action, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnStartBehaviour))]
        [HarmonyPostfix]
        private static void OnStartBehaviour_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnStartBehaviour", ability => ability.OnStartBehaviour(action, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnCriticalIsActivated))]
        [HarmonyPostfix]
        private static void OnCriticalIsActivated_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing, Il2CppSystem.Collections.Generic.Dictionary<BUFF_UNIQUE_KEYWORD, float> affectKeywords)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnCriticalIsActivated", ability => ability.OnCriticalIsActivated(action, coin, timing, affectKeywords.ConvertDictionary()));
        }

        [HarmonyPatch(nameof(SkillModel.OnAttackConfirmed))]
        [HarmonyPostfix]
        private static void OnAttackConfirmed_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BattleUnitModel target, BATTLE_EVENT_TIMING timing, bool isCritical)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnAttackConfirmed", ability => ability.OnAttackConfirmed(action, target, timing));
        }

        [HarmonyPatch(nameof(SkillModel.BeforeGiveAttackDamage))]
        [HarmonyPostfix]
        private static void BeforeGiveAttackDamage_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BattleUnitModel target, bool? isWinDuel, bool isCritical, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "BeforeGiveAttackDamage", ability => ability.BeforeGiveAttackDamage(action, target, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnSucceedAttack))]
        [HarmonyPostfix]
        private static void OnSucceedAttack_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BattleUnitModel target, int finalDmg, int realDmg, bool isCritical, bool? isWinDuel, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnSucceedAttack", ability => ability.OnSucceedAttack(action, coin, target, finalDmg, realDmg, isCritical, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnSucceedEvade))]
        [HarmonyPostfix]
        private static void OnSucceedEvade_Postfix(SkillModel __instance, BattleActionModel attackerAction, BattleActionModel evadeAction, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnSucceedEvade", ability => ability.OnSucceedEvade(attackerAction, evadeAction, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnFailedEvade))]
        [HarmonyPostfix]
        private static void OnFailedEvade_Postfix(SkillModel __instance, BattleActionModel attackerAction, BattleActionModel evadeAction, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnFailedEvade", ability => ability.OnFailedEvade(attackerAction, evadeAction, timing));
        }

        [HarmonyPatch(nameof(SkillModel.BeforeCompleteCommand))]
        [HarmonyPostfix]
        private static void BeforeCompleteCommand_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing, ref int newSkillID, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "BeforeCompleteCommand", ability => ability.BeforeCompleteCommand(action, timing, out int newSkillID), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.OnCompleteCommand))]
        [HarmonyPostfix]
        private static void OnCompleteCommand_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnCompleteCommand", ability => ability.OnCompleteCommand(action, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnStartCoin))]
        [HarmonyPostfix]
        private static void OnStartCoin_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnStartCoin", ability => ability.OnStartCoin(action, coin));
        }

        [HarmonyPatch(nameof(SkillModel.OnEndCoin_BeforeLog))]
        [HarmonyPostfix]
        private static void OnEndCoin_BeforeLog_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, bool isCritical, bool? isWinDuel, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnEndCoin_BeforeLog", ability => ability.OnEndCoin_BeforeLog(action, coin, isCritical, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnEndCoin_AfterLog))]
        [HarmonyPostfix]
        private static void OnEndCoin_AfterLog_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, bool isCritical, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnEndCoin_AfterLog", ability => ability.OnEndCoin_AfterLog(action, coin, isCritical, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnEndAttack))]
        [HarmonyPostfix]
        private static void OnEndAttack_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnEndAttack", ability => ability.OnEndAttack(action, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnGiveBsGaugeUp))]
        [HarmonyPostfix]
        private static void OnGiveBsGaugeUp_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BattleUnitModel target, int value, BATTLE_EVENT_TIMING timing, bool onExplosion)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnGiveBsGaugeUp", ability => ability.OnGiveBsGaugeUp(action, target, value, timing, onExplosion));
        }

        [HarmonyPatch(nameof(SkillModel.OnEndBehaviour))]
        [HarmonyPostfix]
        private static void OnEndBehaviour_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnEndBehaviour", ability => ability.OnEndBehaviour(action, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnEndBehave_Refresh))]
        [HarmonyPostfix]
        private static void OnEndBehave_Refresh_Postfix(SkillModel __instance, BattleActionModel action)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnEndBehave_Refresh", ability => ability.OnEndBehave_Refresh(action));
        }

        [HarmonyPatch(nameof(SkillModel.OnEndTurn))]
        [HarmonyPostfix]
        private static void OnEndTurn_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnEndTurn", ability => ability.OnEndTurn(action, timing));
        }

        [HarmonyPatch(nameof(SkillModel.DoneWithAction))]
        [HarmonyPostfix]
        private static void DoneWithAction_Postfix(SkillModel __instance, BattleActionModel action)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "DoneWithAction", ability => ability.DoneWithAction(action));
        }

        [HarmonyPatch(nameof(SkillModel.OnDiscarded))]
        [HarmonyPostfix]
        private static void OnDiscarded_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnDiscarded", ability => ability.OnDiscarded(action, timing));
        }

        [HarmonyPatch(nameof(SkillModel.StackNextTurnAggroAdder))]
        [HarmonyPostfix]
        private static void StackNextTurnAggroAdder_Postfix(SkillModel __instance, ref int __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchIntLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "StackNextTurnAggroAdder", ability => ability.StackNextTurnAggroAdder(), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.OverrideCanDuel))]
        [HarmonyPostfix]
        private static void OverwriteCanDuel_Postfix(SkillModel __instance, BattleActionModel action, ref bool __result, BattleActionModel opponentActionOrNull = null)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OverwriteCanDuel", ability => ability.OverwriteCanDuel(action, out bool canDuel, opponentActionOrNull), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.CanBeChangedTarget))]
        [HarmonyPostfix]
        private static void CanBeChangedTarget_Postfix(SkillModel __instance, BattleActionModel ownerAction, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "CanBeChangedTarget", ability => ability.CanBeChangedTarget(ownerAction, out bool canChangedTarget), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.CanChangeMainTargetRegardlessSpeed))]
        [HarmonyPostfix]
        private static void CanChangeMainTargetRegardlessSpeed_Postfix(SkillModel __instance, BattleActionModel otherAction, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "CanChangeMainTargetRegardlessSpeed", ability => ability.CanBeChangedTargetIgnoreSpeed(), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.CanBeChangedTargetIgnoreSpeed))]
        [HarmonyPostfix]
        private static void CanBeChangedTargetIgnoreSpeed_Postfix(SkillModel __instance, BattleActionModel action, BattleActionModel otherAction, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "CanBeChangedTargetIgnoreSpeed", ability => ability.CanBeChangedTargetIgnoreSpeed(), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.IsDefenseSkillForOther))]
        [HarmonyPostfix]
        private static void IsDefenseSkillForOther_Postfix(SkillModel __instance, BattleUnitModel self, BattleUnitModel originTarget, BattleActionModel opponentActionOrNull, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsDefenseSkillForOther", ability => ability.IsDefenseSkillForOther(self, originTarget, opponentActionOrNull), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.CanCheckErode))]
        [HarmonyPostfix]
        private static void CanCheckErode_Postfix(SkillModel __instance, BattleActionModel action, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "CanCheckErode", ability => ability.CanCheckErode(action), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.IsReusable))]
        [HarmonyPostfix]
        private static void IsReusable_Postfix(SkillModel __instance, BattleActionModel action, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsReusable", ability => ability.IsReusable(action), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.IsChangeable))]
        [HarmonyPostfix]
        private static void IsChangeable_Postfix(SkillModel __instance, BattleActionModel action, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsChangeable", ability => ability.IsChangeable(action), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.BlockAddSinStock))]
        [HarmonyPostfix]
        private static void BlockAddSinStock_Postfix(SkillModel __instance, BattleActionModel action, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "BlockAddSinStock", ability => ability.BlockAddSinStock(action), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.IsBlockingTargetBurstBuffEffectReact))]
        [HarmonyPostfix]
        private static void IsBlockingTargetBurstBuffEffectReact_Postfix(SkillModel __instance, BattleUnitModel target, int stack, int turn, BattleActionModel selfAction, CoinModel selfCoin, bool isCritical, BATTLE_EVENT_TIMING timing, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsBlockingTargetBurstBuffEffectReact", ability => ability.IsBlockingTargetBurstBuffEffectReact(target, stack, turn, selfAction, selfCoin, isCritical, timing), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.IsBlockingTargetSinkingBuffEffectReact))]
        [HarmonyPostfix]
        private static void IsBlockingTargetSinkingBuffEffectReact_Postfix(SkillModel __instance, BattleUnitModel target, int stack, int turn, BattleActionModel selfAction, CoinModel selfCoin, bool isCritical, BATTLE_EVENT_TIMING timing, ref bool __result)
        {
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "IsBlockingTargetSinkingBuffEffectReact", ability => ability.IsBlockingTargetSinkingBuffEffectReact(target, stack, turn, selfAction, selfCoin, isCritical, timing), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.CanRollCoin))]
        [HarmonyPostfix]
        private static void CanRollCoin_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, ref bool __result, out bool forceToEndSkill)
        {
            forceToEndSkill = false;
            CustomVanillaAbilityHelper.ProcessPatchBoolLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "CanRollCoin", ability => ability.CanRollCoin(action, coin, out bool forceToEndSkill), out __result);
        }

        [HarmonyPatch(nameof(SkillModel.OnSkillChangedEgo))]
        [HarmonyPostfix]
        private static void OnSkillChangedEgo_Postfix(SkillModel __instance, BattleActionModel action, bool isOverClock, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnSkillChangedEgo", ability => ability.OnSkillChangedEgo(action, isOverClock, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnChangeSkillBeforeCompleteCommand))]
        [HarmonyPostfix]
        private static void OnChangeSkillBeforeCompleteCommand_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnChangeSkillBeforeCompleteCommand", ability => ability.OnChangeSkillBeforeCompleteCommand(action, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnCancelAction))]
        [HarmonyPostfix]
        private static void OnCancelAction_Postfix(SkillModel __instance, BattleActionModel action)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnCancelAction", ability => ability.OnCancelAction(action));
        }

        [HarmonyPatch(nameof(SkillModel.OnStartTurn_AfterLog))]
        [HarmonyPostfix]
        private static void OnStartTurn_AfterLog_Postfix(SkillModel __instance, BattleActionModel action, Il2CppSystem.Collections.Generic.List<BattleUnitModel> targets, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnStartTurn_AfterLog", ability => ability.OnStartTurn_AfterLog(action, targets.ToSystem(), timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnAttackCanceledByAbility))]
        [HarmonyPostfix]
        private static void OnAttackCanceledByAbility_Postfix(SkillModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnAttackCanceledByAbility", ability => ability.OnAttackCanceledByAbility(action, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnAfterParryingOnce_BeforeLog))]
        [HarmonyPostfix]
        private static void OnAfterParryingOnce_BeforeLog_Postfix(SkillModel __instance, PARRYING_RESULT reuslt, BattleActionModel ownerAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnAfterParryingOnce_BeforeLog", ability => ability.OnAfterParryingOnce_BeforeLog(reuslt, ownerAction, oppoAction, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnKillTarget))]
        [HarmonyPostfix]
        private static void OnKillTarget_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coinOrNull, BattleUnitModel target, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnKillTarget", ability => ability.OnKillTarget(action, coinOrNull, target, dmgSrcType, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnBreakTarget))]
        [HarmonyPostfix]
        private static void OnBreakTarget_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coinOrNull, BattleUnitModel target, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnBreakTarget", ability => ability.OnBreakTarget(action, coinOrNull, target, dmgSrcType, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnDestroyTargetPart))]
        [HarmonyPostfix]
        private static void OnDestroyTargetPart_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coinOrNull, BattleUnitModel_Abnormality_Part target, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnDestroyTargetPart", ability => ability.OnDestroyTargetPart(action, coinOrNull, target, dmgSrcType, timing));
        }

        [HarmonyPatch(nameof(SkillModel.OnAddCoinByAbility))]
        [HarmonyPostfix]
        private static void OnAddCoinByAbility_Postfix(SkillModel __instance, BattleActionModel action, CoinModel newCoin, BATTLE_EVENT_TIMING timing)
        {
            CustomVanillaAbilityHelper.ProcessPatchVoidLogic<CustomSkillAbilityBase>("skill", __instance.GetID(), "OnAddCoinByAbility", ability => ability.OnAddCoinByAbility(action, newCoin, timing));
        }


    }
}
