namespace Keeper.Masking
{
    /// <summary>
    /// If applied to a property in an analytics event or context, masks an address.
    /// </summary>
    public sealed class MaskAddressAttribute : MaskAttribute
    {
        internal override string? Mask(string? value) => MaskHelper.MaskAddress(value);
    }
}
