using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IKUN_CPU_MONITOR
{
    /// <summary>
    ///  工具类
    /// </summary>
    internal class Utils
    {
        /// <summary>
        ///  设置开机自启动
        /// </summary>
        /// <param name="productName">注册表中显示的名称，最好是程序名</param>
        /// <param name="setOrDelete">true为设置，false为删除</param>
        public static void SetStartup(bool setOrDelete) {
            string keyName = @"Software\Microsoft\Windows\CurrentVersion\Run";
            try
            {
                using (RegistryKey rKey = Registry.CurrentUser.OpenSubKey(keyName, true))
                {
                    if (rKey == null)
                    {
                        throw new UnauthorizedAccessException("无法访问注册表项，请检查权限");
                    }

                    if (setOrDelete)
                    {
                        try
                        {
                            string executablePath = Process.GetCurrentProcess().MainModule.FileName;
                            rKey.SetValue(Application.ProductName, executablePath);
                        }
                        catch (Exception ex)
                        {
                            // 处理无法获取当前进程主模块文件的异常  
                            MessageBox.Show($"无法设置启动项 {Application.ProductName}: {ex.Message}");
                        }
                    }
                    else
                    {
                        rKey.DeleteValue(Application.ProductName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                // 处理其他可能的异常，如注册表访问权限问题等  
                MessageBox.Show($"设置或删除启动项 {Application.ProductName} 时出错: {ex.Message}");
            }
        }

        public static bool IsStartupEnabled()
        {
            string keyName = @"Software\Microsoft\Windows\CurrentVersion\Run";
            using (RegistryKey rKey = Registry.CurrentUser.OpenSubKey(keyName))
            {
                return (rKey.GetValue(Application.ProductName) != null) ? true : false;
            }
        }
    }
 }

