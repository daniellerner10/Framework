namespace Keeper.Masking
{
    /// <summary>
    /// If applied to a property in an analytics event or context, masks a SSN.
    /// </summary>
    public sealed class MaskAll : MaskAttribute
    {
        internal override string? Mask(string? value) => MaskHelper.MaskAll(value);
    }
}
