using System.Net.Http.Headers;
using System.Net.Http.Json;
namespace ASKI_SKIElib;

public partial class LoginPage : ContentPage
{
    private static readonly string SupabaseUrl = "https://ebpzihsgrelwoxlkkfdn.supabase.co/rest/v1/users";
    private static readonly string SupabaseApiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImVicHppaHNncmVsd294bGtrZmRuIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTMxMDk3MDEsImV4cCI6MjA2ODY4NTcwMX0.LGlZhrqPvC-UNLgvNSh2RtlT3ixePfQ1Luy1yt-PaSY";

    public LoginPage()
    {
        InitializeComponent();
        
    }
    public class SupabaseUser
    {
        public string? username { get; set; }
        public string? GradeSection { get; set; }
        public string? password { get; set; }
    }








    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text;
        var password = PasswordEntry.Text;
        var input = username;
        var pass = password;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            StatusLabel.Text = "Please enter both fields.";
            return;
        }

        // Debugging credentials
        if (username.Equals("Dev@123") && password.Equals("$Dev@123"))
        {
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window is not null)
            {
                HamburgerMenuController.SessionState._username = "Developer Mode";
                window.Page = new HomePage();
            }
        }

        try
        {


            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", SupabaseApiKey);
            client.DefaultRequestHeaders.Add("apikey", SupabaseApiKey);

            var url = $"{SupabaseUrl}?password=eq.{pass}&select=username,GradeSection,password";

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                StatusLabel.Text = "Failed to connect to Supabase.";
                return;
            }


            var users = await response.Content.ReadFromJsonAsync<List<SupabaseUser>>();

            if (users != null)
            {
                var match = users.FirstOrDefault(u =>
                    $"{u.username}{u.GradeSection}" == input);

                if (match != null)
                {
                    StatusLabel.Text = "";
                    var sendlogs = new HamburgerMenuController();
                    await sendlogs.InsertMessageAsync($"User {username} logged in successfully.");
                    await sendlogs.SavegradeSection($"{match.GradeSection}");
                    var window = Application.Current?.Windows.FirstOrDefault();
                    await Task.Delay(500); // Optional delay for better UX
                    if (window is not null)
                    {
                        HamburgerMenuController.SessionState._username = match.username;
                        window.Page = new HomePage();
                    }
                }
                else
                {
                    var sendlogs = new HamburgerMenuController();
                    await sendlogs.InsertMessageAsync("Someone Failed to login with their credentials");
                    
                    
                    StatusLabel.TextColor = Colors.Red;
                    StatusLabel.Text = "❌ Invalid username or password.";
                    UsernameEntry.Text = string.Empty;
                    PasswordEntry.Text = string.Empty;
                }
            }


        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"⚠️ Error: {ex.Message}";
        }
    }

}
