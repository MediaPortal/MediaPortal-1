using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents class to edit RTSP connection preference.
    /// </summary>
    internal class RtspConnectionPreferenceEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            if ((context != null) && (context.Instance != null))
            {
                return UITypeEditorEditStyle.Modal;
            }

            return base.GetEditStyle(context);
        }

        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if ((context != null) && (context.Instance != null) && (provider != null))
            {
                IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

                if (editorService != null)
                {
                    RtspUrl url = (RtspUrl)context.Instance;

                    using (RtspConnectionPreference preference = new RtspConnectionPreference())
                    {
                        preference.SameConnectionPreference = url.SameConnectionPreference;
                        preference.UdpPreference = url.UdpPreference;
                        preference.MulticastPreference = url.MulticastPreference;

                        editorService.DropDownControl(preference);

                        url.SameConnectionPreference = preference.SameConnectionPreference;
                        url.UdpPreference = preference.UdpPreference;
                        url.MulticastPreference = preference.MulticastPreference;
                    }
                }
            }

            return value;
        }
    }
}
