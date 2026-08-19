using CustomVanillaAbility.CustomClasses;
using HarmonyLib;
using System;

namespace CustomVanillaAbility.Patches
{
    public static class CustomVanillaAbilityPatches_Passives
    {
        #region DataAndInit

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

            _passiveBundle = bundle;
            CustomVanillaAbilityPatches_BattleUnitModel._passiveBundle = bundle;
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.Init))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryHigh)]
        public static void Init_Postfix(BattleUnitModel owner, PassiveModel __instance)
        {
            try { SafelyExitSkillInit(__instance); }
            catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogError(ex); }
        }

        #endregion

        #region PassiveActive

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

        #endregion

        #region BoolToRemove

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

        #endregion

        #region IntFloat

        [HarmonyPatch(nameof(BattleUnitModel.GetAttackWeightAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAttackWeight_Postfix(BattleActionModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetAttackWeightAdder(__instance);
            }
        }

        [HarmonyPatch(nameof(BattleActionModel.GetAttackHpDmgAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAttackHpDmgAdder_Postfix(CoinModel coin, bool isWinDuel, BattleUnitModel target, BattleActionModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetAttackHpDmgAdder(__instance, coin, isWinDuel, target);
            }
        }

        [HarmonyPatch(nameof(BattleActionModel.GetExpectedAttackHpDmgAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedAttackHpDmgAdder_Postfix(CoinModel coin, bool isWinDuel, BattleUnitModel target, BattleActionModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedAttackHpDmgAdder(__instance, coin, isWinDuel, target);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetCoinScaleAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetCoinScaleAdder_Postfix(BattleActionModel action, BattleActionModel oppoActionOrNull, CoinModel coin, BattleActionModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetCoinScaleAdder(__instance, oppoActionOrNull, coin);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetExpectedCoinScaleAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedCoinScaleAdder_Postfix(CoinModel coin, COIN_ROLL_TYPE rollType, BattleActionModel oppoActionOrNull, SinActionModel targetSinActionOrNull, BattleActionModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedCoinScaleAdder(__instance, coin, rollType, oppoActionOrNull, targetSinActionOrNull);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetSkillPowerResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetSkillPowerResultAdder_Postfix(BATTLE_EVENT_TIMING timing, BattleActionModel attackerActionOrNull, bool calculateSystemAdder, CoinModel coinOrNull, BattleActionModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetSkillPowerResultAdder(__instance, timing, attackerActionOrNull, calculateSystemAdder, coinOrNull);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetExpectedSkillPowerAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedSkillPowerAdder_Postfix(COIN_ROLL_TYPE rollType, SinActionModel expectedTargetSinActionOrNull, BattleActionModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedSkillPowerAdder(__instance, rollType, expectedTargetSinActionOrNull);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetExpectedSkillPowerResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedSkillPowerResultAdder_Postfix(Il2CppSystem.Collections.Generic.List<int> indexes, SinActionModel opppoSinActionOrNull, BattleActionModel oppoActionOrNull, BattleActionModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedSkillPowerResultAdder(__instance, indexes, opppoSinActionOrNull, oppoActionOrNull);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetAttackDmgAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAttackDmgAdder_Postfix(CoinModel coin, BattleUnitModel target, bool isWinDuel, BattleActionModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetAttackDmgAdder(__instance, coin, target, isWinDuel);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetExpectedAttackDmgAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedAttackDmgAdder_Postfix(CoinModel coin, BattleUnitModel targetOrNull, BattleActionModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedAttackDmgAdder(__instance, coin, targetOrNull);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetParryingResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetParryingResultAdder_Postfix(int actorResult, BattleActionModel oppoAction, int oppoResult, int parryingCount, BattleActionModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetParryingResultAdder(__instance, actorResult, oppoAction, oppoResult, parryingCount);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetExpectedParryingResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedParryingResultAdder_Postfix(int actorResult, BattleActionModel oppoActionOrNull, int oppoResult, BattleActionModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedParryingResultAdder(__instance, actorResult, oppoActionOrNull, oppoResult);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetOpponentParryingResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetOpponentParryingResultAdder_Postfix(int actorResult, BattleActionModel oppoAction, int oppoResult, BattleActionModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetOpponentParryingResultAdder(__instance, actorResult, oppoAction, oppoResult);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetExpectedOpponentParryingResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedOpponentParryingResultAdder_Postfix(int actorResult, BattleActionModel oppoAction, int oppoResult, BattleActionModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedOpponentParryingResultAdder(__instance, actorResult, oppoAction, oppoResult);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetCriticalChance))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetCriticalChance_Postfix(CoinModel coin, out Il2CppSystem.Collections.Generic.Dictionary<BUFF_UNIQUE_KEYWORD, float> affectKeywords, BattleActionModel __instance, ref float __result)
        {
            affectKeywords = new Il2CppSystem.Collections.Generic.Dictionary<BUFF_UNIQUE_KEYWORD, float>();
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetCriticalChanceAdder(__instance, coin, ref affectKeywords);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetAttackDmgMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAttackDmgMultiplier_Postfix(CoinModel coin, BattleUnitModel target, bool isWinDuel, bool isCritical, bool isOneSideAttack, OneCoinLog_Attack forEditorItCanBeNull, BattleActionModel __instance, ref float __result)
        {
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetAttackDmgMultiplier(__instance, coin, target, isWinDuel, isCritical, isOneSideAttack, forEditorItCanBeNull);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetExpectedAttackDmgMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedAttackDmgMultiplier_Postfix(CoinModel coin, BattleUnitModel targetOrNull, SinActionModel targetSinActionOrNull, BattleActionModel __instance, ref float __result)
        {
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedAttackDmgMultiplier(__instance, coin, targetOrNull, targetSinActionOrNull);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetGiveBsGaugeUpMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetGiveBsGaugeUpMultiplier_Postfix(bool onGiveExplosion, BattleUnitModel target, CoinModel coinOrNull, BattleActionModel __instance, ref float __result)
        {
            foreach (PassiveModel passive in __instance.Model.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetGiveBsGaugeUpMultiplier(__instance, onGiveExplosion, target, coinOrNull);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetActionSlotAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetActionSlotAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetActionSlotAdder();
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetSpeedAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetSpeedAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetSpeedAdder();
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetTakeAttackDmgMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetTakeAttackDmgMultiplier_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel attacker, bool isCritical, BattleUnitModel __instance, ref float __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetTakeAttackDmgMultiplier(action, coin, attacker, isCritical);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetExpectedTakeAttackDmgMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedTakeAttackDmgMultiplier_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel attacker, BattleUnitModel __instance, ref float __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetExpectedTakeAttackDmgMultiplier(action, coin, attacker);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetMentalSystemResultIncreaseAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetMentalSystemResultIncreaseAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetMentalSystemResultIncreaseAdder();
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetMentalSystemResultDecreaseAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetMentalSystemResultDecreaseAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetMentalSystemResultDecreaseAdder();
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetSinBuffDamageAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetSinBuffDamageAdder_Postfix(int dmg, BUFF_UNIQUE_KEYWORD keyword, BattleUnitModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetSinBuffDamageAdder(keyword, dmg);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetSinBuffDamageMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetSinBuffDamageMultiplier_Postfix(BUFF_UNIQUE_KEYWORD keyword, BattleUnitModel __instance, ref float __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetSinBuffDamageMultiplier(keyword);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetMaxHpAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetMaxHpAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetMaxHpAdder();
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetMaxSpeedAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetMaxSpeedAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetMaxSpeedAdder();
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetMinSpeedAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetMinSpeedAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetMinSpeedAdder();
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetDefenseAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetDefenseAdder_Postfix(BattleUnitModel __instance, ref int __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetDefenseAdder();
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetTakeHpHealMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetTakeHpHealMultiplier_Postfix(BattleUnitModel healerOrNull, BattleActionModel actionOrNull, ABILITY_SOURCE_TYPE srcType, BattleUnitModel __instance, ref float __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetTakeHpHealMultiplier(healerOrNull, actionOrNull, srcType);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetMaxHpMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetMaxHpMultiplier_Postfix(BattleUnitModel __instance, ref float __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetMaxHpMultiplier();
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetAtkResistMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAtkResistMultiplier_Postfix(ATK_BEHAVIOUR type, BattleUnitModel __instance, ref float __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetAtkResistMultiplier(type);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetDefenseMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetDefenseMultiplier_Postfix(BattleUnitModel __instance, ref float __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetDefenseMultiplier();
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetAttributeResistAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAttributeResistAdder_Postfix(ATTRIBUTE_TYPE type, BattleUnitModel __instance, ref float __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetAttributeResistAdder(type);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetAttributeResistMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAttributeResistMultiplier_Postfix(ATTRIBUTE_TYPE type, BattleUnitModel __instance, ref float __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetAttributeResistMultiplier(type);
            }
        }

        [HarmonyPatch(nameof(BattleUnitModel.GetHpHealMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetHpHealMultiplier_Postfix(BattleUnitModel target, BattleUnitModel __instance, ref float __result)
        {
            foreach (PassiveModel passive in __instance.GetAllActivatedPassives())
            {
                if (!_passiveBundle.ProcessPatchListLogic(passive.GetID(), passive, out CustomPassiveAbilityHolder abilityHolder)) continue;
                __result += abilityHolder.GetHpHealMultiplier(target);
            }
        }

        #endregion

        #region Timing

        [HarmonyPatch(nameof(PassiveModel.OnRoundStart_After_Event))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnRoundStart_After_Event(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnRoundStart_After_Event(timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnRoundStart_After_Event_DeadOrRetreated))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnRoundStart_After_Event_DeadOrRetreated(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnRoundStart_After_Event_DeadOrRetreated(timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnReturnToField))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnReturnToField(int retreatTurn, BattleUnitModel triggerUnit, BUFF_UNIQUE_KEYWORD retreatKeyword, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnReturnToField(retreatTurn, triggerUnit, retreatKeyword, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnReturnToFieldOtherUnit_DeadOrRetreated))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnReturnToFieldOtherUnit_DeadOrRetreated(BattleUnitModel returnUnit, int retreatTurn, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnReturnToFieldOtherUnit_DeadOrRetreated(returnUnit, retreatTurn, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnReleaseStandByOtherUnit_DeadOrRetreated))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnReleaseStandByOtherUnit_DeadOrRetreated(BattleUnitModel addedUnit, Il2CppSystem.Collections.Generic.List<BattleUnitModel> addedUnitList, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnReleaseStandByOtherUnit_DeadOrRetreated(addedUnit, addedUnitList);
        }

        [HarmonyPatch(nameof(PassiveModel.OnReleaseStandBy))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnReleaseStandBy(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnReleaseStandBy(timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnCompleteCommand))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnCompleteCommand(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnCompleteCommand(timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnRoundEnd))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnRoundEnd(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnRoundEnd(timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnRetreat))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnRetreat(BattleUnitModel triggerUnit, BUFF_UNIQUE_KEYWORD retreatKeyword, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnRetreat(triggerUnit, retreatKeyword, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnRoundEnd_After))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnRoundEnd_After(PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnRoundEnd_After();
        }

        [HarmonyPatch(nameof(PassiveModel.OnStageEnd))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStageEnd(PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnStageEnd();
        }

        [HarmonyPatch(nameof(PassiveModel.OnDestroy))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnDestroy(BattleUnitModel destroyerOrNull, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnDestroy(destroyerOrNull, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnOtherPartDestroyed))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnOtherPartDestroyed(BattleUnitModel_Abnormality_Part destroyedPart, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnOtherPartDestroyed(destroyedPart);
        }

        [HarmonyPatch(nameof(PassiveModel.OnPartDestroyed))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnPartDestroyed(BattleUnitModel_Abnormality_Part destroyedPart, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnPartDestroyed(destroyedPart, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnPartBreaked))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnPartBreaked(BattleUnitModel_Abnormality_Part breakedPart, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnPartBreaked(breakedPart, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnPartRecoverBreak))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnPartRecoverBreak(BattleUnitModel_Abnormality_Part recoveredPart, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnPartRecoverBreak(recoveredPart, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnRegenerate))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnRegenerate(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnRegenerate(timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnPartRegenerate))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnPartRegenerate(BattleUnitModel_Abnormality_Part part, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnPartRegenerate(part, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnBeforeDefense))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnBeforeDefense(BattleActionModel action, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnBeforeDefense(action);
        }

        [HarmonyPatch(nameof(PassiveModel.OnBattleStart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnBattleStart(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnBattleStart(timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnBattleEnd))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnBattleEnd(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnBattleEnd(timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnStartBehaviour))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStartBehaviour(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnStartBehaviour(action, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnSucceedAttack))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnSucceedAttack(BattleActionModel action, CoinModel coin, BattleUnitModel target, int finalDmg, int realDmg, bool isCritical, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnSucceedAttack(action, coin, target, finalDmg, realDmg, isCritical, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnRollOneCoin_AfterAttack))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnRollOneCoin_AfterAttack(BattleActionModel action, CoinModel coin, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnRollOneCoin_AfterAttack(action, coin);
        }

        [HarmonyPatch(nameof(PassiveModel.OnEndAttack))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndAttack(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnEndAttack(action, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnEndEnemyAttack))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndEnemyAttack(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnEndEnemyAttack(action, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnEndAttackPart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndAttackPart(BattleUnitModel_Abnormality_Part part, BattleActionModel action, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnEndAttackPart(part, action);
        }

        [HarmonyPatch(nameof(PassiveModel.OnDuelAfter_BeforeLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnDuelAfter_BeforeLog(BattleActionModel selfAction, BattleActionModel oppoAction, int parryingCount, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnDuelAfter_BeforeLog(selfAction, oppoAction, parryingCount, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnWinParrying))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnWinParrying(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnWinParrying(selfAction, oppoAction, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnLoseParrying))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnLoseParrying(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnLoseParrying(selfAction, oppoAction, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnWinDuel))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnWinDuel(BattleActionModel selfAction, BattleActionModel oppoAction, int parryingCount, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnWinDuel(selfAction, oppoAction, parryingCount, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnLoseDuel))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnLoseDuel(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnLoseDuel(selfAction, oppoAction, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.RightBeforeOtherUnitGiveBuffBySkill))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void RightBeforeOtherUnitGiveBuffBySkill(BattleUnitModel giver, BattleUnitModel target, BUFF_UNIQUE_KEYWORD bufKeyword, int stack, int turn, SkillModel skill, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.RightBeforeOtherUnitGiveBuffBySkill(giver, target, bufKeyword, stack, turn, skill, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.RightAfterOtherUnitGiveBuffBySkill))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void RightAfterOtherUnitGiveBuffBySkill(BattleUnitModel giver, BattleUnitModel target, BUFF_UNIQUE_KEYWORD bufKeyword, int stack, int turn, SkillModel skill, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.RightAfterOtherUnitGiveBuffBySkill(giver, target, bufKeyword, stack, turn, skill, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.RightAfterLosingBuff))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void RightAfterLosingBuff(int loseStack, int loseTurn, BATTLE_EVENT_TIMING timing, BuffInfo info, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.RightAfterLosingBuff(loseStack, loseTurn, timing, info);
        }

        [HarmonyPatch(nameof(PassiveModel.OnSucceedToGiveSwitchToSpecialVibration))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnSucceedToGiveSwitchToSpecialVibration(BattleUnitModel target, BUFF_UNIQUE_KEYWORD keyword, int prevStack, int prevTurn, int afterStack, int afterTurn, BATTLE_EVENT_TIMING timing, ABILITY_SOURCE_TYPE abilitySourceType, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnSucceedToGiveSwitchToSpecialVibration(target, keyword, prevStack, prevTurn, afterStack, afterTurn, timing, abilitySourceType);
        }

        [HarmonyPatch(nameof(PassiveModel.RightAfterGetAnyBuff))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void RightAfterGetAnyBuff(BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, int activeRound, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing, BattleUnitModel giverOrNull, BattleActionModel actionOrNull, int overStack, int overTurn, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.RightAfterGetAnyBuff(keyword, stack, turn, activeRound, srcType, timing, giverOrNull, actionOrNull, overStack, overTurn);
        }

        [HarmonyPatch(nameof(PassiveModel.RightAfterGetAnyBuffAtPart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void RightAfterGetAnyBuffAtPart(BattleUnitModel_Abnormality_Part part, BUFF_UNIQUE_KEYWORD keyword, int stack, int activeRound, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing, BattleUnitModel giverOrNull, BattleActionModel actionOrNull, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.RightAfterGetAnyBuffAtPart(part, keyword, stack, activeRound, srcType, timing, giverOrNull, actionOrNull);
        }

        [HarmonyPatch(nameof(PassiveModel.CheckLoseBuffStackAndTurn))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CheckLoseBuffStackAndTurn(BuffInfo info, int loseStack, int loseTurn, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.CheckLoseBuffStackAndTurn(info, loseStack, loseTurn, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnDie))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnDie(BattleUnitModel killer, BattleActionModel actionOrNull, DAMAGE_SOURCE_TYPE dmgSrcType, BUFF_UNIQUE_KEYWORD keyword, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnDie(killer, actionOrNull, dmgSrcType, keyword, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnBreak))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnBreak(BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BATTLE_EVENT_TIMING timing, bool isBreakForcely, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnBreak(attackerOrNull, actionOrNull, timing, isBreakForcely);
        }

        [HarmonyPatch(nameof(PassiveModel.OnEnemyBrokenByAttacker))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEnemyBrokenByAttacker(BattleActionModel actionOrNull, BattleUnitModel target, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnEnemyBrokenByAttacker(actionOrNull, target, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnDieOtherUnit))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnDieOtherUnit(BattleUnitModel killer, BattleUnitModel dead, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE dmgSrcType, BUFF_UNIQUE_KEYWORD keyword, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnDieOtherUnit(killer, dead, timing, dmgSrcType, keyword);
        }

        [HarmonyPatch(nameof(PassiveModel.OnDiscardSinOtherUnit))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnDiscardSinOtherUnit(BattleUnitModel discardUnit, UnitSinModel sin, BATTLE_EVENT_TIMING timing, BattleActionModel actionOrNull, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnDiscardSinOtherUnit(discardUnit, sin, timing, actionOrNull);
        }

        [HarmonyPatch(nameof(PassiveModel.OnDiscardSin))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnDiscardSin(UnitSinModel sin, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnDiscardSin(sin, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnBreakOtherUnit))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnBreakOtherUnit(BattleUnitModel breakedUnit, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnBreakOtherUnit(breakedUnit, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnVibrationExplosionOtherUnit))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnVibrationExplosionOtherUnit(BattleUnitModel explodedUnit, BattleUnitModel giverOrNull, BattleActionModel actionOrNull, ABILITY_SOURCE_TYPE abilitySrc, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnVibrationExplosionOtherUnit(explodedUnit, giverOrNull, actionOrNull, abilitySrc, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnTakeAttackDamageOtherUnit))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeAttackDamageOtherUnit(BattleActionModel action, int realDmg, int hpDmg, BattleUnitModel attackedUnit, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnTakeAttackDamageOtherUnit(action, realDmg, hpDmg, attackedUnit, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnUseBloodDinnerUnit))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnUseBloodDinnerUnit(BattleUnitModel usedUnit, int stack, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnUseBloodDinnerUnit(usedUnit, stack, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnAddUnit))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAddUnit(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnAddUnit(timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnAddUnitPart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAddUnitPart(BattleUnitModel part, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnAddUnitPart(part, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnAddUnitView))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAddUnitView(BattleUnitView view, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnAddUnitView(view);
        }

        [HarmonyPatch(nameof(PassiveModel.OnStageStart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStageStart(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnStageStart(timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnWaveStart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnWaveStart(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnWaveStart(timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnPanicOrLowMorale))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnPanicOrLowMorale(PANIC_LEVEL level, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnPanicOrLowMorale(level, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnStartTurn_BeforeLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStartTurn_BeforeLog(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnStartTurn_BeforeLog(action, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnStartTurn_AfterLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStartTurn_AfterLog(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnStartTurn_AfterLog(action, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnStartDuel))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStartDuel(BattleActionModel ownerAction, BattleActionModel opponentAction, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnStartDuel(ownerAction, opponentAction, timing);
        }

        /*
        [HarmonyPatch(nameof(PassiveModel.BeforeGiveAttackDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BeforeGiveAttackDamage(BattleActionModel action, CoinModel coin, BattleUnitModel target, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.BeforeGiveAttackDamage(action, coin, target, timing);
        }
        */

        [HarmonyPatch(nameof(PassiveModel.BeforeTakeAttackDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BeforeTakeAttackDamage(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.BeforeTakeAttackDamage(action, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.BeforePartTakeAttackDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BeforePartTakeAttackDamage(BattleUnitModel_Abnormality abnormality, BattleUnitModel_Abnormality_Part part, BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.BeforePartTakeAttackDamage(abnormality, part, action, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnTakeAttackDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeAttackDamage(BattleActionModel action, CoinModel coin, int realDmg, int hpDmg, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnTakeAttackDamage(action, coin, realDmg, hpDmg, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnTakeAttackDamagePart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeAttackDamagePart(BattleUnitModel_Abnormality_Part part, BattleActionModel attackerAction, CoinModel coin, int value, BATTLE_EVENT_TIMING timing, bool isCritical, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnTakeAttackDamagePart(part, attackerAction, coin, value, timing, isCritical);
        }

        [HarmonyPatch(nameof(PassiveModel.OnGiveHpDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnGiveHpDamage(BattleUnitModel target, int value, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnGiveHpDamage(target, value, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnGiveMpDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnGiveMpDamage(BattleUnitModel target, int value, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnGiveMpDamage(target, value);
        }

        [HarmonyPatch(nameof(PassiveModel.OnTakeMpDmg_SinBuff))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeMpDmg_SinBuff(int value, BATTLE_EVENT_TIMING timing, BUFF_UNIQUE_KEYWORD keyword, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnTakeMpDmg_SinBuff(value, timing, keyword);
        }

        [HarmonyPatch(nameof(PassiveModel.OnKillTarget))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnKillTarget(BattleActionModel actionOrNull, BattleUnitModel target, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnKillTarget(actionOrNull, target, dmgSrcType, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnStartCoin))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStartCoin(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnStartCoin(action, coin, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnEndCoin_BeforeLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndCoin_BeforeLog(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnEndCoin_BeforeLog(action, coin, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnEndCoin_AfterLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndCoin_AfterLog(BattleActionModel action, CoinModel coin, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnEndCoin_AfterLog(action, coin);
        }

        [HarmonyPatch(nameof(PassiveModel.OnEndBehaviour))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndBehaviour(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnEndBehaviour(action, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnEndBehave_Refresh))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndBehave_Refresh(BattleActionModel action, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnEndBehave_Refresh(action);
        }

        [HarmonyPatch(nameof(PassiveModel.OnEndTurn))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndTurn(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnEndTurn(action, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnCriticalActivated))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnCriticalActivated(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnCriticalActivated(action, coin, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.BeforeAttack))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BeforeAttack(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.BeforeAttack(action, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnAttackConfirmed))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAttackConfirmed(BattleActionModel action, CoinModel coin, BattleUnitModel target, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnAttackConfirmed(action, coin, target, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnStartPhase))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStartPhase(PHASE phase, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnStartPhase(phase, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnZeroHp))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnZeroHp(PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnZeroHp();
        }

        [HarmonyPatch(nameof(PassiveModel.OnResult_OnAction))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnResult_OnAction(BattleActionModel action, CoinModel coin, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnResult_OnAction(action, coin);
        }

        [HarmonyPatch(nameof(PassiveModel.OnResult_OnParrying))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnResult_OnParrying(BattleActionModel action, BattleActionModel oppoAction, CoinModel coin, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnResult_OnParrying(action, oppoAction, coin);
        }

        [HarmonyPatch(nameof(PassiveModel.OnSucceedEvade))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnSucceedEvade(BattleActionModel evadeAction, BattleActionModel attackAction, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnSucceedEvade(evadeAction, attackAction, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnActivateImmortality))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnActivateImmortality(BattleUnitModel immortalActivator, BATTLE_EVENT_TIMING timing, BattleActionModel actionOrNull, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnActivateImmortality(immortalActivator, timing, actionOrNull);
        }

        [HarmonyPatch(nameof(PassiveModel.OnActivateAbnormalityImmortality))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnActivateAbnormalityImmortality(BATTLE_EVENT_TIMING timing, BattleActionModel actionOrNull, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnActivateAbnormalityImmortality(timing, actionOrNull);
        }

        [HarmonyPatch(nameof(PassiveModel.OnDestroyShield))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnDestroyShield(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnDestroyShield(timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnRecoverBreak))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnRecoverBreak(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnRecoverBreak(timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnGiveBsGaugeUp))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnGiveBsGaugeUp(BattleUnitModel giver, BattleUnitModel target, int value, BATTLE_EVENT_TIMING timing, bool onExplosion, ABILITY_SOURCE_TYPE abilitySrc, BattleActionModel actionOrNull, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnGiveBsGaugeUp(giver, target, value, timing, onExplosion, abilitySrc, actionOrNull);
        }

        [HarmonyPatch(nameof(PassiveModel.OnTakeHpDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeHpDamage(int finalDamage, int hpDamage, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BUFF_UNIQUE_KEYWORD keyword, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnTakeHpDamage(finalDamage, hpDamage, timing, sourceType, attackerOrNull, actionOrNull, keyword);
        }

        [HarmonyPatch(nameof(PassiveModel.OnTakeHpDamagePart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeHpDamagePart(BattleUnitModel_Abnormality_Part part, int finalDamage, int hpDamage, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BUFF_UNIQUE_KEYWORD keyword, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnTakeHpDamagePart(part, finalDamage, hpDamage, timing, sourceType, attackerOrNull, actionOrNull, keyword);
        }

        [HarmonyPatch(nameof(PassiveModel.OnTakeHpDamageOtherUnit))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeHpDamageOtherUnit(BattleUnitModel damaged, int finalDamage, int hpDamage, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, Il2CppSystem.Collections.Generic.List<BattleUnitModel> relatedUnitsOrNull, BUFF_UNIQUE_KEYWORD keyword, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnTakeHpDamageOtherUnit(damaged, finalDamage, hpDamage, timing, sourceType, attackerOrNull, actionOrNull, relatedUnitsOrNull, keyword);
        }

        [HarmonyPatch(nameof(PassiveModel.OnTakeAttackConfirmed))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeAttackConfirmed(BattleActionModel action, CoinModel coin, BattleUnitModel attacker, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnTakeAttackConfirmed(action, coin, attacker, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnGiveImmortalState))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnGiveImmortalState(BattleUnitModel immortalTaker, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnGiveImmortalState(immortalTaker, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnAfterTryTakeHpHeal))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAfterTryTakeHpHeal(BattleUnitModel healerOrNull, int tryHeal, int resultHeal, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnAfterTryTakeHpHeal(healerOrNull, tryHeal, resultHeal, srcType, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnBeforeTryTakeMpHeal))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnBeforeTryTakeMpHeal(BattleUnitModel healerOrNull, int tryHeal, int resultHeal, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnBeforeTryTakeMpHeal(healerOrNull, tryHeal, resultHeal, srcType, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnAfterTryTakeMpHeal))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAfterTryTakeMpHeal(BattleUnitModel healerOrNull, int tryHeal, int resultHeal, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnAfterTryTakeMpHeal(healerOrNull, tryHeal, resultHeal, srcType, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnChangeHp))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnChangeHp(int oldHp, int newHp, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BUFF_UNIQUE_KEYWORD keyword, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnChangeHp(oldHp, newHp, dmgSrcType, timing, attackerOrNull, actionOrNull, keyword);
        }

        [HarmonyPatch(nameof(PassiveModel.OnChangeMp))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnChangeMp(int oldMp, int newMp, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnChangeMp(oldMp, newMp);
        }

        [HarmonyPatch(nameof(PassiveModel.OnChangeMpOther))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnChangeMpOther(BattleUnitModel mpChangeUnit, int oldMp, int newMp, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnChangeMpOther(mpChangeUnit, oldMp, newMp);
        }

        [HarmonyPatch(nameof(PassiveModel.OnTakeMpDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeMpDamage(BattleUnitModel attacker, int value, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleActionModel actionOrNull, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnTakeMpDamage(attacker, value, timing, sourceType, actionOrNull);
        }

        [HarmonyPatch(nameof(PassiveModel.OnTakeMpDamageOther))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeMpDamageOther(BattleUnitModel mpDmgUnit, BattleUnitModel attackerOrNull, int value, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleActionModel actionOrNull, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnTakeMpDamageOther(mpDmgUnit, attackerOrNull, value, timing, sourceType, actionOrNull);
        }

        [HarmonyPatch(nameof(PassiveModel.OnAfterTryTakeMpHealOther))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAfterTryTakeMpHealOther(BattleUnitModel mpHealUnit, BattleUnitModel healerOrNull, int tryHeal, int resultHeal, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnAfterTryTakeMpHealOther(mpHealUnit, healerOrNull, tryHeal, resultHeal, srcType, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnUseBuffTurnBySkill))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnUseBuffTurnBySkill(SkillModel skill, BUFF_UNIQUE_KEYWORD bufKeyword, int turn, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnUseBuffTurnBySkill(skill, bufKeyword, turn, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnUseBuff))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnUseBuff(BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnUseBuff(keyword, stack, turn, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.BeforeUseBuff))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BeforeUseBuff(BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.BeforeUseBuff(keyword, stack, turn, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnPickSkills))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnPickSkills(PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnPickSkills();
        }

        [HarmonyPatch(nameof(PassiveModel.OnAddActionToPart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAddActionToPart(BattleUnitModel_Abnormality_Part part, BattleActionModel action, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnAddActionToPart(part, action);
        }

        [HarmonyPatch(nameof(PassiveModel.RightAfterDestroyAnyBuff))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void RightAfterDestroyAnyBuff(BuffInfo destroyedBuffInfo, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.RightAfterDestroyAnyBuff(destroyedBuffInfo, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnCanceledByLackOfBuffsAtStartCoin))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnCanceledByLackOfBuffsAtStartCoin(BattleActionModel action, CoinModel coin, Il2CppSystem.Collections.Generic.List<BUFF_UNIQUE_KEYWORD> lackOfBuffs, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnCanceledByLackOfBuffsAtStartCoin(action, coin, lackOfBuffs, timing);
        }

        [HarmonyPatch(nameof(PassiveModel.OnFailedToGetBuff))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnFailedToGetBuff(BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, int activeRound, ABILITY_SOURCE_TYPE abilitySrcType, BATTLE_EVENT_TIMING timing, BattleUnitModel giverOrNull, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnFailedToGetBuff(keyword, stack, turn, activeRound, abilitySrcType, timing, giverOrNull);
        }

        [HarmonyPatch(nameof(PassiveModel.OnUseCoinConsume))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnUseCoinConsume(BattleUnitModel owner, BattleActionModel action, CoinModel coin, BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!_passiveBundle.ProcessPatchListLogic(__instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            abilityHolder.OnUseCoinConsume(owner, action, coin, keyword, stack, turn, timing);
        }

        #endregion

    }
}