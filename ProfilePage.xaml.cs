
namespace ASKI_SKIElib;

using Microsoft.Maui.Controls;
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            InitializeComponent();

              
            lblName.Text = HamburgerMenuController.SessionState._username;

            

            lblGradeSectionStrand.Text = HamburgerMenuController.SessionState.gradeSection;
        lblSubjects.Text = "Math, Physics, Computer Science";
        }
    }

