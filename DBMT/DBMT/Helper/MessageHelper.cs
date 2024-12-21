using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using WinRT.Interop;
using DBMT_Core;

namespace DBMT
{
    
    public static class MessageHelper
    {
        //TODO 使用MessageDialog的方式弹出对话框有BUG！如果启动了3Dmigoto Loader.exe的话再弹出对话框就全是白屏了，原因未知
        public static async Task<bool> Show(string ContentChinese,string ContentEnglish="")
        {
            var messageDialog = new MessageDialog(ContentChinese, "提示");

            if (GlobalConfig.GameCfg.Value.Language && ContentEnglish != "")
            {
                messageDialog = new MessageDialog(ContentEnglish, "Tips");
            }
            //如果遇到这里说App.m_window是空指针引用问题,说明你给它附加了调试器
            //附加调试器之后这里就无法正常获取窗口句柄了。
            nint windowHandle = WindowNative.GetWindowHandle(App.m_window);
            InitializeWithWindow.Initialize(messageDialog, windowHandle);
            await messageDialog.ShowAsync();
            return true;
        }
    }
}
