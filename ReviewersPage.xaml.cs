
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace ASKI_SKIElib;

public partial class ReviewersPage : ContentPage
{
    private string _revieweruuid = HamburgerMenuController.SessionState.reviewer;
    private string _username = HamburgerMenuController.SessionState._username;
    private HamburgerMenuController menuController;
    private const string SupabaseUrl = "https://ebpzihsgrelwoxlkkfdn.supabase.co/rest/v1/reviewers";
    private const string SupabaseApiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImVicHppaHNncmVsd294bGtrZmRuIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTMxMDk3MDEsImV4cCI6MjA2ODY4NTcwMX0.LGlZhrqPvC-UNLgvNSh2RtlT3ixePfQ1Luy1yt-PaSY";
    
    
    public class Reviewer
    {
        [JsonPropertyName("pdf_url")]
        public string Pdf_Url { get; set; } = "";
    }


    

    public ReviewersPage()
    {
        InitializeComponent();
        LoadReviewerPdfAsync(_revieweruuid);


        menuController = new HamburgerMenuController(NavOverlay, HamburgerNav, _username);

        // Event wiring

        MenuButton.Clicked += menuController.OnMenuButtonClicked;

        NavOverlay.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() => menuController.OnOverlayTapped(NavOverlay, EventArgs.Empty))
        });

        

        HomeButton.Clicked += menuController.NavigateHome;
        EbooksButton.Clicked += menuController.NavigateEbooks;
        ReviewersButton.Clicked += menuController.NavigateReviewers;
        PernotesButton.Clicked += menuController.NavigatePernotes;
        AboutButton.Clicked += menuController.NavigateAbout;
        ProfileButton.Clicked += menuController.NavigateProfile;
    }





    private Stream _pdfStream;
    public async Task LoadReviewerPdfAsync(string reviewerUuid)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseApiKey);
            client.DefaultRequestHeaders.Add("apikey", SupabaseApiKey);

            var url = $"{SupabaseUrl}?uuid=eq.{reviewerUuid}&select=pdf_url";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var list = System.Text.Json.JsonSerializer.Deserialize<List<Reviewer>>(json);
            string pdfUrl = list?.FirstOrDefault()?.Pdf_Url;

            if (string.IsNullOrEmpty(pdfUrl))
            {
                await DisplayAlert("Error", $"PDF URL not found: {pdfUrl} ,   json: {json} , list: {list} ", "OK");
                return;
            }

            string fileName = Path.GetFileName(new Uri(pdfUrl).LocalPath);
            string cachePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            if (!File.Exists(cachePath))
            {
                try
                {
                    // Only download if not cached
                    using var client2 = new HttpClient();
                    var pdfBytes = await client2.GetByteArrayAsync(pdfUrl);
                    await File.WriteAllBytesAsync(cachePath, pdfBytes);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to download PDF: {ex.Message}", "OK");
                    return;
                }
            }


            // Load into Syncfusion PDF Viewer
            
            _pdfStream = File.OpenRead(cachePath);
            PdfViewer.LoadDocument(_pdfStream);

            // Open the cached file
            //await Launcher.OpenAsync(new OpenFileRequest
            //{
            //    File = new ReadOnlyFile(cachePath),
            //    Title = "Open PDF"
            //});
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        //end of method
    }


    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Dispose the stream when leaving the page
        _pdfStream?.Dispose();
        _pdfStream = null;

    }


    //end of class
}