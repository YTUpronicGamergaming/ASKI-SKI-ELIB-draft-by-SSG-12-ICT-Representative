using Microsoft.Maui.Controls.Shapes;
namespace ASKI_SKIElib;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.IO;
public partial class EbooksPage : ContentPage
{
    private CancellationTokenSource _downloadCts = new(); 
    
    private string _cachedFilePath = "";

    public  string _username = HamburgerMenuController.SessionState._username;
    private HamburgerMenuController menuController;
    private static readonly string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImVicHppaHNncmVsd294bGtrZmRuIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTMxMDk3MDEsImV4cCI6MjA2ODY4NTcwMX0.LGlZhrqPvC-UNLgvNSh2RtlT3ixePfQ1Luy1yt-PaSY";

    private const string SupabaseUrl = "https://ebpzihsgrelwoxlkkfdn.supabase.co";
    private readonly HttpClient httpClient = new();


    public EbooksPage()
    {
        InitializeComponent();
        LoadBooks();
        




        var navBarGrid = new Grid
        {
            Padding = 16,
            BackgroundColor = Color.FromArgb("#082567"),
            ColumnDefinitions =
        {
            new ColumnDefinition { Width = GridLength.Star },
            new ColumnDefinition { Width = GridLength.Auto }
        },
            RowSpacing = 0,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,


        };

        var MenuButton = new Button
        {
            Text = "☰",
            BackgroundColor = Colors.Transparent,
            TextColor = Colors.Black,
            FontSize = 24,
            Padding = new Thickness(8, 0),
            HorizontalOptions = LayoutOptions.End
        };


        navBarGrid.Add(MenuButton);
        Grid.SetColumn(MenuButton, 1);

        var titleLabel = new Label
        {
            //Text = "📚 eBook Viewer",
            Text = $"ASKI-SKI Elibrary  {_username}",
            FontSize = 21,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#008080"),
            HorizontalOptions = LayoutOptions.Start

        };
        navBarGrid.Add(titleLabel);
        Grid.SetColumn(titleLabel, 0);



        // Set the custom nav bar
        NavigationPage.SetTitleView(this, navBarGrid);


        NavigationPage.SetHasNavigationBar(this, true); // or false
        NavigationPage.SetHasBackButton(this, false); // or false
        Shell.SetNavBarIsVisible(this, false);






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

    private async void LoadBooks(string search = "")
    {
        BooksLayout.Children.Clear();

        try
        {
            var tableUrl = $"{SupabaseUrl}/rest/v1/ebooks?select=title,cover_url,file_path";
            if (!string.IsNullOrWhiteSpace(search))
            {
                tableUrl += $"&title=ilike.*{search}*";
            }

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
            httpClient.DefaultRequestHeaders.Add("apikey", SupabaseKey);

            var response = await httpClient.GetAsync(tableUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var books = JArray.Parse(json);

            if (!books.Any()) 
            {
                BooksLayout.Children.Add(new Label { Text = "No books found." });
                return;
            }

            foreach (var book in books)
            {
                string title = book["title"]?.ToString() ?? "Untitled";
                string coverUrl = book["cover_url"]?.ToString() ?? "";
                string filePath = book["file_path"]?.ToString() ?? "";

                string bookcover = $"{SupabaseUrl}/storage/v1/object/public/{coverUrl}";

                var cover = new Image
                {
                    Source = ImageSource.FromUri(new Uri(bookcover)),
                    WidthRequest = 120,
                    HeightRequest = 180,
                    Aspect = Aspect.AspectFit
                };

                var titleLabel = new Label
                {
                    Text = title,
                    TextColor = Colors.Black,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center
                };

                var openButton = new Button
                {
                    Text = "Open PDF",

                    BackgroundColor = Colors.RoyalBlue,
                    TextColor = Colors.White
                };

                openButton.Clicked += async (s, e) =>
                {
                    string fullPdfUrl = $"{SupabaseUrl}/storage/v1/object/public/{filePath}";
                    string fileName = Path.GetFileName(new Uri(fullPdfUrl).LocalPath);

                    try
                    {
                        var sendlogs = new HamburgerMenuController();
                        await sendlogs.InsertMessageAsync($"User {_username} Is Downloading {title}.");
                        ShowOverlay("Downloading PDF...");

                        await DownloadAndCachePdfAsync(fullPdfUrl, fileName);

                        ShowOverlay("Opening PDF...", false);

                        string cachePath = Path.Combine(FileSystem.CacheDirectory, fileName);
                        await Task.Delay(2000);
                        await Launcher.OpenAsync(new OpenFileRequest
                        {
                            File = new ReadOnlyFile(cachePath),
                            Title = "Open PDF"
                        });

                        await Task.Delay(1000);
                    }
                    catch (OperationCanceledException)
                    {
                        await DisplayAlert("Canceled", "Download was canceled.", "OK");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", $"Failed to open PDF: {ex.Message}", "OK");
                    }
                    finally
                    {
                        HideOverlay();
                    }
                };

                var bookCard = new Border
                {
                    StrokeShape = new RoundRectangle { CornerRadius = 12 },
                    BackgroundColor = Colors.White,
                    Padding = 12,
                    Margin = new Thickness(10),

                    Content = new VerticalStackLayout
                    {
                        Children = { titleLabel, cover, openButton }
                    }
                };

                BooksLayout.Children.Add(bookCard);
            }

        }


        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }

        
    }

    private void OnSearchClicked(object sender, EventArgs e)
    {
        LoadBooks(SearchEntry.Text);
    }

    private async Task DownloadAndCachePdfAsync(string pdfUrl, string fileName)
    {
        try
        {
            var cachePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            _cachedFilePath = cachePath;

            if (File.Exists(cachePath))
                return;

            // Show overlay
            OverlayContainer.IsVisible = true;
            OverlayLabel.Text = "Downloading...";
            DownloadProgressBar.Progress = 0;

            using var httpClient = new HttpClient();

            using var response = await httpClient.GetAsync(pdfUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = totalBytes != -1;

            using var inputStream = await response.Content.ReadAsStreamAsync();
            using var outputStream = File.OpenWrite(cachePath);

            var buffer = new byte[8192];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await inputStream.ReadAsync(buffer)) > 0)
            {
                await outputStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalRead += bytesRead;

                if (canReportProgress)
                {
                    double progress = (double)totalRead / totalBytes;
                    MainThread.BeginInvokeOnMainThread(() => DownloadProgressBar.Progress = progress);
                }

                // Optional delay to simulate slow connection (for demo/testing):
                // await Task.Delay(50);
            }

            OverlayLabel.Text = "Opening...";
            await Task.Delay(1000); // short delay for smoother UX
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to download PDF: {ex.Message}", "OK");
        }
        finally
        {
            OverlayContainer.IsVisible = false;
        }
    }


    private void OnOverlayCancelClicked(object sender, EventArgs e)
    {
        _downloadCts.Cancel();
        HideOverlay();
    }

    private void ShowOverlay(string message, bool showProgress = true)
    {
        OverlayLabel.Text = message;
        DownloadProgressBar.Progress = 0;
        DownloadProgressBar.IsVisible = showProgress;
        OverlayContainer.IsVisible = true;
    }

    private void HideOverlay()
    {
        OverlayContainer.IsVisible = false;
        DownloadProgressBar.Progress = 0;
        _downloadCts = new CancellationTokenSource(); // Reset
    }

}
