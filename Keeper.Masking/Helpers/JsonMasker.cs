using System.Text;
using System.Text.RegularExpressions;

namespace Keeper.Masking
{
    internal class JsonMasker(List<PropertyToMask> PropertiesToMask)
    {
        protected JsonMasker() : this([]) { }

        public static JsonMasker NullMasker = new NullJsonMasker();

        private const string Name = "n";
        private const string Value = "v";

        private Regex? _maskRegex;

        private Dictionary<string, MaskAttribute>? _attributes;
        private Dictionary<string, MaskAttribute> Attributes => _attributes 
            ??= PropertiesToMask.ToDictionary(
                    x => x.Name.ToLowerInvariant(), 
                    x => x.MaskAttribute
                );

        public virtual string Mask(string json)
        {
            _maskRegex ??= BuildMaskRegex();

            return _maskRegex.Replace(json, m =>
            {
                var name = m.Groups[Name].Value.ToLowerInvariant();
                var valueGroup = m.Groups[Value];
                var value = valueGroup.Value;
                var maskAttribute = Attributes[name];
                var masked = maskAttribute.Mask(value);

                var replaceString = new StringBuilder(m.Length);
                replaceString.Append(m.Value[..(valueGroup.Index - m.Index)]);
                replaceString.Append(masked);
                replaceString.Append('"');

                return replaceString.ToString();
            });
        }

        private Regex BuildMaskRegex()
        {
            var regexString =
                string.Join(
                    "|",
                    Attributes
                    .Keys
                    .Select(name =>
                    {
                        return $"\"(?<{Name}>{name})\":\\s*\"(?<{Value}>.*?)(?<!(?<!\\\\)\\\\)\"";
                    })
                );

            return new Regex(regexString, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
        }
    }

    internal class NullJsonMasker : JsonMasker
    {
        public override string Mask(string json) => json;
    }

    internal record PropertyToMask(string Name, MaskAttribute MaskAttribute);
}