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
        protected PASSIVE_STATUS _status = PASSIVE_STATUS.DEACTIVE;


        public bool _isActivated;
        public bool _isActivatedThisTurn;
        public bool _isActivatedOnThisBattle;
        public List<CustomPassiveAbilityBase> passiveList;


        public CustomPassiveAbilityHolder(List<CustomPassiveAbilityBase> customPassiveList)
        {
            this.passiveList = customPassiveList;
        }


        public void Init(PassiveModel passive)
        {
            this.passiveModel = passive;
            this.owner = passive.Owner;
            attributeResonanceCondition = passive.ClassInfo.GetAttributeResonanceConditionList();
            attributeStockCondition = passive.ClassInfo.GetAttributeStockConditionList();

            if ((attributeResonanceCondition == null || attributeResonanceCondition.Count <= 0) && (attributeStockCondition == null || attributeStockCondition.Count <= 0) == false) OnPassiveActivated();
            else CheckActiveCondition();

            foreach (CustomPassiveAbilityBase customPassive in passiveList) customPassive.Init(this);
        }

        public void OnPassiveActivated()
        {
            if (_isActivated) return;

            _status = PASSIVE_STATUS.ACTIVE;
            _isActivated = true;

            if (!_isActivatedThisTurn)
            {
                foreach (CustomPassiveAbilityBase customPassive in this.passiveList) customPassive.OnUpdateStatus(true);
                _isActivatedThisTurn = true;
            }

            _isActivatedOnThisBattle = true;

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
                foreach (CustomPassiveAbilityBase customPassive in this.passiveList) customPassive.OnUpdateStatus(false);
                _isActivatedThisTurn = false;
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

            foreach (var data in attributeResonanceCondition)
            {
                int value = resManager.GetAttributeResonance(owner.Faction, data.AttributeType);
                if (value < data.Value) return false;
            }

            return true;
        }

        private bool CheckStockConditions(SinManager.EgoStockManager stockManager)
        {
            if (attributeStockCondition == null || attributeStockCondition.Count == 0) return true;

            foreach (var data in attributeStockCondition)
            {
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
    }
}
