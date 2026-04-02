using System;
using System.Collections.Generic;
using HarmonyLib;
using CustomVanillaAbility.CustomClasses;

namespace CustomVanillaAbility.Patches
{
    public static class CustomVanillaAbilityPatches_SkillModel
    {
        public static CustomAbilityBundle _skillBundle = new();

        public static void SafelyExitSkillInit(SkillModel __instance)
        {
            CustomVanillaAbilityMain main = CustomVanillaAbilityMain.Instance;

            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue("skill", out CustomAbilityBundle bundle)) return;
            if (!bundle.affectedLookup.Contains(__instance.GetID())) return;
            if (bundle.customAbilityTable.TryGetValue(__instance, out _)) return;

            System.Collections.Generic.List<CustomAbilityBase> newAbilities = [];
            int baseIndex = __instance.GetAbilityList().Count;

            Il2CppSystem.Collections.Generic.List<AbilityData> skillAbilityList = __instance.skillData.abilityScriptList;

            for (int i = 0; i < skillAbilityList.Count; i++)
            {
                AbilityData selectedData = skillAbilityList[i];
                try
                {
                    string scriptName = null;
                    foreach (string lookup in bundle.abilityLookup)
                    {
                        if (!selectedData.scriptName.StartsWith(lookup)) continue;
                        scriptName = lookup; break;
                    }
                    if (scriptName == null) continue;

                    if (!bundle.abilityClassDict.TryGetValue(scriptName, out System.Type template)) continue;
                    CustomSkillAbilityBase ability = (CustomSkillAbilityBase)Activator.CreateInstance(template);
                    int idx = baseIndex + newAbilities.Count + 1;
                    ability.Init(__instance, selectedData.scriptName, selectedData.Value, idx, selectedData.TurnLimit, selectedData.BuffData);
                    if (selectedData.ConditionalData != null) ability.AttachConditionalData(selectedData.ConditionalData);
                    if (selectedData.TurnLimit != 0) ability.InitLimitedActivateCountData(selectedData.TurnLimit);
                    newAbilities.Add(ability);
                }
                catch (System.Exception ex) { main.Log.LogError(ex); }
            } 

            bundle.customAbilityTable.Add(__instance, newAbilities);
            if (_skillBundle != bundle) _skillBundle = bundle;
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.Init), new Type[] { })]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryHigh)]
        public static void Init_Postfix(SkillModel __instance)
        {
            try { SafelyExitSkillInit(__instance); }
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


        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.CanTeamKillOnStableOverclock))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CanTeamKillOnStableOverclock_Postfix(BattleActionModel action, SkillModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.IgnoreDefenseSkill);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.CanTeamKillOnStableOverclock(action))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsShow))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IsShow_Postfix(SkillModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.IsShow);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.IsShow())
                    {
                        __result = false;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IgnoreDefenseSkill))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IgnoreDefenseSkill_Postfix(BattleActionModel action, BattleUnitModel target, SkillModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.IgnoreDefenseSkill);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.IgnoreDefenseSkill(action, target))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsActionable))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IsActionable_Postfix(BattleActionModel action, SkillModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.IsActionable);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.IsActionable(action))
                    {
                        __result = false;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsPanicBlock))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IsPanicBlock_Postfix(SkillModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.IsPanicBlock);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.IsPanicBlock())
                    {
                        __result = true;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsSkillAbsorbingThisDamage))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IsSkillAbsorbingThisDamage_Postfix(SkillModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.IsSkillAbsorbingThisDamage);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.IsSkillAbsorbingThisDamage())
                    {
                        __result = true;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.CanUseSkill))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CanUseSkill_Postfix(BattleUnitModel actor, SkillModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.CanUseSkill);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.CanUseSkill(actor))
                    {
                        __result = false;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.CanDealTarget))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CanDealTarget_Postfix(BattleActionModel action, BattleUnitModel target, CoinModel coin, SkillModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.CanDealTarget);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.CanDealTarget(action, target, coin))
                    {
                        __result = false;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.AttackByMpDmgRatherThanHpDmg))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void AttackByMpDmgRatherThanHpDmg_Postfix(BattleActionModel action, CoinModel coin, int resultDmg, BattleUnitModel target, SkillModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.AttackByMpDmgRatherThanHpDmg);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.AttackByMpDmgRatherThanHpDmg(action, coin, resultDmg, target))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.CanBeChangedTarget))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CanBeChangedTarget_Postfix(BattleActionModel ownerAction, SkillModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.CanBeChangedTarget);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.CanBeChangedTarget(ownerAction, out bool canChangedTarget))
                    {
                        __result = false;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.CanChangeMainTargetRegardlessSpeed))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CanChangeMainTargetRegardlessSpeed_Postfix(BattleActionModel otherAction, SkillModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.CanChangeMainTargetRegardlessSpeed);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.CanChangeMainTargetRegardlessSpeed(otherAction))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.CanBeChangedTargetIgnoreSpeed))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CanBeChangedTargetIgnoreSpeed_Postfix(BattleActionModel action, BattleActionModel otherAction, SkillModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.CanBeChangedTargetIgnoreSpeed);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.CanBeChangedTargetIgnoreSpeed(action, otherAction))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsDefenseSkillForOther))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IsDefenseSkillForOther_Postfix(BattleUnitModel self, BattleUnitModel originTarget, BattleActionModel opponentActionOrNull, SkillModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.IsDefenseSkillForOther);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.IsDefenseSkillForOther(self, originTarget, opponentActionOrNull))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.CanCheckErode))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void CanCheckErode_Postfix(BattleActionModel action, SkillModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.CanCheckErode);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.CanCheckErode(action))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsReusable))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IsReusable_Postfix(BattleActionModel action, SkillModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.IsReusable);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.IsReusable(action))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsChangeable))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IsChangeable_Postfix(BattleActionModel action, SkillModel __instance, ref bool __result)
        {
            if (__result == false) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.IsChangeable);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (!realAbility.IsChangeable(action))
                    {
                        __result = false;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.BlockAddSinStock))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BlockAddSinStock_Postfix(BattleActionModel action, SkillModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.BlockAddSinStock);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.BlockAddSinStock(action))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsBlockingTargetBurstBuffEffectReact))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IsBlockingTargetBurstBuffEffectReact_Postfix(BattleUnitModel target, int stack, int turn, BattleActionModel selfAction, CoinModel selfCoin, bool isCritical, BATTLE_EVENT_TIMING timing, SkillModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.IsBlockingTargetBurstBuffEffectReact);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.IsBlockingTargetBurstBuffEffectReact(target, stack, turn, selfAction, selfCoin, isCritical, timing))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsBlockingTargetSinkingBuffEffectReact))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IsBlockingTargetSinkingBuffEffectReact_Postfix(BattleUnitModel target, int stack, int turn, BattleActionModel selfAction, CoinModel selfCoin, bool isCritical, BATTLE_EVENT_TIMING timing, SkillModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.IsBlockingTargetSinkingBuffEffectReact);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.IsBlockingTargetSinkingBuffEffectReact(target, stack, turn, selfAction, selfCoin, isCritical, timing))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.IsRetreatSkill))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void IsRetreatSkill_Postfix(BattleActionModel action, SkillModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.IsRetreatSkill);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.IsRetreatSkill(action))
                    {
                        __result = true;
                        return;
                    }
                }
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


        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetSkillLevelAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetSkillLevelAdder_Postfix(BattleActionModel action, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetSkillLevelAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetSkillLevelAdder(action); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetSkillPowerAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetSkillPowerAdder_Postfix(BattleActionModel action, COIN_ROLL_TYPE rollType, Il2CppSystem.Collections.Generic.List<CoinModel> coins, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetSkillPowerAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetSkillPowerAdder(action, rollType); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedSkillPowerAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedSkillPowerAdder_Postfix(BattleActionModel action, COIN_ROLL_TYPE rollType, SinActionModel expectedTargetSinActionOrNull, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetExpectedSkillPowerAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetExpectedSkillPowerAdder(action, rollType, expectedTargetSinActionOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetEvadeSkillPowerAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetEvadeSkillPowerAdder_Postfix(BattleActionModel evadeAction, BattleActionModel attackerAction, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetEvadeSkillPowerAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetEvadeSkillPowerAdder(evadeAction, attackerAction); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetSkillPowerResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetSkillPowerResultAdder_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, CoinModel coinOrNull, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetSkillPowerResultAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetSkillPowerResultAdder(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetCoinScaleAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetCoinScaleAdder_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, BattleActionModel oppoActionOrNull, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetCoinScaleAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetCoinScaleAdder(action, coin, oppoActionOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedCoinScaleAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedCoinScaleAdder_Postfix(SkillModel __instance, BattleActionModel action, CoinModel coin, COIN_ROLL_TYPE rollType, SinActionModel targetSinActionOrNull, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetExpectedCoinScaleAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetExpectedCoinScaleAdder(action, coin, rollType, targetSinActionOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetEvadeCoinScaleAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetEvadeCoinScaleAdder_Postfix(BattleActionModel evadeAction, BattleActionModel attackerAction, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetEvadeCoinScaleAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetEvadeCoinScaleAdder(evadeAction, attackerAction); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedEvadeCoinScaleAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedEvadeCoinScaleAdder_Postfix(BattleActionModel evadeAction, BattleActionModel attackerAction, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetExpectedEvadeCoinScaleAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetExpectedEvadeCoinScaleAdder(evadeAction, attackerAction); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedSkillPowerResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedSkillPowerResultAdder_Postfix(BattleActionModel action, BattleUnitModel expectedTargetOrNull, SinActionModel expectedTargetSinActionOrNull, BattleActionModel expectedOppoActionOrNull, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetExpectedSkillPowerResultAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetExpectedSkillPowerResultAdder(action, expectedTargetOrNull, expectedTargetSinActionOrNull, expectedOppoActionOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetParryingResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetParryingResultAdder_Postfix(BattleActionModel actorAction, int actorResult, BattleActionModel oppoAction, int oppoResult, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetParryingResultAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetParryingResultAdder(actorAction, actorResult, oppoAction, oppoResult); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedParryingResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedParryingResultAdder_Postfix(BattleActionModel actorAction, int actorResult, BattleActionModel oppoActionOrNull, int oppoResult, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetExpectedParryingResultAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetExpectedParryingResultAdder(actorAction, actorResult, oppoActionOrNull, oppoResult); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetOpponentParryingResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetOpponentParryingResultAdder_Postfix(BattleActionModel actorAction, int actorResult, BattleActionModel oppoAction, int oppoResult, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetOpponentParryingResultAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetOpponentParryingResultAdder(actorAction, actorResult, oppoAction, oppoResult); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedOpponentParryingResultAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedOpponentParryingResultAdder_Postfix(BattleActionModel actorAction, int actorResult, BattleActionModel oppoAction, int oppoResult, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetExpectedOpponentParryingResultAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetExpectedOpponentParryingResultAdder(actorAction, actorResult, oppoAction, oppoResult); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetAttackDmgMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAttackDmgMultiplier_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel target, bool isWinDuel, bool isCritical, SkillModel __instance, ref float __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetAttackDmgMultiplier);

            float tempResult = __result;
            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { tempResult *= realAbility.GetAttackDmgMultiplier(action, coin, target, isCritical); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
            __result = tempResult;
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedAttackDmgMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedAttackDmgMultiplier_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel targetOrNull, SinActionModel targetSinActionOrNull, SkillModel __instance, ref float __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetExpectedAttackDmgMultiplier);

            float tempResult = __result;
            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { tempResult *= realAbility.GetExpectedAttackDmgMultiplier(action, targetOrNull, coin); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
            __result = tempResult;
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetAttackDmgAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAttackDmgAdder_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel target, bool isWinDuel, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetAttackDmgAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetAttackDmgAdder(action, target); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedAttackDmgAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedAttackDmgAdder_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel targetOrNull, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetExpectedAttackDmgAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetExpectedAttackDmgAdder(action, targetOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetAttackHpDmgAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAttackHpDmgAdder_Postfix(BattleUnitModel target, CoinModel coin, bool isWinDuel, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetAttackHpDmgAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetAttackHpDmgAdder(target); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetExpectedAttackHpDmgAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetExpectedAttackHpDmgAdder_Postfix(BattleUnitModel target, CoinModel coin, bool isWinDuel, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetExpectedAttackHpDmgAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetExpectedAttackHpDmgAdder(target); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetCriticalChanceMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetCriticalChanceMultiplier_Postfix(BattleActionModel action, SkillModel __instance, ref float __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetCriticalChanceMultiplier);

            float tempResult = __result;
            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { tempResult *= realAbility.GetCriticalChanceMultiplier(action); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
            __result = tempResult;
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetCriticalChanceAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetCriticalChanceAdder_Postfix(BattleActionModel action, CoinModel coin, SkillModel __instance, ref float __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetCriticalChanceAdder);

            float tempResult = __result;
            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { tempResult += realAbility.GetCriticalChanceAdder(action, coin); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
            __result = tempResult;
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetCoinProb), new Type[] { typeof(UnitModel), typeof(float) })]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetCoinProb_UnitModel_Postfix(UnitModel unit, float defaultProb, SkillModel __instance, ref float __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetCoinProb);

            float tempResult = __result;
            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { tempResult += realAbility.GetCoinProb(defaultProb); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
            __result = tempResult;
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetCoinProb), new Type[] { typeof(BattleUnitModel), typeof(float) })]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetCoinProb_BattleUnitModel_Postfix(BattleUnitModel unit, float defaultProb, SkillModel __instance, ref float __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetCoinProb);

            float tempResult = __result;
            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { tempResult += realAbility.GetCoinProb(defaultProb); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
            __result = tempResult;
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetGiveBuffStackAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetGiveBuffStackAdder_Postfix(BattleActionModel action, CoinModel coinOrNull, BattleUnitModel target, BUFF_UNIQUE_KEYWORD keyword, int stack, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetGiveBuffStackAdder);

            int tempResult = stack;
            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { tempResult = realAbility.GetBuffStackAdder(action, coinOrNull, target, keyword, tempResult); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
            __result = tempResult;
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetUseBuffTurnAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetUseBuffTurnAdder_Postfix(BattleActionModel action, int turn, BUFF_UNIQUE_KEYWORD buf, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetUseBuffTurnAdder);

            int tempResult = turn;
            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { tempResult = realAbility.GetUseBuffTurnAdder(action, tempResult, buf); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
            __result = tempResult;
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetGiveBuffTurnAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetGiveBuffTurnAdder_Postfix(BattleActionModel action, BattleUnitModel target, BUFF_UNIQUE_KEYWORD keyword, int turn, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetGiveBuffTurnAdder);

            int tempResult = turn;
            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { tempResult = realAbility.GetBuffTurnAdder(action, target, keyword, tempResult); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
            __result = tempResult;
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetAdditionalActivateCountForDefenseSkill))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetAdditionalActivateCountForDefenseSkill_Postfix(BattleUnitModel owner, SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetAdditionalActivateCountForDefenseSkill);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.GetAdditionalActivateCountForDefenseSkill(owner); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetGiveBsGaugeUpMultiplier))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetGiveBsGaugeUpMultiplier_Postfix(bool onGiveExplosion, BattleUnitModel target, BattleActionModel action, CoinModel coinOrNull, SkillModel __instance, ref float __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetGiveBsGaugeUpMultiplier);

            float tempResult = __result;
            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { tempResult *= realAbility.GetGiveBsGaugeUpMultiplier(onGiveExplosion, target, action, coinOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
            __result = tempResult;
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.StackNextTurnAggroAdder))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void StackNextTurnAggroAdder_Postfix(SkillModel __instance, ref int __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.StackNextTurnAggroAdder);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { __result += realAbility.StackNextTurnAggroAdder(); }
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


        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.BeforeAttack))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BeforeAttack_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.BeforeAttack);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.BeforeAttack(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnBattleStart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnBattleStart_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnBattleStart);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnBattleStart(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnRoundEnd))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnRoundEnd_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnRoundEnd);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnRoundEnd(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnBeforeTurn))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnBeforeTurn_Postfix(BattleActionModel action, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnBeforeTurn);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnBeforeTurn(action); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnBeforeDefense))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnBeforeDefense_Postfix(BattleActionModel action, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnBeforeDefense);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnBeforeDefense(action); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnStartTurn_BeforeLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStartTurn_BeforeLog_Postfix(BattleActionModel action, Il2CppSystem.Collections.Generic.List<BattleUnitModel> targets, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnStartTurn_BeforeLog);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnStartTurn_BeforeLog(action, targets, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnTryEvade))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnTryEvade_Postfix(BattleActionModel action, BattleActionModel attackerAction, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnTryEvade);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnTryEvade(action, attackerAction, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnStartDuel))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStartDuel_Postfix(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnStartDuel);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnStartDuel(selfAction, oppoAction, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnBeforeParryingOnce))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnBeforeParryingOnce_Postfix(BattleActionModel ownerAction, BattleActionModel oppoAction, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnBeforeParryingOnce);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnBeforeParryingOnce(ownerAction, oppoAction); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnBeforeParryingOnce_AfterLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnBeforeParryingOnce_AfterLog_Postfix(BattleActionModel ownerAction, BattleActionModel oppoAction, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnBeforeParryingOnce_AfterLog);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnBeforeParryingOnce(ownerAction, oppoAction); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnDuelAfter_BeforeLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnDuelAfter_BeforeLog_Postfix(SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnDuelAfter_BeforeLog);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnDuelAfter_BeforeLog(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnDuelAfter_AfterLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnDuelAfter_AfterLog_Postfix(SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnDuelAfter_AfterLog);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnDuelAfter_AfterLog(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnWinParrying))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnWinParrying_Postfix(BattleActionModel selfAction, BattleActionModel oppoAction, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnWinParrying);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnWinParrying(selfAction, oppoAction); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnLoseParrying))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnLoseParrying_Postfix(BattleActionModel selfAction, BattleActionModel oppoAction, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnLoseParrying);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnLoseParrying(selfAction, oppoAction); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnWinDuel))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnWinDuel_Postfix(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing, int parryingCount, BattleLog_Parrying lastLogOrNull, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnWinDuel);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnWinDuel(selfAction, oppoAction, timing, parryingCount, lastLogOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnLoseDuel))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnLoseDuel_Postfix(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnLoseDuel);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnLoseDuel(selfAction, oppoAction, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.BeforeBehaviour))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BeforeBehaviour_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.BeforeBehaviour);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.BeforeBehaviour(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnStartBehaviour))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStartBehaviour_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnStartBehaviour);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnStartBehaviour(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnAttackConfirmed))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAttackConfirmed_Postfix(BattleActionModel action, CoinModel coin, BattleUnitModel target, BATTLE_EVENT_TIMING timing, bool isCritical, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnAttackConfirmed);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnAttackConfirmed(action, target, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnSucceedEvade))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnSucceedEvade_Postfix(BattleActionModel attackerAction, BattleActionModel evadeAction, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnSucceedEvade);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnSucceedEvade(attackerAction, evadeAction, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnFailedEvade))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnFailedEvade_Postfix(BattleActionModel attackerAction, BattleActionModel evadeAction, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnFailedEvade);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnFailedEvade(attackerAction, evadeAction, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnSkillChangedEgo))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnSkillChangedEgo_Postfix(BattleActionModel action, bool isOverClock, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnSkillChangedEgo);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnSkillChangedEgo(action, isOverClock, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnChangeSkillBeforeCompleteCommand))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnChangeSkillBeforeCompleteCommand_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnChangeSkillBeforeCompleteCommand);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnChangeSkillBeforeCompleteCommand(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnCancelAction))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnCancelAction_Postfix(BattleActionModel action, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnCancelAction);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnCancelAction(action); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnStartTurn_AfterLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnStartTurn_AfterLog_Postfix(BattleActionModel action, Il2CppSystem.Collections.Generic.List<BattleUnitModel> targets, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnStartTurn_AfterLog);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnStartTurn_AfterLog(action, targets, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnAttackCanceledByAbility))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAttackCanceledByAbility_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnAttackCanceledByAbility);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnAttackCanceledByAbility(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnAfterParryingOnce_BeforeLog))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAfterParryingOnce_BeforeLog_Postfix(PARRYING_RESULT reuslt, BattleActionModel ownerAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnAfterParryingOnce_BeforeLog);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnAfterParryingOnce_BeforeLog(reuslt, ownerAction, oppoAction, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnKillTarget))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnKillTarget_Postfix(BattleActionModel action, CoinModel coinOrNull, BattleUnitModel target, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnKillTarget);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnKillTarget(action, coinOrNull, target, dmgSrcType, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnBreakTarget))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnBreakTarget_Postfix(BattleActionModel action, CoinModel coinOrNull, BattleUnitModel target, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnBreakTarget);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnBreakTarget(action, coinOrNull, target, dmgSrcType, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnDestroyTargetPart))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnDestroyTargetPart_Postfix(BattleActionModel action, CoinModel coinOrNull, BattleUnitModel_Abnormality_Part target, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnDestroyTargetPart);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnDestroyTargetPart(action, coinOrNull, target, dmgSrcType, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnAddCoinByAbility))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnAddCoinByAbility_Postfix(BattleActionModel action, CoinModel newCoin, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnAddCoinByAbility);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnAddCoinByAbility(action, newCoin, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnCriticalIsActivated))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnCriticalIsActivated_Postfix(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing, Il2CppSystem.Collections.Generic.Dictionary<BUFF_UNIQUE_KEYWORD, float> affectKeywords, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnCriticalIsActivated);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnCriticalIsActivated(action, coin, timing, affectKeywords.ConvertDictionary()); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnCompleteCommand))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnCompleteCommand_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnCompleteCommand);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnCompleteCommand(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OnUseCoinConsume))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OnUseCoinConsume_Postfix(BattleActionModel action, CoinModel coin, BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OnUseCoinConsume);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnUseCoinConsume(action, coin, keyword, stack, turn, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.AfterRecheckTargetList))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void AfterRecheckTargetList_Postfix(BattleActionModel action, bool valid, bool mainTargetAlive, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.AfterRecheckTargetList);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.AfterRecheckTargetList(action, valid, mainTargetAlive); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.BeforeCompleteCommand))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BeforeCompleteCommand_Postfix(BattleActionModel action, BATTLE_EVENT_TIMING timing, ref int newSkillID, SkillModel __instance, ref bool __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.BeforeCompleteCommand);

            bool tempResult = false;
            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.BeforeCompleteCommand(action, timing, out int newSkillIDFromAbility))
                    {
                        newSkillID = newSkillIDFromAbility;
                        tempResult = true;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
            if (tempResult) __result = tempResult;
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


        /*
        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OverwriteCriticalResult))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OverwriteCriticalResult_Postfix(BattleActionModel action, CoinModel coin, bool tempCritical, SkillModel __instance, ref bool __result, out Il2CppSystem.Nullable<bool> overwirteCriticalResult)
        {
            overwirteCriticalResult = false;
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OverwriteCriticalResult);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try 
                { 
                    if (realAbility.OverwriteCriticalResult(action, coin, tempCritical, out Il2CppSystem.Nullable<bool> newOverwriteResult))
                    {
                        if (newOverwriteResult.HasValue)
                        {
                            overwirteCriticalResult = newOverwriteResult;
                            return;
                        }
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OverwriteSkillIconID))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OverwriteSkillIconID_Postfix(SkillModel __instance, ref string __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OverwriteSkillIconID);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try 
                { 
                    string newResult = realAbility.OverwriteSkillIconID();
                    if (!string.IsNullOrEmpty(newResult))
                    {
                        __result = newResult;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.OverrideCanDuel))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void OverwriteCanDuel_Postfix(bool value, SkillModel __instance)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.OverrideCanDuel);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OverwriteCanDuel(value); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }


        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.BlockLoseBuffByReactWithAction))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BlockLoseBuffByReactWithAction_Postfix(BattleActionModel action, CoinModel coinOrNull, BUFF_UNIQUE_KEYWORD keyword, Il2CppSystem.Nullable<bool> isCritical, SkillModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.BlockLoseBuffByReactWithAction);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.BlockLoseBuffByReactWithAction(action, keyword, isCritical.Value))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.BlockGivingBuff))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void BlockGivingBuff_Postfix(BattleActionModel action, BattleUnitModel buffTarget, BUFF_UNIQUE_KEYWORD keyword, CoinModel coinOrNull, Il2CppSystem.Nullable<bool> isCritical, SkillModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.BlockGivingBuff);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.BlockGivingBuff(action, buffTarget, keyword, coinOrNull, isCritical.Value))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.ExpectedBlockGivingBuff))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void ExpectedBlockGivingBuff_Postfix(BattleActionModel action, BattleUnitModel buffTarget, BUFF_UNIQUE_KEYWORD keyword, CoinModel coinOrNull, Il2CppSystem.Nullable<bool> isCritical, SkillModel __instance, ref bool __result)
        {
            if (__result == true) return;

            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.ExpectedBlockGivingBuff);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    if (realAbility.ExpectedBlockGivingBuff(action, buffTarget, keyword, coinOrNull, isCritical.Value))
                    {
                        __result = true;
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetPrimeTargets))]
        [HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
        public static void GetPrimeTargets_Postfix(BattleActionModel action, SkillModel __instance, ref Il2CppSystem.Collections.Generic.List<PrimeTargetData> __result)
        {
            if (!CustomVanillaAbilityHelper.ProcessPatchListLogic(_skillBundle, __instance.GetID(), __instance, out System.Collections.Generic.List<CustomAbilityBase> abilityList)) return;
            string methodName = nameof(SkillModel.GetPrimeTargets);

            foreach (CustomAbilityBase ability in abilityList)
            {
                if (ability is not CustomSkillAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    System.Collections.Generic.List<PrimeTargetData> primeTargetData = realAbility.GetPrimeTargets(action);
                    if (primeTargetData != null && primeTargetData.Count > 0)
                    {
                        __result = primeTargetData.ToIl2Cpp();
                        return;
                    }
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }
        */
    }
}
