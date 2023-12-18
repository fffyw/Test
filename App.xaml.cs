using IRW.LOG;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BRToolBox
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        System.Threading.Mutex mutex;

        public App()
        {
            Startup += new StartupEventHandler(App_Startup);

            DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(
                Application_DispatcherUnhandledException);

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(
                CurrentDomain_UnhandledException);
        }

        void App_Startup(object sender, StartupEventArgs e)
        {

            bool ret;
            mutex = new System.Threading.Mutex(true, "MESService", out ret);

            if (!ret)
            {
                MessageBox.Show("程序已经在运行");
                Environment.Exit(0);
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = e.ExceptionObject as Exception;
                string errorMsg = "系统发生异常 : \n\n";
                MessageBox.Show($"{errorMsg}{ex.Message}{Environment.NewLine}{ex.StackTrace}");

                SimpleLog.Info($"{errorMsg}:{ex.Message} --- { ex.StackTrace} ");
            }
            catch
            {
                MessageBox.Show("不可恢复的异常，应用程序将退出！");
            }
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = e.Exception;
                string errorMsg = "系统发生异常 : \n\n";
                MessageBox.Show($"{errorMsg}{ex.Message}{Environment.NewLine}{ex.StackTrace}");

                SimpleLog.Info($"{errorMsg}:{ex.Message} --- { ex.StackTrace} ");
            }
            catch
            {
                MessageBox.Show("不可恢复的程序异常，应用程序将退出！");
            }
        }
    }
}
