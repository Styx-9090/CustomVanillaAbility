using CustomVanillaAbility.CustomClasses;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace CustomVanillaAbility.Patches
{
    public static class CustomVanillaAbilityPatches_PassiveModel
    {
        public static CustomPassiveAbilityBundle _passiveBundle = new();

        public static void SafelyExitSkillInit(PassiveModel __instance)
        {
            CustomVanillaAbilityMain main = CustomVanillaAbilityMain.Instance;
            if (!CustomVanillaAbilityHelper.InitSetup<CustomPassiveAbilityBundle>("passive", __instance.GetID(), __instance, out CustomPassiveAbilityBundle bundle)) return;

            System.Collections.Generic.List<CustomPassiveAbilityBase> newAbilities = [];
            var passiveAbilityList = __instance.ClassInfo.GetConditionIDList();

            for (int i = 0; i < passiveAbilityList.Count; i++)
            {
                string selectedScript = passiveAbilityList[i];

                try
                {

                    if (selectedScript.StartsWith("Reg", StringComparison.OrdinalIgnoreCase))
                    {
                        string cleanedName = selectedScript[3..];

                        if (CustomVanillaAbilityHelper.TryToCreateRegexLinked_Passive(bundle, cleanedName, out CustomPassiveAbilityBase customPassiveAbility))
                        {
                            newAbilities.Add(customPassiveAbility);
                            continue;
                        }
                    }


                    if (!bundle.abilityTypeByLookup.TryGetValue(selectedScript, out var template))
                    {
                        int varScriptLenght = selectedScript.Length;

                        int underscoreIndex = selectedScript.IndexOf('_');
                        string key = underscoreIndex > 0 ? selectedScript[..underscoreIndex] : selectedScript;

                        if (!bundle.abilityClassDict.TryGetValue(key, out template))
                        {
                            bundle.abilityTypeByLookup[selectedScript] = null;
                            continue;
                        }

                        bundle.abilityTypeByLookup[selectedScript] = template;
                    }

                    if (template != null)
                    {
                        newAbilities.Add(CustomVanillaAbilityHelper.CreateCustomPassiveAbility(template, selectedScript));
                        continue;
                    }


                    if (!CustomVanillaAbilityHelper.TryToCreateRegexLinked_Passive(bundle, selectedScript, out CustomPassiveAbilityBase fallbackCustomPassiveAbility)) continue;
                    newAbilities.Add(fallbackCustomPassiveAbility);
                }
                catch (Exception ex)
                {
                    main.Log.LogError(ex);
                }
            }

            CustomPassiveAbilityHolder passiveHolder = new(newAbilities);
            passiveHolder.Init(__instance);
            bundle.customAbilityHolderTable.Add(__instance, passiveHolder);

            if (_passiveBundle != bundle)
            {
                _passiveBundle = bundle;
                CustomVanillaAbilityPatches_BattleUnitModel._passiveBundle = bundle;
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.Init))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryHigh)]
        public static void Init_Postfix(BattleUnitModel owner, PassiveModel __instance)
        {
            try { SafelyExitSkillInit(__instance); }
            catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogError(ex); }
        }


        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.IsActive))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryHigh)]
        public static void IsHide_Postfix(PassiveModel __instance, ref bool __result)
        {
            try
            {
                if (__instance.Script != null) return;
                if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder holder)) return;

                __result = holder.IsActive();
            }
            catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogError(ex); }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.IsActive))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryHigh)]
        public static void IsActive_Postfix(PassiveModel __instance, ref bool __result)
        {
            try
            {
                if (__instance.Script != null) return;
                if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder holder)) return;

                __result = holder.IsActive();
            }
            catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogError(ex); }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.CheckActiveCondition))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryHigh)]
        public static void CheckActiveCondition_Postfix(PassiveModel __instance, ref bool __result)
        {
            try
            {
                if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder holder)) return;

                holder.CheckActiveCondition();
                if (__instance._script == null) __result = holder.IsActive();
            }
            catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogError(ex); }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.GetSatisfiedResonanceStatus))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryHigh)]
        public static void GetSatisfiedResonanceStatus_Postfix(PassiveModel __instance, ref PASSIVE_STATUS __result)
        {
            try
            {
                if (__instance.Script != null) return;
                if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder holder)) return;

                __result = holder.GetPassiveStatus();
            }
            catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogError(ex); }
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

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.IsTargetable))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IsTargetable_Postfix(BattleUnitModel attacker, PassiveModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IsTargetable);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.IsTargetable(attacker))
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.IsTargetableParts))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IsTargetableParts_Postfix(BattleUnitModel attacker, PassiveModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IsTargetableParts);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.IsTargetableParts(attacker))
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.CanBeChangedTarget))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CanBeChangedTarget_Postfix(BattleActionModel action, PassiveModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CanBeChangedTarget);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.CanBeChangedTarget(action))
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.CanGiveConcentratedAttack))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CanGiveConcentratedAttack_Postfix(BattleActionModel action, PassiveModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CanGiveConcentratedAttack);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.CanGiveConcentratedAttack(action))
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.IsRegeneratable))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IsRegeneratable_Postfix(PassiveModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IsRegeneratable);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.IsRegeneratable())
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.SpreadHpDmgToAbnormality))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void SpreadHpDmgToAbnormality_Postfix(int value, BattleUnitModel attackerOrNull, BattleActionModel attackerActionOrNull, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing, BUFF_UNIQUE_KEYWORD keyword, PassiveModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.SpreadHpDmgToAbnormality);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.SpreadHpDmgToAbnormality(value, attackerOrNull, attackerActionOrNull, dmgSrcType, timing, keyword))
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.SpreadHpDmgFromAbnormalityPart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void SpreadHpDmgFromPart_Postfix(AB_PART_TYPE partType, int value, BattleUnitModel attackerOrNull, BattleActionModel attackerActionOrNull, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing, BUFF_UNIQUE_KEYWORD keyword, PassiveModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.SpreadHpDmgFromAbnormalityPart);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.SpreadHpDmgFromPart(partType, value, attackerOrNull, attackerActionOrNull, dmgSrcType, timing, keyword))
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.CheckImmortal))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CheckImmortal_Postfix(BATTLE_EVENT_TIMING timing, int newHp, bool isInstantDeath, BUFF_UNIQUE_KEYWORD buff, PassiveModel __instance, ref bool __result, BattleActionModel actionOrNull = null)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CheckImmortal);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.CheckImmortal(timing, newHp, isInstantDeath, buff, actionOrNull))
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.IsAbnormalityImmortal))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IsAbnormalityImmortal_Postfix(BATTLE_EVENT_TIMING timing, int newHp, bool isInstantDeath, BUFF_UNIQUE_KEYWORD buff, PassiveModel __instance, ref bool __result, BattleActionModel actionOrNull = null)
        {
            if (__result == true) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IsAbnormalityImmortal);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.IsAbnormalityImmortal(timing, newHp, isInstantDeath, buff, actionOrNull))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.CheckImmortalOtherUnit))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CheckImmortalOtherUnit_Postfix(BattleUnitModel checkTarget, int newHp, bool isInstantDeath, BUFF_UNIQUE_KEYWORD buf, PassiveModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CheckImmortalOtherUnit);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.CheckImmortalOtherUnit(checkTarget, newHp, isInstantDeath, buf))
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.CanTeamKill))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CanTeamKill_Postfix(BattleActionModel action, PassiveModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CanTeamKill);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.CanTeamKill(action))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.IsActionable))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IsActionable_Postfix(PassiveModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IsActionable);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.IsActionable())
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.ChangeResistOnBreak))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void ChangeResistOnBreak_Postfix(PassiveModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.ChangeResistOnBreak);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.ChangeResistOnBreak())
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.ChangeResistOnBreak_Part))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void ChangeResistOnBreak_Part_Postfix(BattleUnitModel_Abnormality_Part part, PassiveModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.ChangeResistOnBreak_Part);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.ChangeResistOnBreak_Part(part))
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.CanPickSkill))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CanPickSkill_Postfix(PassiveModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CanPickSkill);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.CanPickSkill())
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.IgnoreCheckBreak))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IgnoreCheckBreak_Postfix(DAMAGE_SOURCE_TYPE dmgSrcType, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BUFF_UNIQUE_KEYWORD keyword, PassiveModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IgnoreCheckBreak);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.IgnoreCheckBreak(dmgSrcType, attackerOrNull, actionOrNull, keyword))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.IgnoreBreak))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IgnoreBreak_Postfix(PassiveModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IgnoreBreak);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.IgnoreBreak())
                    {
                        __result = true;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.IgnoreBreakExceptForcedCase))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IgnoreBreakExceptForcedCase_Postfix(PassiveModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IgnoreBreakExceptForcedCase);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.IgnoreBreakExceptForcedCase())
                    {
                        __result = true;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.IgnorePanic))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IgnorePanic_Postfix(PassiveModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IgnorePanic);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.IgnorePanic())
                    {
                        __result = true;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.HasFakeDead))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void HasFakeDead_Postfix(PassiveModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.HasFakeDead);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.HasFakeDead())
                    {
                        __result = true;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.CanCreateEmptySlot))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CanCreateEmptySlot_Postfix(PassiveModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CanCreateEmptySlot);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.CanCreateEmptySlot())
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.CanCreateEmptySlotPart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CanCreateEmptySlotPart_Postfix(BattleUnitModel_Abnormality_Part part, PassiveModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CanCreateEmptySlotPart);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.CanCreateEmptySlotPart(part))
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.CanTakeMpHeal))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CanTakeMpHeal_Postfix(BattleUnitModel healerOrNull, int mp, ABILITY_SOURCE_TYPE srcType, PassiveModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CanTakeMpHeal);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.CanTakeMpHeal(healerOrNull, mp, srcType))
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.IsChangeTakeDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IsChangeTakeDamage_Postfix(BattleActionModel action, CoinModel coinOrNull, int resultDmg, DAMAGE_SOURCE_TYPE dmgSrcType, BUFF_UNIQUE_KEYWORD keyword, PassiveModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IsChangeTakeDamage);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.IsChangeTakeDamage(action, coinOrNull, resultDmg, dmgSrcType, keyword))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.CheckIfTurnToCorpesOnDie))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CheckIfTurnToCorpesOnDie_Postfix(PassiveModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CheckIfTurnToCorpesOnDie);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.CheckIfTurnToCorpesOnDie())
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.IsAllowedToGiveBuff))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IsAllowedToGiveBuff_Postfix(BUFF_UNIQUE_KEYWORD keyword, PassiveModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IsAllowedToGiveBuff);

            foreach (CustomPassiveAbilityBase realAbility in abilityHolder.passiveList)
            {

                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.IsAllowedToGiveBuff(keyword))
                    {
                        __result = false;
                        return;
                    }
                }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }
    }
}