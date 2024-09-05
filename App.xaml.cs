using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Input;
using System.Windows;

namespace loginLidControl
{
	public partial class App : Application
	{
		public TaskbarIcon notifyIcon;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			//create the notifyicon (it's a resource declared in App.xaml)
			notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
			notifyIcon.ToolTipText = "Waiting for lid event...";
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
