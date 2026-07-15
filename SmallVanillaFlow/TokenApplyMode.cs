namespace SmallVanillaFlow
{
    /// <summary>
    /// Controls how configured starting tokens are applied.
    /// </summary>
    public enum TokenApplyMode
    {
        /// <summary>
        /// Replace the vanilla starting token value.
        /// </summary>
        Set,

        /// <summary>
        /// Add the configured amount on top of the vanilla value.
        /// </summary>
        Add,
    }
}
