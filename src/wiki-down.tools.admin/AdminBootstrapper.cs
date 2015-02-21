using System.Windows;
using Caliburn.Micro;
using wiki_down.tools.admin.ViewModels;

namespace wiki_down.tools.admin
{
    public class AdminBootstrapper : BootstrapperBase
    {
        public AdminBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}