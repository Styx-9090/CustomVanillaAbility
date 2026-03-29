using System;

namespace CustomVanillaAbility.CustomClasses
{
    public class CustomAbilityBundle
    {
        public bool availableState;

        internal System.Collections.Generic.Dictionary<string, Type> abilityClassDict;
        internal System.Collections.Generic.Dictionary<object, System.Collections.Generic.List<CustomAbilityBase>> customAbilityTable;
        internal System.Collections.Generic.HashSet<string> abilityLookup;
        internal System.Collections.Generic.HashSet<int> affectedLookup;


        public CustomAbilityBundle()
        {
            availableState = false;
            abilityClassDict = new System.Collections.Generic.Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            customAbilityTable = new System.Collections.Generic.Dictionary<object, System.Collections.Generic.List<CustomAbilityBase>>();
            affectedLookup = new System.Collections.Generic.HashSet<int>();
            abilityLookup = new System.Collections.Generic.HashSet<string>();
        }


        public void SafeClean()
        {
            customAbilityTable?.Clear();
            affectedLookup?.Clear();
        }
    }
}
