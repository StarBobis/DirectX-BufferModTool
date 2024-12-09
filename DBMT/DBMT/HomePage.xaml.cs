using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DBMT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        //DesktopAcrylicController m_backdropController;

        //private bool TrySetSystemBackdrop()
        //{
        //    if (DesktopAcrylicController.IsSupported())
        //    {
        //        m_backdropController = new DesktopAcrylicController();
        //        return true;
        //    }
        //    return false;
        //}

        public HomePage()
        {
            this.InitializeComponent();
        }

        private void OpenLinkButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && Uri.IsWellFormedUriString(button.Tag.ToString(), UriKind.Absolute))
            {
                IAsyncOperation<bool> asyncOperation = Launcher.LaunchUriAsync(new Uri(button.Tag.ToString()));
            }
        }
    }
}
