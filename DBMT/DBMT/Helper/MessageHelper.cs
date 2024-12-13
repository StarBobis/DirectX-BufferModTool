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
        public static async Task<bool> Show(string ContentChinese,string ContentEnglish="")
        {
            var messageDialog = new MessageDialog(ContentChinese, "Tips");
            //如果遇到这里说App.m_window是空指针引用问题
            //检查Nuget包是否正确安装以及VisualStudioInstaller中的依赖项是否正确安装。
            //记得把咱们DBMT的ReleasePackage的内容放到编译出的文件夹里
            nint windowHandle = WindowNative.GetWindowHandle(App.m_window);
            InitializeWithWindow.Initialize(messageDialog, windowHandle);
            await messageDialog.ShowAsync();
            return true;
        }
    }
}
