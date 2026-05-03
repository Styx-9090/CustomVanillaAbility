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
        protected HashSet<string> timingHash;
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
            timingHash = new();

            foreach (CustomPassiveAbilityBase customPassive in passiveList)
            {
                customPassive.Init(this);
                timingHash.UnionWith(customPassive._triggerMethodHash);
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

        public void PassiveRemoveScript()
        {
            if (this.passiveModel._script == null) return;

            this._tempStorage = this.passiveModel._script;
            this.passiveModel._script = null;
        }

        public void PassiveAddScript(PassiveAbility newScript = null)
        {
            if (this.passiveModel._script != null) return;

            this.passiveModel._script = (newScript == null) ? this._tempStorage : newScript;
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

        public bool HasCustomAbility<T>(bool checkInheritance = true) where T : CustomPassiveAbilityBase
        {
            Type targetType = typeof(T);

            foreach (CustomPassiveAbilityBase customAbility in passiveList)
            { 
                if (checkInheritance) if (customAbility is T) return true;
                else if (customAbility.GetType() == targetType) return true;
            }

            return false;
        }

        public void AddCustomAbility<T>(T newAbility, bool avoidDuplicates = true) where T : CustomPassiveAbilityBase
        {
            if (avoidDuplicates && this.HasCustomAbility<T>()) return;

            this.passiveList.Add(newAbility);
            newAbility.Init(this);
            timingHash.UnionWith(newAbility._triggerMethodHash);
        }
    }
}
