using System;
using SysCol = System.Collections.Generic;

namespace ChaosFramework.Input.Layouts
{
    public abstract class HidLayout
    {
        public readonly string name;
        public readonly Type enumType;

        public HidLayout(string name, Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException($"{nameof(enumType)} must be an enum type.", nameof(enumType));

            this.name = name;
            this.enumType = enumType;
        }

        public abstract bool ParseUsage(string usageString, out object result);
        public abstract object GetUsage(uint hidUsageID, uint subIndex);

        public override string ToString() => $"{nameof(HidLayout)}{{ {name} }}";
    }

    public class HidLayout<UsageEnum>
        : HidLayout
        where UsageEnum : struct
    {
        protected SysCol.Dictionary<uint, SysCol.Dictionary<uint, UsageEnum>> hidToUsage
            = new SysCol.Dictionary<uint, SysCol.Dictionary<uint, UsageEnum>>();

        protected SysCol.Dictionary<UsageEnum, Tuple<uint, uint>> usageToHid
            = new SysCol.Dictionary<UsageEnum, Tuple<uint, uint>>();

        public HidLayout(string name)
            : base(name, typeof(UsageEnum))
        { }

        public Tuple<uint, uint> GetHid(UsageEnum usage) => usageToHid[usage];
        public bool HasAxis(UsageEnum usage) => usageToHid.ContainsKey(usage);

        public bool TryGetEnumGeneric(uint hidUsageID, uint subIndex, out UsageEnum result)
        {
            SysCol.Dictionary<uint, UsageEnum> dict;
            if (hidToUsage.TryGetValue(hidUsageID, out dict))
                if (dict.TryGetValue(subIndex, out result))
                    return true;

            result = default(UsageEnum);
            return false;
        }

        public override object GetUsage(uint hidUsageID, uint subIndex)
        {
            UsageEnum result;
            if (TryGetEnumGeneric(hidUsageID, subIndex, out result))
                return result;

            return null;
        }

        public override bool ParseUsage(string usageString, out object result)
        {
            UsageEnum res;
            if (Enum.TryParse(usageString, out res))
            {
                result = res;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        internal void AddAxis(UsageEnum usage, uint hidUsageID, uint subIndex)
        {
            SysCol.Dictionary<uint, UsageEnum> dict;
            if (!hidToUsage.TryGetValue(hidUsageID, out dict))
                hidToUsage[hidUsageID] = dict = new SysCol.Dictionary<uint, UsageEnum>();

            hidToUsage[hidUsageID][subIndex] = usage;
            usageToHid[usage] = new Tuple<uint, uint>(hidUsageID, subIndex);
        }

        public override string ToString() => $"{nameof(HidLayout)}<{typeof(UsageEnum).Name}>{{ {name} }}";
    }
}
