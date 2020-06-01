namespace Uneti.NET
{
    /// <summary>
    /// Edit operations.
    /// </summary>
    public enum Operation
    {
        /// <summary>
        /// The value was added. (i.e. null -> B)
        /// </summary>
        Added,
        
        /// <summary>
        /// The value was modified. (i.e. A -> B)
        /// </summary>
        Modified,
        
        /// <summary>
        /// The value was removed. (i.e. A -> null)
        /// </summary>
        Removed
    }
}