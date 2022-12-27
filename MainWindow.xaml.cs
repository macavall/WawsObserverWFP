using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WawsObserverWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml 
    /// </summary>
    public partial class MainWindow : Window
    {
        #region WinINet.DLL method definition stated here
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetGetCookieEx(
            string url,
            string cookieName,
            StringBuilder cookieData,
            ref int size,
            Int32 dwFlags,
            IntPtr lpReserved);

        private const Int32 InternetCookieHttponly = 0x2000;
        /// <summary>
        /// Gets the URI cookie container.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            // Navigate to the WawsObserver site
            WebBrowser.Navigate("https://wawsobserver.azurewebsites.windows.net/");

            // Attain the Authentication Cookie Name and Value
            // Cookie Name: AppServiceAuthSession
            GetCookies();
        }
        
        public void GetCookies()
        {
            // Get the Cookie Collection
            var cookieCollection = WebBrowser.CookieContainer.GetCookies(WebBrowser.Source);

            // Get the Cookie Name and Value
            foreach (Cookie cookie in cookieCollection)
            {
                if (cookie.Name == "AppServiceAuthSession")
                {
                    // Do something with the cookie value
                    string cookieValue = cookie.Value;
                }
            }
        }
    }
}
