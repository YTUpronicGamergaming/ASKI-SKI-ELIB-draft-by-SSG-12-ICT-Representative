using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Maui.Controls.Shapes;


namespace ASKI_SKIElib;

public partial class HomePage : ContentPage
{
    public string _username = HamburgerMenuController.SessionState._username;
    private HamburgerMenuController menuController;
    private static readonly string SupabaseApiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImVicHppaHNncmVsd294bGtrZmRuIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTMxMDk3MDEsImV4cCI6MjA2ODY4NTcwMX0.LGlZhrqPvC-UNLgvNSh2RtlT3ixePfQ1Luy1yt-PaSY";
    private static readonly string Durl = "https://ebpzihsgrelwoxlkkfdn.supabase.co/rest/v1/announcements";
    private const string SupabaseUrl = "https://ebpzihsgrelwoxlkkfdn.supabase.co";


    public HomePage()
    {
        
        InitializeComponent();
        
        
        WelcomeLabel.Text = $"{_username}";



        menuController = new HamburgerMenuController(NavOverlay, HamburgerNav, _username);

        // Event wiring

        MenuButton.Clicked += menuController.OnMenuButtonClicked;

        NavOverlay.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() => menuController.OnOverlayTapped(NavOverlay, EventArgs.Empty))
        });

        MyScrollView.Scrolled += menuController.OnScrolled; // Make sure it's named correctly

        HomeButton.Clicked += menuController.NavigateHome;
        EbooksButton.Clicked += menuController.NavigateEbooks;
        ReviewersButton.Clicked += menuController.NavigateReviewers;
        PernotesButton.Clicked += menuController.NavigatePernotes;
        AboutButton.Clicked += menuController.NavigateAbout;
        ProfileButton.Clicked += menuController.NavigateProfile;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var announcements = await FetchAnnouncementsAsync();

        if (announcements.Count == 0)
        {
            AnnouncementsLayout.Children.Add(new Label
            {
                Text = "No announcements yet.",
                TextColor = Colors.Gray,
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Center
            });
        }

        foreach (var item in announcements)
        {
            var gmt8Time = item.created_at.ToUniversalTime().AddHours(8);
            string formatted = gmt8Time.ToString("MMMM d, yyyy h:mm tt");

           
            var contentLabel = new Label
            {
                Text = item.content,
                FontSize = 14,
                TextColor = Colors.DarkSlateGray,
                LineBreakMode = LineBreakMode.WordWrap
            };

            
            var contentContainer = new Grid
            {
                HeightRequest = 155,
                IsClippedToBounds = true,
                Children = { contentLabel }
            };
            var shadowOverlay = new BoxView
            {
                HeightRequest = 44,
                VerticalOptions = LayoutOptions.End,
                Background = new LinearGradientBrush
                {
                    GradientStops = new GradientStopCollection
            {
                new GradientStop(Colors.Transparent, 0f),
                new GradientStop(Colors.White, 1f)
            },
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1)
                }
            };
            contentContainer.Children.Add(shadowOverlay);
            var readMoreLabel = new Label
            {
                Text = "Read more...",
                TextColor = Colors.Gray,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                
            };

            // Add the label on top of the shadow
            contentContainer.Children.Add(readMoreLabel);



            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                if (contentContainer.HeightRequest == 155)
                {
                    contentContainer.HeightRequest = -1; // auto size
                    readMoreLabel.Text = "Hide";
                    shadowOverlay.IsVisible = false; // remove shadow when expanded
                }
                else
                {
                    contentContainer.HeightRequest = 155;
                    readMoreLabel.Text = "Read more...";
                    shadowOverlay.IsVisible = true; // show shadow again when collapsed
                }
            };
            contentContainer.GestureRecognizers.Add(tapGesture);

            var card = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                Padding = new Thickness(20),
                BackgroundColor = Colors.White,
                Content = new VerticalStackLayout
                {
                    Spacing = 30,
                    Children = {
                    new Label
                    {
                        Text = item.title,
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.Black
                    },
                    new Image
                    {
                        Source = $"{SupabaseUrl}{item.media}",
                        Aspect = Aspect.AspectFill,
                        MaximumHeightRequest = 200,
                        MaximumWidthRequest = 300,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    contentContainer, // expandable content
                    new Label
                    {
                        Text = $"{formatted} By - {item.announcer}",
                        FontSize = 12,
                        TextColor = Colors.Gray,
                        HorizontalOptions = LayoutOptions.End
                    },
                }
                }
            };

            AnnouncementsLayout.Children.Add(card);
        }
    }



    // Define the model class
    public class Announcement
    {

        public string? title { get; set; }
        public string? content { get; set; }
        public DateTime created_at { get; set; }
        public string? announcer { get; set; }
        public string? media { get; set; }

    }

    // Supabase REST API call
    public async Task<List<Announcement>> FetchAnnouncementsAsync()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("apikey", SupabaseApiKey);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {SupabaseApiKey}");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Calculate 7 days ago in ISO 8601 (UTC)
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7).ToString("o");

        // Supabase/PostgREST filter: created_at >= sevenDaysAgo
        var url = $"{Durl}?select=*&order=id.desc&created_at=gte.{sevenDaysAgo}";

        try
        {
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<Announcement>>(json);
            return result ?? new List<Announcement>();
        }
        catch
        {
            await DisplayAlert("Error", "Failed to load announcements.", "OK");
            return new List<Announcement>();
        }
    }

}




