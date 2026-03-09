using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomVanillaAbility.CustomClasses
{
    public class CustomAbilityBundle
    {
        public bool availableState;

        internal System.Collections.Generic.HashSet<CustomAbilityBase> abilityHash;
        internal System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<CustomAbilityBase>> customAbilityDict;
        internal System.Collections.Generic.HashSet<string> abilityLookup;
        internal System.Collections.Generic.HashSet<int> affectedLookup;


        public CustomAbilityBundle()
        {
            availableState = false;
            abilityHash = new System.Collections.Generic.HashSet<CustomAbilityBase>();
            customAbilityDict = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<CustomAbilityBase>>();
            affectedLookup = new System.Collections.Generic.HashSet<int>();
            abilityLookup = new System.Collections.Generic.HashSet<string>();
        }


        public void ClearAll()
        {
            customAbilityDict.Clear();
            abilityLookup.Clear();
            affectedLookup.Clear();
            abilityHash.Clear();
        }
    }
}
