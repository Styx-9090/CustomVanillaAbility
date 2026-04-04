using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CustomVanillaAbility.CustomClasses
{
    public abstract class CustomSkillAbilityBase : CustomActionAbilityBase
    {
        public SkillModel _skillModel;
        public SkillAbility _reserveAbility = new();
        public string _extractedData;


        protected override DAMAGE_SOURCE_TYPE _damageSourceType
        {
            get { return DAMAGE_SOURCE_TYPE.SKILL; }
        }

        protected override ABILITY_SOURCE_TYPE _abilitySourceType
        {
            get { return ABILITY_SOURCE_TYPE.SKILL; }
        }

        protected int JsonIntegerValue
        {
            get { return Util.Round(this._jsonValue); }
        }

        public bool IsRandomPickableSkill
        {
            get { return this._isRandomPickableSkill; }
        }

        protected virtual bool _isRandomPickableSkill
        {
            get { return false; }
        }

        public CustomSkillAbilityBase()
        {
            this._bloodDinnerIndex = -1;
        }


        /// <summary>
        /// Reminder to always use "base.Init(skill, scriptName, jsonValue, idx, turnLimit, info);" as the first line when overriding
        /// </summary>
        /// /// <param name="info">This is actually never null, but I left it like in the original code just in-case</param>
        public virtual void Init(SkillModel skill, string scriptName, float jsonValue, int idx, int turnLimit, BuffReferenceData info = null)
        {
            this._skillModel = skill;
            this._index = idx;
            this._jsonValue = jsonValue;
            this._reserveAbility.Init(skill, scriptName, jsonValue, idx, turnLimit, info);
            
            int extractedDataInt = scriptName.IndexOf('_');
            this._extractedData = (extractedDataInt > 0) ? scriptName[(extractedDataInt + 1)..] : string.Empty;

            this._bannedMethodTriggerNames.Add("Init");
            this._bannedMethodTriggerNames.Add("AttachConditionalData");
            this._bannedMethodTriggerNames.Add("InitLimitedActivateCountData");
            this._bannedMethodTriggerNames.Add("ReturnUniqueData");
            this.SetTrigger("skill");
        }

        public override object ReturnUniqueData()
        {
            return this._skillModel;
        }

        public virtual void AttachConditionalData(ConditionalData conditionalData)
        {
            this._conditionalData = conditionalData;
        }

        public virtual void InitLimitedActivateCountData(int turnLimit)
        {
            this._limitedActivateCount = turnLimit;
        }

        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//

        public virtual bool IsShow()
        {
            return true;
        }

        public virtual bool CanBeChangedTargetIgnoreSpeed()
        {
            return false;
        }

        public virtual bool IgnoreDefenseSkill(BattleActionModel action, BattleUnitModel target)
        {
            return false;
        }

        public virtual bool IsActionable(BattleActionModel action)
        {
            return true;
        }

        public virtual bool IsPanicBlock()
        {
            return false;
        }

        public virtual bool IsSkillAbsorbingThisDamage()
        {
            return false;
        }

        public virtual bool CanUseSkill(BattleUnitModel actor)
        {
            return true;
        }

        public virtual bool CanDealTarget(BattleActionModel action, BattleUnitModel target, CoinModel coin)
        {
            return true;
        }

        public virtual bool CanTeamKillOnStableOverclock(BattleActionModel action)
        {
            return false;
        }

        public virtual bool IsRetreatSkill(BattleActionModel action)
        {
            return false;
        }

        public virtual bool AttackByMpDmgRatherThanHpDmg(BattleActionModel action, CoinModel coin, int resultDmg, BattleUnitModel target)
        {
            return false;
        }

        /*
        public virtual bool BlockLoseBuffByReactWithAction(BattleActionModel action, BUFF_UNIQUE_KEYWORD keyword, bool? isCritical)
        {
            return false;
        }

        public virtual bool BlockGivingBuff(BattleActionModel action, BattleUnitModel buffTarget, BUFF_UNIQUE_KEYWORD keyword, CoinModel coinOrNull, bool? isCritical)
        {
            return false;
        }

        public virtual bool ExpectedBlockGivingBuff(BattleActionModel action, BattleUnitModel buffTarget, BUFF_UNIQUE_KEYWORD keyword, CoinModel coinOrNull, bool? isCritical)
        {
            return false;
        }

        public virtual bool OverwriteCriticalResult(BattleActionModel action, CoinModel coin, bool tempCritical, out bool? overwirteCriticalResult)
        {
            overwirteCriticalResult = null;
            return false;
        }
        */

        public virtual bool BeforeCompleteCommand(BattleActionModel action, BATTLE_EVENT_TIMING timing, out int newSkillID)
        {
            newSkillID = 0;
            return false;
        }

        public virtual bool OverwriteCanDuel(bool value)
        {
            return false;
        }

        public virtual bool CanBeChangedTarget(BattleActionModel ownerAction, out bool canChangedTarget)
        {
            canChangedTarget = false;
            return false;
        }

        public virtual bool CanChangeMainTargetRegardlessSpeed(BattleActionModel otherAction)
        {
            return false;
        }

        public virtual bool CanBeChangedTargetIgnoreSpeed(BattleActionModel action, BattleActionModel otherAction)
        {
            return false;
        }

        public virtual bool IsDefenseSkillForOther(BattleUnitModel self, BattleUnitModel originTarget, BattleActionModel opponentActionOrNull)
        {
            return false;
        }

        public virtual bool CanCheckErode(BattleActionModel action)
        {
            return false;
        }

        public virtual bool IsReusable(BattleActionModel action)
        {
            return false;
        }

        public virtual bool IsChangeable(BattleActionModel action)
        {
            return false;
        }

        public virtual bool BlockAddSinStock(BattleActionModel action)
        {
            return false;
        }

        public virtual bool IsBlockingTargetBurstBuffEffectReact(BattleUnitModel target, int stack, int turn, BattleActionModel selfAction, CoinModel selfCoin, bool isCritical, BATTLE_EVENT_TIMING timing)
        {
            return false;
        }

        public virtual bool IsBlockingTargetSinkingBuffEffectReact(BattleUnitModel target, int stack, int turn, BattleActionModel selfAction, CoinModel selfCoin, bool isCritical, BATTLE_EVENT_TIMING timing)
        {
            return false;
        }

        public virtual bool CanRollCoin(BattleActionModel action, CoinModel coin, out bool forceToEndSkill)
        {
            forceToEndSkill = false;
            return true;
        }

        public virtual bool OverwriteParryingResult(BattleActionModel actorAction, int tempActorResult, BattleActionModel oppoAction, int tempOppoResult, out int? overwriteResult)
        {
            overwriteResult = null;
            return false;
        }

        public virtual bool TryGetOverwriteAtkBehaviour(CoinModel coin, out ATK_BEHAVIOUR atkBehaviour)
        {
            atkBehaviour = ATK_BEHAVIOUR.NONE;
            return false;
        }

        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//

        public virtual string OverwriteSkillIconID()
        {
            return null;
        }

        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//

        public virtual int StackNextTurnAggroAdder()
        {
            return 0;
        }

        public virtual int GetSkillLevelAdder(BattleActionModel action)
        {
            return 0;
        }

        public virtual int GetSkillPowerAdder(BattleActionModel action, COIN_ROLL_TYPE rollType)
        {
            return 0;
        }

        public virtual int GetExpectedSkillPowerAdder(BattleActionModel action, COIN_ROLL_TYPE rollType, SinActionModel expectedTargetSinActionOrNull)
        {
            return 0;
        }

        public virtual int GetEvadeSkillPowerAdder(BattleActionModel evadeAction, BattleActionModel oppoAction)
        {
            return 0;
        }

        public virtual int GetExpectedEvadeSkillPowerAdder(BattleActionModel evadeAction, BattleActionModel oppoAction)
        {
            return 0;
        }

        public virtual int GetCoinScaleAdder(BattleActionModel action, CoinModel coin, BattleActionModel oppoActionOrNull)
        {
            return 0;
        }

        public virtual int GetExpectedCoinScaleAdder(BattleActionModel action, CoinModel coin, COIN_ROLL_TYPE rollType, SinActionModel targetSinActionOrNull)
        {
            return 0;
        }

        public virtual int GetSkillPowerResultAdder(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            return 0;
        }

        public virtual int GetEvadeCoinScaleAdder(BattleActionModel evadeAction, BattleActionModel oppoAction)
        {
            return 0;
        }

        public virtual int GetExpectedEvadeCoinScaleAdder(BattleActionModel evadeAction, BattleActionModel oppoAction)
        {
            return 0;
        }

        public virtual int GetExpectedSkillPowerResultAdder(BattleActionModel action, BattleUnitModel expectedTargetOrNull, SinActionModel expectedTargetSinActionOrNull, BattleActionModel expectedOppoActionOrNull)
        {
            return 0;
        }

        public virtual int GetParryingResultAdder(BattleActionModel actorAction, int actorResult, BattleActionModel oppoAction, int oppoResult)
        {
            return 0;
        }

        public virtual int GetExpectedParryingResultAdder(BattleActionModel actorAction, int actorResult, BattleActionModel oppoActionOrNull, int oppoResult)
        {
            return 0;
        }

        public virtual int GetOpponentParryingResultAdder(BattleActionModel actorAction, int actorResult, BattleActionModel oppoAction, int oppoResult)
        {
            return 0;
        }

        public virtual int GetExpectedOpponentParryingResultAdder(BattleActionModel actorAction, int actorResult, BattleActionModel oppoAction, int oppoResult)
        {
            return 0;
        }

        public virtual int GetAttackDmgAdder(BattleActionModel action, BattleUnitModel target)
        {
            return 0;
        }

        public virtual int GetExpectedAttackDmgAdder(BattleActionModel action, BattleUnitModel targetOrNull)
        {
            return 0;
        }

        public virtual int GetAttackHpDmgAdder(BattleUnitModel target)
        {
            return 0;
        }

        public virtual int GetExpectedAttackHpDmgAdder(BattleUnitModel target)
        {
            return 0;
        }

        public virtual int GetTargetNumAdder(BattleActionModel action)
        {
            return 0;
        }

        public virtual int? OverwriteTargetNum(BattleActionModel action)
        {
            return null;
        }

        public virtual int GetBuffStackAdder(BattleActionModel action, CoinModel coinOrNull, BattleUnitModel target, BUFF_UNIQUE_KEYWORD keyword, int stack)
        {
            return 0;
        }

        public virtual int GetBuffTurnAdder(BattleActionModel action, BattleUnitModel target, BUFF_UNIQUE_KEYWORD keyword, int turn)
        {
            return 0;
        }

        public virtual int GetUseBuffTurnAdder(BattleActionModel action, int turn, BUFF_UNIQUE_KEYWORD keyword)
        {
            return 0;
        }

        public virtual int GetAdditionalActivateCountForDefenseSkill(BattleUnitModel owner)
        {
            return 0;
        }

        public virtual int GetGivingStackBuff(UnitModel unit, SkillModel skill, BUFF_UNIQUE_KEYWORD buff, GIVING_BUFF_SKILL_TARGET_TYPE targetType)
        {
            return 0;
        }

        public virtual int GetGivingTurnBuff(UnitModel unit, SkillModel skill, BUFF_UNIQUE_KEYWORD buff, GIVING_BUFF_SKILL_TARGET_TYPE targetType)
        {
            return 0;
        }

        public virtual int ChangeAttackDamage(BattleActionModel action, BattleUnitModel target, CoinModel coin, int resultDmg, bool isCritical, BATTLE_EVENT_TIMING timing)
        {
            return resultDmg;
        }

        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//

        public virtual float GetAttackDmgMultiplier(BattleActionModel action, CoinModel coin, BattleUnitModel target, bool isCritical)
        {
            return 1f;
        }

        public virtual float GetExpectedAttackDmgMultiplier(BattleActionModel action, BattleUnitModel target, CoinModel coin)
        {
            return 1f;
        }

        public virtual float GetCriticalChanceAdder(BattleActionModel action, CoinModel coin)
        {
            return 0f;
        }

        public virtual float GetCriticalChanceMultiplier(BattleActionModel action)
        {
            return 1f;
        }

        public virtual float GetGiveBsGaugeUpMultiplier(bool onGiveExplosion, BattleUnitModel target, BattleActionModel action, CoinModel coinOrNull)
        {
            return 1f;
        }

        public virtual float GetCoinProb(float prob)
        {
            return prob;
        }

        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//

        public virtual global::ATTRIBUTE_TYPE OverwriteAttributeType()
        {
            return global::ATTRIBUTE_TYPE.NONE;
        }

        /*
        public virtual COIN_RESULT GetForcedCoinResult(BattleActionModel action, bool isParrying)
        {
            return COIN_RESULT.NONE;
        }
        */

        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//

        public virtual List<PrimeTargetData> GetPrimeTargets(BattleActionModel action)
        {
            return [];
        }

        public virtual List<AB_PART_TYPE> GetLinkedParts(UnitModel abnormality)
        {
            return [];
        }

        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//

        public virtual ValueTuple<int, int> GetMultifliedDamage(BattleActionModel action, BattleUnitModel target)
        {
            return new ValueTuple<int, int>(0, 0);
        }

        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//

        public virtual void OverwriteTargetableList(BattleActionModel action, List<SinActionModel> targetableSlotListOrNull, List<BattleUnitModel> targetableUnitListOrNull, List<SinActionModel> addedSlotListOrNull)
        {
        }

        public virtual void BeforeBehaviour(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void BeforeAttack(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void BeforeGiveAttackDamage(BattleActionModel action, BattleUnitModel target, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnCompleteCommand(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnBattleStart(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnRoundEnd(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnSkillChanged(BattleActionModel action, bool isOverClock, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnBeforeTurn(BattleActionModel action)
        {
        }

        public virtual void OnBeforeDefense(BattleActionModel action)
        {
        }

        public virtual void OnStartTurn_BeforeLog(BattleActionModel action, Il2CppSystem.Collections.Generic.List<BattleUnitModel> targetList, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnTryEvade(BattleActionModel action, BattleActionModel attackerAction, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void AfterRecheckTargetList(BattleActionModel action, bool valid, bool mainTargetAlive)
        {
        }

        public virtual void OnStartDuel(BattleActionModel ownerAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnBeforeParryingOnce(BattleActionModel ownerAction, BattleActionModel oppoAction)
        {
        }

        public virtual void OnBeforeParryingOnce_AfterLog(BattleActionModel ownerAction, BattleActionModel oppoAction)
        {
        }

        public virtual void OnDuelAfter_BeforeLog()
        {
        }

        public virtual void OnDuelAfter_AfterLog()
        {
        }

        public virtual void OnWinParrying(BattleActionModel ownerAction, BattleActionModel oppoAction)
        {
        }

        public virtual void OnLoseParrying(BattleActionModel ownerAction, BattleActionModel oppoAction)
        {
        }

        public virtual void OnWinDuel(BattleActionModel ownerAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing, int parryingCount, BattleLog_Parrying lastLogOrNull)
        {
        }

        public virtual void OnLoseDuel(BattleActionModel ownerAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnStartCoin(BattleActionModel action, CoinModel coin)
        {
        }

        public virtual void OnEndCoin_BeforeLog(BattleActionModel action, CoinModel coin, bool isCritical, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnEndCoin_AfterLog(BattleActionModel action, CoinModel coin, bool isCritical, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnRerollCoin(BattleActionModel action, CoinModel originalCoin, CoinModel newCoin, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnUseCoinConsume(BattleActionModel action, CoinModel coin, BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnCriticalIsActivated(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing, Dictionary<BUFF_UNIQUE_KEYWORD, float> affectKeywords)
        {
        }

        public virtual void OnAttackConfirmed(BattleActionModel action, BattleUnitModel target, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnSucceedAttack(BattleActionModel action, CoinModel coin, BattleUnitModel target, int finalDmg, int realDmg, bool isCritical, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnEndAttack(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnGiveBsGaugeUp(BattleActionModel action, BattleUnitModel target, int value, BATTLE_EVENT_TIMING timing, bool onExplosion)
        {
        }

        public virtual void OnSucceedEvade(BattleActionModel attackerAction, BattleActionModel evadeAction, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnFailedEvade(BattleActionModel attackerAction, BattleActionModel evadeAction, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnEndBehaviour(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnEndBehave_Refresh(BattleActionModel action)
        {
        }

        public virtual void OnEndTurn(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void DoneWithAction(BattleActionModel action)
        {
        }

        public virtual void OnDiscarded(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnStartBehaviour(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnUseBuffStackBySkill(BattleUnitModel owner, BattleActionModel action, BUFF_UNIQUE_KEYWORD bufKeyword, BATTLE_EVENT_TIMING timing, int stack)
        {
        }

        public virtual void OnUseBuffTurnBySkill(BattleUnitModel owner, BattleActionModel action, BUFF_UNIQUE_KEYWORD bufKeyword, BATTLE_EVENT_TIMING timing, int turn)
        {
        }

        public virtual void RightBeforeGiveBuffBySkill(BattleActionModel action, BattleUnitModel target, BUFF_UNIQUE_KEYWORD bufKeyword, int originalStack, int originalTurn, int activeRound, BATTLE_EVENT_TIMING timing, bool isCritical)
        {
        }

        public virtual void RightAfterGiveBuffBySkill(BattleActionModel action, BattleUnitModel target, BUFF_UNIQUE_KEYWORD bufKeyword, int originalStack, int originalTurn, int resultStack, int resultTurn, int activeRound, BATTLE_EVENT_TIMING timing, bool isCritical)
        {
        }

        public virtual void OnSkillChangedEgo(BattleActionModel action, bool isOverClock, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnChangeSkillBeforeCompleteCommand(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnCancelAction(BattleActionModel action)
        {
        }

        public virtual void OnStartTurn_AfterLog(BattleActionModel action, Il2CppSystem.Collections.Generic.List<BattleUnitModel> targetList, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnAttackCanceledByAbility(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnAfterParryingOnce_BeforeLog(PARRYING_RESULT reuslt, BattleActionModel ownerAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnKillTarget(BattleActionModel action, CoinModel coinOrNull, BattleUnitModel target, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnBreakTarget(BattleActionModel action, CoinModel coinOrNull, BattleUnitModel target, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnDestroyTargetPart(BattleActionModel action, CoinModel coinOrNull, BattleUnitModel_Abnormality_Part target, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing)
        {
        }

        public virtual void OnAddCoinByAbility(BattleActionModel action, CoinModel newCoin, BATTLE_EVENT_TIMING timing)
        {
        }


        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//


        protected void GiveBuff_BySkill(BattleUnitModel target, BattleActionModel action, BUFF_UNIQUE_KEYWORD bufKeyword, int stack, int turn, int activeRound, BATTLE_EVENT_TIMING timing, CoinModel coinOrNull = null, bool isCritical = false)
        {
            BuffInfo buffInfo = target._buffDetail.GetActivatedBuff(bufKeyword, activeRound);
            int originalStack = (buffInfo != null) ? buffInfo.GetStack() : 0;
            int originalTurn = (buffInfo != null) ? buffInfo.GetTurn() : 0;

            this.RightBeforeGiveBuffBySkill(action, target, bufKeyword, stack, turn, activeRound, timing, isCritical);
            this._reserveAbility.GiveBuff_BySkill(target, action, bufKeyword, stack, turn, activeRound, timing, coinOrNull, (Il2CppSystem.Nullable<bool>)isCritical);
            this.RightAfterGiveBuffBySkill(action, target, bufKeyword, originalStack, originalTurn, originalStack + stack, originalTurn + turn, activeRound, timing, isCritical);
        }

        protected void UseBuffStack(BattleUnitModel owner, BattleActionModel action, BUFF_UNIQUE_KEYWORD bufKeyword, BATTLE_EVENT_TIMING timing, int stack, [Optional] int? overwriteStack)
        {
            this._reserveAbility.UseBuffStack(owner, action, bufKeyword, timing, stack, (Il2CppSystem.Nullable<int>)overwriteStack);
            this.OnUseBuffStackBySkill(owner, action, bufKeyword, timing, stack);
        }

        protected void UseBuffAllStack(BattleUnitModel owner, BattleActionModel action, BUFF_UNIQUE_KEYWORD bufKeyword, BATTLE_EVENT_TIMING timing, out int usedStack)
        {
            this._reserveAbility.UseBuffAllStack(owner, action, bufKeyword, timing, out usedStack);
            this.OnUseBuffStackBySkill(owner, action, bufKeyword, timing, usedStack);
        }

        protected void UseBuffTurn(BattleUnitModel owner, BattleActionModel action, BUFF_UNIQUE_KEYWORD bufKeyword, BATTLE_EVENT_TIMING timing, int turn)
        {
            this._reserveAbility.UseBuffTurn(owner, action, bufKeyword, timing, turn);
            this.OnUseBuffTurnBySkill(owner, action, bufKeyword, timing, turn);
        }

        protected void UseBuffAllTurn(BattleUnitModel owner, BattleActionModel action, BUFF_UNIQUE_KEYWORD bufKeyword, BATTLE_EVENT_TIMING timing, out int usedTurn)
        {
            this._reserveAbility.UseBuffAllTurn(owner, action, bufKeyword, timing, out usedTurn);
            this.OnUseBuffTurnBySkill(owner, action, bufKeyword, timing, usedTurn);
        }

        protected void CopyCoinModelAndAddToListByIndex(BattleActionModel action, int idx, BATTLE_EVENT_TIMING timing)
        {
            this._reserveAbility.CopyCoinModelAndAddToListByIndex(action, idx, timing);
        }

        protected void CopyAllCoinModelAndAddToList(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            this._reserveAbility.CopyAllCoinModelAndAddToList(action, timing);
        }

        protected void RemoveCoinModelFromListByIndex(int idx)
        {
            if (idx >= 0 && idx < this._skillModel._coinList.Count)
            {
                this._skillModel._coinList.RemoveAt(idx);
            }
        }

        protected void DisbleCoin(int idx)
        {
            this._skillModel.DisableCoin(idx);
        }

        protected void ReuseAction(BattleActionModel action)
        {
            BattleUnitModel model = action._model;
            model.ReuseAction(action, out BattleActionModel _, false);
        }

        protected void ReuseAction(BattleActionModel action, SKILL_TARGET_TYPE targetType)
        {
            BattleUnitModel model = action._model;
            model.ReuseAction(action, targetType, false);
        }

        protected virtual bool CheckAbilityActivateCount(BattleUnitModel owner)
        {
            if (owner != null)
            {
                string stringKey = this.GetStringKey();
                return owner.CheckAbilityActivateCount(stringKey, this._limitedActivateCount);
            }
            return false;
        }

        private string GetStringKey()
        {
            string text = this._skillModel.GetID().ToStringSmallGC();
            string text2 = this._index.ToStringSmallGC();
            return text + "-" + text2;
        }
    }
}