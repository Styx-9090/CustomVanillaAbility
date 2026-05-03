using CustomVanillaAbility.CustomClasses;
using Lethe.Patches;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static BattleActionModel.TargetDataDetail;

namespace CustomVanillaAbility
{
    public static class CustomVanillaAbilityHelper
    {
        public static bool IsOverride(this System.Reflection.MethodInfo method)
        {
            return method.GetBaseDefinition() != method;
        }

        public static bool InitSetup<T>(string bundleName, long id, object instance, out T bundle) where T : CustomAbilityBundle
        {
            bundle = null;

            if (!CustomVanillaAbilityMain.Instance.customAbilityDict.TryGetValue(bundleName, out CustomAbilityBundle preBundle)) return false;
            if (preBundle is not T finalBundle) return false;
            if (!finalBundle.affectedLookup.Contains(id)) return false;
            if (finalBundle.ContainValue(instance)) return false;

            bundle = finalBundle;
            return true;
        }

        public static CustomPassiveAbilityBase CreateCustomPassiveAbility(Type abilityType, string scriptName, Regex regex = null)
        {
            CustomPassiveAbilityBase ability = (CustomPassiveAbilityBase)Activator.CreateInstance(abilityType);
            ability.AttachNameData(scriptName, regex);

            return ability;
        }

        public static bool TryToCreateRegexLinked_Passive(CustomAbilityBundle bundle, string scriptName, out CustomPassiveAbilityBase customSkillResult)
        {
            customSkillResult = null;

            if (!bundle.abilityRegTypeByLookup.TryGetValue(scriptName, out var regType))
            {
                Regex matchedRegex = null;
                Type matchedType = null;

                foreach (var reg in bundle.regexLookup)
                {
                    if (!reg.IsMatch(scriptName)) continue;
                    matchedRegex = reg;

                    if (bundle.abilityClassRegDict.TryGetValue(reg, out matchedType)) break;
                    matchedRegex = null;
                }

                regType = (matchedRegex, matchedType);

                bundle.abilityRegTypeByLookup[scriptName] = regType;
                if (matchedType == null) return false;
            }

            if (regType.Item1 == null || regType.Item2 == null) return false;

            customSkillResult = CustomVanillaAbilityHelper.CreateCustomPassiveAbility(regType.Item2, scriptName, regType.Item1);
            return true;
        }

        public static CustomSkillAbilityBase CreateCustomSkillAbility(Type skillAbilityType, SkillModel skill, int idx, AbilityData selectedData, Regex regex = null)
        {
            CustomSkillAbilityBase ability = (CustomSkillAbilityBase)Activator.CreateInstance(skillAbilityType);
            ability.Init(skill, selectedData.scriptName, selectedData.Value, idx, selectedData.TurnLimit, selectedData.BuffData);
            if (selectedData.ConditionalData != null) ability.AttachConditionalData(selectedData.ConditionalData);
            if (selectedData.TurnLimit != 0) ability.InitLimitedActivateCountData(selectedData.TurnLimit);
            ability.AttachNameData(selectedData.scriptName, regex);

            return ability;
        }

        public static bool TryToCreateRegexLinked_Skill(CustomAbilityBundle bundle, string varScriptName, SkillModel skill, int idx, AbilityData selectedData, out CustomSkillAbilityBase customSkillResult)
        {
            customSkillResult = null;

            if (!bundle.abilityRegTypeByLookup.TryGetValue(varScriptName, out var regType))
            {
                Regex matchedRegex = null;
                Type matchedType = null;

                foreach (var reg in bundle.regexLookup)
                {
                    if (!reg.IsMatch(varScriptName)) continue;
                    matchedRegex = reg;

                    if (bundle.abilityClassRegDict.TryGetValue(reg, out matchedType)) break;
                    matchedRegex = null;
                }

                regType = (matchedRegex, matchedType);

                bundle.abilityRegTypeByLookup[varScriptName] = regType;
                if (matchedType == null) return false;
            }
            
            if (regType.Item1 == null || regType.Item2 == null) return false;

            customSkillResult = CustomVanillaAbilityHelper.CreateCustomSkillAbility(regType.Item2, skill, idx, selectedData, regType.Item1);
            return true;
        }

        public static List<BattleUnitModel> ShuffleUnits(List<BattleUnitModel> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = new Random().Next(n + 1);
                if (k == n) continue;
                BattleUnitModel hold_value = list[k];
                list[k] = list[n];
                list[n] = hold_value;
            }

            return list;
        }

        public static System.Collections.Generic.List<BattleUnitModel> GetCustomTargetingList(BattleObjectManager battleObjectManager, string param, BattleUnitModel owner, BattleUnitModel target)
        {
            System.Collections.Generic.List<BattleUnitModel> list = [];
            UNIT_FACTION filterFaction = UNIT_FACTION.NONE;

            UNIT_FACTION enemyFaction = owner.Faction == UNIT_FACTION.PLAYER ? UNIT_FACTION.ENEMY : UNIT_FACTION.PLAYER;

            bool filterKeyword = param.Contains('$');

            bool noCores = param.Contains("NoCores");
            bool noParts = param.Contains("NoParts");

            bool assistance = param.Contains("Assist");

            if (param.Contains("Enemy")) filterFaction = enemyFaction;
            else if (param.Contains("Ally")) filterFaction = owner.Faction;

            if (filterKeyword)
            {
                string[] circles = param.Split('$');
                param = circles[0];
                BUFF_UNIQUE_KEYWORD bufKeyword = CustomBuffs.ParseBuffUniqueKeyword(circles[1]);

                foreach (BattleUnitModel unit in battleObjectManager.GetAliveList(bufKeyword, 0, assistance, filterFaction)) list.Add(unit);
            }
            else
            {
                if (noCores) foreach (BattleUnitModel unit in battleObjectManager.GetAliveListExceptAbnormalitySelf(filterFaction, assistance)) list.Add(unit);
                else if (noParts) foreach (BattleUnitModel unit in battleObjectManager.GetAliveListExceptAbnormalityPart(filterFaction, assistance)) list.Add(unit);
                else foreach (BattleUnitModel unit in battleObjectManager.GetAliveList(assistance, filterFaction)) list.Add(unit);
            }

            if (param.Contains("AbnoOnly"))
            {
                System.Collections.Generic.List<BattleUnitModel> goodones = new(list.Capacity);
                foreach (BattleUnitModel unit in list) if (unit.IsAbnormalityOrPart) goodones.Add(unit);
                list = goodones;
            }
            else if (param.Contains("NoAbnos"))
            {
                System.Collections.Generic.List<BattleUnitModel> goodones = new(list.Capacity);
                foreach (BattleUnitModel unit in list) if (!unit.IsAbnormalityOrPart) goodones.Add(unit);
                list = goodones;
            }

            if (param.Contains("ExceptSelf")) list.Remove(owner);
            if (param.Contains("ExceptTarget")) list.Remove(target);

            if (param.Contains("Random")) list = ShuffleUnits(list);
            else if (param.Contains("Deploy")) list.Sort((x, y) => x.PARTICIPATE_ORDER.CompareTo(y.PARTICIPATE_ORDER));
            else if (param.Contains("Reversedeploy")) list.Sort((x, y) => y.PARTICIPATE_ORDER.CompareTo(x.PARTICIPATE_ORDER));

            if (param.StartsWith("Slowest")) list.Sort((x, y) => x.GetOriginSpeedForCompare().CompareTo(y.GetOriginSpeedForCompare()));
            else if (param.StartsWith("Fastest")) list.Sort((x, y) => y.GetOriginSpeedForCompare().CompareTo(x.GetOriginSpeedForCompare()));
            else if (param.StartsWith("HighestHPRatio")) list.Sort((x, y) => y.GetHpRatio().CompareTo(x.GetHpRatio()));
            else if (param.StartsWith("LowestHPRatio")) list.Sort((x, y) => x.GetHpRatio().CompareTo(y.GetHpRatio()));
            else if (param.StartsWith("HighestHP")) list.Sort((x, y) => y.Hp.CompareTo(x.Hp));
            else if (param.StartsWith("LowestHP")) list.Sort((x, y) => x.Hp.CompareTo(y.Hp));
            else if (param.StartsWith("HighestMaxHP")) list.Sort((x, y) => y.MaxHp.CompareTo(x.MaxHp));
            else if (param.StartsWith("LowestMaxHP")) list.Sort((x, y) => x.MaxHp.CompareTo(y.MaxHp));
            else if (param.StartsWith("HighestMP")) list.Sort((x, y) => y.Mp.CompareTo(x.Mp));
            else if (param.StartsWith("LowestMP")) list.Sort((x, y) => x.Mp.CompareTo(y.Mp));
            else if (param.StartsWith("HighestSP")) list.Sort((x, y) => y.Mp.CompareTo(x.Mp));
            else if (param.StartsWith("LowestSP")) list.Sort((x, y) => x.Mp.CompareTo(y.Mp));

            return list;
        }

        public static BattleUnitModel GetTargetModel(string param, BattleActionModel action)
        {
            BattleUnitModel self = action.Model;

            switch (param)
            {
                case "Null": return null;
                case "Self": return self;
                case "SelfCore":
                    {
                        BattleUnitModel_Abnormality_Part part = self.TryCast<BattleUnitModel_Abnormality_Part>();
                        if (part != null) return part.Abnormality;
                        else return self;
                    }
                case "Target": return action.GetOriginMainTarget();
                case "TargetCore":
                    {
                        BattleUnitModel_Abnormality_Part part = action.GetOriginMainTarget().TryCast<BattleUnitModel_Abnormality_Part>();
                        if (part != null) return part.Abnormality;
                        else return action.GetMainTarget();
                    }
                case "MainTarget":
                    {
                        if (action == null) return null;
                        TargetDataSet targetDataSet = action._targetDataDetail.GetCurrentTargetSet();
                        return targetDataSet.GetMainTarget();
                    }
            }

            if (param.StartsWith("id"))
            {
                SinManager sinManager_inst = Singleton<SinManager>.Instance;
                BattleObjectManager battleObjectManager = sinManager_inst._battleObjectManager;
                string id_string = param[2..];
                if (!int.TryParse(id_string, out int id)) return null;
                return battleObjectManager.GetModelByUnitID(id);
            }
            else if (param.StartsWith("inst"))
            {
                SinManager sinManager_inst = Singleton<SinManager>.Instance;
                BattleObjectManager battleObjectManager = sinManager_inst._battleObjectManager;

                string id_string = param[4..];
                if (!int.TryParse(id_string, out int id)) return null;

                foreach (BattleUnitModel unit in battleObjectManager.GetModelList()) if (unit.InstanceID == id) return unit;
                return null;
            }
            else if (param.StartsWith("adj"))
            {
                BattleObjectManager battleObjectManager_inst = SingletonBehavior<BattleObjectManager>.Instance;
                if (battleObjectManager_inst == null) return null;
                BattleUnitModel foundUnit = null;

                string side_string = param[3..];
                if (side_string == "Left")
                {
                    Il2CppSystem.Collections.Generic.List<BattleUnitModel> modelList = battleObjectManager_inst.GetPrevUnitsByPortrait(self, 1);
                    if (modelList.Count > 0) foundUnit = modelList.ToArray()[0];
                }
                else
                {
                    Il2CppSystem.Collections.Generic.List<BattleUnitModel> modelList = battleObjectManager_inst.GetNextUnitsByPortrait(self, 1);
                    if (modelList.Count > 0) foundUnit = modelList.ToArray()[0];
                }
                return foundUnit;
            }
            else
            {
                BattleObjectManager battleObjectManager_inst = SingletonBehavior<BattleObjectManager>.Instance;
                if (battleObjectManager_inst == null) return null;
                BattleUnitModel foundUnit = null;

                System.Collections.Generic.List<BattleUnitModel> list = GetCustomTargetingList(battleObjectManager_inst, param, self, action.GetOriginMainTarget());
                if (list.Count > 0) foundUnit = list.ToArray()[0];
                return foundUnit;
            }
        }

        public static Il2CppSystem.Collections.Generic.List<BattleUnitModel> GetTargetModelList(string param, BattleActionModel action)
        {
            Il2CppSystem.Collections.Generic.List<BattleUnitModel> unitList = new(8);
            SinManager sinManager_inst = Singleton<SinManager>.Instance;
            BattleObjectManager battleObjectManager = sinManager_inst._battleObjectManager;

            BattleUnitModel self = action.Model;
            BattleUnitModel target = action.GetOriginMainTarget();

            switch (param)
            {
                case "Null": return unitList;
                case "Self":
                    {
                        unitList.Add(self);
                        return unitList;
                    }
                case "SelfCore":
                    {
                        BattleUnitModel_Abnormality_Part part = self.TryCast<BattleUnitModel_Abnormality_Part>();
                        if (part != null) unitList.Add(part.Abnormality);
                        else unitList.Add(self);

                        return unitList;
                    }
                case "SelfParts":
                    {

                        BattleUnitModel_Abnormality_Part part = self.TryCast<BattleUnitModel_Abnormality_Part>();
                        BattleUnitModel_Abnormality isLikeAnAbno = null;
                        if (part != null) isLikeAnAbno = part.Abnormality;
                        else isLikeAnAbno = self.TryCast<BattleUnitModel_Abnormality>();
                        

                        if (isLikeAnAbno == null) return unitList;

                        var LikeListOfAbnoParts = isLikeAnAbno._partList;
                        foreach (var partOfAbno in LikeListOfAbnoParts) unitList.Add(partOfAbno);
                        

                        return unitList;
                    }
                case "Target":
                    {
                        if (target != null) unitList.Add(target);
                        return unitList;
                    }
                case "TargetCore":
                    {
                        BattleUnitModel_Abnormality_Part part = target.TryCast<BattleUnitModel_Abnormality_Part>();
                        if (part != null) unitList.Add(part.Abnormality);
                        else unitList.Add(target);
                        return unitList;
                    }
                case "TargetParts":
                    {

                        BattleUnitModel_Abnormality_Part part = target.TryCast<BattleUnitModel_Abnormality_Part>();
                        BattleUnitModel_Abnormality isLikeAnAbno = null;
                        if (part != null) isLikeAnAbno = part.Abnormality;
                        else isLikeAnAbno = target.TryCast<BattleUnitModel_Abnormality>();

                        if (isLikeAnAbno == null) return unitList;

                        var LikeListOfAbnoParts = isLikeAnAbno._partList;
                        foreach (var partOfAbno in LikeListOfAbnoParts) unitList.Add(partOfAbno);

                        return unitList;
                    }
                case "MainTarget":
                    {
                        if (action == null) 
                        { 
                            unitList.Add(null); 
                            return unitList; 
                        }
                        TargetDataSet targetDataSet = action._targetDataDetail.GetCurrentTargetSet();
                        unitList.Add(targetDataSet.GetMainTarget());
                        return unitList;
                    }
                case "EveryTarget":
                    {
                        TargetDataSet targetDataSet = action._targetDataDetail.GetCurrentTargetSet();
                        unitList.Add(targetDataSet.GetMainTarget());
                        foreach (SinActionModel sinActionModel in targetDataSet.GetSubTargetSinActionList())
                        {
                            BattleUnitModel model = sinActionModel.UnitModel;
                            if (!unitList.Contains(model)) unitList.Add(sinActionModel.UnitModel);
                        }
                        return unitList;
                    }
                case "SubTarget":
                    {
                        TargetDataSet targetDataSet = action._targetDataDetail.GetCurrentTargetSet();
                        foreach (SinActionModel sinActionModel in targetDataSet.GetSubTargetSinActionList())
                        {
                            BattleUnitModel model = sinActionModel.UnitModel;
                            if (!unitList.Contains(model)) unitList.Add(sinActionModel.UnitModel);
                        }
                        return unitList;
                    }
                case "All":
                    return battleObjectManager.GetModelList();
            }

            if (param.StartsWith("id"))
            {
                string id_string = param[2..];
                if (!int.TryParse(id_string, out int id)) return null;
                foreach (BattleUnitModel unit in battleObjectManager.GetModelList()) if (unit.GetUnitID() == id) unitList.Add(unit);
                return unitList;
            }
            if (param.StartsWith("inst"))
            {
                string id_string = param[4..];
                if (!int.TryParse(id_string, out int id)) return null;
                foreach (BattleUnitModel unit in battleObjectManager.GetModelList()) if (unit.InstanceID == id) unitList.Add(unit);
                return unitList;
            }
            if (param.StartsWith("adj"))
            {
                string side_string = param[3..];
                if (side_string == "Left")
                {
                    Il2CppSystem.Collections.Generic.List<BattleUnitModel> modelList = battleObjectManager.GetPrevUnitsByPortrait(self, 1);
                    if (modelList.Count > 0) unitList.Add(modelList.ToArray()[0]);
                }
                else
                {
                    Il2CppSystem.Collections.Generic.List<BattleUnitModel> modelList = battleObjectManager.GetNextUnitsByPortrait(self, 1);
                    if (modelList.Count > 0) unitList.Add(modelList.ToArray()[0]);
                }
                return unitList;
            }

            UNIT_FACTION enemyFaction = self.Faction == UNIT_FACTION.PLAYER ? UNIT_FACTION.ENEMY : UNIT_FACTION.PLAYER;

            if (param == "EveryCoreAlly")
            {
                foreach (BattleUnitModel unit in battleObjectManager.GetAliveList(false, self.Faction))
                {
                    if (unit is BattleUnitModel_Abnormality || !unit.IsAbnormalityOrPart) unitList.Add(unit);
                }
            }
            else if (param == "EveryAbnoCoreAlly")
            {
                foreach (BattleUnitModel unit in battleObjectManager.GetAliveList(false, self.Faction))
                {
                    if (unit is BattleUnitModel_Abnormality) unitList.Add(unit);
                }
            }
            else if (param == "EveryCoreEnemy")
            {
                foreach (BattleUnitModel unit in battleObjectManager.GetAliveList(false, enemyFaction))
                {
                    if (unit is BattleUnitModel_Abnormality || !unit.IsAbnormalityOrPart) unitList.Add(unit);
                }
            }
            else if (param == "EveryAbnoCoreEnemy")
            {
                foreach (BattleUnitModel unit in battleObjectManager.GetAliveList(false, enemyFaction))
                {
                    if (unit is BattleUnitModel_Abnormality) unitList.Add(unit);
                }
            }
            else
            {
                System.Collections.Generic.List<BattleUnitModel> list = GetCustomTargetingList(battleObjectManager, param, self, target);

                int num = 1;
                string text = Regex.Replace(param, "\\D", "");
                if (text.Length > 0) num = int.Parse(text);
                

                num = Math.Min(num, list.Count);
                if (num > 0)
                {
                    for (int i = 0; i < num; i++) unitList.Add(list.ToArray()[i]);
                }
            }

            return unitList;
        }
    }
}
