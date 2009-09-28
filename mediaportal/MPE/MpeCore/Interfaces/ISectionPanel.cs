using MpeCore.Classes;
using MpeCore.Classes.SectionPanel;

namespace MpeCore.Interfaces
{
    public interface ISectionPanel
    {
        string Name { get; set; }
        SectionParamCollection Params { get; set; }
        bool Unique { get; set; }

        SectionParamCollection Init();

        SectionParamCollection GetDefaultParams();

        /// <summary>
        /// Previews the section form, but no change made.
        /// </summary>
        /// <param name="packageClass">The package class.</param>
        /// <param name="sectionItem">The param collection.</param>
        void Preview(PackageClass packageClass, SectionItem sectionItem);

        SectionResponseEnum Execute(PackageClass packageClass, SectionItem sectionItem);
    }
}