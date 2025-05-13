namespace Keeper.Masking
{
    /// <summary>
    /// Base class for mask attributes.
    /// </summary>

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class MaskAttribute : Attribute
    {
        internal abstract string? Mask(string? value);
    }
}
