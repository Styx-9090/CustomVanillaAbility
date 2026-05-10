using CustomVanillaAbility.CustomClasses;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace CustomVanillaAbility.Patches
{
    public static class CustomVanillaAbilityPatches_BattleUnitModel
    {
        public static CustomSkillAbilityBundle _skillBundle = new();
        public static CustomPassiveAbilityBundle _passiveBundle = new();

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.BeforeTakeAttackDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BeforeTakeAttackDamage_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, BattleUnitModel __instance)
        {
            if (_skillBundle.ProcessPatchListLogic(action.GetSkillID(), action.Skill, out System.Collections.Generic.List<CustomAbilityBase> abilityList))
            {
                string methodName = nameof(BattleUnitModel.BeforeGiveAttackDamage);

                foreach (CustomAbilityBase ability in abilityList)
                {
                    if (ability is not CustomSkillAbilityBase realAbility) continue;
                    if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                    try { realAbility.BeforeGiveAttackDamage(action, __instance, timing); }
                    catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
                }
            }

            var passiveList = action.Model.GetAllActivatedPassives();
            for (int i = 0, count = passiveList.Count; i < count; i++)
            {
                PassiveModel passive = passiveList[i];
                int id = passive.GetID();

                if (_passiveBundle.ProcessPatchListLogic(id, passive, out CustomPassiveAbilityHolder holder)) holder.BeforeGiveAttackDamage(action, __instance, timing);
            }

            passiveList = __instance.GetAllActivatedPassives();
            for (int i = 0, count = passiveList.Count; i < count; i++)
            {
                PassiveModel passive = passiveList[i];
                int id = passive.GetID();

                if (_passiveBundle.ProcessPatchListLogic(id, passive, out CustomPassiveAbilityHolder holder)) holder.BeforeTakeAttackDamage(action, timing);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.OnTakeAttackDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeAttackDamage_Postfix(BattleActionModel action, CoinModel coin, int realDmg, int hpDamage, BATTLE_EVENT_TIMING timing, bool isCritical, BattleUnitModel __instance)
        {
            if (_skillBundle.ProcessPatchListLogic(action.GetSkillID(), action.Skill, out System.Collections.Generic.List<CustomAbilityBase> abilityList))
            {
                string methodName = nameof(BattleUnitModel.OnSucceedAttack);

                foreach (CustomAbilityBase ability in abilityList)
                {
                    if (ability is not CustomSkillAbilityBase realAbility) continue;
                    if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                    try { realAbility.OnSucceedAttack(action, coin, __instance, hpDamage, realDmg, isCritical, timing); }
                    catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
                }
            }

            var passiveList = action.Model.GetAllActivatedPassives();
            for (int i = 0, count = passiveList.Count; i < count; i++)
            {
                PassiveModel passive = passiveList[i];
                int id = passive.GetID();

                if (_passiveBundle.ProcessPatchListLogic(id, passive, out CustomPassiveAbilityHolder holder)) holder.OnSucceedAttack(action, coin, __instance, hpDamage, realDmg, isCritical, timing);
            }

            passiveList = __instance.GetAllActivatedPassives();
            for (int i = 0, count = passiveList.Count; i < count; i++)
            {
                PassiveModel passive = passiveList[i];
                int id = passive.GetID();

                if (_passiveBundle.ProcessPatchListLogic(id, passive, out CustomPassiveAbilityHolder holder)) holder.OnTakeAttackDamage(action, coin, realDmg, hpDamage, timing);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.OnEndCoin_BeforeLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndCoin_BeforeLog_Postfix(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing, BattleUnitModel __instance)
        {
            if (!_skillBundle.ProcessPatchListLogic(action.GetSkillID(), action.Skill, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(BattleUnitModel.OnEndCoin_BeforeLog);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnEndCoin_BeforeLog(action, coin, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------------------------//

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetCoinScaleAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetCoinScaleAdder_Postfix(BattleActionModel action, BattleActionModel oppoActionOrNull, CoinModel coin, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetCoinScaleAdder(action, oppoActionOrNull, coin);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetExpectedCoinScaleAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedCoinScaleAdder_Postfix(BattleActionModel action, CoinModel coin, COIN_ROLL_TYPE rollType, SinActionModel targetSinActionOrNull, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedCoinScaleAdder(action, coin, rollType, targetSinActionOrNull);
            }
        }

        // ============================================
        // INT Adders
        // ============================================

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetTakeBuffTurnAdderOtherUnit))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetTakeBuffTurnAdderOtherUnit_Postfix(BattleUnitModel taker, BattleActionModel action, BUFF_UNIQUE_KEYWORD keyword, int originalTurn, BATTLE_EVENT_TIMING timing, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetTakeBuffTurnAdderOtherUnit(taker, action, keyword, originalTurn, timing);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetGiveBuffStackAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetGiveBuffStackAdder_Postfix(BattleActionModel action, SkillModel skill, BattleUnitModel target, BUFF_UNIQUE_KEYWORD buf, int turn, BATTLE_EVENT_TIMING timing, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetGiveBuffStackAdder(action, skill, target, buf, turn, timing);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetGiveBuffTurnAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetGiveBuffTurnAdder_Postfix(BattleActionModel action, SkillModel skill, CoinModel coinOrNull, BattleUnitModel target, BUFF_UNIQUE_KEYWORD buf, int currentStack, int currentTurn, BATTLE_EVENT_TIMING timing, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetGiveBuffTurnAdder(action, skill, coinOrNull, target, buf, currentStack, currentTurn, timing);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetTakeBuffStackAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetTakeBuffStackAdder_Postfix(BattleActionModel action, SkillModel skill, BUFF_UNIQUE_KEYWORD buf, int originStack, BATTLE_EVENT_TIMING timing, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetTakeBuffStackAdder(action, skill, buf, originStack, timing);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetTakeBuffTurnAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetTakeBuffTurnAdder_Postfix(BattleActionModel action, SkillModel skill, BUFF_UNIQUE_KEYWORD buf, int originalTurn, BATTLE_EVENT_TIMING timing, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetTakeBuffTurnAdder(action, skill, buf, originalTurn, timing);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetAggroAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAggroAdder_Postfix(SinActionModel sinaction, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetAggroAdder(sinaction);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetAttackWeightAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAttackWeightAdder_Postfix(BattleActionModel action, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetAttackWeightAdder(action);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetMentalSystemResultIncreaseAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetMentalSystemResultIncreaseAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetMentalSystemResultIncreaseAdder();
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetMentalSystemResultDecreaseAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetMentalSystemResultDecreaseAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetMentalSystemResultDecreaseAdder();
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetUseBuffTurnAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetUseBuffTurnAdder_Postfix(BattleActionModel action, SkillModel skill, int turn, BUFF_UNIQUE_KEYWORD buf, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetUseBuffTurnAdder(action, skill, turn, buf);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetSinBuffDamageAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetSinBuffDamageAdder_Postfix(BUFF_UNIQUE_KEYWORD buff, int dmg, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetSinBuffDamageAdder(buff, dmg);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetMaxBuffStackAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetMaxBuffStackAdder_Postfix(BUFF_UNIQUE_KEYWORD keyword, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetMaxBuffStackAdder(keyword);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetMaxBuffTurnAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetMaxBuffTurnAdder_Postfix(BUFF_UNIQUE_KEYWORD keyword, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetMaxBuffTurnAdder(keyword);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetTakeHpHealAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetTakeHpHealAdder_Postfix(BattleUnitModel healerOrNull, ABILITY_SOURCE_TYPE srcType, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetTakeHpHealAdder(healerOrNull, srcType);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetActionSlotAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetActionSlotAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetActionSlotAdder();
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetMaxHpAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetMaxHpAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetMaxHpAdder();
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetMaxHpAdderPart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetMaxHpAdderPart_Postfix(BattleUnitModel_Abnormality_Part part, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetMaxHpAdderPart(part);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetSpeedAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetSpeedAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetSpeedAdder();
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetMaxSpeedAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetMaxSpeedAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetMaxSpeedAdder();
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetMinSpeedAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetMinSpeedAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetMinSpeedAdder();
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetParryingResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetParryingResultAdder_Postfix(BattleActionModel action, int actorResult, BattleActionModel oppoAction, int oppoResult, int parryingCount, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetParryingResultAdder(action, actorResult, oppoAction, oppoResult, parryingCount);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetExpectedParryingResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedParryingResultAdder_Postfix(BattleActionModel action, int actorResult, BattleActionModel oppoActionOrNull, int oppoResult, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedParryingResultAdder(action, actorResult, oppoActionOrNull, oppoResult);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetOpponentParryingResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetOpponentParryingResultAdder_Postfix(BattleActionModel action, int actorResult, BattleActionModel oppoAction, int oppoResult, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetOpponentParryingResultAdder(action, actorResult, oppoAction, oppoResult);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetExpectedOpponentParryingResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedOpponentParryingResultAdder_Postfix(BattleActionModel action, int actorResult, BattleActionModel oppoAction, int oppoResult, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedOpponentParryingResultAdder(action, actorResult, oppoAction, oppoResult);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetSkillPowerAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetSkillPowerAdder_Postfix(BattleActionModel action, COIN_ROLL_TYPE rollType, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetSkillPowerAdder(action, rollType);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetExpectedSkillPowerAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedSkillPowerAdder_Postfix(BattleActionModel action, Il2CppSystem.Collections.Generic.List<BattleActionModel> prevActions, COIN_ROLL_TYPE rollType, SinActionModel expectedTargetSinActionOrNull, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedSkillPowerAdder(action, prevActions, rollType, expectedTargetSinActionOrNull);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetSkillPowerResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetSkillPowerResultAdder_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetSkillPowerResultAdder(action, timing);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetExpectedSkillPowerResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedSkillPowerResultAdder_Postfix(BattleActionModel action, Il2CppSystem.Collections.Generic.List<BattleActionModel> prevActions, BattleUnitModel expectedTarget, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedSkillPowerResultAdder(action, prevActions, expectedTarget);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetAttackAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAttackAdder_Postfix(BattleActionModel actionOrNull, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetAttackAdder(actionOrNull);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetDefenseAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetDefenseAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetDefenseAdder();
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetAttackDmgAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAttackDmgAdder_Postfix(BattleActionModel action, BattleUnitModel target, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetAttackDmgAdder(action, target);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetExpectedAttackDmgAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedAttackDmgAdder_Postfix(BattleActionModel action, BattleUnitModel targetOrNull, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedAttackDmgAdder(action, targetOrNull);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetTakeAttackDmgAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetTakeAttackDmgAdder_Postfix(BattleActionModel action, BattleUnitModel attacker, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetTakeAttackDmgAdder(action, attacker);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetAttackDmgAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAttackHpDmgAdder_Postfix(BattleActionModel action, BattleUnitModel target, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetAttackHpDmgAdder(target);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetExpectedAttackDmgAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedAttackHpDmgAdder_Postfix(BattleActionModel action, BattleUnitModel targetOrNull, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedAttackHpDmgAdder(action, targetOrNull);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetTakeHpDmgAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetTakeHpDmgAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetTakeHpDmgAdder();
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetExpectedTakeHpDmgAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedTakeHpDmgAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedTakeHpDmgAdder();
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.ChangeTakeDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void ChangeTakeDamage_Postfix(BattleActionModel action, CoinModel coinOrNull, int resultDmg, DAMAGE_SOURCE_TYPE dmgSrcType, BUFF_UNIQUE_KEYWORD keyword, BATTLE_EVENT_TIMING timing, BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result = abilityHolder.ChangeTakeDamage(action, coinOrNull, __result, dmgSrcType, keyword, timing);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetCoinProbAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetCoinProbAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetCoinProbAdder();
            }
        }

        // ============================================
        // FLOAT Multipliers
        // ============================================

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetTakeBuffStackMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetTakeBuffStackMultiplier_Postfix(SkillModel skill, BUFF_UNIQUE_KEYWORD buf, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetTakeBuffStackMultiplier(skill, buf);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetTakeBuffTurnMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetTakeBuffTurnMultiplier_Postfix(SkillModel skill, BUFF_UNIQUE_KEYWORD buf, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetTakeBuffTurnMultiplier(skill, buf);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetSinBuffDamageMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetSinBuffDamageMultiplier_Postfix(BUFF_UNIQUE_KEYWORD buff, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetSinBuffDamageMultiplier(buff);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetTakeHpHealMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetTakeHpHealMultiplier_Postfix(BattleUnitModel healerOrNull, ABILITY_SOURCE_TYPE srcType, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetTakeHpHealMultiplier(healerOrNull, srcType);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetTakeHpHealMultiplierPart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetTakeHpHealMultiplierPart_Postfix(BattleUnitModel_Abnormality_Part part, BattleUnitModel healerOrNull, ABILITY_SOURCE_TYPE srcType, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetTakeHpHealMultiplierPart(part, healerOrNull, srcType);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetGiveBsGaugeUpMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetGiveBsGaugeUpMultiplier_Postfix(bool onGiveExplosion, BattleActionModel actionOrNull, CoinModel coinOrNull, BattleUnitModel target, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetGiveBsGaugeUpMultiplier(onGiveExplosion, actionOrNull, coinOrNull);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetTakeBsGaugeUpMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetBsGaugeUpMultiplier_Postfix(bool onGiveExplosion, BattleActionModel actionOrNull, CoinModel coinOrNull, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetBsGaugeUpMultiplier(onGiveExplosion, actionOrNull, coinOrNull);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetMaxHpMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetMaxHpMultiplier_Postfix(BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetMaxHpMultiplier();
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetMaxHpMultiplierPartToAbnormality))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetMaxHpMultiplierPartToAbnormality_Postfix(BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetMaxHpMultiplierPartToAbnormality();
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetAtkResistAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAtkResistAdder_Postfix(ATK_BEHAVIOUR type, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetAtkResistAdder(type);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetAtkResistMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAtkResistMultiplier_Postfix(ATK_BEHAVIOUR type, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetAtkResistMultiplier(type);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetDefenseMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetDefenseMultiplier_Postfix(BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetDefenseMultiplier();
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetAttributeResistAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAttributeResistAdder_Postfix(global::ATTRIBUTE_TYPE type, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetAttributeResistAdder(type);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetAttributeResistMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAttributeResistMultiplier_Postfix(global::ATTRIBUTE_TYPE type, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetAttributeResistMultiplier(type);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetAttackDmgMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAttackDmgMultiplier_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel target, bool isWinDuel, bool isCritical, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetAttackDmgMultiplier(action, coin, target, isWinDuel, isCritical);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetExpectedAttackDmgMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedAttackDmgMultiplier_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel targetOrNull, SinActionModel targetSinActionOrNull, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedAttackDmgMultiplier(action, coin, targetOrNull, targetSinActionOrNull);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetTakeAttackDmgMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetTakeAttackDmgMultiplier_Postfix(BattleActionModel action, BattleUnitModel attacker, bool isCritical, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetTakeAttackDmgMultiplier(action, attacker, isCritical);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetExpectedTakeAttackDmgMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedTakeAttackDmgMultiplier_Postfix(BattleActionModel action, BattleUnitModel attacker, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedTakeAttackDmgMultiplier(action, attacker);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetHpHealMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetHpHealMultiplier_Postfix(BattleUnitModel target, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetHpHealMultiplier(target);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetCriticalChanceAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetCriticalChanceAdder_Postfix(Il2CppSystem.Collections.Generic.Dictionary<BUFF_UNIQUE_KEYWORD, float> affectKeywords, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetCriticalChanceAdder(affectKeywords);
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.GetCriticalDamageRatioResultMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetCriticalDamageRatioResultMultiplier_Postfix(BattleActionModel action, BattleUnitModel __instance, ref float __result)
        {
            var passiveList = __instance.GetAllActivatedPassives();
            if (passiveList.Count <= 0) return;

            for (int i = 0; i < passiveList.Count; i++)
            {
                if (!_passiveBundle.ProcessPatchListLogic(passiveList[i].GetID(), passiveList[i], out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetCriticalDamageRatioResultMultiplier(action);
            }
        }
    }
}
