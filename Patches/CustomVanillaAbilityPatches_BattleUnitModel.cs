using CustomVanillaAbility.CustomClasses;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace CustomVanillaAbility.Patches
{
    public static class CustomVanillaAbilityPatches_BattleUnitModel
    {
        public static CustomSkillAbilityBundle _skillBundle = CustomVanillaAbilityPatches_SkillModel._skillBundle;
        public static CustomPassiveAbilityBundle _passiveBundle = CustomVanillaAbilityPatches_PassiveModel._passiveBundle;

        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.BeforeTakeAttackDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BeforeTakeAttackDamage_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, BattleUnitModel __instance)
        {
            if (!_skillBundle.ProcessPatchListLogic(action.GetSkillID(), action.Skill, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(BattleUnitModel.BeforeTakeAttackDamage);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try {  realAbility.BeforeGiveAttackDamage(action, __instance, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            var passiveList = __instance.GetAllActivatedPassives();
            for (int i = 0; i < passiveList.Count; i++)
            {
                PassiveModel passive = passiveList[i];

                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder giverAbilityHolder)) continue;

                foreach (CustomAbilityBase ability in giverAbilityHolder.passiveList)
                {
                    if (ability is not CustomPassiveAbilityBase realAbility) continue;
                    if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                    try { realAbility.BeforeTakeAttackDamage(action, timing); }
                    catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
                }
            }
        }


        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.OnTakeAttackDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeAttackDamage_Postfix(BattleActionModel action, CoinModel coin, int realDmg, int hpDamage, BATTLE_EVENT_TIMING timing, bool isCritical, BattleUnitModel __instance)
        {
            if (!_skillBundle.ProcessPatchListLogic(action.GetSkillID(), action.Skill, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(BattleUnitModel.OnTakeAttackDamage);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnSucceedAttack(action, coin, __instance, hpDamage, realDmg, isCritical, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
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

        [HarmonyPatch(typeof(SkillModel), nameof(BattleUnitModel.ChangeAttackDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void ChangeAttackDamage_Postfix(BattleActionModel action, BattleUnitModel target, CoinModel coin, int resultDmg, ref bool isCritical, BATTLE_EVENT_TIMING timing, BattleUnitModel __instance, ref int __result)
        {
            if (!_skillBundle.ProcessPatchListLogic(action.GetSkillID(), action.Skill, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(BattleUnitModel.ChangeAttackDamage);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    int changeAttackDmgResult = realAbility.ChangeAttackDamage(action, target, coin, resultDmg, isCritical, timing);
                    if (changeAttackDmgResult != resultDmg) { __result = changeAttackDmgResult; break; }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }
    }
}
