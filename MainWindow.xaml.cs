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

        }
        
        public static string GetUriCookieContainer(Uri uri)
        {
            // Get the bare string list of cookies
            var authCookie = String.Empty;

            string cookieString = String.Empty;
            CookieContainer cookies = null;

            // Determine the size of the cookie
            int datasize = 8192 * 16;

            StringBuilder cookieData = new StringBuilder(datasize);

            if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
            {
                // 
                if (datasize < 0)
                {
                    return null;
                }

                // Allocate stringbuilder large enough to hold the cookie
                cookieData = new StringBuilder(datasize);

                // Checking if the InternetGetCookieEx method is available for use
                if (!InternetGetCookieEx(
                        uri.ToString(),
                        null, cookieData,
                        ref datasize,
                        InternetCookieHttponly,
                        IntPtr.Zero))
                {
                    return null;
                }
            }

            if (cookieData.Length > 0)
            {
                // Creating a cookie container
                cookies = new CookieContainer();

                // Values for cookies come in looking like this
                // CookieName1=Value1; CookieName2=Value2; CookieName3=Value3
                // We need to split this into individual cookies
                // and add them to the CookieContainer
                // We also need to make sure that the cookie name and value are separated by a = sign
                // If there is no = sign, then we don't have a valid cookie
                // and we should ignore it
                // Also, if the cookie name is empty, we should ignore it
                // as well
                var cookieArray = cookieData.ToString().Split(';');

                // Loop through each individual cookie
                // and populate the authCookie variable
                Console.WriteLine("\n========================");
                Console.WriteLine("WinInet Cookies Found: ");

                for (int x = 0; x < cookieArray.Length; x++)
                {
                    if (cookieArray[x].Contains("AppServiceAuthSession"))
                    {
                        Console.WriteLine(cookieArray[x]);
                        authCookie = cookieArray[x];
                    }
                }

                Console.WriteLine("========================\n");

                cookies.SetCookies(uri, cookieData.ToString().Replace(';', ','));
            }

            return authCookie;
        }

        // When page is loaded execute GetUriCookieContainer with the WawsObserver
        // URL to get the AppServiceAuthSession Cookie Value
        private void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            // Get the Authentication Cookie Value for later use
            string authCookie = GetUriCookieContainer(
                new Uri("https://wawsobserver.azurewebsites.windows.net/")
                );
        }
    }
}
