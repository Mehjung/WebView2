using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Web.WebView2.Core;


namespace WebView2
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            webView.NavigationStarting += EnsureHttps;
            addressBar.KeyDown += AddressBar_KeyDown;
            addressBar.Text = "https://www.google.de";
            InitializeAsync();
        }

        async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.WebMessageReceived += UpdateAddressBar;

            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.postMessage(window.document.URL);");
            // await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.addEventListener(\'message\', event => alert(event.data));");
        }

        void UpdateAddressBar(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            String uri = args.TryGetWebMessageAsString();
            addressBar.Text = uri;
            webView.CoreWebView2.PostWebMessageAsString(uri);
        }
        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            NavigateToAddress();
        }

        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            // Überprüfen, ob die Return-Taste gedrückt wurde
            if (e.Key == Key.Return)
            {
                NavigateToAddress();
                e.Handled = true; // Verhindert weitere Verarbeitung des Events
            }
        }

        private void NavigateToAddress()
        {
            if (webView != null && webView.CoreWebView2 != null && !string.IsNullOrWhiteSpace(addressBar.Text))
            {
                string urlInput = addressBar.Text.Trim();
                Uri uri;

                // Überprüfen, ob die Eingabe bereits ein Schema enthält
                if (!urlInput.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !urlInput.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    // Standardmäßig https hinzufügen, wenn kein Schema vorhanden ist
                    urlInput = "https://" + urlInput;
                }

                // Überprüfen, ob die korrigierte Adresse eine gültige URL ist
                if (Uri.TryCreate(urlInput, UriKind.Absolute, out uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                {
                    webView.CoreWebView2.Navigate(uri.ToString());
                }
                else
                {
                    MessageBox.Show("Bitte geben Sie eine gültige URL ein.", "Ungültige URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }


        void EnsureHttps(object sender, CoreWebView2NavigationStartingEventArgs args)
        {
            Console.WriteLine(args.Uri);
            String uri = args.Uri;
            if (!uri.StartsWith("https://"))
            {
                webView.CoreWebView2.ExecuteScriptAsync($"alert('{uri} is not safe, try an https link')");
                args.Cancel = true;
            }
        }
    }
}
