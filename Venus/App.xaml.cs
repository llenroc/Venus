using System;
using System.Windows;
using Microsoft.Practices.ServiceLocation;
using Venus.Infrastructure;
using Venus.Infrastructure.Events;
using Venus.ViewModel;

namespace Venus
{
    public partial class App : Application
    {
        public static IServiceLocator IoC => ServiceLocator.Current;
        public static RemoteDataManager DataManager { get; private set; }
        public static TimerManager TimerManager { get; private set; }
        public static Portfolio Portfolio { get; private set; }
        public static ConfigurationManager ConfigurationManager { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += UnhandledExceptionHandler;

            Portfolio = new Portfolio(IoC.GetInstance<IEventPublisher>());

            TimerManager = new TimerManager(IoC.GetInstance<IEventPublisher>());

            ConfigurationManager = new ConfigurationManager(
                IoC.GetInstance<IEventPublisher>(), 
                IoC.GetInstance<IFileService>());

            ConfigurationManager.Init();

            DataManager = new RemoteDataManager(IoC.GetInstance<IEventPublisher>());

            DataManager.Init();
            DataManager.UpdateAllAccounts();

            var window = new MainWindow
            {
                DataContext = ServiceLocator.Current.GetInstance<MainViewModel>()
            };
            Current.MainWindow = window;
            Current.MainWindow.Show();

            base.OnStartup(e);
        }

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            // TODO: Create better error message display/handle system. 

            MessageBox.Show(
                Current.MainWindow, 
                $"Venus has encountered an unhandled exception and will now close. " +
                $"\n\n{args.ExceptionObject.GetType()}");
            Application.Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            //TODO: Add cleanup of all application resources

            base.OnExit(e);
        }
    }
}
