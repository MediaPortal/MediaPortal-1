using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using MediaPortal;


namespace MediaPortal
{
	public class CoreExtensionControl: Page
	{
        private CoreWindow _coreWindow;
        /// <summary>
        /// The CoreExtensionControl Class controls all the input/output from an Core-Extension.
        /// </summary>
        public void SetCoreWindow(CoreWindow win)
        {
            _coreWindow = win;
        }
        public CoreExtensionControl()
        {
        }
        
        public void GUIElementAction(object GUIElement,RoutedEventArgs e)
        {
            if(GUIElement.GetType()==typeof(System.Windows.Controls.Button))
                MessageBox.Show(((Button)GUIElement).Name);
        }

        public void NavigateTo(Page pageToLoad)
        {
            _coreWindow.Content = pageToLoad;
        }
 
    }
}
