using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using WinRT.Interop;

namespace DBMT
{
    public static class MessageHelper
    {


        public static async void Show(string ContentChinese,string ContentEnglish="")
        {
            var messageDialog = new MessageDialog(ContentChinese, "提示");
            nint windowHandle = WindowNative.GetWindowHandle(App.m_window);
            InitializeWithWindow.Initialize(messageDialog, windowHandle);
            await messageDialog.ShowAsync();
        }

    }
}
