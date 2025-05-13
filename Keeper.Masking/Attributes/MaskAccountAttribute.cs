namespace Keeper.Masking
{
    /// <summary>
    /// If applied to a property in an analytics event or context, masks a card.
    /// </summary>
    public sealed class MaskAccountAttribute : MaskAttribute
    {
        internal override string? Mask(string? value) => MaskHelper.MaskAccount(value);
    }
}
