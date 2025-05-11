using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using WinRT.Interop;
using DBMT_Core;
using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace DBMT
{
    
    public static class MessageHelper
    {

        public static async Task<bool> ShowConfirm(string ContentChinese, string ContentEnglish = "")
        {
            try
            {
                string TipContent = ContentChinese;
                ContentDialog subscribeDialog = new ContentDialog
                {
                    Title = "Tips",
                    Content = TipContent,
                    PrimaryButtonText = "OK", // 更改为确认
                    CloseButtonText = "Cancel", // 添加取消按钮
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = App.m_window.Content.XamlRoot // 确保设置 XamlRoot
                };

                ContentDialogResult result = await subscribeDialog.ShowAsync();

                // 根据用户点击的按钮返回相应的结果
                return result == ContentDialogResult.Primary; // 如果点击的是确认按钮，则返回true；否则返回false
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }


        public static async Task<bool> Show(string ContentChinese,string ContentEnglish="")
        {
            try
            {
                string TipContent = ContentChinese;

                ContentDialog subscribeDialog = new ContentDialog{
                    Title = "Tips",
                    Content = TipContent,
                    PrimaryButtonText = "OK",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = App.m_window.Content.XamlRoot // 确保设置 XamlRoot
                };

                ContentDialogResult result = await subscribeDialog.ShowAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }

        }

        public static async Task<bool> ShowConfirm(XamlRoot xamlRoot, string ContentChinese, string ContentEnglish = "")
        {
            try
            {
                string TipContent = ContentChinese;
                ContentDialog subscribeDialog = new ContentDialog
                {
                    Title = "Tips",
                    Content = TipContent,
                    PrimaryButtonText = "OK", // 更改为确认
                    CloseButtonText = "Cancel", // 添加取消按钮
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = xamlRoot // 确保设置 XamlRoot
                };

                ContentDialogResult result = await subscribeDialog.ShowAsync();

                // 根据用户点击的按钮返回相应的结果
                return result == ContentDialogResult.Primary; // 如果点击的是确认按钮，则返回true；否则返回false
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        public static async Task<bool> Show(XamlRoot xamlRoot,string ContentChinese, string ContentEnglish = "")
        {
            try
            {
                string TipContent = ContentChinese;

                ContentDialog subscribeDialog = new ContentDialog
                {
                    Title = "Tips",
                    Content = TipContent,
                    PrimaryButtonText = "OK",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = xamlRoot // 确保设置 XamlRoot
                };

                ContentDialogResult result = await subscribeDialog.ShowAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }

        }
    }
}
