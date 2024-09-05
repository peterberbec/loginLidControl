using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Input;
using System.Windows;
using System.Threading;

namespace loginLidControl
{
	public partial class App : Application
	{
		private static Mutex _mutex = null;

		public TaskbarIcon notifyIcon;

		protected override void OnStartup(StartupEventArgs e)
		{
			const string appName = "loginLidControl";
			bool createdNew;

			_mutex = new Mutex(true, appName, out createdNew);

			if (!createdNew)
			{
				//app is already running! Exiting the application
				MessageBox.Show("Application already running.", "mutex collision", MessageBoxButton.OK, MessageBoxImage.Error);
				Application.Current.Shutdown();
			}
			//create the notifyicon (it's a resource declared in App.xaml)
			notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
			notifyIcon.ToolTipText = "Waiting for lid event...";

			base.OnStartup(e);
		}

		protected override void OnExit(ExitEventArgs e)
		{
			notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
			base.OnExit(e);
		}
	}
	public class NotifyIconControl
	{
		public ICommand ExitCommand
		{
			get
			{
				return new DelegateCommand { CommandAction = () => System.Windows.Application.Current.Shutdown() };
			}
		}
	}
	public class DelegateCommand : ICommand
	{
		public Action CommandAction { get; set; }
		public Func<bool> CanExecuteFunc { get; set; }

		public void Execute(object parameter)
		{
			CommandAction();
		}

		public bool CanExecute(object parameter)
		{
			return CanExecuteFunc == null || CanExecuteFunc();
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}
	}
}
