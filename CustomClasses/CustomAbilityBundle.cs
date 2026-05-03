using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SimpleJSON;

namespace CustomVanillaAbility.CustomClasses
{
    public class CustomAbilityBundle
    {
        public bool availableState;
        public JSONArray nodes;

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
        internal readonly ConditionalWeakTable<SkillModel, List<CustomAbilityBase>> customAbilityTable = new();

        public override bool ContainValue(object key)
        {
            return customAbilityTable.TryGetValue((SkillModel)key, out _);
        }

        public bool ProcessPatchListLogic(long id, SkillModel instance, out List<CustomAbilityBase> returnObject)
        {
            returnObject = null;

            if (!this.affectedLookup.Contains(id)) return false;
            if (this.customAbilityTable.TryGetValue(instance, out returnObject) && returnObject.Count > 0) return true;

            return false;
        }
    }


    public class CustomPassiveAbilityBundle : CustomAbilityBundle
    {
        internal readonly Dictionary<PassiveModel, CustomPassiveAbilityHolder> customAbilityHolderTable = new();

        public override bool ContainValue(object key)
        {
            return customAbilityHolderTable.ContainsKey((PassiveModel)key);
        }

        public bool ProcessPatchListLogic(long id, PassiveModel instance, out CustomPassiveAbilityHolder returnObject)
        {
            returnObject = null;

            if (!this.affectedLookup.Contains(id)) return false;
            if (this.customAbilityHolderTable.TryGetValue(instance, out returnObject) && returnObject.passiveList.Count > 0) return true;

            return false;
        }
    }
}
