using CustomVanillaAbility.Patches;
using CustomVanillaAbility.TestingClass;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

        public int GetExpectedCoinScaleAdder(BattleActionModel action, CoinModel coin, COIN_ROLL_TYPE rollType, SinActionModel expectedTargetSinActionOrNull)
        {
            string methodName = nameof(PassiveModel.GetExpectedCoinScaleAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetExpectedCoinScaleAdder(action, coin, rollType, expectedTargetSinActionOrNull); }
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

        public int GetExpectedAttackHpDmgAdder(BattleActionModel action, BattleUnitModel targetOrNull)
        {
            string methodName = nameof(PassiveModel.GetExpectedAttackHpDmgAdder);
            if (!this.timingDict.ContainsKey(methodName)) return 0;

            int result = 0;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { result += realAbility.GetExpectedAttackHpDmgAdder(action, targetOrNull); }
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


        public float GetTakeBuffStackMultiplier(SkillModel skill, BUFF_UNIQUE_KEYWORD buf)
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

        public float GetTakeBuffTurnMultiplier(SkillModel skill, BUFF_UNIQUE_KEYWORD buf)
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

        public float GetSinBuffDamageMultiplier(BUFF_UNIQUE_KEYWORD buff)
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

        public float GetTakeHpHealMultiplier(BattleUnitModel healerOrNull, ABILITY_SOURCE_TYPE srcType)
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

        public float GetTakeHpHealMultiplierPart(BattleUnitModel_Abnormality_Part part, BattleUnitModel healerOrNull, ABILITY_SOURCE_TYPE srcType)
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

        public float GetGiveBsGaugeUpMultiplier(bool onGiveExplosion, BattleActionModel actionOrNull, CoinModel coinOrNull)
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

        public float GetBsGaugeUpMultiplier(bool onGiveExplosion, BattleActionModel actionOrNull, CoinModel coinOrNull)
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

        public float GetMaxHpMultiplier()
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

        public float GetMaxHpMultiplierPartToAbnormality()
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

        public float GetAtkResistAdder(ATK_BEHAVIOUR type)
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

        public float GetAtkResistMultiplier(ATK_BEHAVIOUR type)
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

        public float GetDefenseMultiplier()
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

        public float GetAttributeResistAdder(global::ATTRIBUTE_TYPE type)
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

        public float GetAttributeResistMultiplier(global::ATTRIBUTE_TYPE type)
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

        public float GetAttackDmgMultiplier(BattleActionModel action, CoinModel coin, BattleUnitModel target, bool isWinDuel, bool isCritical)
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

        public float GetExpectedAttackDmgMultiplier(BattleActionModel action, CoinModel coin, BattleUnitModel targetOrNull, SinActionModel targetSinActionOrNull)
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

        public float GetTakeAttackDmgMultiplier(BattleActionModel action, BattleUnitModel attacker, bool isCritical)
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

        public float GetExpectedTakeAttackDmgMultiplier(BattleActionModel action, BattleUnitModel attacker)
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

        public float GetHpHealMultiplier(BattleUnitModel target)
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

        public float GetCriticalChanceAdder(Il2CppSystem.Collections.Generic.Dictionary<BUFF_UNIQUE_KEYWORD, float> affectKeywords)
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

        public float GetCriticalDamageRatioResultMultiplier(BattleActionModel action)
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

        public void OnAddUnit(BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnAddUnit);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnAddUnit(timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnAddUnitPart(BattleUnitModel part, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnAddUnitPart);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnAddUnitPart(part, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnAddUnitView(BattleUnitView view)
        {
            string methodName = nameof(PassiveModel.OnAddUnitView);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnAddUnitView(view); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnStageStart(BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnStageStart);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnStageStart(timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnWaveStart(BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnWaveStart);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnWaveStart(timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnRoundStart_After_Event(BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnRoundStart_After_Event);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnRoundStart_After_Event(timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnRoundStart_After_Event_DeadOrRetreated(BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnRoundStart_After_Event_DeadOrRetreated);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnRoundStart_After_Event_DeadOrRetreated(timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnReturnToField(int retreatTurn, BattleUnitModel triggerUnit, BUFF_UNIQUE_KEYWORD retreatKeyword, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnReturnToField);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnReturnToField(retreatTurn, triggerUnit, retreatKeyword, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnReturnToFieldOtherUnit_DeadOrRetreated(BattleUnitModel returnUnit, int retreatTurn, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnReturnToFieldOtherUnit_DeadOrRetreated);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnReturnToFieldOtherUnit_DeadOrRetreated(returnUnit, retreatTurn, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnReleaseStandByOtherUnit_DeadOrRetreated(BattleUnitModel addedUnit, Il2CppSystem.Collections.Generic.List<BattleUnitModel> addedUnitList)
        {
            string methodName = nameof(PassiveModel.OnReleaseStandByOtherUnit_DeadOrRetreated);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnReleaseStandByOtherUnit_DeadOrRetreated(addedUnit, addedUnitList); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnReleaseStandBy(BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnReleaseStandBy);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnReleaseStandBy(timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnPanicOrLowMorale(PANIC_LEVEL level, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnPanicOrLowMorale);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnPanicOrLowMorale(level, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnCompleteCommand(BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnCompleteCommand);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnCompleteCommand(timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnBattleStart(BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnBattleStart);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnBattleStart(timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnBattleEnd(BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnBattleEnd);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnBattleEnd(timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnStartTurn_BeforeLog(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnStartTurn_BeforeLog);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnStartTurn_BeforeLog(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnStartTurn_AfterLog(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnStartTurn_AfterLog);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnStartTurn_AfterLog(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnStartDuel(BattleActionModel ownerAction, BattleActionModel opponentAction, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnStartDuel);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnStartDuel(ownerAction, opponentAction, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnResult_OnAction(BattleActionModel action, CoinModel coin)
        {
            string methodName = nameof(PassiveModel.OnResult_OnAction);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnResult_OnAction(action, coin); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnResult_OnParrying(BattleActionModel action, BattleActionModel oppoAction, CoinModel coin)
        {
            string methodName = nameof(PassiveModel.OnResult_OnParrying);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnResult_OnParrying(action, oppoAction, coin); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnWinParrying(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnWinParrying);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnWinParrying(selfAction, oppoAction, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnLoseParrying(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnLoseParrying);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnLoseParrying(selfAction, oppoAction, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnDuelAfter_BeforeLog(BattleActionModel selfAction, BattleActionModel oppoAction, int parryingCount, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnDuelAfter_BeforeLog);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnDuelAfter_BeforeLog(selfAction, oppoAction, parryingCount, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnWinDuel(BattleActionModel selfAction, BattleActionModel oppoAction, int parryingCount, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnWinDuel);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnWinDuel(selfAction, oppoAction, parryingCount, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnLoseDuel(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnLoseDuel);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnLoseDuel(selfAction, oppoAction, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnGiveHpDamage(BattleUnitModel target, int value, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnGiveHpDamage);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnGiveHpDamage(target, value, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnGiveMpDamage(BattleUnitModel target, int value)
        {
            string methodName = nameof(PassiveModel.OnGiveMpDamage);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnGiveMpDamage(target, value); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnTakeMpDmg_SinBuff(int value, BATTLE_EVENT_TIMING timing, BUFF_UNIQUE_KEYWORD keyword)
        {
            string methodName = nameof(PassiveModel.OnTakeMpDmg_SinBuff);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnTakeMpDmg_SinBuff(value, timing, keyword); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void BeforeAttack(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.BeforeAttack);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.BeforeAttack(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnCriticalActivated(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnCriticalActivated);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnCriticalActivated(action, coin, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnStartCoin(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnStartCoin);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnStartCoin(action, coin, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnEndCoin_BeforeLog(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnEndCoin_BeforeLog);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnEndCoin_BeforeLog(action, coin, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnEndCoin_AfterLog(BattleActionModel action, CoinModel coin)
        {
            string methodName = nameof(PassiveModel.OnEndCoin_AfterLog);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnEndCoin_AfterLog(action, coin); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnSucceedEvade(BattleActionModel evadeAction, BattleActionModel attackAction, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnSucceedEvade);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnSucceedEvade(evadeAction, attackAction, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnAttackConfirmed(BattleActionModel action, CoinModel coin, BattleUnitModel target, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnAttackConfirmed);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnAttackConfirmed(action, coin, target, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnKillTarget(BattleActionModel actionOrNull, BattleUnitModel target, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnKillTarget);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnKillTarget(actionOrNull, target, dmgSrcType, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnStartBehaviour(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnStartBehaviour);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnStartBehaviour(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnSucceedAttack(BattleActionModel action, CoinModel coin, BattleUnitModel target, int finalDmg, int realDmg, bool isCritical, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnSucceedAttack);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnSucceedAttack(action, coin, target, finalDmg, realDmg, isCritical, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnRollOneCoin_AfterAttack(BattleActionModel action, CoinModel coin)
        {
            string methodName = nameof(PassiveModel.OnRollOneCoin_AfterAttack);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnRollOneCoin_AfterAttack(action, coin); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnEndAttack(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnEndAttack);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnEndAttack(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnEndAttackPart(BattleUnitModel_Abnormality_Part part, BattleActionModel action)
        {
            string methodName = nameof(PassiveModel.OnEndAttackPart);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnEndAttackPart(part, action); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnEndBehaviour(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnEndBehaviour);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnEndBehaviour(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnEndBehave_Refresh(BattleActionModel action)
        {
            string methodName = nameof(PassiveModel.OnEndBehave_Refresh);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnEndBehave_Refresh(action); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnEndTurn(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnEndTurn);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnEndTurn(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnActivateImmortality(BattleUnitModel immortalActivator, BATTLE_EVENT_TIMING timing, BattleActionModel actionOrNull)
        {
            string methodName = nameof(PassiveModel.OnActivateImmortality);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnActivateImmortality(immortalActivator, timing, actionOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnActivateAbnormalityImmortality(BATTLE_EVENT_TIMING timing, BattleActionModel actionOrNull)
        {
            string methodName = nameof(PassiveModel.OnActivateAbnormalityImmortality);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnActivateAbnormalityImmortality(timing, actionOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnDestroyShield(BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnDestroyShield);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnDestroyShield(timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnRecoverBreak(BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnRecoverBreak);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnRecoverBreak(timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnGiveBsGaugeUp(BattleUnitModel giver, BattleUnitModel target, int value, BATTLE_EVENT_TIMING timing, bool onExplosion, ABILITY_SOURCE_TYPE abilitySrc, BattleActionModel actionOrNull)
        {
            string methodName = nameof(PassiveModel.OnGiveBsGaugeUp);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnGiveBsGaugeUp(giver, target, value, timing, onExplosion, abilitySrc, actionOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void BeforePartTakeAttackDamage(BattleUnitModel_Abnormality abnormality, BattleUnitModel_Abnormality_Part part, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.BeforePartTakeAttackDamage);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.BeforePartTakeAttackDamage(abnormality, part, action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void BeforeGiveAttackDamage(BattleActionModel action, BattleUnitModel target, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.BeforeGiveAttackDamage);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;


                try { realAbility.BeforeGiveAttackDamage(action, target, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void BeforeTakeAttackDamage(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.BeforeTakeAttackDamage);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;


                try { realAbility.BeforeTakeAttackDamage(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnTakeAttackDamage(BattleActionModel action, CoinModel coin, int totalDmg, int hpDmg, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnTakeAttackDamage);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnTakeAttackDamage(action, coin, totalDmg, hpDmg, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnTakeAttackDamagePart(BattleUnitModel_Abnormality_Part part, BattleActionModel attackerAction, CoinModel coin, int value, BATTLE_EVENT_TIMING timing, bool isCritical)
        {
            string methodName = nameof(PassiveModel.OnTakeAttackDamagePart);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnTakeAttackDamagePart(part, attackerAction, coin, value, timing, isCritical); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnEndEnemyAttack(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnEndEnemyAttack);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnEndEnemyAttack(action, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnBeforeDefense(BattleActionModel action)
        {
            string methodName = nameof(PassiveModel.OnBeforeDefense);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnBeforeDefense(action); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnRoundEnd(BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnRoundEnd);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnRoundEnd(timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnRetreat(BattleUnitModel triggerUnit, BUFF_UNIQUE_KEYWORD retreatKeyword, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnRetreat);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnRetreat(triggerUnit, retreatKeyword, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnRoundEnd_After()
        {
            string methodName = nameof(PassiveModel.OnRoundEnd_After);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnRoundEnd_After(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnStageEnd()
        {
            string methodName = nameof(PassiveModel.OnStageEnd);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnStageEnd(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void RightBeforeOtherUnitGiveBuffBySkill(BattleUnitModel giver, BattleUnitModel target, BUFF_UNIQUE_KEYWORD bufKeyword, int stack, int turn, SkillModel skill, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.RightBeforeOtherUnitGiveBuffBySkill);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.RightBeforeOtherUnitGiveBuffBySkill(giver, target, bufKeyword, stack, turn, skill, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void RightAfterOtherUnitGiveBuffBySkill(BattleUnitModel giver, BattleUnitModel target, BUFF_UNIQUE_KEYWORD bufKeyword, int stack, int turn, SkillModel skill, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.RightAfterOtherUnitGiveBuffBySkill);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.RightAfterOtherUnitGiveBuffBySkill(giver, target, bufKeyword, stack, turn, skill, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void RightAfterLosingBuff(int loseStack, int loseTurn, BATTLE_EVENT_TIMING timing, BuffInfo info)
        {
            string methodName = nameof(PassiveModel.RightAfterLosingBuff);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.RightAfterLosingBuff(loseStack, loseTurn, timing, info); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnSucceedToGiveSwitchToSpecialVibration(BattleUnitModel target, BUFF_UNIQUE_KEYWORD keyword, int prevStack, int prevTurn, int afterStack, int afterTurn, BATTLE_EVENT_TIMING timing, ABILITY_SOURCE_TYPE abilitySourceType)
        {
            string methodName = nameof(PassiveModel.OnSucceedToGiveSwitchToSpecialVibration);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnSucceedToGiveSwitchToSpecialVibration(target, keyword, prevStack, prevTurn, afterStack, afterTurn, timing, abilitySourceType); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void RightAfterGetAnyBuff(BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, int activeRound, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing, BattleUnitModel giverOrNull, BattleActionModel actionOrNull, int overStack, int overTurn)
        {
            string methodName = nameof(PassiveModel.RightAfterGetAnyBuff);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.RightAfterGetAnyBuff(keyword, stack, turn, activeRound, srcType, timing, giverOrNull, actionOrNull, overStack, overTurn); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void RightAfterGetAnyBuffAtPart(BattleUnitModel_Abnormality_Part part, BUFF_UNIQUE_KEYWORD keyword, int stack, int activeRound, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing, BattleUnitModel giverOrNull, BattleActionModel actionOrNull)
        {
            string methodName = nameof(PassiveModel.RightAfterGetAnyBuffAtPart);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.RightAfterGetAnyBuffAtPart(part, keyword, stack, activeRound, srcType, timing, giverOrNull, actionOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnDestroy(BattleUnitModel destroyerOrNull, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnDestroy);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnDestroy(destroyerOrNull, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnOtherPartDestroyed(BattleUnitModel_Abnormality_Part destroyedPart)
        {
            string methodName = nameof(PassiveModel.OnOtherPartDestroyed);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnOtherPartDestroyed(destroyedPart); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnPartDestroyed(BattleUnitModel_Abnormality_Part destroyedPart, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnPartDestroyed);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnPartDestroyed(destroyedPart, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnPartBreaked(BattleUnitModel_Abnormality_Part breakedPart, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnPartBreaked);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnPartBreaked(breakedPart, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnPartRecoverBreak(BattleUnitModel_Abnormality_Part recoveredPart, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnPartRecoverBreak);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnPartRecoverBreak(recoveredPart, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnRegenerate(BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnRegenerate);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnRegenerate(timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnPartRegenerate(BattleUnitModel_Abnormality_Part part, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnPartRegenerate);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnPartRegenerate(part, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnZeroHp()
        {
            string methodName = nameof(PassiveModel.OnZeroHp);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnZeroHp(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnDie(BattleUnitModel killer, BattleActionModel actionOrNull, DAMAGE_SOURCE_TYPE dmgSrcType, BUFF_UNIQUE_KEYWORD keyword, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnDie);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnDie(killer, actionOrNull, dmgSrcType, keyword, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnBreak(BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BATTLE_EVENT_TIMING timing, bool isBreakForcely)
        {
            string methodName = nameof(PassiveModel.OnBreak);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnBreak(attackerOrNull, actionOrNull, timing, isBreakForcely); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnEnemyBrokenByAttacker(BattleActionModel actionOrNull, BattleUnitModel target, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnEnemyBrokenByAttacker);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnEnemyBrokenByAttacker(actionOrNull, target, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnDieOtherUnit(BattleUnitModel killer, BattleUnitModel dead, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE dmgSrcType, BUFF_UNIQUE_KEYWORD keyword)
        {
            string methodName = nameof(PassiveModel.OnDieOtherUnit);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnDieOtherUnit(killer, dead, timing, dmgSrcType, keyword); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnBreakOtherUnit(BattleUnitModel breakedUnit, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnBreakOtherUnit);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnBreakOtherUnit(breakedUnit, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnDiscardSinOtherUnit(BattleUnitModel discardUnit, UnitSinModel sin, BATTLE_EVENT_TIMING timing, BattleActionModel actionOrNull)
        {
            string methodName = nameof(PassiveModel.OnDiscardSinOtherUnit);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnDiscardSinOtherUnit(discardUnit, sin, timing, actionOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnDiscardSin(UnitSinModel sin, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnDiscardSin);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnDiscardSin(sin, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnVibrationExplosionOtherUnit(BattleUnitModel explodedUnit, BattleUnitModel giverOrNull, BattleActionModel actionOrNull, ABILITY_SOURCE_TYPE abilitySrc, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnVibrationExplosionOtherUnit);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnVibrationExplosionOtherUnit(explodedUnit, giverOrNull, actionOrNull, abilitySrc, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnTakeAttackDamageOtherUnit(BattleActionModel action, int realDmg, int hpDmg, BattleUnitModel attackedUnit, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnTakeAttackDamageOtherUnit);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnTakeAttackDamageOtherUnit(action, realDmg, hpDmg, attackedUnit, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnUseBloodDinnerUnit(BattleUnitModel usedUnit, int stack, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnUseBloodDinnerUnit);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnUseBloodDinnerUnit(usedUnit, stack, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnGiveImmortalState(BattleUnitModel immortalTaker, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnGiveImmortalState);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnGiveImmortalState(immortalTaker, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnAfterTryTakeHpHeal(BattleUnitModel healerOrNull, int tryHeal, int resultHeal, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnAfterTryTakeHpHeal);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnAfterTryTakeHpHeal(healerOrNull, tryHeal, resultHeal, srcType, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnBeforeTryTakeMpHeal(BattleUnitModel healerOrNull, int tryHeal, int resultHeal, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnBeforeTryTakeMpHeal);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnBeforeTryTakeMpHeal(healerOrNull, tryHeal, resultHeal, srcType, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnAfterTryTakeMpHeal(BattleUnitModel healerOrNull, int tryHeal, int resultHeal, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnAfterTryTakeMpHeal);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnAfterTryTakeMpHeal(healerOrNull, tryHeal, resultHeal, srcType, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void CheckLoseBuffStackAndTurn(BuffInfo info, int loseStack, int loseTurn, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.CheckLoseBuffStackAndTurn);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.CheckLoseBuffStackAndTurn(info, loseStack, loseTurn, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnStartPhase(PHASE phase, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnStartPhase);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnStartPhase(phase, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnTakeHpDamage(int finalDamage, int hpDamage, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BUFF_UNIQUE_KEYWORD keyword)
        {
            string methodName = nameof(PassiveModel.OnTakeHpDamage);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnTakeHpDamage(finalDamage, hpDamage, timing, sourceType, attackerOrNull, actionOrNull, keyword); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnTakeHpDamagePart(BattleUnitModel_Abnormality_Part part, int finalDamage, int hpDamage, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BUFF_UNIQUE_KEYWORD keyword)
        {
            string methodName = nameof(PassiveModel.OnTakeHpDamagePart);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnTakeHpDamagePart(part, finalDamage, hpDamage, timing, sourceType, attackerOrNull, actionOrNull, keyword); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnTakeHpDamageOtherUnit(BattleUnitModel damaged, int finalDamage, int hpDamage, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, Il2CppSystem.Collections.Generic.List<BattleUnitModel> relatedUnitsOrNull, BUFF_UNIQUE_KEYWORD keyword)
        {
            string methodName = nameof(PassiveModel.OnTakeHpDamageOtherUnit);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnTakeHpDamageOtherUnit(damaged, finalDamage, hpDamage, timing, sourceType, attackerOrNull, actionOrNull, relatedUnitsOrNull, keyword); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnTakeAttackConfirmed(BattleActionModel action, CoinModel coin, BattleUnitModel attacker, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnTakeAttackConfirmed);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnTakeAttackConfirmed(action, coin, attacker, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnChangeHp(int oldHp, int newHp, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BUFF_UNIQUE_KEYWORD keyword)
        {
            string methodName = nameof(PassiveModel.OnChangeHp);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnChangeHp(oldHp, newHp, dmgSrcType, timing, attackerOrNull, actionOrNull, keyword); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnChangeMp(int oldMp, int newMp)
        {
            string methodName = nameof(PassiveModel.OnChangeMp);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnChangeMp(oldMp, newMp); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnChangeMpOther(BattleUnitModel mpChangeUnit, int oldMp, int newMp)
        {
            string methodName = nameof(PassiveModel.OnChangeMpOther);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnChangeMpOther(mpChangeUnit, oldMp, newMp); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnTakeMpDamage(BattleUnitModel attacker, int value, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleActionModel actionOrNull)
        {
            string methodName = nameof(PassiveModel.OnTakeMpDamage);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnTakeMpDamage(attacker, value, timing, sourceType, actionOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnTakeMpDamageOther(BattleUnitModel mpDmgUnit, BattleUnitModel attackerOrNull, int value, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleActionModel actionOrNull)
        {
            string methodName = nameof(PassiveModel.OnTakeMpDamageOther);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnTakeMpDamageOther(mpDmgUnit, attackerOrNull, value, timing, sourceType, actionOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnAfterTryTakeMpHealOther(BattleUnitModel mpHealUnit, BattleUnitModel healerOrNull, int tryHeal, int resultHeal, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnAfterTryTakeMpHealOther);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnAfterTryTakeMpHealOther(mpHealUnit, healerOrNull, tryHeal, resultHeal, srcType, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnUseBuffTurnBySkill(SkillModel skill, BUFF_UNIQUE_KEYWORD bufKeyword, int turn, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnUseBuffTurnBySkill);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnUseBuffTurnBySkill(skill, bufKeyword, turn, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnUseBuff(BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnUseBuff);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnUseBuff(keyword, stack, turn, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void BeforeUseBuff(BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.BeforeUseBuff);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.BeforeUseBuff(keyword, stack, turn, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnPickSkills()
        {
            string methodName = nameof(PassiveModel.OnPickSkills);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnPickSkills(); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnAddActionToPart(BattleUnitModel_Abnormality_Part part, BattleActionModel action)
        {
            string methodName = nameof(PassiveModel.OnAddActionToPart);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnAddActionToPart(part, action); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void RightAfterDestroyAnyBuff(BuffInfo destroyedBuffInfo, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.RightAfterDestroyAnyBuff);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.RightAfterDestroyAnyBuff(destroyedBuffInfo, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnCanceledByLackOfBuffsAtStartCoin(BattleActionModel action, CoinModel coin, Il2CppSystem.Collections.Generic.List<BUFF_UNIQUE_KEYWORD> lackOfBuffs, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnCanceledByLackOfBuffsAtStartCoin);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnCanceledByLackOfBuffsAtStartCoin(action, coin, lackOfBuffs, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnFailedToGetBuff(BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, int activeRound, ABILITY_SOURCE_TYPE abilitySrcType, BATTLE_EVENT_TIMING timing, BattleUnitModel giverOrNull)
        {
            string methodName = nameof(PassiveModel.OnFailedToGetBuff);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnFailedToGetBuff(keyword, stack, turn, activeRound, abilitySrcType, timing, giverOrNull); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }

        public void OnUseCoinConsume(BattleUnitModel owner, BattleActionModel action, CoinModel coin, BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, BATTLE_EVENT_TIMING timing)
        {
            string methodName = nameof(PassiveModel.OnUseCoinConsume);
            if (!this.timingDict.ContainsKey(methodName)) return;

            foreach (CustomAbilityBase ability in this.passiveList)
            {
                if (ability is not CustomPassiveAbilityBase realAbility) continue;
                if (!realAbility._triggerMethodHash.Contains(methodName)) continue;

                try { realAbility.OnUseCoinConsume(owner, action, coin, keyword, stack, turn, timing); }
                catch (System.Exception ex) { CustomVanillaAbilityMain.Instance.Log.LogInfo("Error at method with name = " + methodName + " || returning error = " + ex); }
            }
        }
    }
}
