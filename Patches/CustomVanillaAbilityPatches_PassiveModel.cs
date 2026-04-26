using System;
using Il2CppSystem.Collections.Generic;
using HarmonyLib;
using CustomVanillaAbility.CustomClasses;
using System.Text.RegularExpressions;

namespace CustomVanillaAbility.Patches
{
    public static class CustomVanillaAbilityPatches_PassiveModel
    {
        public static CustomPassiveAbilityBundle _passiveBundle = new();

        public static void SafelyExitSkillInit(PassiveModel __instance)
        {
            CustomVanillaAbilityMain main = CustomVanillaAbilityMain.Instance;
            if (!CustomVanillaAbilityHelper.InitSetup<CustomPassiveAbilityBundle>("passive", __instance.GetID(), out CustomPassiveAbilityBundle bundle)) return;

            if (_passiveBundle != bundle) _passiveBundle = bundle;
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.Init))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryHigh)]
        public static void Init_Postfix(PassiveModel __instance)
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
                if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder holder)) return;

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
                if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder holder)) return;

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
                if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder holder)) return;

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
                if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder holder)) return;

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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IsTargetable);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IsTargetableParts);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CanBeChangedTarget);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CanGiveConcentratedAttack);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IsRegeneratable);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.SpreadHpDmgToAbnormality);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.SpreadHpDmgFromAbnormalityPart);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CheckImmortal);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IsAbnormalityImmortal);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CheckImmortalOtherUnit);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CanTeamKill);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IsActionable);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.ChangeResistOnBreak);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.ChangeResistOnBreak_Part);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CanPickSkill);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IgnoreCheckBreak);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IgnoreBreak);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IgnoreBreakExceptForcedCase);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IgnorePanic);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.HasFakeDead);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CanCreateEmptySlot);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CanCreateEmptySlotPart);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CanTakeMpHeal);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IsChangeTakeDamage);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CheckIfTurnToCorpesOnDie);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.IsAllowedToGiveBuff);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
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


        /// <summary>
        /// /
        /// </summary>
        /// <param name="action"></param>
        /// <param name="__instance"></param>
        /// <param name="__result"></param>


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


        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnAddUnit))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAddUnit_Postfix(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnAddUnit);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnAddUnit(timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnAddUnitPart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAddUnitPart_Postfix(BattleUnitModel part, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnAddUnitPart);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnAddUnitPart(part, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnAddUnitView))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAddUnitView_Postfix(BattleUnitView view, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnAddUnitView);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnAddUnitView(view); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnStageStart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStageStart_Postfix(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnStageStart);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnStageStart(timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnWaveStart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnWaveStart_Postfix(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnWaveStart);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnWaveStart(timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnRoundStart_After_Event))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnRoundStart_After_Event_Postfix(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnRoundStart_After_Event);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnRoundStart_After_Event(timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnRoundStart_After_Event_DeadOrRetreated))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnRoundStart_After_Event_DeadOrRetreated_Postfix(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnRoundStart_After_Event_DeadOrRetreated);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnRoundStart_After_Event_DeadOrRetreated(timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnReturnToField))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnReturnToField_Postfix(int retreatTurn, BattleUnitModel triggerUnit, BUFF_UNIQUE_KEYWORD retreatKeyword, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnReturnToField);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnReturnToField(retreatTurn, triggerUnit, retreatKeyword, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnReturnToFieldOtherUnit_DeadOrRetreated))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnReturnToFieldOtherUnit_DeadOrRetreated_Postfix(BattleUnitModel returnUnit, int retreatTurn, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnReturnToFieldOtherUnit_DeadOrRetreated);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnReturnToFieldOtherUnit_DeadOrRetreated(returnUnit, retreatTurn, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnReleaseStandByOtherUnit_DeadOrRetreated))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnReleaseStandByOtherUnit_DeadOrRetreated_Postfix(BattleUnitModel addedUnit, List<BattleUnitModel> addedUnitList, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnReleaseStandByOtherUnit_DeadOrRetreated);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnReleaseStandByOtherUnit_DeadOrRetreated(addedUnit, addedUnitList); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnReleaseStandBy))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnReleaseStandBy_Postfix(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnReleaseStandBy);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnReleaseStandBy(timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnPanicOrLowMorale))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnPanicOrLowMorale_Postfix(PANIC_LEVEL level, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnPanicOrLowMorale);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnPanicOrLowMorale(level, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnCompleteCommand))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnCompleteCommand_Postfix(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnCompleteCommand);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnCompleteCommand(timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnBattleStart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnBattleStart_Postfix(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnBattleStart);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnBattleStart(timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnBattleEnd))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnBattleEnd_Postfix(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnBattleEnd);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnBattleEnd(timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnStartTurn_BeforeLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStartTurn_BeforeLog_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnStartTurn_BeforeLog);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnStartTurn_BeforeLog(action, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnStartTurn_AfterLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStartTurn_AfterLog_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnStartTurn_AfterLog);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnStartTurn_AfterLog(action, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnStartDuel))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStartDuel_Postfix(BattleActionModel ownerAction, BattleActionModel opponentAction, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnStartDuel);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnStartDuel(ownerAction, opponentAction, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnResult_OnAction))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnResult_OnAction_Postfix(BattleActionModel action, CoinModel coin, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnResult_OnAction);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnResult_OnAction(action, coin); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnResult_OnParrying))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnResult_OnParrying_Postfix(BattleActionModel action, BattleActionModel oppoAction, CoinModel coin, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnResult_OnParrying);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnResult_OnParrying(action, oppoAction, coin); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnWinParrying))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnWinParrying_Postfix(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnWinParrying);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnWinParrying(selfAction, oppoAction, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnLoseParrying))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnLoseParrying_Postfix(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnLoseParrying);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnLoseParrying(selfAction, oppoAction, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnDuelAfter_BeforeLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnDuelAfter_BeforeLog_Postfix(BattleActionModel selfAction, BattleActionModel oppoAction, int parryingCount, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnDuelAfter_BeforeLog);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnDuelAfter_BeforeLog(selfAction, oppoAction, parryingCount, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnWinDuel))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnWinDuel_Postfix(BattleActionModel selfAction, BattleActionModel oppoAction, int parryingCount, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnWinDuel);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnWinDuel(selfAction, oppoAction, parryingCount, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnLoseDuel))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnLoseDuel_Postfix(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnLoseDuel);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnLoseDuel(selfAction, oppoAction, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        /*
        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.BeforeGiveAttackDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BeforeGiveAttackDamage_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel target, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.BeforeGiveAttackDamage);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.BeforeGiveAttackDamage(action, coin, target, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }
        */

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnGiveHpDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnGiveHpDamage_Postfix(BattleUnitModel target, int value, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnGiveHpDamage);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnGiveHpDamage(target, value, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnGiveMpDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnGiveMpDamage_Postfix(BattleUnitModel target, int value, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnGiveMpDamage);

            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnGiveMpDamage(target, value); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnTakeMpDmg_SinBuff))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeMpDmg_SinBuff_Postfix(int value, BATTLE_EVENT_TIMING timing, BUFF_UNIQUE_KEYWORD keyword, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnTakeMpDmg_SinBuff);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnTakeMpDmg_SinBuff(value, timing, keyword); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.BeforeAttack))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BeforeAttack_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.BeforeAttack);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.BeforeAttack(action, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnCriticalActivated))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnCriticalActivated_Postfix(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnCriticalActivated);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnCriticalActivated(action, coin, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnStartCoin))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStartCoin_Postfix(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnStartCoin);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnStartCoin(action, coin, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnEndCoin_BeforeLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndCoin_BeforeLog_Postfix(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnEndCoin_BeforeLog);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnEndCoin_BeforeLog(action, coin, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnEndCoin_AfterLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndCoin_AfterLog_Postfix(BattleActionModel action, CoinModel coin, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnEndCoin_AfterLog);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnEndCoin_AfterLog(action, coin); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnSucceedEvade))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnSucceedEvade_Postfix(BattleActionModel evadeAction, BattleActionModel attackAction, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnSucceedEvade);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnSucceedEvade(evadeAction, attackAction, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnAttackConfirmed))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAttackConfirmed_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel target, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnAttackConfirmed);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnAttackConfirmed(action, coin, target, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnKillTarget))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnKillTarget_Postfix(BattleActionModel actionOrNull, BattleUnitModel target, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnKillTarget);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnKillTarget(actionOrNull, target, dmgSrcType, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnStartBehaviour))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStartBehaviour_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnStartBehaviour);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnStartBehaviour(action, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }
        
        /*
        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnSucceedAttack))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnSucceedAttack_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel target, int finalDmg, int realDmg, bool isCritical, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnSucceedAttack);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnSucceedAttack(action, coin, target, finalDmg, realDmg, isCritical, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }
        */

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnRollOneCoin_AfterAttack))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnRollOneCoin_AfterAttack_Postfix(BattleActionModel action, CoinModel coin, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnRollOneCoin_AfterAttack);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnRollOneCoin_AfterAttack(action, coin); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnEndAttack))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndAttack_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnEndAttack);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnEndAttack(action, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnEndAttackPart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndAttackPart_Postfix(BattleUnitModel_Abnormality_Part part, BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnEndAttackPart);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnEndAttackPart(part, action); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnEndBehaviour))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndBehaviour_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnEndBehaviour);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnEndBehaviour(action, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnEndBehave_Refresh))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndBehave_Refresh_Postfix(BattleActionModel action, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnEndBehave_Refresh);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnEndBehave_Refresh(action); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnEndTurn))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndTurn_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnEndTurn);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnEndTurn(action, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnActivateImmortality))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnActivateImmortality_Postfix(BattleUnitModel immortalActivator, BATTLE_EVENT_TIMING timing, BattleActionModel actionOrNull, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnActivateImmortality);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnActivateImmortality(immortalActivator, timing, actionOrNull); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnActivateAbnormalityImmortality))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnActivateAbnormalityImmortality_Postfix(BATTLE_EVENT_TIMING timing, BattleActionModel actionOrNull, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnActivateAbnormalityImmortality);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnActivateAbnormalityImmortality(timing, actionOrNull); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnDestroyShield))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnDestroyShield_Postfix(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnDestroyShield);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnDestroyShield(timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnRecoverBreak))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnRecoverBreak_Postfix(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnRecoverBreak);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnRecoverBreak(timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnGiveBsGaugeUp))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnGiveBsGaugeUp_Postfix(BattleUnitModel giver, BattleUnitModel target, int value, BATTLE_EVENT_TIMING timing, bool onExplosion, ABILITY_SOURCE_TYPE abilitySrc, BattleActionModel actionOrNull, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnGiveBsGaugeUp);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnGiveBsGaugeUp(giver, target, value, timing, onExplosion, abilitySrc, actionOrNull); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.BeforeTakeAttackDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BeforeTakeAttackDamage_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.BeforeTakeAttackDamage);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.BeforeTakeAttackDamage(action, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.BeforePartTakeAttackDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BeforePartTakeAttackDamage_Postfix(BattleUnitModel_Abnormality abnormality, BattleUnitModel_Abnormality_Part part, BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.BeforePartTakeAttackDamage);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.BeforePartTakeAttackDamage(abnormality, part, action, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnTakeAttackDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeAttackDamage_Postfix(BattleActionModel action, CoinModel coin, int totalDmg, int hpDmg, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnTakeAttackDamage);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnTakeAttackDamage(action, coin, totalDmg, hpDmg, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnTakeAttackDamagePart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeAttackDamagePart_Postfix(BattleUnitModel_Abnormality_Part part, BattleActionModel attackerAction, CoinModel coin, int value, BATTLE_EVENT_TIMING timing, bool isCritical, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnTakeAttackDamagePart);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnTakeAttackDamagePart(part, attackerAction, coin, value, timing, isCritical); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnEndEnemyAttack))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEndEnemyAttack_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnEndEnemyAttack);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnEndEnemyAttack(action, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnBeforeDefense))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnBeforeDefense_Postfix(BattleActionModel action, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnBeforeDefense);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnBeforeDefense(action); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnRoundEnd))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnRoundEnd_Postfix(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnRoundEnd);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnRoundEnd(timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnRetreat))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnRetreat_Postfix(BattleUnitModel triggerUnit, BUFF_UNIQUE_KEYWORD retreatKeyword, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnRetreat);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnRetreat(triggerUnit, retreatKeyword, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnRoundEnd_After))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void Refresh_OnRoundEndAfter_Postfix(PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnRoundEnd_After);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnRoundEndAfter(); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnStageEnd))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStageEnd_Postfix(PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnStageEnd);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnStageEnd(); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.RightBeforeOtherUnitGiveBuffBySkill))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void RightBeforeOtherUnitGiveBuffBySkill_Postfix(BattleUnitModel giver, BattleUnitModel target, BUFF_UNIQUE_KEYWORD bufKeyword, int stack, int turn, SkillModel skill, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.RightBeforeOtherUnitGiveBuffBySkill);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.RightBeforeOtherUnitGiveBuffBySkill(giver, target, bufKeyword, stack, turn, skill, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.RightAfterOtherUnitGiveBuffBySkill))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void RightAfterOtherUnitGiveBuffBySkill_Postfix(BattleUnitModel giver, BattleUnitModel target, BUFF_UNIQUE_KEYWORD bufKeyword, int stack, int turn, SkillModel skill, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.RightAfterOtherUnitGiveBuffBySkill);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.RightAfterOtherUnitGiveBuffBySkill(giver, target, bufKeyword, stack, turn, skill, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.RightAfterLosingBuff))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void RightAfterLosingBuff_Postfix(int loseStack, int loseTurn, BATTLE_EVENT_TIMING timing, BuffInfo info, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.RightAfterLosingBuff);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.RightAfterLosingBuff(loseStack, loseTurn, timing, info); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        /*
        [HarmonyPatch(typeof(PassiveModel), nameof(BattleUnitModel.TakeSwitchVibrationToSpecial))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnSwitchTargetVibrationToSpecial_Postfix(BattleUnitModel target, BUFF_UNIQUE_KEYWORD keyword, int prevStack, int prevTurn, int afterStack, int afterTurn, BATTLE_EVENT_TIMING timing, ABILITY_SOURCE_TYPE abilitySourceType, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(BattleUnitModel.TakeSwitchVibrationToSpecial);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnSwitchTargetVibrationToSpecial(target, keyword, prevStack, prevTurn, afterStack, afterTurn, timing, abilitySourceType); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }
        */

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.RightAfterGetAnyBuff))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void RightAfterGetAnyBuff_Postfix(BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, int activeRound, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing, BattleUnitModel giverOrNull, BattleActionModel actionOrNull, int overStack, int overTurn, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.RightAfterGetAnyBuff);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.RightAfterGetAnyBuff(keyword, stack, turn, activeRound, srcType, timing, giverOrNull, actionOrNull, overStack, overTurn); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.RightAfterGetAnyBuffAtPart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void RightAfterGetAnyBuffAtPart_Postfix(BattleUnitModel_Abnormality_Part part, BUFF_UNIQUE_KEYWORD keyword, int stack, int activeRound, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing, BattleUnitModel giverOrNull, BattleActionModel actionOrNull, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.RightAfterGetAnyBuffAtPart);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.RightAfterGetAnyBuffAtPart(part, keyword, stack, activeRound, srcType, timing, giverOrNull, actionOrNull); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnDestroy))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnDestroy_Postfix(BattleUnitModel destroyerOrNull, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnDestroy);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnDestroy(destroyerOrNull, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnOtherPartDestroyed))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnOtherPartDestroyed_Postfix(BattleUnitModel_Abnormality_Part destroyedPart, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnOtherPartDestroyed);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnOtherPartDestroyed(destroyedPart); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnPartDestroyed))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnPartDestroyed_Postfix(BattleUnitModel_Abnormality_Part destroyedPart, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnPartDestroyed);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnPartDestroyed(destroyedPart, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnPartBreaked))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnPartBreaked_Postfix(BattleUnitModel_Abnormality_Part breakedPart, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnPartBreaked);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnPartBreaked(breakedPart, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnPartRecoverBreak))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnPartRecoverBreak_Postfix(BattleUnitModel_Abnormality_Part recoveredPart, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnPartRecoverBreak);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnPartRecoverBreak(recoveredPart, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnRegenerate))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnRegenerate_Postfix(BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnRegenerate);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnRegenerate(timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnPartRegenerate))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnPartRegenerate_Postfix(BattleUnitModel_Abnormality_Part part, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnPartRegenerate);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnPartRegenerate(part, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnZeroHp))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnZeroHp_Postfix(PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnZeroHp);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnZeroHp(); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnDie))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnDie_Postfix(BattleUnitModel killer, BattleActionModel actionOrNull, DAMAGE_SOURCE_TYPE dmgSrcType, BUFF_UNIQUE_KEYWORD keyword, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnDie);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnDie(killer, actionOrNull, dmgSrcType, keyword, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnBreak))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnBreak_Postfix(BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BATTLE_EVENT_TIMING timing, bool isBreakForcely, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnBreak);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnBreak(attackerOrNull, actionOrNull, timing, isBreakForcely); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnEnemyBrokenByAttacker))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnEnemyBrokenByAttacker_Postfix(BattleActionModel actionOrNull, BattleUnitModel target, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnEnemyBrokenByAttacker);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnEnemyBrokenByAttacker(actionOrNull, target, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnDieOtherUnit))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnDieOtherUnit_Postfix(BattleUnitModel killer, BattleUnitModel dead, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE dmgSrcType, BUFF_UNIQUE_KEYWORD keyword, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnDieOtherUnit);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnDieOtherUnit(killer, dead, timing, dmgSrcType, keyword); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnDiscardSinOtherUnit))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnDiscardSinOtherUnit_Postfix(BattleUnitModel discardUnit, UnitSinModel sin, BATTLE_EVENT_TIMING timing, BattleActionModel actionOrNull, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnDiscardSinOtherUnit);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnDiscardSinOtherUnit(discardUnit, sin, timing, actionOrNull); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnDiscardSin))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnDiscardSin_Postfix(UnitSinModel sin, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnDiscardSin);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnDiscardSin(sin, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnVibrationExplosionOtherUnit))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnVibrationExplosionOtherUnit_Postfix(BattleUnitModel explodedUnit, BattleUnitModel giverOrNull, BattleActionModel actionOrNull, ABILITY_SOURCE_TYPE abilitySrc, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnVibrationExplosionOtherUnit);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnVibrationExplosionOtherUnit(explodedUnit, giverOrNull, actionOrNull, abilitySrc, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnTakeAttackDamageOtherUnit))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeAttackDamageOtherUnit_Postfix(BattleActionModel action, int realDmg, int hpDmg, BattleUnitModel attackedUnit, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnTakeAttackDamageOtherUnit);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnTakeAttackDamageOtherUnit(action, realDmg, hpDmg, attackedUnit, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnUseBloodDinnerUnit))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnUseBloodDinnerUnit_Postfix(BattleUnitModel usedUnit, int stack, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnUseBloodDinnerUnit);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnUseBloodDinnerUnit(usedUnit, stack, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnGiveImmortalState))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnGiveImmortalState_Postfix(BattleUnitModel immortalTaker, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnGiveImmortalState);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnGiveImmortalState(immortalTaker, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnAfterTryTakeHpHeal))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAfterTryTakeHpHeal_Postfix(BattleUnitModel healerOrNull, int tryHeal, int resultHeal, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnAfterTryTakeHpHeal);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnAfterTryTakeHpHeal(healerOrNull, tryHeal, resultHeal, srcType, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnBeforeTryTakeMpHeal))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnBeforeTryTakeMpHeal_Postfix(BattleUnitModel healerOrNull, int tryHeal, int resultHeal, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnBeforeTryTakeMpHeal);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnBeforeTryTakeMpHeal(healerOrNull, tryHeal, resultHeal, srcType, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnAfterTryTakeMpHeal))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAfterTryTakeMpHeal_Postfix(BattleUnitModel healerOrNull, int tryHeal, int resultHeal, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnAfterTryTakeMpHeal);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnAfterTryTakeMpHeal(healerOrNull, tryHeal, resultHeal, srcType, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.CheckLoseBuffStackAndTurn))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CheckLoseBuffStackAndTurn_Postfix(BuffInfo info, ref int loseStack, ref int loseTurn, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.CheckLoseBuffStackAndTurn);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.CheckLoseBuffStackAndTurn(info, loseStack, loseTurn, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnStartPhase))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStartPhase_Postfix(PHASE phase, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnStartPhase);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnStartPhase(phase, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnTakeHpDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeHpDamage_Postfix(int finalDamage, int hpDamage, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BUFF_UNIQUE_KEYWORD keyword, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnTakeHpDamage);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnTakeHpDamage(finalDamage, hpDamage, timing, sourceType, attackerOrNull, actionOrNull, keyword); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnTakeHpDamagePart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeHpDamagePart_Postfix(BattleUnitModel_Abnormality_Part part, int finalDamage, int hpDamage, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BUFF_UNIQUE_KEYWORD keyword, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnTakeHpDamagePart);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnTakeHpDamagePart(part, finalDamage, hpDamage, timing, sourceType, attackerOrNull, actionOrNull, keyword); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnTakeHpDamageOtherUnit))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeHpDamageOtherUnit_Postfix(BattleUnitModel damaged, int finalDamage, int hpDamage, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, List<BattleUnitModel> relatedUnitsOrNull, BUFF_UNIQUE_KEYWORD keyword, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnTakeHpDamageOtherUnit);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnTakeHpDamageOtherUnit(damaged, finalDamage, hpDamage, timing, sourceType, attackerOrNull, actionOrNull, relatedUnitsOrNull, keyword); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnTakeAttackConfirmed))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeAttackConfirmed_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel attacker, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnTakeAttackConfirmed);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnTakeAttackConfirmed(action, coin, attacker, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnChangeHp))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnChangeHp_Postfix(int oldHp, int newHp, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BUFF_UNIQUE_KEYWORD keyword, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnChangeHp);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnChangeHp(oldHp, newHp, dmgSrcType, timing, attackerOrNull, actionOrNull, keyword); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnChangeMp))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnChangeMp_Postfix(int oldMp, int newMp, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnChangeMp);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnChangeMp(oldMp, newMp); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnChangeMpOther))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnChangeMpOther_Postfix(BattleUnitModel mpChangeUnit, int oldMp, int newMp, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnChangeMpOther);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnChangeMpOther(mpChangeUnit, oldMp, newMp); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnTakeMpDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeMpDamage_Postfix(BattleUnitModel attacker, int value, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleActionModel actionOrNull, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnTakeMpDamage);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnTakeMpDamage(attacker, value, timing, sourceType, actionOrNull); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnTakeMpDamageOther))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTakeMpDamageOther_Postfix(BattleUnitModel mpDmgUnit, BattleUnitModel attackerOrNull, int value, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BUFF_UNIQUE_KEYWORD buffKeyword, BattleActionModel actionOrNull, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnTakeMpDamageOther);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnTakeMpDamageOther(mpDmgUnit, attackerOrNull, value, timing, sourceType, actionOrNull); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnAfterTryTakeMpHealOther))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAfterTryTakeMpHealOther_Postfix(BattleUnitModel mpHealUnit, BattleUnitModel healerOrNull, int tryHeal, int resultHeal, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnAfterTryTakeMpHealOther);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnAfterTryTakeMpHealOther(mpHealUnit, healerOrNull, tryHeal, resultHeal, srcType, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnUseBuffTurnBySkill))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnUseBuffTurnBySkill_Postfix(SkillModel skill, BUFF_UNIQUE_KEYWORD bufKeyword, int turn, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnUseBuffTurnBySkill);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnUseBuffTurnBySkill(skill, bufKeyword, turn, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnUseBuff))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnUseBuff_Postfix(BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnUseBuff);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnUseBuff(keyword, stack, turn, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.BeforeUseBuff))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BeforeUseBuff_Postfix(BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.BeforeUseBuff);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.BeforeUseBuff(keyword, stack, turn, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnPickSkills))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnPickSkills_Postfix(PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnPickSkills);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnPickSkills(); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnAddActionToPart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAddActionToPart_Postfix(BattleUnitModel_Abnormality_Part part, BattleActionModel action, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnAddActionToPart);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnAddActionToPart(part, action); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.RightAfterDestroyAnyBuff))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void RightAfterDestroyAnyBuff_Postfix(BuffInfo destroyedBuffInfo, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.RightAfterDestroyAnyBuff);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.RightAfterDestroyAnyBuff(destroyedBuffInfo, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnCanceledByLackOfBuffsAtStartCoin))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnCanceledByLackOfBuffsAtStartCoin_Postfix(BattleActionModel action, CoinModel coin, List<BUFF_UNIQUE_KEYWORD> lackOfBuffs, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnCanceledByLackOfBuffsAtStartCoin);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnCanceledByLackOfBuffsAtStartCoin(action, coin, lackOfBuffs, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnFailedToGetBuff))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnFailedToGetBuff_Postfix(BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, int activeRound, ABILITY_SOURCE_TYPE abilitySrcType, BATTLE_EVENT_TIMING timing, BattleUnitModel giverOrNull, BattleActionModel giverActionOrNull, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnFailedToGetBuff);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnFailedToGetBuff(keyword, stack, turn, activeRound, abilitySrcType, timing, giverOrNull); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
            }
        }

        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.OnUseCoinConsume))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnUseCoinConsume_Postfix(BattleUnitModel owner, BattleActionModel action, CoinModel coin, BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, BATTLE_EVENT_TIMING timing, PassiveModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic_Passive(_passiveBundle, __instance.GetID(), __instance, out CustomPassiveAbilityHolder abilityHolder)) return;
            string methodName = nameof(PassiveModel.OnUseCoinConsume);
            foreach (CustomAbilityBase ability in abilityHolder.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;
                try { realAbility.OnUseCoinConsume(owner, action, coin, keyword, stack, turn, timing); }
                catch (Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo($"Error at {methodName}: {ex}"); }
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


    }
}
