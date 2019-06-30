using Regexer.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using Prism.Mvvm;

namespace Regexer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

            containerRegistry.RegisterForNavigation<MainPage>();
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();
            ViewModelLocationProvider.Register<MainPage, ViewModels.MainPageViewModel>();
        }
    }
}
