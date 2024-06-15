namespace Exiled.CustomRoles.API.Features.Interfaces
{
    /// <summary>
    /// An interface for abstract classes what can be used in configs
    /// </summary>
    public interface IAbstractResolvabel
    {
        /// <summary>
        /// Must contain the name of the class.
        /// </summary>
        public string DeriveClassName { get; set; }
    }
}