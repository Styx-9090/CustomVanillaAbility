using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomVanillaAbility.CustomClasses
{
    public class CustomAbilityBundle
    {
        public bool availableState;

        internal System.Collections.Generic.Dictionary<string, CustomAbilityBase> abilityClassDict;
        internal System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<CustomAbilityBase>> customAbilityDict;
        internal System.Collections.Generic.HashSet<string> abilityLookup;
        internal System.Collections.Generic.HashSet<int> affectedLookup;


        public CustomAbilityBundle()
        {
            availableState = false;
            abilityClassDict = new System.Collections.Generic.Dictionary<string, CustomAbilityBase>(StringComparer.OrdinalIgnoreCase);
            customAbilityDict = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<CustomAbilityBase>>();
            affectedLookup = new System.Collections.Generic.HashSet<int>();
            abilityLookup = new System.Collections.Generic.HashSet<string>();
        }


        public void SafeClean()
        {
            customAbilityDict.Clear();
            abilityLookup.Clear();
            affectedLookup.Clear();
        }
    }
}
