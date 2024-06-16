namespace Exiled.API.Interfaces
{
    /// <summary>
    /// An interface for abstract classes what can be used in configs
    /// </summary>
    public interface IAbstractResolvable
    {
        /// <summary>
        /// must contain the name of the class.
        /// </summary>
        public string AbilityType { get; set; }
    }
}