using System;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Maui.Controls;
using System.IO;


namespace ASKI_SKIElib;

public partial class Checker : ContentPage
{
    bool isCountingDown = true;
    bool istestingpage = false;
    public Checker()
    {
        InitializeComponent();

        if (istestingpage)
        {
            Test();
        }else CheckAppUpdate();
        
    }
    private async void devAcs(object sender, EventArgs e)
    {
        isCountingDown = false;
        Application.Current.MainPage = new LoginPage();

    }

    private async void Test()
    {
        try
        {
            isCountingDown = false;
            await Task.Delay(2000);
            StatusLabel.Text = "Navigating to Test a Page";

            Application.Current.MainPage = new ReviewersPage();
        }
        catch (Exception ex) { await DisplayAlert("", $"{ex}", "ok"); }


    }

    private string url;
    private static readonly string SupabaseApiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImVicHppaHNncmVsd294bGtrZmRuIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTMxMDk3MDEsImV4cCI6MjA2ODY4NTcwMX0.LGlZhrqPvC-UNLgvNSh2RtlT3ixePfQ1Luy1yt-PaSY";
    private static readonly string Durl = "https://ebpzihsgrelwoxlkkfdn.supabase.co/rest/v1/utils";

    private async Task CheckAppUpdate()
    {
        try
        {


            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("apikey", SupabaseApiKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseApiKey);

            url = $"{Durl}?id=eq.1&select=desc";
            var response = await client.GetStringAsync(url);

            var versions = JArray.Parse(response);
            if (versions.Count > 0)
            {
                string latestVersion = versions[0]["desc"]?.ToString();

                if (latestVersion == HamburgerMenuController.SessionState.currentVersion)
                {
                    StatusLabel.Text = "✅ App is up to date. ✅";
                    OverlayContainer.IsVisible = false;
                    await Task.Delay(1000);
                    StatusLabel.Text = "⚠️ Disclaimer: The app is in its early stage, be aware of bugs ⚠️";
                    await Task.Delay(3000);
                    await CheckServerStatusAsync();
                }
                else
                {
                    await Task.Delay(1000);
                    UpdateButton.IsVisible = false;
                    OverlayContainer.IsVisible = true;
                    OverlayLabel.Text = $" Current Version: {HamburgerMenuController.SessionState.currentVersion} ";
                    await Task.Delay(1800);
                    UpdateButton.IsVisible = true;
                    OverlayLabel.Text = $"⚠️ Update available: {latestVersion} ⚠️";
                    
                }
            }
            else
            {
                StatusLabel.Text = "⚠️ No version info received from server.";
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"⚠️ Error checking for updates: {ex.Message} ⚠️";
        }
    }

    private async void UpdateLink(object sender, EventArgs e)
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.Add("apikey", SupabaseApiKey);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseApiKey);

        url = $"{Durl}?id=eq.1&select=links";
        var response = await client.GetStringAsync(url);

        var link = JArray.Parse(response);
        string Upurl = "https://www.example.com";
        if (link.Count > 0)
        {
            string? dbUrl = link[0]["links"]?.ToString();
            if (!string.IsNullOrWhiteSpace(dbUrl))
            {
                Upurl = dbUrl;
            }
        }
        

        try
        {
            var sendlogs = new HamburgerMenuController();
            await sendlogs.InsertMessageAsync("Someone is updating their app..");
            Uri uri = new Uri(Upurl);
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            // Handle any errors that might occur when opening the browser
            await DisplayAlert("WebEngine", $"Error opening browser: {ex.Message}", "OK");
        }
    }



    public async Task CheckServerStatusAsync()
    {

        try
        {


            var client = new HttpClient();
            url = $"{Durl}?id=eq.443";
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Add headers to request
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseApiKey);
            request.Headers.Add("apikey", SupabaseApiKey);

            // Send request
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorText = "Please call the admins for help";//await response.Content.ReadAsStringAsync();
                StatusLabel.Text = $"❌ Failed to connect to the database. {response.StatusCode}: {errorText}";

            }
            else if (json.Contains("\"status\":true"))
            {
                StatusLabel.Text = "⚠️ Server is under maintenance. ⚠️";
            }
            else
            {
                StatusLabel.Text = "✅ Server is running normally. ✅";
                await Task.Delay(500);
                var window = Application.Current?.Windows.FirstOrDefault();
                if (window is not null)
                {
                    window.Page = new LoginPage();
                }
            }
        }
        catch (HttpRequestException)
        {
            StatusLabel.Text = "❌ No internet connection or server unreachable. App is Restarting ❌";
            for (int i = 10; i >= 1; i--)
            {
                if (!isCountingDown) break;
                StatusLabel.Text = $"❌ No internet connection and server unreachable. Auto Restart in {i} sec ❌";
                await Task.Yield();
                await Task.Delay(1000);

            }
            if (isCountingDown)
            {
                StatusLabel.Text = "Restarting...";
                await Task.Yield();
                await Task.Delay(2000);

                var window = Application.Current?.Windows.FirstOrDefault();
                if (window is not null)
                {
                    window.Page = new LoginPage();
                }

            }
        }

        catch (TaskCanceledException)
        {
            StatusLabel.Text = "❌ Request timed out. Please check your internet. ❌";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"⚠️ Error: {ex.Message} ⚠️";
        }

    }
}
