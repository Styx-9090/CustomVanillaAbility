using System;
using System.Text.RegularExpressions;

namespace CustomVanillaAbility.CustomClasses
{
    public class CustomAbilityBundle
    {
        public bool availableState;

        internal System.Collections.Generic.HashSet<Type> abilityTypeHash;
        internal System.Collections.Generic.Dictionary<string, Type> abilityTypeByLookup;
        internal System.Runtime.CompilerServices.ConditionalWeakTable<object, System.Collections.Generic.List<CustomAbilityBase>> customAbilityTable;


        internal System.Collections.Generic.Dictionary<string, Type> abilityClassDict;
        internal System.Collections.Generic.HashSet<string> abilityLookup;
        internal System.Collections.Generic.HashSet<long> affectedLookup;

        internal System.Collections.Generic.Dictionary<Regex, Type> abilityClassRegDict;
        internal System.Collections.Generic.List<Regex> regexLookup;
        internal System.Collections.Generic.Dictionary<string, (Regex, Type)> abilityRegTypeByLookup;


        public CustomAbilityBundle()
        {
            availableState = false;
            abilityTypeHash = [];

            abilityClassDict = new System.Collections.Generic.Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            customAbilityTable = [];

            affectedLookup = [];
            abilityLookup = [];
            abilityTypeByLookup = [];

            regexLookup = [];
            abilityClassRegDict = [];
            abilityRegTypeByLookup = new System.Collections.Generic.Dictionary<string, (Regex, Type)>(StringComparer.OrdinalIgnoreCase);
        }


        public void SafeClean()
        {
            customAbilityTable?.Clear();
            affectedLookup?.Clear();
        }
    }
}
