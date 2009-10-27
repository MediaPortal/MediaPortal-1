
using MpeCore.Classes;

namespace MpeCore.Interfaces
{
    public interface IVersionProvider
    {
        string DisplayName { get;}
        bool Validate(DependencyItem componentItem);
        VersionInfo Version(string id);
    }
}