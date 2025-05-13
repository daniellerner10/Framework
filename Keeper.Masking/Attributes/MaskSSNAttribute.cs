namespace Keeper.Masking
{
    /// <summary>
    /// If applied to a property in an analytics event or context, masks a SSN.
    /// </summary>
    public sealed class MaskSSNAttribute : MaskAttribute
    {
        internal override string? Mask(string? value) => MaskHelper.MaskSSN(value);
    }
}
