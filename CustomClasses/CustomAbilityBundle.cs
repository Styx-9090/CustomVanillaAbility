using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CustomVanillaAbility.CustomClasses
{
    public class CustomAbilityBundle
    {
        public bool availableState;

        internal readonly System.Collections.Generic.HashSet<Type> abilityTypeHash = new();
        internal readonly System.Collections.Generic.Dictionary<string, Type> abilityTypeByLookup = new(StringComparer.OrdinalIgnoreCase);

        internal readonly System.Collections.Generic.Dictionary<string, Type> abilityClassDict = new();
        internal readonly System.Collections.Generic.HashSet<string> abilityLookup = new();
        internal readonly System.Collections.Generic.HashSet<long> affectedLookup = new();


        internal readonly System.Collections.Generic.Dictionary<Regex, Type> abilityClassRegDict = new();
        internal readonly System.Collections.Generic.List<Regex> regexLookup = new();
        internal readonly System.Collections.Generic.Dictionary<string, (Regex, Type)> abilityRegTypeByLookup = new(StringComparer.OrdinalIgnoreCase);

        public virtual bool ContainValue(object key)
        {
            return false;
        }
    }

    public class CustomSkillAbilityBundle : CustomAbilityBundle
    {
        internal readonly ConditionalWeakTable<SkillModel, System.Collections.Generic.List<CustomAbilityBase>> customAbilityTable = new();

        public override bool ContainValue(object key)
        {
            return customAbilityTable.TryGetValue((SkillModel)key, out _);
        }
    }


    public class CustomPassiveAbilityBundle : CustomAbilityBundle
    {
        internal readonly ConditionalWeakTable<PassiveModel, CustomPassiveAbilityHolder> customAbilityHolderTable = new();

        public override bool ContainValue(object key)
        {
            return customAbilityHolderTable.TryGetValue((PassiveModel)key, out _);
        }
    }
}
