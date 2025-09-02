namespace Systems.SimpleInventory.Data.Enums
{
    public enum UseItemResult
    {
        Unknown,
        
        /// <summary>
        ///     Item is invalid (most likely not useable)
        /// </summary>
        InvalidItem,
        
        /// <summary>
        ///     Item cannot be used
        /// </summary>
        CannotBeUsed,
        
        /// <summary>
        ///     Item used successfully
        /// </summary>
        UsedSuccessfully
    }
}