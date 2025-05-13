namespace Keeper.Masking
{
    internal static class MaskHelper
    {
        public static string? MaskAddress(object? obj)
        {
            if (obj is null)
                return null;
            else
            {
                var value = obj.ToString();
                var lines = value?.Split('\n');
                if (lines?.Length > 0)
                {
                    lines[0] = new string('*', lines[0].Length);
                    return string.Join("\n", lines);
                }
                else
                    return null;
            }
        }

        public static string? MaskCard(object? obj)
        {
            if (obj is null)
                return null;

            var value = obj.ToString()?.Replace("-", "");
            if (value is null)
                return null;
            else if (value.Length < 5)
                return "****";
            else
            {
                var maskSize = value.Length - 4;
                return string.Concat(new string('*', maskSize), value.Substring(maskSize));
            }
        }

        public static string? MaskAccount(object? obj)
        {
            if (obj is null)
                return null;

            var value = obj.ToString()?.Replace("-", "");
            if (value is null)
                return null;
            else if (value.Length < 5)
                return "****";
            else
            {
                var maskSize = value.Length - 4;
                return string.Concat(new string('*', maskSize), value.Substring(maskSize));
            }
        }

        public static string? MaskSSN(object? obj)
        {
            const string MASK = "******";

            if (obj is null)
                return null;

            var value = obj.ToString()?.Replace("-", "");

            if (value is null)
                return null;
            else if (value.Length > 5)
                return string.Concat(MASK, value.AsSpan(6));
            else
                return MASK;
        }

        public static string? MaskAll(object? obj)
        {
            if (obj is null)
                return null;

            return new string('*', obj.ToString()?.Length ?? 4);
        }
    }
}
