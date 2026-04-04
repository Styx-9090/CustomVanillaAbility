using System;
using System.Collections.Generic;
using HarmonyLib;
using CustomVanillaAbility.CustomClasses;

namespace CustomVanillaAbility.Patches
{
    public static class CustomVanillaAbilityPatches_BattleUnitModel
    {
        public static CustomAbilityBundle _skillBundle = CustomVanillaAbilityPatches_SkillModel._skillBundle;

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.BeforeTakeAttackDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BeforeTakeAttackDamage_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, BattleUnitModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, action.GetSkillID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(BattleUnitModel.BeforeTakeAttackDamage);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try {  realAbility.BeforeGiveAttackDamage(action, __instance, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }


        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.OnTakeAttackDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeAttackDamage_Postfix(BattleActionModel action, CoinModel coin, int realDmg, int hpDmg, BATTLE_EVENT_TIMING timing, bool isCritical, BattleUnitModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, action.GetSkillID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(BattleUnitModel.OnTakeAttackDamage);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnSucceedAttack(action, coin, __instance, hpDmg, realDmg, isCritical, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.OnEndCoin_BeforeLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndCoin_BeforeLog_Postfix(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing, BattleUnitModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, action.GetSkillID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(BattleUnitModel.OnEndCoin_BeforeLog);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnEndCoin_BeforeLog(action, coin, false, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(BattleUnitModel.ChangeAttackDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void ChangeAttackDamage_Postfix(BattleActionModel action, BattleUnitModel target, CoinModel coin, int resultDmg, ref bool isCritical, BATTLE_EVENT_TIMING timing, BattleUnitModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, action.GetSkillID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(BattleUnitModel.ChangeAttackDamage);

            int tempResult = resultDmg;
            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    int changeAttackDmgResult = realAbility.ChangeAttackDamage(action, target, coin, tempResult, isCritical, timing);
                    if (changeAttackDmgResult != resultDmg) { __result = changeAttackDmgResult; break; }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }
    }
}
