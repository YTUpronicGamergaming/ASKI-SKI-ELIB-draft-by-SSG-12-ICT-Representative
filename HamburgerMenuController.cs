
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json; 
using STJ = System.Text.Json;
namespace ASKI_SKIElib 
{

    
    public class HamburgerMenuController
    {

        public static class SessionState
        {
            public static string currentVersion { get; set; } = "0.3.0-alpha";
            public static string _username { get; set; } = "Anonymous";
            public static string gradeSection { get; set; } = "notSaved";
            public static string reviewer { get; set; } = "6a6d0dc7-607d-4ad9-89e1-87b5db947681";
        }


        private readonly BoxView NavOverlay;
        private readonly Grid HamburgerNav;
        private bool isMenuVisible = false;
        private double lastScrollY = 0;

        public class StatusResponse
        {
            public bool Status { get; set; }
        }
        public HamburgerMenuController()
        {

        }
        private static readonly string SAK = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImVicHppaHNncmVsd294bGtrZmRuIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTMxMDk3MDEsImV4cCI6MjA2ODY4NTcwMX0.LGlZhrqPvC-UNLgvNSh2RtlT3ixePfQ1Luy1yt-PaSY";
        private static readonly string Durl = "https://ebpzihsgrelwoxlkkfdn.supabase.co/rest/v1/utils";

        public async Task SavegradeSection(string content) {

            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("apikey", SAK);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SAK);
            var url = $"{Durl}?id=eq.114124&select=status";
            var response = await client.GetStringAsync(url);

            var list = STJ.JsonSerializer.Deserialize<List<StatusResponse>>(response);

            bool status = list?.FirstOrDefault()?.Status ?? false;


            


                if (status) 
                {
                    SessionState.reviewer = "6a6d0dc7-607d-4ad9-89e1-87b5db947681";
                }else {
                    content.Split('@')[0].TrimStart('.');

            // Step 1: Create a dictionary of your mappings
            var pdfMap = new Dictionary<string, string>
            {
                { "12ict",  "uuid1" },
                { "12he",   "uuid2" },
                { "12abm", "uuid3" },
                { "12stem",  "uuid1" },
                { "12humms",   "uuid2" },
                { "11ict", "uuid3" },
                { "11he", "uuid3" },
                { "11abm", "uuid3" },
                { "11stem", "uuid3" },
                { "11humms", "uuid3" }
            };

                    // Step 2: Check if the dictionary has the content
                    if (pdfMap.TryGetValue(content, out var pdfId))
                    {
                        // Step 3: Replace content with the PDF id
                        SessionState.reviewer = pdfId;
                    }
               
                }
                

        }

        public async Task InsertMessageAsync(string content)
        {
            try
            {
                var httpClient = new HttpClient();

                string url = "https://ebpzihsgrelwoxlkkfdn.supabase.co/rest/v1/logs";
                string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImVicHppaHNncmVsd294bGtrZmRuIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTMxMDk3MDEsImV4cCI6MjA2ODY4NTcwMX0.LGlZhrqPvC-UNLgvNSh2RtlT3ixePfQ1Luy1yt-PaSY"; 
                // You can keep your key here in testing, but store it securely in production

                var data = new
                {
                    content = content
                };

                var json = JsonConvert.SerializeObject(data);
                var httpcontent = new StringContent(json, Encoding.UTF8, "application/json");

                // Set required headers
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
                //httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");
                httpcontent.Headers.Add("Prefer", "return=representation");

                // Send POST request
                var response = await httpClient.PostAsync(url, httpcontent);


                // Check if the response is successful
                //string responseText = await response.Content.ReadAsStringAsync();
                //await Application.Current.MainPage.DisplayAlert("Insert Failed", responseText, "OK");
                
        }
            catch (Exception ex)
            {
                Application.Current?.Windows[0]?.Page?.DisplayAlert("",$"🔥 Exception: {ex.Message}","OK");
            }
        }

        public HamburgerMenuController(BoxView navOverlay, Grid hamburgerNav, string username)
        {
            NavOverlay = navOverlay;
            HamburgerNav = hamburgerNav;
            SessionState._username = username;
        }

        public async void OnMenuButtonClicked(object? sender, EventArgs e)
        {
            isMenuVisible = !isMenuVisible;
            NavOverlay.IsVisible = isMenuVisible;

            HamburgerNav.IsVisible = true;
            await HamburgerNav.TranslateTo(isMenuVisible ? 0 : 200, 0, 200, Easing.CubicInOut);

            if (!isMenuVisible)
            {
                HamburgerNav.IsVisible = false;
                NavOverlay.IsVisible = false;
            }
        }

        public async void OnOverlayTapped(object? sender, EventArgs e)
        {
            isMenuVisible = false;

            await HamburgerNav.TranslateTo(200, 0, 200, Easing.CubicInOut);

            HamburgerNav.IsVisible = false;
            NavOverlay.IsVisible = false;
        }

        public async void OnScrolled(object? sender, ScrolledEventArgs e)
        {
            if (isMenuVisible && e.ScrollY > lastScrollY + 20)
            {
                await HamburgerNav.TranslateTo(200, 0, 150);
                HamburgerNav.IsVisible = false;
                isMenuVisible = false;
            }

            lastScrollY = e.ScrollY;
        }


        public async void NavigateHome(object? sender, EventArgs e)
        {
            HighlightButton(sender as Button);
            await Task.Delay(200);
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window is not null)
            {
                window.Page = new HomePage();
            }
        }

        public async void NavigateEbooks(object? sender, EventArgs e)
        { 
            HighlightButton(sender as Button);
            await Task.Delay(200);
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window is not null)
            {

                window.Page = new NavigationPage(new EbooksPage());

            }
        }

        public async void NavigateReviewers(object? sender, EventArgs e)
        {
            HighlightButton(sender as Button);
            await Task.Delay(200);
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window is not null)
            {
                window.Page = new ReviewersPage();
            }
        }

        public async void NavigatePernotes(object? sender, EventArgs e)
        {
                HighlightButton(sender as Button);
                await Task.Delay(200);
                var window = Application.Current?.Windows.FirstOrDefault();
                if (window is not null)
                {
                    //window.Page = new ReviewersPage(_username);
                } 
        }

        public async void NavigateAbout(object? sender, EventArgs e)
        {
            try
            {
                HighlightButton(sender as Button);
                await Task.Delay(200);
                var window = Application.Current?.Windows.FirstOrDefault();
                if (window is not null)
                {
                    //window.Page = new ReviewersPage(_username);
                }
            }
            catch (Exception ex)
            {
                Application.Current?.Windows[0]?.Page?.DisplayAlert("Insert Failed", ex.ToString(), "OK");
            }
            
        }

        public async void NavigateProfile(object? sender, EventArgs e)
        {
            try
            {
                HighlightButton(sender as Button);
                await Task.Delay(200);
                var window = Application.Current?.Windows.FirstOrDefault();
                if (window is not null)
                {
                    window.Page = new ProfilePage();
                }
            }
            catch (Exception ex)
            {
                Application.Current?.Windows[0]?.Page?.DisplayAlert("Insert Failed", ex.ToString(), "OK");
            }
            
        }

        private void HighlightButton(Button? selected)
        {
            foreach (var view in HamburgerNav.Children)
            {
                if (view is VerticalStackLayout stack)
                {
                    foreach (var child in stack.Children)
                    {
                        if (child is Button btn)
                            btn.BackgroundColor = (btn == selected) ? Colors.LightBlue : Colors.Transparent;
                    }
                }
            }
        }

    }
}

