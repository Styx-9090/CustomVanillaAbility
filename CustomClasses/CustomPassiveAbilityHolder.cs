using CustomVanillaAbility.Patches;
using CustomVanillaAbility.TestingClass;
using System;
using System.Collections.Generic;

namespace CustomVanillaAbility.CustomClasses
{
    public class CustomPassiveAbilityHolder
    {
        public PassiveModel passiveModel;
        protected BattleUnitModel owner;
        protected Il2CppSystem.Collections.Generic.List<PassiveConditionStaticData> attributeResonanceCondition;
        protected Il2CppSystem.Collections.Generic.List<PassiveConditionStaticData> attributeStockCondition;
        protected Dictionary<string, int> timingDict;
        protected PASSIVE_STATUS _status = PASSIVE_STATUS.DEACTIVE;
        protected PassiveAbility _tempStorage = null;
        private PassiveAbility _originalScript = null;


        public bool _isActivated;
        public bool _isActivatedThisTurn;
        public bool _isActivatedOnThisBattle;
        public List<CustomPassiveAbilityBase> passiveList;


        public CustomPassiveAbilityHolder()
        {

        }

        public CustomPassiveAbilityHolder(List<CustomPassiveAbilityBase> customPassiveList)
        {
            this.passiveList = customPassiveList;
        }


        public void Init(PassiveModel passive)
        {
            this.passiveModel = passive;
            this.owner = passive.Owner;
            this._originalScript = passive.Script;
            attributeResonanceCondition = passive.ClassInfo.GetAttributeResonanceConditionList();
            attributeStockCondition = passive.ClassInfo.GetAttributeStockConditionList();
            timingDict = new();

            foreach (CustomPassiveAbilityBase customPassive in passiveList)
            {
                customPassive.Init(this);
                foreach (string timing in customPassive._triggerMethodHash) RegisterTiming(timing);
            }

            if ((attributeResonanceCondition == null || attributeResonanceCondition.Count <= 0) && (attributeStockCondition == null || attributeStockCondition.Count <= 0) == false) OnPassiveActivated();
            else CheckActiveCondition();
        }

        public void OnPassiveActivated()
        {
            if (_isActivated) return;

            _status = PASSIVE_STATUS.ACTIVE;
            _isActivated = true;
            _isActivatedOnThisBattle = true;

            if (!_isActivatedThisTurn)
            {
                _isActivatedThisTurn = true;
                foreach (CustomPassiveAbilityBase customPassive in this.passiveList) customPassive.OnUpdateStatus(true);
            }

            if (this.owner.IsAbnormalityOrPart && !this.owner.IsShadowEnemy())
            {
                UnlockInformationManager instance = Singleton<UnlockInformationManager>.Instance;
                instance.UnlockPassiveStatus(this.owner.GetOriginUnitID(), this.passiveModel.GetID());
            }
        }

        public void OnPassiveDeactivated()
        {
            if (!_isActivated) return;

            _status = PASSIVE_STATUS.DEACTIVE;
            _isActivated = false;

            if (_isActivatedThisTurn)
            {
                _isActivatedThisTurn = false;
                foreach (CustomPassiveAbilityBase customPassive in this.passiveList) customPassive.OnUpdateStatus(false);
            }
        }

        public void CheckActiveCondition()
        {
            var sinManager = Singleton<SinManager>.Instance;
            var stockManager = sinManager._egoStockMangaer;
            var resManager = sinManager._resManager;

            bool stockOk = CheckStockConditions(stockManager);
            bool resOk = CheckResonanceConditions(resManager);

            if (stockOk && resOk) OnPassiveActivated();
            else OnPassiveDeactivated();
        }

        private bool CheckResonanceConditions(SinManager.ResonanceManager resManager)
        {
            if (attributeResonanceCondition == null || attributeResonanceCondition.Count == 0) return true;

            for (int i = 0; i < attributeStockCondition.Count; i++)
            {
                PassiveConditionStaticData data = attributeResonanceCondition[i];
                int value = resManager.GetAttributeResonance(owner.Faction, data.AttributeType);
                if (value < data.Value) return false;
            }

            return true;
        }

        private bool CheckStockConditions(SinManager.EgoStockManager stockManager)
        {
            if (attributeStockCondition == null || attributeStockCondition.Count == 0) return true;

            for (int i = 0; i < attributeStockCondition.Count; i++)
            {
                PassiveConditionStaticData data = attributeStockCondition[i];
                int value = stockManager.GetAttributeStockNumberByAttributeType(owner.Faction, data.AttributeType);
                if (value < data.Value) return false;
            }

            return true;
        }

        public PASSIVE_STATUS GetPassiveStatus()
        {
            return this._status;
        }

        public bool IsActive()
        {
            return this._status != PASSIVE_STATUS.DEACTIVE;
        }

        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//

        public void RegisterTiming(string timing)
        {
            if (this.timingDict.ContainsKey(timing)) this.timingDict[timing]++;
            else this.timingDict[timing] = 1;
        }

        public void UnregisterTiming(string timing)
        {
            if (!this.timingDict.TryGetValue(timing, out int count)) return;

            if (--count <= 0) this.timingDict.Remove(timing);
            else this.timingDict[timing] = count;
        }

        public void PassiveRemoveScript()
        {
            if (this.passiveModel._script == null) return;

            this._tempStorage = this.passiveModel._script;
            this.passiveModel._script = null;
        }

        public void PassiveAddScript(PassiveAbility newScript = null)
        {
            if (this.passiveModel._script != null) return;

            this.passiveModel._script = newScript ?? this._tempStorage;
        }

        public bool PassiveHasScript(out int result)
        {
            result = 0;
            if (this.passiveModel.Script == null) return false;
            else
            {
                if (this.passiveModel.Script == this._originalScript) result = 1;
                else if (this.passiveModel.Script != this._originalScript && this.passiveModel.Script.GetIl2CppType() == this._originalScript.GetIl2CppType()) result = 3;
                else result = 2;
            }
            return true;
        }

        public bool HasCustomAbility<T>(T ability = null, bool checkInheritance = false) where T : CustomPassiveAbilityBase
        {
            if (ability != null) return this.passiveList.Contains(ability);

            Type targetType = typeof(T);
            foreach (CustomPassiveAbilityBase customAbility in passiveList)
            {
                if (CustomVanillaAbilityHelper.CheckInheritance<T>(customAbility, targetType, checkInheritance) > 0) return true; 
            }

            return false;
        }

        public void AddCustomAbility<T>(T newAbility, bool avoidDuplicates = true, bool checkInheritance = false) where T : CustomPassiveAbilityBase
        {
            if (avoidDuplicates && this.HasCustomAbility<T>(null, checkInheritance)) return;

            this.passiveList.Add(newAbility);
            newAbility.Init(this);
            foreach (string timing in newAbility._triggerMethodHash) RegisterTiming(timing);
        }

        public void RemoveCustomAbility<T>(T oldAbility, bool includeDuplicates = false, bool checkInheritance = false) where T : CustomPassiveAbilityBase
        {
            if (oldAbility != null)
            {
                if (this.passiveList.Remove(oldAbility))
                    foreach (string timing in oldAbility._triggerMethodHash) UnregisterTiming(timing);

                return;
            }


            Type targetType = typeof(T);
            for (int i = passiveList.Count - 1; i >= 0; i--)
            {
                CustomPassiveAbilityBase customAbility = passiveList[i];

                if (CustomVanillaAbilityHelper.CheckInheritance<T>(customAbility, targetType, checkInheritance) <= 0)  continue;

                passiveList.RemoveAt(i);
                foreach (string timing in customAbility._triggerMethodHash) UnregisterTiming(timing);

                if (!includeDuplicates) break;
            }
        }

        public int TestSingularAbilityType<T>(Func<CustomPassiveAbilityBase, int> methodFunc, bool countDuplicates, out bool foundValidResult, bool checkInheritance = false, int skipCount = 0) where T : CustomPassiveAbilityBase
        {
            foundValidResult = false;
            int result = 0;
            int correctResult = 0;
            Type targetType = typeof(T);

            foreach (CustomPassiveAbilityBase customAbility in passiveList)
            {
                if (CustomVanillaAbilityHelper.CheckInheritance<T>(customAbility, targetType, checkInheritance) > 0)
                { 
                    if (correctResult++ < skipCount) continue;
                    result += methodFunc(customAbility);
                    foundValidResult = true;
                    if (!countDuplicates) break;
                }
            }

            return result;
        }


        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//


        public int GetTakeBuffTurnAdderOtherUnit(BattleUnitModel taker, BattleActionModel action, BUFF_UNIQUE_KEYWORD keyword, int originalTurn, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.GetTakeBuffTurnAdderOtherUnit);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetTakeBuffTurnAdderOtherUnit(taker, action, keyword, originalTurn, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetGiveBuffStackAdder(BattleActionModel action, SkillModel skill, BattleUnitModel target, BUFF_UNIQUE_KEYWORD buf, int turn, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.GetGiveBufStackAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetGiveBuffStackAdder(action, skill, target, buf, turn, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetGiveBuffTurnAdder(BattleActionModel action, SkillModel skill, CoinModel coinOrNull, BattleUnitModel target, BUFF_UNIQUE_KEYWORD buf, int currentStack, int currentTurn, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.GetGiveBufTurnAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetGiveBuffTurnAdder(action, skill, coinOrNull, target, buf, currentStack, currentTurn, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetTakeBuffStackAdder(BattleActionModel action, SkillModel skill, BUFF_UNIQUE_KEYWORD buf, int originStack, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.GetTakeBuffStackAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetTakeBuffStackAdder(action, skill, buf, originStack, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetTakeBuffTurnAdder(BattleActionModel action, SkillModel skill, BUFF_UNIQUE_KEYWORD buf, int originalTurn, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.GetTakeBuffTurnAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetTakeBuffTurnAdder(action, skill, buf, originalTurn, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetAggroAdder(SinActionModel sinAction)
        {
            string methodName = nameof(PassiveModel.GetAggroAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetAggroAdder(sinAction); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetAttackWeightAdder(BattleActionModel action)
        {
            string methodName = nameof(PassiveModel.GetAttackWeightAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetAttackWeightAdder(action); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetMentalSystemResultIncreaseAdder()
        {
            string methodName = nameof(PassiveModel.GetMentalSystemResultIncreaseAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetMentalSystemResultIncreaseAdder(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetMentalSystemResultDecreaseAdder()
        {
            string methodName = nameof(PassiveModel.GetMentalSystemResultDecreaseAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetMentalSystemResultDecreaseAdder(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        /*
        public int GetGiveBuffStackAdder(BattleActionModel action, SkillModel skill, CoinModel coinOrNull, BattleUnitModel target, BUFF_UNIQUE_KEYWORD buf, int currentStack, BATTLE_EVENT_TIMING timing, bool isCritical)
        {
            return 0;
        }
        */

        public int GetUseBuffTurnAdder(BattleActionModel action, SkillModel skill, int turn, BUFF_UNIQUE_KEYWORD buf)
        {
            string methodName = nameof(PassiveModel.GetUseBuffTurnAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetUseBuffTurnAdder(action, skill, turn, buf); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetSinBuffDamageAdder(BUFF_UNIQUE_KEYWORD buff, int dmg)
        {
            string methodName = nameof(PassiveModel.GetSinBuffDamageAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetSinBuffDamageAdder(buff, dmg); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetMaxBuffStackAdder(BUFF_UNIQUE_KEYWORD keyword)
        {
            string methodName = nameof(PassiveModel.GetMaxBuffStackAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetMaxBuffStackAdder(keyword); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetMaxBuffTurnAdder(BUFF_UNIQUE_KEYWORD keyword)
        {
            string methodName = nameof(PassiveModel.GetMaxBuffTurnAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetMaxBuffTurnAdder(keyword); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetTakeHpHealAdder(BattleUnitModel healerOrNull, ABILITY_SOURCE_TYPE srcType)
        {
            string methodName = nameof(PassiveModel.GetTakeHpHealAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetTakeHpHealAdder(healerOrNull, srcType); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetActionSlotAdder()
        {
            string methodName = nameof(PassiveModel.GetActionSlotAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetActionSlotAdder(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetMaxHpAdder()
        {
            string methodName = nameof(PassiveModel.GetMaxHpAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetMaxHpAdder(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetMaxHpAdderPart(BattleUnitModel_Abnormality_Part part)
        {
            string methodName = nameof(PassiveModel.GetMaxHpAdderPart);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetMaxHpAdderPart(part); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetSpeedAdder()
        {
            string methodName = nameof(PassiveModel.GetSpeedAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetSpeedAdder(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetMaxSpeedAdder()
        {
            string methodName = nameof(PassiveModel.GetMaxSpeedAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetMaxSpeedAdder(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetMinSpeedAdder()
        {
            string methodName = nameof(PassiveModel.GetMinSpeedAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetMinSpeedAdder(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetCoinScaleAdder(BattleActionModel action, BattleActionModel oppoActionOrNull, CoinModel coin)
        {
            string methodName = nameof(PassiveModel.GetCoinScaleAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetCoinScaleAdder(action, oppoActionOrNull, coin); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetExpectedCoinScaleAdder(BattleActionModel action, CoinModel coin, SinActionModel expectedTargetSinActionOrNull)
        {
            string methodName = nameof(PassiveModel.GetExpectedCoinScaleAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetExpectedCoinScaleAdder(action, coin, expectedTargetSinActionOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetParryingResultAdder(BattleActionModel action, int actorResult, BattleActionModel oppoAction, int oppoResult, int parryingCount)
        {
            string methodName = nameof(PassiveModel.GetParryingResultAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetParryingResultAdder(action, actorResult, oppoAction, oppoResult, parryingCount); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetExpectedParryingResultAdder(BattleActionModel action, int actorResult, BattleActionModel oppoActionOrNull, int oppoResult)
        {
            string methodName = nameof(PassiveModel.GetExpectedParryingResultAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetExpectedParryingResultAdder(action, actorResult, oppoActionOrNull, oppoResult); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetOpponentParryingResultAdder(BattleActionModel action, int actorResult, BattleActionModel oppoAction, int oppoResult)
        {
            string methodName = nameof(PassiveModel.GetOpponentParryingResultAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetOpponentParryingResultAdder(action, actorResult, oppoAction, oppoResult); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetExpectedOpponentParryingResultAdder(BattleActionModel action, int actorResult, BattleActionModel oppoAction, int oppoResult)
        {
            string methodName = nameof(PassiveModel.GetExpectedOpponentParryingResultAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetExpectedOpponentParryingResultAdder(action, actorResult, oppoAction, oppoResult); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetSkillPowerAdder(BattleActionModel action, COIN_ROLL_TYPE rollType)
        {
            string methodName = nameof(PassiveModel.GetSkillPowerAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetSkillPowerAdder(action, rollType); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetExpectedSkillPowerAdder(BattleActionModel action, Il2CppSystem.Collections.Generic.List<BattleActionModel> prevActions, COIN_ROLL_TYPE rollType, SinActionModel expectedTargetSinActionOrNull)
        {
            string methodName = nameof(PassiveModel.GetExpectedSkillPowerAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetExpectedSkillPowerAdder(action, prevActions, rollType, expectedTargetSinActionOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetSkillPowerResultAdder(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.GetSkillPowerResultAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetSkillPowerResultAdder(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetExpectedSkillPowerResultAdder(BattleActionModel action, Il2CppSystem.Collections.Generic.List<BattleActionModel> prevActions, BattleUnitModel expectedTarget)
        {
            string methodName = nameof(PassiveModel.GetExpectedSkillPowerResultAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetExpectedSkillPowerResultAdder(action, prevActions, expectedTarget); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetAttackAdder(BattleActionModel actionOrNull)
        {
            string methodName = nameof(PassiveModel.GetAttackAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetAttackAdder(actionOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetDefenseAdder()
        {
            string methodName = nameof(PassiveModel.GetDefenseAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetDefenseAdder(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }
        public int GetAttackDmgAdder(BattleActionModel action, BattleUnitModel target)
        {
            string methodName = nameof(PassiveModel.GetAttackDmgAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetAttackDmgAdder(action, target); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetExpectedAttackDmgAdder(BattleActionModel action, BattleUnitModel targetOrNull)
        {
            string methodName = nameof(PassiveModel.GetExpectedAttackDmgAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetExpectedAttackDmgAdder(action, targetOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetTakeAttackDmgAdder(BattleActionModel action, BattleUnitModel attacker)
        {
            string methodName = nameof(PassiveModel.GetTakeAttackDmgAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetTakeAttackDmgAdder(action, attacker); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetAttackHpDmgAdder(BattleUnitModel target)
        {
            string methodName = nameof(PassiveModel.GetAttackHpDmgAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetAttackHpDmgAdder(target); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetExpectedAttackHpDmgAdder(BattleUnitModel target)
        {
            string methodName = nameof(PassiveModel.GetExpectedAttackHpDmgAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetExpectedAttackHpDmgAdder(target); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetTakeHpDmgAdder()
        {
            string methodName = nameof(PassiveModel.GetTakeHpDmgAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetTakeHpDmgAdder(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int GetExpectedTakeHpDmgAdder()
        {
            string methodName = nameof(PassiveModel.GetExpectedTakeHpDmgAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetExpectedTakeHpDmgAdder(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public int ChangeTakeDamage(BattleActionModel action, CoinModel coinOrNull, int resultDmg, DAMAGE_SOURCE_TYPE dmgSrcType, BUFF_UNIQUE_KEYWORD keyword, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.GetTakeBuffTurnAdderOtherUnit);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try
                {
                    int changedDamage = realAbility.ChangeTakeDamage(action, coinOrNull, resultDmg, dmgSrcType, keyword, timing);
                    if (changedDamage != resultDmg) return changedDamage;
                }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return resultDmg;
        }

        public int GetCoinProbAdder()
        {
            string methodName = nameof(PassiveModel.GetCoinProbAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetCoinProbAdder(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }


        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------//


        public virtual float GetTakeBuffStackMultiplier(SkillModel skill, BUFF_UNIQUE_KEYWORD buf)
        {
            string methodName = nameof(PassiveModel.GetTakeBuffStackMultiplier);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetTakeBuffStackMultiplier(skill, buf); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetTakeBuffTurnMultiplier(SkillModel skill, BUFF_UNIQUE_KEYWORD buf)
        {
            string methodName = nameof(PassiveModel.GetTakeBuffTurnMultiplier);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetTakeBuffTurnMultiplier(skill, buf); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetSinBuffDamageMultiplier(BUFF_UNIQUE_KEYWORD buff)
        {
            string methodName = nameof(PassiveModel.GetSinBuffDamageMultiplier);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetSinBuffDamageMultiplier(buff); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetTakeHpHealMultiplier(BattleUnitModel healerOrNull, ABILITY_SOURCE_TYPE srcType)
        {
            string methodName = nameof(PassiveModel.GetTakeHpHealMultiplier);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetTakeHpHealMultiplier(healerOrNull, srcType); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetTakeHpHealMultiplierPart(BattleUnitModel_Abnormality_Part part, BattleUnitModel healerOrNull, ABILITY_SOURCE_TYPE srcType)
        {
            string methodName = nameof(PassiveModel.GetTakeHpHealMultiplierPart);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetTakeHpHealMultiplierPart(part, healerOrNull, srcType); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetGiveBsGaugeUpMultiplier(bool onGiveExplosion, BattleActionModel actionOrNull, CoinModel coinOrNull)
        {
            string methodName = nameof(PassiveModel.GetGiveBsGaugeUpMultiplier);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetGiveBsGaugeUpMultiplier(onGiveExplosion, actionOrNull, coinOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetBsGaugeUpMultiplier(bool onGiveExplosion, BattleActionModel actionOrNull, CoinModel coinOrNull)
        {
            string methodName = nameof(PassiveModel.GetBsGaugeUpMultiplier);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetBsGaugeUpMultiplier(onGiveExplosion, actionOrNull, coinOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetMaxHpMultiplier()
        {
            string methodName = nameof(PassiveModel.GetMaxHpMultiplier);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetMaxHpMultiplier(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetMaxHpMultiplierPartToAbnormality()
        {
            string methodName = nameof(PassiveModel.GetMaxHpMultiplierPartToAbnormality);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetMaxHpMultiplierPartToAbnormality(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetAtkResistAdder(ATK_BEHAVIOUR type)
        {
            string methodName = nameof(PassiveModel.GetAtkResistAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetAtkResistAdder(type); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetAtkResistMultiplier(ATK_BEHAVIOUR type)
        {
            string methodName = nameof(PassiveModel.GetAtkResistMultiplier);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetAtkResistMultiplier(type); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetDefenseMultiplier()
        {
            string methodName = nameof(PassiveModel.GetDefenseMultiplier);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetDefenseMultiplier(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetAttributeResistAdder(global::ATTRIBUTE_TYPE type)
        {
            string methodName = nameof(PassiveModel.GetAttributeResistAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetAttributeResistAdder(type); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetAttributeResistMultiplier(global::ATTRIBUTE_TYPE type)
        {
            string methodName = nameof(PassiveModel.GetAttributeResistMultiplier);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetAttributeResistMultiplier(type); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetAttackDmgMultiplier(BattleActionModel action, CoinModel coin, BattleUnitModel target, bool isWinDuel, bool isCritical)
        {
            string methodName = nameof(PassiveModel.GetAttackDmgMultiplier);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetAttackDmgMultiplier(action, coin, target, isWinDuel, isCritical); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetExpectedAttackDmgMultiplier(BattleActionModel action, CoinModel coin, BattleUnitModel targetOrNull, SinActionModel targetSinActionOrNull)
        {
            string methodName = nameof(PassiveModel.GetExpectedAttackDmgMultiplier);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetExpectedAttackDmgMultiplier(action, coin, targetOrNull, targetSinActionOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetTakeAttackDmgMultiplier(BattleActionModel action, BattleUnitModel attacker, bool isCritical)
        {
            string methodName = nameof(PassiveModel.GetTakeAttackDmgMultiplier);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetTakeAttackDmgMultiplier(action, attacker, isCritical); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetExpectedTakeAttackDmgMultiplier(BattleActionModel action, BattleUnitModel attacker)
        {
            string methodName = nameof(PassiveModel.GetExpectedTakeAttackDmgMultiplier);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetExpectedTakeAttackDmgMultiplier(action, attacker); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetHpHealMultiplier(BattleUnitModel target)
        {
            string methodName = nameof(PassiveModel.GetHpHealMultiplier);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetHpHealMultiplier(target); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetCriticalChanceAdder(Il2CppSystem.Collections.Generic.Dictionary<BUFF_UNIQUE_KEYWORD, float> affectKeywords)
        {
            string methodName = nameof(PassiveModel.GetCriticalChanceAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetCriticalChanceAdder(affectKeywords); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }

        public virtual float GetCriticalDamageRatioResultMultiplier(BattleActionModel action)
        {
            string methodName = nameof(PassiveModel.GetCriticalDamageRatioResultMultiplier);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            float result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetCriticalDamageRatioResultMultiplier(action); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }

            return result;
        }
    }
}
