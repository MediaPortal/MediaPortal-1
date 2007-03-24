
namespace MCEControls
{
    /// <summary>
    /// define 5 special softkey
    /// </summary>
    /// NOTE: doesn't overload the char type because 
    /// 1. there is no a proper char can represent to Close.
    /// 2. probably bad extensibilty if we want to add more key types.
    /// 3. the SoftKey-Char is 1-N, while SoftKey-controlKey
    /// should be 1-1.
    public enum ControlKey
    {
        /// <summary>
        /// Not a control Key
        /// </summary>
        None = 0,
        /// <summary>
        /// The BackSpace Key
        /// </summary>
        Back = 1,
        /// <summary>
        /// The Space Key
        /// </summary>
        Space = 2,
        /// <summary>
        /// The Close Key
        /// </summary>
        Close = 3,
        /// <summary>
        /// The Shift Key
        /// </summary>
        Shift = 4,
        /// <summary>
        /// The Caps Key
        /// </summary>
        Caps = 5
    }
}
