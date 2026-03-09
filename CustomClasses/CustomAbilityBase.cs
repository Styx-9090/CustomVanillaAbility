using System;
using System.Collections.Generic;

namespace CustomVanillaAbility.CustomClasses
{
    public abstract class CustomAbilityBase
    {
        public AbilityBase _reservedBaseAbility;


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
                return new System.Collections.Generic.List<string>();
            }
        }
    }


    //--------------------------------------------------------------------------//
    //--------------------------------------------------------------------------//
    //--------------------------------------------------------------------------//
    //--------------------------------------------------------------------------//


    public abstract class CustomActionAbilityBase : CustomAbilityBase
    {
        public int BloodDinnerIndex
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
