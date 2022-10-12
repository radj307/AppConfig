using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AppConfig
{
    internal static class PropertyInfoExtensions
    {
        internal static bool IsAutoImplemented(this PropertyInfo pInfo) => (pInfo.GetMethod is not null && pInfo.GetMethod.GetCustomAttribute<CompilerGeneratedAttribute>(true) != null) || (pInfo.SetMethod is not null && pInfo.SetMethod.GetCustomAttribute<CompilerGeneratedAttribute>(true) != null);
    }
    internal class JsonLoaderContractResolver : DefaultContractResolver
    {
        public bool AllowCustomProperties { get; set; } = false;

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (member.GetCustomAttribute<IgnoreAttribute>() != null)
            {
                prop.Ignored = true;
            }
            else if (!AllowCustomProperties && member is PropertyInfo pInfo)
            {
                if (!pInfo.IsAutoImplemented())
                {
                    prop.Ignored = true;
                }
            }

            return prop;
        }
    }
}