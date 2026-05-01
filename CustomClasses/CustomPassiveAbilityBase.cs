using Il2CppSystem.Collections.Generic;

namespace CustomVanillaAbility.CustomClasses
{
    public abstract class CustomPassiveAbilityBase : CustomActionAbilityBase
    {
        protected BattleUnitModel _owner;
        protected UNIT_FACTION _faction = UNIT_FACTION.NONE;
        protected PASSIVE_STATUS _status;
        protected PassiveModel _passiveModel;
        protected CustomPassiveAbilityHolder _passiveHolder;

        protected override ABILITY_SOURCE_TYPE _abilitySourceType
        {
            get { return ABILITY_SOURCE_TYPE.PASSIVE; }
        }

        protected override DAMAGE_SOURCE_TYPE _damageSourceType
        {
            get { return DAMAGE_SOURCE_TYPE.PASSIVE; }
        }

        public bool ShowOnLeftPassiveUI
        {
            get { return _showOnLeftPassiveUI; }
        }

        protected virtual bool _showOnLeftPassiveUI
        {
            get { return true; }
        }

        public bool IsHide
        {
            get { return _isHide; }
        }

        protected virtual bool _isHide
        {
            get { return false; }
        }

        public void Init(CustomPassiveAbilityHolder passiveAbilityHolder)
        {
            _passiveHolder = passiveAbilityHolder;
            _passiveModel = passiveAbilityHolder.passiveModel;
            _owner = _passiveModel.Owner;
            _faction = _owner.Faction;
            Init();
        }

        public UNIT_FACTION GetOwnerFaction()
        {
            return this._faction;
        }


        public UNIT_FACTION GetOpponentFaction()
        {
            return (this._faction == UNIT_FACTION.PLAYER) ? UNIT_FACTION.ENEMY : UNIT_FACTION.PLAYER;
        }


        public BattleUnitModel GetOwner()
        {
            return this._owner;
        }

        public virtual void OnUpdateStatus(bool deactivated)
        {

        }

        public virtual void Init()
        {

        }

        public virtual void Init_After()
        {
            
        }


        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//


        public virtual bool IsTargetable(BattleUnitModel attacker)
        {
            return true;
        }

        public virtual bool IsTargetableParts(BattleUnitModel attacker)
        {
            return true;
        }

        public virtual bool CanBeChangedTarget(BattleActionModel action)
        {
            return true;
        }

        public virtual bool CanGiveConcentratedAttack(BattleActionModel action)
        {
            return true;
        }

        public virtual bool IsRegeneratable()
        {
            return true;
        }

        public virtual bool SpreadHpDmgToAbnormality(int value, BattleUnitModel attackerOrNull, BattleActionModel attackerActionOrNull, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing, BUFF_UNIQUE_KEYWORD keyword)
        {
            return true;
        }

        public virtual bool SpreadHpDmgFromPart(AB_PART_TYPE partType, int value, BattleUnitModel attackOrNull, BattleActionModel attackerActionOrNull, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing, BUFF_UNIQUE_KEYWORD keyword)
        {
            return true;
        }

        public virtual bool CheckImmortal(BATTLE_EVENT_TIMING timing, int newHp, bool isInstantDeath, BUFF_UNIQUE_KEYWORD buff, BattleActionModel actionOrNull)
        {
            return true;
        }

        public virtual bool IsAbnormalityImmortal(BATTLE_EVENT_TIMING timing, int newHp, bool isInstantDeath, BUFF_UNIQUE_KEYWORD buff, BattleActionModel actionOrNull)
        {
            return false;
        }

        public virtual bool CheckImmortalOtherUnit(BattleUnitModel checkTarget, int newHp, bool isInstantDeath, BUFF_UNIQUE_KEYWORD buff)
        {
            return true;
        }

        public virtual bool CanTeamKill(BattleActionModel action)
        {
            return false;
        }

        public virtual bool IsActionable()
        {
            return true;
        }

        public virtual bool ChangeResistOnBreak()
        {
            return true;
        }

        public virtual bool ChangeResistOnBreak_Part(BattleUnitModel_Abnormality_Part part)
        {
            return true;
        }

        public virtual bool CanPickSkill()
        {
            return true;
        }

        public virtual bool IgnoreCheckBreak(DAMAGE_SOURCE_TYPE dmgSrcType, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BUFF_UNIQUE_KEYWORD keyword)
        {
            return false;
        }

        public virtual bool IgnoreBreak()
        {
            return false;
        }

        public virtual bool IgnoreBreakExceptForcedCase()
        {
            return false;
        }

        public virtual bool IgnorePanic()
        {
            return false;
        }

        public virtual bool HasFakeDead()
        {
            return false;
        }

        public virtual bool CanCreateEmptySlot()
        {
            return true;
        }

        public virtual bool CanCreateEmptySlotPart(BattleUnitModel_Abnormality_Part part)
        {
            return true;
        }

        public virtual bool CanTakeMpHeal(BattleUnitModel healerOrNull, int mp, ABILITY_SOURCE_TYPE srcType)
        {
            return true;
        }

        public virtual bool IsChangeTakeDamage(BattleActionModel action, CoinModel coinOrNull, int resultDmg, DAMAGE_SOURCE_TYPE dmgSrcType, BUFF_UNIQUE_KEYWORD keyword)
        {
            return false;
        }

        public virtual bool CheckIfTurnToCorpesOnDie()
        {
            return true;
        }

        public virtual bool IsAllowedToGiveBuff(BUFF_UNIQUE_KEYWORD keyword)
        {
            return true;
        }


        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//

        public virtual int GetTakeBuffTurnAdderOtherUnit(BattleUnitModel taker, BattleActionModel action, BUFF_UNIQUE_KEYWORD keyword, int originalTurn, BATTLE_EVENT_TIMING timing)
        {
            return 0;
        }

        public virtual int GetGiveBuffStackAdder(BattleActionModel action, SkillModel skill, BattleUnitModel target, BUFF_UNIQUE_KEYWORD buf, int turn, BATTLE_EVENT_TIMING timing)
        {
            return 0;
        }

        public virtual int GetGiveBuffTurnAdder(BattleActionModel action, SkillModel skill, CoinModel coinOrNull, BattleUnitModel target, BUFF_UNIQUE_KEYWORD buf, int turn, BATTLE_EVENT_TIMING timing)
        {
            return 0;
        }

        public virtual int GetTakeBuffStackAdder(BattleActionModel action, SkillModel skill, BUFF_UNIQUE_KEYWORD buf, int originStack, BATTLE_EVENT_TIMING timing)
        {
            return 0;
        }

        public virtual int GetTakeBuffTurnAdder(BattleActionModel action, SkillModel skill, BUFF_UNIQUE_KEYWORD buf, int originalTurn, BATTLE_EVENT_TIMING timing)
        {
            return 0;
        }

        public virtual int GetAggroAdder(SinActionModel sinaction)
        {
            return 0;
        }

        public virtual int GetAttackWeightAdder(BattleActionModel action)
        {
            return 0;
        }

        public virtual int GetMentalSystemResultIncreaseAdder()
        {
            return 0;
        }

        public virtual int GetMentalSystemResultDecreaseAdder()
        {
            return 0;
        }

        /*
        public virtual int GetGiveBuffStackAdder(BattleActionModel action, SkillModel skill, CoinModel coinOrNull, BattleUnitModel target, BUFF_UNIQUE_KEYWORD buf, int currentStack, BATTLE_EVENT_TIMING timing, bool isCritical)
        {
            return 0;
        }
        */

        public virtual int GetUseBuffTurnAdder(BattleActionModel action, SkillModel skill, int turn, BUFF_UNIQUE_KEYWORD buf)
        {
            return 0;
        }

        public virtual int GetSinBuffDamageAdder(BUFF_UNIQUE_KEYWORD buff, int dmg)
        {
            return 0;
        }

        public virtual int GetMaxBuffStackAdder(BUFF_UNIQUE_KEYWORD keyword)
        {
            return 0;
        }

        public virtual int GetMaxBuffTurnAdder(BUFF_UNIQUE_KEYWORD keyword)
        {
            return 0;
        }

        public virtual int GetTakeHpHealAdder(BattleUnitModel healerOrNull, ABILITY_SOURCE_TYPE srcType)
        {
            return 0;
        }

        public virtual int GetActionSlotAdder()
        {
            return 0;
        }

        public virtual int GetMaxHpAdder()
        {
            return 0;
        }

        public virtual int GetMaxHpAdderPart(BattleUnitModel_Abnormality_Part part)
        {
            return 0;
        }

        public virtual int GetSpeedAdder()
        {
            return 0;
        }

        public virtual int GetMaxSpeedAdder()
        {
            return 0;
        }

        public virtual int GetMinSpeedAdder()
        {
            return 0;
        }

        public virtual int GetCoinScaleAdder(BattleActionModel action, BattleActionModel oppoActionOrNull, CoinModel coin)
        {
            return 0;
        }

        public virtual int GetExpectedCoinScaleAdder(BattleActionModel action, CoinModel coin, SinActionModel expectedTargetSinActionOrNull)
        {
            return 0;
        }

        public virtual int GetParryingResultAdder(BattleActionModel action, int actorResult, BattleActionModel oppoAction, int oppoResult, int parryingCount)
        {
            return 0;
        }

        public virtual int GetExpectedParryingResultAdder(BattleActionModel action, int actorResult, BattleActionModel oppoActionOrNull, int oppoResult)
        {
            return 0;
        }

        public virtual int GetOpponentParryingResultAdder(BattleActionModel action, int actorResult, BattleActionModel oppoAction, int oppoResult)
        {
            return 0;
        }

        public virtual int GetExpectedOpponentParryingResultAdder(BattleActionModel action, int actorResult, BattleActionModel oppoAction, int oppoResult)
        {
            return 0;
        }

        public virtual int GetSkillPowerAdder(BattleActionModel action, COIN_ROLL_TYPE rollType)
        {
            return 0;
        }

        public virtual int GetExpectedSkillPowerAdder(BattleActionModel action, List<BattleActionModel> prevActions, COIN_ROLL_TYPE rollType, SinActionModel expectedTargetSinActionOrNull)
        {
            return 0;
        }

        public virtual int GetSkillPowerResultAdder(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            return 0;
        }

        public virtual int GetExpectedSkillPowerResultAdder(BattleActionModel action, List<BattleActionModel> prevActions, BattleUnitModel expectedTarget)
        {
            return 0;
        }

        public virtual int GetAttackAdder(BattleActionModel actionOrNull)
        {
            return 0;
        }

        public virtual int GetDefenseAdder()
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

        public virtual int GetTakeAttackDmgAdder(BattleActionModel action, BattleUnitModel attacker)
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

        public virtual int GetTakeHpDmgAdder()
        {
            return 0;
        }

        public virtual int GetExpectedTakeHpDmgAdder()
        {
            return 0;
        }

        public virtual int ChangeTakeDamage(BattleActionModel action, CoinModel coinOrNull, int resultDmg, DAMAGE_SOURCE_TYPE dmgSrcType, BUFF_UNIQUE_KEYWORD keyword, BATTLE_EVENT_TIMING timing)
        {
            return 0;
        }

        public virtual int GetCoinProbAdder()
        {
            return 0;
        }


        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//


        public virtual float GetTakeBuffStackMultiplier(SkillModel skill, BUFF_UNIQUE_KEYWORD buf)
        {
            return 0;
        }

        public virtual float GetTakeBuffTurnMultiplier(SkillModel skill, BUFF_UNIQUE_KEYWORD buf)
        {
            return 0;
        }

        public virtual float GetSinBuffDamageMultiplier(BUFF_UNIQUE_KEYWORD buff)
        {
            return 0;
        }

        public virtual float GetTakeHpHealMultiplier(BattleUnitModel healerOrNull, ABILITY_SOURCE_TYPE srcType)
        {
            return 0;
        }

        public virtual float GetTakeHpHealMultiplierPart(BattleUnitModel_Abnormality_Part part, BattleUnitModel healerOrNull, ABILITY_SOURCE_TYPE srcType)
        {
            return 0;
        }

        public virtual float GetGiveBsGaugeUpMultiplier(bool onGiveExplosion, BattleActionModel actionOrNull, CoinModel coinOrNull)
        {
            return 0;
        }

        public virtual float GetBsGaugeUpMultiplier(bool onGiveExplosion, BattleActionModel actionOrNull, CoinModel coinOrNull)
        {
            return 0;
        }

        public virtual float GetMaxHpMultiplier()
        {
            return 0;
        }

        public virtual float GetMaxHpMultiplierPartToAbnormality()
        {
            return 0;
        }

        public virtual float GetAtkResistAdder(ATK_BEHAVIOUR type)
        {
            return 0;
        }

        public virtual float GetAtkResistMultiplier(ATK_BEHAVIOUR type)
        {
            return 0;
        }

        public virtual float GetDefenseMultiplier()
        {
            return 0;
        }

        public virtual float GetAttributeResistAdder(global::ATTRIBUTE_TYPE type)
        {
            return 0;
        }

        public virtual float GetAttributeResistMultiplier(global::ATTRIBUTE_TYPE type)
        {
            return 0;
        }

        public virtual float GetAttackDmgMultiplier(BattleActionModel action, CoinModel coin, BattleUnitModel target, bool isWinDuel, bool isCritical)
        {
            return 0;
        }

        public virtual float GetExpectedAttackDmgMultiplier(BattleActionModel action, CoinModel coin, BattleUnitModel targetOrNull, SinActionModel targetSinActionOrNull)
        {
            return 0;
        }

        public virtual float GetTakeAttackDmgMultiplier(BattleActionModel action, BattleUnitModel attacker, bool isCritical)
        {
            return 0;
        }

        public virtual float GetExpectedTakeAttackDmgMultiplier(BattleActionModel action, BattleUnitModel attacker)
        {
            return 0;
        }

        public virtual float GetHpHealMultiplier(BattleUnitModel target)
        {
            return 0;
        }

        public virtual float GetCriticalChanceAdder(Dictionary<BUFF_UNIQUE_KEYWORD, float> affectKeywords)
        {
            return 0;
        }

        public virtual float GetCriticalDamageRatioResultMultiplier(BattleActionModel action)
        {
            return 0;
        }


        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//


        public virtual List<PrimeTargetData> GetPrimeTargets(BattleActionModel action)
        {
            return null;
        }



        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//
        //------------------------------------------------------------------------------------------//

        public virtual void OnAddUnit(BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnAddUnitPart(BattleUnitModel part, BATTLE_EVENT_TIMING timing)
        {

        }


        public virtual void OnAddUnitView(BattleUnitView view)
        {

        }

        public virtual void OnStageStart(BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnWaveStart(BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnRoundStart_After_Event(BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnRoundStart_After_Event_DeadOrRetreated(BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnReturnToField(int retreatTurn, BattleUnitModel triggerUnit, BUFF_UNIQUE_KEYWORD retreatKeyword, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnReturnToFieldOtherUnit_DeadOrRetreated(BattleUnitModel returnUnit, int retreatTurn, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnReleaseStandByOtherUnit_DeadOrRetreated(BattleUnitModel addedUnit, List<BattleUnitModel> addedUnitList)
        {

        }

        public virtual void OnReleaseStandBy(BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnPanicOrLowMorale(PANIC_LEVEL level, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnCompleteCommand(BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnBattleStart(BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnBattleEnd(BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnStartTurn_BeforeLog(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnStartTurn_AfterLog(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnStartDuel(BattleActionModel ownerAction, BattleActionModel opponentAction, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnResult_OnAction(BattleActionModel action, CoinModel coin)
        {

        }

        public virtual void OnResult_OnParrying(BattleActionModel action, BattleActionModel oppoAction, CoinModel coin)
        {

        }

        public virtual void OnWinParrying(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnLoseParrying(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnDuelAfter_BeforeLog(BattleActionModel selfAction, BattleActionModel oppoAction, int parryingCount, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnWinDuel(BattleActionModel selfAction, BattleActionModel oppoAction, int parryingCount, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnLoseDuel(BattleActionModel selfAction, BattleActionModel oppoAction, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void BeforeGiveAttackDamage(BattleActionModel action, CoinModel coin, BattleUnitModel target, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnGiveHpDamage(BattleUnitModel target, int value, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnGiveMpDamage(BattleUnitModel target, int value)
        {

        }

        public virtual void OnTakeMpDmg_SinBuff(int value, BATTLE_EVENT_TIMING timing, BUFF_UNIQUE_KEYWORD keyword)
        {

        }

        public virtual void BeforeAttack(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnCriticalActivated(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnStartCoin(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnEndCoin_BeforeLog(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnEndCoin_AfterLog(BattleActionModel action, CoinModel coin)
        {

        }

        public virtual void OnSucceedEvade(BattleActionModel evadeAction, BattleActionModel attackAction, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnAttackConfirmed(BattleActionModel action, CoinModel coin, BattleUnitModel target, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnKillTarget(BattleActionModel actionOrNull, BattleUnitModel target, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnStartBehaviour(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnSucceedAttack(BattleActionModel action, CoinModel coin, BattleUnitModel target, int finalDmg, int realDmg, bool isCritical, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnRollOneCoin_AfterAttack(BattleActionModel action, CoinModel coin)
        {

        }

        public virtual void OnEndAttack(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnEndAttackPart(BattleUnitModel_Abnormality_Part part, BattleActionModel action)
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

        public virtual void OnActivateImmortality(BattleUnitModel immortalActivator, BATTLE_EVENT_TIMING timing, BattleActionModel actionOrNull)
        {

        }

        public virtual void OnActivateAbnormalityImmortality(BATTLE_EVENT_TIMING timing, BattleActionModel actionOrNull)
        {

        }

        public virtual void OnDestroyShield(BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnRecoverBreak(BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnGiveBsGaugeUp(BattleUnitModel giver, BattleUnitModel target, int value, BATTLE_EVENT_TIMING timing, bool onExplosion, ABILITY_SOURCE_TYPE abilitySrc, BattleActionModel actionOrNull)
        {

        }

        public virtual void BeforeTakeAttackDamage(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void BeforePartTakeAttackDamage(BattleUnitModel_Abnormality abnormality, BattleUnitModel_Abnormality_Part part, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnTakeAttackDamage(BattleActionModel action, CoinModel coin, int totalDmg, int hpDmg, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnTakeAttackDamagePart(BattleUnitModel_Abnormality_Part part, BattleActionModel attackerAction, CoinModel coin, int value, BATTLE_EVENT_TIMING timing, bool isCritical)
        {

        }

        public virtual void OnEndEnemyAttack(BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnBeforeDefense(BattleActionModel action)
        {

        }

        public virtual void OnRoundEnd(BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnRetreat(BattleUnitModel triggerUnit, BUFF_UNIQUE_KEYWORD retreatKeyword, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnRoundEndAfter()
        {

        }

        public virtual void OnStageEnd()
        {

        }

        public virtual void RightBeforeOtherUnitGiveBuffBySkill(BattleUnitModel giver, BattleUnitModel target, BUFF_UNIQUE_KEYWORD bufKeyword, int stack, int turn, SkillModel skill, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void RightAfterOtherUnitGiveBuffBySkill(BattleUnitModel giver, BattleUnitModel target, BUFF_UNIQUE_KEYWORD bufKeyword, int stack, int turn, SkillModel skill, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void RightAfterLosingBuff(int loseStack, int loseTurn, BATTLE_EVENT_TIMING timing, BuffInfo info)
        {

        }

        public virtual void OnSwitchTargetVibrationToSpecial(BattleUnitModel target, BUFF_UNIQUE_KEYWORD keyword, int prevStack, int prevTurn, int afterStack, int afterTurn, BATTLE_EVENT_TIMING timing, ABILITY_SOURCE_TYPE abilitySourceType)
        {

        }

        public virtual void RightAfterGetAnyBuff(BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, int activeRound, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing, BattleUnitModel giverOrNull, BattleActionModel actionOrNull, int overStack, int overTurn)
        {

        }

        public virtual void RightAfterGetAnyBuffAtPart(BattleUnitModel_Abnormality_Part part, BUFF_UNIQUE_KEYWORD keyword, int stack, int activeRound, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing, BattleUnitModel giverOrNull, BattleActionModel actionOrNull)
        {

        }

        public virtual void OnDestroy(BattleUnitModel destroyerOrNull, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnOtherPartDestroyed(BattleUnitModel_Abnormality_Part destroyedPart)
        {

        }

        public virtual void OnPartDestroyed(BattleUnitModel_Abnormality_Part destroyedPart, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnPartBreaked(BattleUnitModel_Abnormality_Part breakedPart, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnPartRecoverBreak(BattleUnitModel_Abnormality_Part recoveredPart, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnRegenerate(BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnPartRegenerate(BattleUnitModel_Abnormality_Part part, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnZeroHp()
        {

        }

        public virtual void OnDie(BattleUnitModel killer, BattleActionModel actionOrNull, DAMAGE_SOURCE_TYPE dmgSrcType, BUFF_UNIQUE_KEYWORD keyword, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnBreak(BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BATTLE_EVENT_TIMING timing, bool isBreakForcely)
        {

        }

        public virtual void OnEnemyBrokenByAttacker(BattleActionModel actionOrNull, BattleUnitModel target, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnDieOtherUnit(BattleUnitModel killer, BattleUnitModel dead, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE dmgSrcType, BUFF_UNIQUE_KEYWORD keyword)
        {

        }

        public virtual void OnBreakOtherUnit(BattleUnitModel breakedUnit, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnDiscardSinOtherUnit(BattleUnitModel discardUnit, UnitSinModel sin, BATTLE_EVENT_TIMING timing, BattleActionModel actionOrNull)
        {

        }

        public virtual void OnDiscardSin(UnitSinModel sin, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnVibrationExplosionOtherUnit(BattleUnitModel explodedUnit, BattleUnitModel giverOrNull, BattleActionModel actionOrNull, ABILITY_SOURCE_TYPE abilitySrc, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnTakeAttackDamageOtherUnit(BattleActionModel action, int realDmg, int hpDmg, BattleUnitModel attackedUnit, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnUseBloodDinnerUnit(BattleUnitModel usedUnit, int stack, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnGiveImmortalState(BattleUnitModel immortalTaker, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnAfterTryTakeHpHeal(BattleUnitModel healerOrNull, int tryHeal, int resultHeal, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnBeforeTryTakeMpHeal(BattleUnitModel healerOrNull, int tryHeal, int resultHeal, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnAfterTryTakeMpHeal(BattleUnitModel healerOrNull, int tryHeal, int resultHeal, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void CheckLoseBuffStackAndTurn(BuffInfo info, int loseStack, int loseTurn, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnStartPhase(PHASE phase, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnTakeHpDamage(int finalDamage, int hpDamage, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BUFF_UNIQUE_KEYWORD keyword)
        {

        }

        public virtual void OnTakeHpDamagePart(BattleUnitModel_Abnormality_Part part, int finalDamage, int hpDamage, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BUFF_UNIQUE_KEYWORD keyword)
        {

        }

        public virtual void OnTakeHpDamageOtherUnit(BattleUnitModel damaged, int finalDamage, int hpDamage, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, List<BattleUnitModel> relatedUnitsOrNull, BUFF_UNIQUE_KEYWORD keyword)
        {

        }

        public virtual void OnTakeAttackConfirmed(BattleActionModel action, CoinModel coin, BattleUnitModel attacker, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnChangeHp(int oldHp, int newHp, DAMAGE_SOURCE_TYPE dmgSrcType, BATTLE_EVENT_TIMING timing, BattleUnitModel attackerOrNull, BattleActionModel actionOrNull, BUFF_UNIQUE_KEYWORD keyword)
        {

        }

        public virtual void OnChangeMp(int oldMp, int newMp)
        {

        }

        public virtual void OnChangeMpOther(BattleUnitModel mpChangeUnit, int oldMp, int newMp)
        {

        }

        public virtual void OnTakeMpDamage(BattleUnitModel attacker, int value, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleActionModel actionOrNull)
        {

        }

        public virtual void OnTakeMpDamageOther(BattleUnitModel mpDmgUnit, BattleUnitModel attackerOrNull, int value, BATTLE_EVENT_TIMING timing, DAMAGE_SOURCE_TYPE sourceType, BattleActionModel actionOrNull)
        {

        }

        public virtual void OnAfterTryTakeMpHealOther(BattleUnitModel mpHealUnit, BattleUnitModel healerOrNull, int tryHeal, int resultHeal, ABILITY_SOURCE_TYPE srcType, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnUseBuffTurnBySkill(SkillModel skill, BUFF_UNIQUE_KEYWORD bufKeyword, int turn, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnUseBuff(BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void BeforeUseBuff(BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnPickSkills()
        {

        }

        public virtual void OnAddActionToPart(BattleUnitModel_Abnormality_Part part, BattleActionModel action)
        {

        }

        public virtual void RightAfterDestroyAnyBuff(BuffInfo destroyedBuffInfo, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnCanceledByLackOfBuffsAtStartCoin(BattleActionModel action, CoinModel coin, List<BUFF_UNIQUE_KEYWORD> lackOfBuffs, BATTLE_EVENT_TIMING timing)
        {

        }

        public virtual void OnFailedToGetBuff(BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, int activeRound, ABILITY_SOURCE_TYPE abilitySrcType, BATTLE_EVENT_TIMING timing, BattleUnitModel giverOrNull)
        {

        }

        public virtual void OnUseCoinConsume(BattleUnitModel owner, BattleActionModel action, CoinModel coin, BUFF_UNIQUE_KEYWORD keyword, int stack, int turn, BATTLE_EVENT_TIMING timing)
        {

        }

    }
}
