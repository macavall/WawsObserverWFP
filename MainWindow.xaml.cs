using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

            //Task.Factory.StartNew(async () =>
            //{
            //    await BrowserNavigate();
            //});

            WebBrowser.Navigated += ViewerWebBrowserControlView_Navigated;

            // Attain the Authentication Cookie Name and Value
            // Cookie Name: AppServiceAuthSession

        }

        public async Task BrowserNavigate()
        {
            var loadTask = Task.Factory.StartNew(() =>
            {
                WebBrowser.Navigate("https://wawsobserver.azurewebsites.windows.net/");
            });
            await loadTask;
        }

        public static CookieContainer GetUriCookieContainerAsString(Uri uri)
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
                        cookies.Add(uri, new Cookie(cookieArray[x].Split('=')[0].Trim(), cookieArray[x].Split('=')[1].Trim()));
                        authCookie = cookieArray[x];
                    }
                }

                Console.WriteLine("========================\n");

                cookies.SetCookies(uri, cookieData.ToString().Replace(';', ','));
            }

            return cookies;
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
                        cookies.Add(uri, new Cookie(cookieArray[x].Split('=')[0].Trim(), cookieArray[x].Split('=')[1].Trim()));
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
        private async void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            await Task.Delay(1);

            var browser = sender as WebBrowser;

            if (browser == null || browser.Document == null)
                return;

            dynamic document = browser.Document;

            if (document.readyState != "complete")
                return;

            dynamic script = document.createElement("script");
            script.type = @"text/javascript";
            script.text = @"window.onerror = function(msg,url,line){return true;}";
            document.head.appendChild(script);

            // Get the Authentication Cookie Value for later use
            CookieContainer authCookie = GetUriCookieContainerAsString(
                new Uri("https://wawsobserver.azurewebsites.windows.net/")
                );

            AuthClass.AuthCookieContainer = authCookie;

            //await this.Dispatcher.InvokeAsync(() =>
            //{
            //    // Update UI elements here
            //    TextBoxValue1.Text = authCookie;
            //});

            //TextBoxValue1.Text = authCookie;
        }

        public static class AuthClass
        {
            public static string AuthCookie { get; set; }
            public static CookieContainer AuthCookieContainer { get; set; }
        }

        // The code below this comment is for removing the script error
        // message that was previously showing up for the WebOc
        void ViewerWebBrowserControlView_Navigated(object sender, NavigationEventArgs e)
        {
            BrowserHandler.SetSilent(WebBrowser, true); // make it silent
        }

        public static class BrowserHandler
        {
            private const string IWebBrowserAppGUID = "0002DF05-0000-0000-C000-000000000046";
            private const string IWebBrowser2GUID = "D30C1661-CDAF-11d0-8A3E-00C04FC9E26E";

            public static void SetSilent(System.Windows.Controls.WebBrowser browser, bool silent)
            {
                if (browser == null)
                    MessageBox.Show("No Internet Connection");

                // get an IWebBrowser2 from the document
                IOleServiceProvider sp = browser.Document as IOleServiceProvider;
                if (sp != null)
                {
                    Guid IID_IWebBrowserApp = new Guid(IWebBrowserAppGUID);
                    Guid IID_IWebBrowser2 = new Guid(IWebBrowser2GUID);

                    object webBrowser;
                    sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                    if (webBrowser != null)
                    {
                        webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                    }
                }
            }

        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);


        }

        private async Task ProcessDataAsync()
        {
            string appName = "caseintake"; // Replace with your app name
            string linux = "";
            string numOfSites = "";
            string slotCount = "";
            string workerCount = "";
            string customContainer = "";
            string num32 = "";
            string name = "";
            string alwaysOn = "";
            string affinity = "";
            string stampName = "";
            string asp = "";
            string clr = "";
            string created = "";
            string sku = "";
            string auth = "";
            string kind = "";
            string vNet = "";
            string region = "";

            string extractedValue = String.Empty;

            string triggers = "";
            int triggerCount = 0;

            Console.WriteLine(appName);

            string uriString = $"https://wawsobserver.azurewebsites.windows.net/api/sites/{TextBox1.Text}";

            //int index = AuthClass.AuthCookie.IndexOf('='); // Find the index of '='
            //if (index != -1)
            //{
            //    extractedValue = AuthClass.AuthCookie.Substring(index + 1); // Get the substring after '='
            //    Console.WriteLine(extractedValue); // Output: 123456789
            //}

            Uri uri = new Uri(uriString);

            string contentType = "application/json";
            string method = "GET";

            var cookie = new Cookie("AppServiceAuthSession", extractedValue);
            cookie.Domain = uri.DnsSafeHost;

            var cookieContainer = AuthClass.AuthCookieContainer;
            // cookieContainer.Add(cookie);

            var handler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer
            };

            var client = new HttpClient(handler);

            var headers = new Dictionary<string, string>
           {
               {"Referer", $"https://wawsobserver.azurewebsites.windows.net/sites/{appName}"}
           };

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));

            HttpResponseMessage response = await client.GetAsync(uri);
            dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            var azureResourceResult = new AzureResource();
            var fieldValueList = new List<string>();
            var fieldList = new List<string>()
            {
                "Linux",
                "NumOfSites",
                "SlotCount",
                "WorkerCount",
                "CustomContainer",
                "Num32",
                "Name",
                "AlwaysOn",
                "Affinity",
                "StampName",
                "Asp",
                "Clr",
                "Created",
                "Sku",
                "Auth",
                "Kind",
                "VNet",
                "Region"
            };

            // Needed for if statements below
            azureResourceResult.Sku = ((dynamic)((dynamic)result)[0].sku).Value.ToString();

            azureResourceResult.Linux = ((dynamic)((dynamic)result)[0].server_farm).is_linux.Value.ToString();
            fieldValueList.Add(((dynamic)((dynamic)result)[0].server_farm).is_linux.Value.ToString());

            if (!azureResourceResult.Sku.ToLower().Contains("dynamic"))
            {
                azureResourceResult.NumOfSites = ((dynamic)((dynamic)result)[0].web_workers)[0].site_count.Value.ToString();
                fieldValueList.Add(((dynamic)((dynamic)result)[0].web_workers)[0].site_count.Value.ToString());
            }
            else
            {
                azureResourceResult.NumOfSites = "Consumption Plan - Site N/A";
                fieldValueList.Add("Consumption Plan - Site N/A");
            }


            azureResourceResult.SlotCount = ((dynamic)((dynamic)result)[0].slots).Count.ToString();
            fieldValueList.Add(((dynamic)((dynamic)result)[0].slots).Count.ToString());


            if (!sku.ToLower().Contains("dynamic"))
            {
                azureResourceResult.WorkerCount = ((dynamic)((dynamic)result)[0].web_workers).Count.ToString();
                fieldValueList.Add(((dynamic)((dynamic)result)[0].web_workers).Count.ToString());
            }
            else
            {
                azureResourceResult.WorkerCount = "Consumption Plan - Site N/A";
                fieldValueList.Add("Consumption Plan - Site N/A");
            }

            azureResourceResult.CustomContainer = ((dynamic)((dynamic)result)[0].linux_fx_version).Value.ToString();
            fieldValueList.Add(((dynamic)((dynamic)result)[0].linux_fx_version).Value.ToString());

            azureResourceResult.Num32 = ((dynamic)((dynamic)result)[0].options).Value.ToString();
            fieldValueList.Add(((dynamic)((dynamic)result)[0].options).Value.ToString());

            azureResourceResult.Name = ((dynamic)((dynamic)result)[0].name).Value.ToString();
            fieldValueList.Add(((dynamic)((dynamic)result)[0].name).Value.ToString());

            azureResourceResult.AlwaysOn = ((dynamic)((dynamic)result)[0].always_on).Value.ToString();
            fieldValueList.Add(((dynamic)((dynamic)result)[0].always_on).Value.ToString());

            azureResourceResult.Affinity = ((dynamic)((dynamic)result)[0].client_affinity_enabled).Value.ToString();
            fieldValueList.Add(((dynamic)((dynamic)result)[0].client_affinity_enabled).Value.ToString());

            azureResourceResult.StampName = ((dynamic)((dynamic)result)[0].webspace).stamp.name.Value.ToString();
            fieldValueList.Add(((dynamic)((dynamic)result)[0].webspace).stamp.name.Value.ToString());

            azureResourceResult.Asp = ((dynamic)((dynamic)result)[0].virtual_farm_name).Value.ToString();
            fieldValueList.Add(((dynamic)((dynamic)result)[0].virtual_farm_name).Value.ToString());

            azureResourceResult.Clr = ((dynamic)((dynamic)result)[0].clr_version).Value.ToString();
            fieldValueList.Add(((dynamic)((dynamic)result)[0].clr_version).Value.ToString());

            azureResourceResult.Created = ((dynamic)((dynamic)result)[0]).created.Value.ToString();
            fieldValueList.Add(((dynamic)((dynamic)result)[0]).created.Value.ToString());

            // Moving SKU to above for if statement
            fieldValueList.Add(((dynamic)((dynamic)result)[0].sku).Value.ToString());

            azureResourceResult.Auth = ((dynamic)((dynamic)result)[0].site_auth_enabled).Value.ToString();
            fieldValueList.Add(((dynamic)((dynamic)result)[0].site_auth_enabled).Value.ToString());

            azureResourceResult.Kind = ((dynamic)((dynamic)result)[0].kind).Value.ToString();
            fieldValueList.Add(((dynamic)((dynamic)result)[0].kind).Value.ToString());

            azureResourceResult.VNet = ((dynamic)((dynamic)result)[0].vnet_name).Value.ToString();
            fieldValueList.Add(((dynamic)((dynamic)result)[0].vnet_name).Value.ToString());

            azureResourceResult.Region = ((dynamic)((dynamic)result)[0].webspace).name.Value.ToString();
            fieldValueList.Add(((dynamic)((dynamic)result)[0].webspace).name.Value.ToString());

            //CreateDictionary(azureResourceResult);

            string tempString = string.Empty;

            TextBoxResult.Text = string.Empty;

            for (int x = 0; x < fieldList.Count; x++)
            {
                // tempString = "test" with new line
                try
                {
                    tempString += fieldList[x] + ": " + fieldValueList[x] + "\n";

                    Console.WriteLine(fieldList[x] + ": " + fieldValueList[x]);
                }
                catch (Exception ex)
                {
                    tempString += fieldList[x] + ": " + "NO VALUE" + "\n";
                    Console.WriteLine(ex.Message);
                }


            }

            TextBoxResult.Text = tempString;

            //await Console.Out.WriteLineAsync(result);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await ProcessDataAsync();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (TextBoxValue1.Visibility == Visibility.Visible)
            {
                TextBoxValue1.Visibility = Visibility.Hidden;
                AuthButton.Content = "Show Auth Cookie";
            }
            else
            {
                TextBoxValue1.Visibility = Visibility.Visible;
                AuthButton.Content = "Hide Auth Cookie";
            }
        }

        public class AzureResource
        {
            public string Linux { get; set; }
            public string NumOfSites { get; set; }
            public string SlotCount { get; set; }
            public string WorkerCount { get; set; }
            public string CustomContainer { get; set; }
            public string Num32 { get; set; }
            public string Name { get; set; }
            public string AlwaysOn { get; set; }
            public string Affinity { get; set; }
            public string StampName { get; set; }
            public string Asp { get; set; }
            public string Clr { get; set; }
            public string Created { get; set; }
            public string Sku { get; set; }
            public string Auth { get; set; }
            public string Kind { get; set; }
            public string VNet { get; set; }
            public string Region { get; set; }
        }

        public void CreateDictionary(AzureResource azRes)
        {
            var dict = new Dictionary<string, object>();

            Type type = azRes.GetType();

            //var fields = type.GetFields();

            foreach (FieldInfo field in type.GetFields())
            {
                // Get the field name and value
                dict.Add(field.Name, field.GetValue(azRes));

                // Print the field name and value
                //Console.WriteLine($"Field Name: {fieldName}, Field Value: {fieldValue}");
            }

            foreach (KeyValuePair<string, object> kvp in dict)
            {
                Console.WriteLine($"Key = {kvp.Key}, Value = {kvp.Value}");
            }
        }

    }
}
