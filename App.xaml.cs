namespace ASKI_SKIElib
{
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JEaF1cWmhIfEx1RHxQdld5ZFRHallYTnNWUj0eQnxTdEBjUXxecXVVQGBdVEZ3XEleYw==");
            InitializeComponent();

        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new Checker());
        }
    }
}