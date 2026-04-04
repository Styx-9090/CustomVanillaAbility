using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace CustomVanillaAbility.CustomClasses
{
    public abstract class CustomAbilityBase
    {
        public AbilityBase _reservedBaseAbility;
        public System.Collections.Generic.HashSet<string> _triggerMethodHash = new System.Collections.Generic.HashSet<string>();
        public System.Collections.Generic.HashSet<string> _bannedMethodTriggerNames = new System.Collections.Generic.HashSet<string>();

        public ABILITY_SOURCE_TYPE AbilitySourceType
        {
            get
            {
                return _abilitySourceType;
            }
        }

        protected virtual ABILITY_SOURCE_TYPE _abilitySourceType
        {
            get
            {
                return ABILITY_SOURCE_TYPE.NONE;
            }
        }


        public DAMAGE_SOURCE_TYPE DamageSourceType
        {

            get
            {
                return _damageSourceType;
            }
        }

        protected virtual DAMAGE_SOURCE_TYPE _damageSourceType
        {
            get
            {
                return DAMAGE_SOURCE_TYPE.NONE;
            }
        }


        protected virtual string _mainKeyword
        {
            get
            {
                return "";
            }
        }

        protected virtual System.Collections.Generic.List<string> _subKeywords
        {
            get
            {
                return [];
            }
        }

        public virtual void SetTrigger(string abilityType)
        {
            this._triggerMethodHash = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.IsVirtual && !m.IsFinal && m.IsOverride() && !_bannedMethodTriggerNames.Any(banned => m.Name.Contains(banned)))
                .Select(m => m.Name).ToHashSet();
        }

        public virtual object ReturnUniqueData()
        {
            return null;
        }
    }

    //--------------------------------------------------------------------------//
    //--------------------------------------------------------------------------//
    //--------------------------------------------------------------------------//
    //--------------------------------------------------------------------------//


    public abstract class CustomActionAbilityBase : CustomAbilityBase
    {
        public float _jsonValue;
        public int _index;
        public int _limitedActivateCount;
        public ConditionalData _conditionalData;
        public BuffReferenceData _info;


        public virtual int BloodDinnerIndex
        {
            get
            {
                return this._bloodDinnerIndex;
            }
            set
            {
                this._bloodDinnerIndex = value;
            }
        }

        protected int _bloodDinnerIndex = -1;
    }
}
