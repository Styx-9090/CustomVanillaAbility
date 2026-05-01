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

        public virtual bool ProcessPatchListLogic<T>(long id, object instance, out T returnObject)
        {
            returnObject = default;
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

        public override bool ProcessPatchListLogic<T>(long id, object instance, out T returnObject)
        {
            returnObject = default;

            if (typeof(T) != typeof(List<CustomAbilityBase>)) return false;

            if (!this.affectedLookup.Contains(id)) return false;
            if (this.customAbilityTable.TryGetValue((SkillModel)instance, out var abilityList) && abilityList.Count > 0)
            {
                returnObject = (T)(object)abilityList;
                return true;
            }

            return false;
        }
    }


    public class CustomPassiveAbilityBundle : CustomAbilityBundle
    {
        internal readonly ConditionalWeakTable<PassiveModel, CustomPassiveAbilityHolder> customAbilityHolderTable = new();

        public override bool ContainValue(object key)
        {
            return customAbilityHolderTable.TryGetValue((PassiveModel)key, out _);
        }

        public override bool ProcessPatchListLogic<T>(long id, object instance, out T returnObject)
        {
            returnObject = default;

            if (typeof(T) != typeof(CustomPassiveAbilityHolder)) return false;

            if (!this.affectedLookup.Contains(id)) return false;
            if (this.customAbilityHolderTable.TryGetValue((PassiveModel)instance, out var abilityList))
            {
                returnObject = (T)(object)abilityList;
                return true;
            }

            return false;
        }
    }
}
