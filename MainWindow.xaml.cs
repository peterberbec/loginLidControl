using Hardcodet.Wpf.TaskbarNotification;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Interop;
using System.ComponentModel;
using Microsoft.Win32;
using System.Windows;

namespace loginLidControl
{
	public partial class MainWindow : Window
	{
		[DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
		private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, Int32 Flags);

		internal struct POWERBROADCAST_SETTING
		{
			public Guid PowerSetting;
			public uint DataLength;
			public byte Data;
		}

		private Guid GUID_LIDSWITCH_STATE_CHANGE = new Guid(0xBA3E0F4D, 0xB817, 0x4094, 0xA2, 0xD1, 0xD5, 0x63, 0x79, 0xE6, 0xA0, 0xF3);
		private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
		private const int PBT_POWERSETTINGCHANGE = 0x8013;
		private const int WM_POWERBROADCAST = 0x0218;
		private const int Fingerprint = 1;
		private const int PIN = 0;
		private readonly string[] loginUUID = {"{D6886603-9D2F-4EB2-B667-1971041FA96B}",	"{BEC09223-B018-416D-A0AC-523971B639F5}" };
		private readonly string[] loginType = {"PIN",								"Fingerprint" };
		private bool? _previousLidState = null;
		private TaskbarIcon notifyIcon;
		private string currentUserSID;

		public MainWindow()
		{
			InitializeComponent();
			this.SourceInitialized += MainWindow_SourceInitialized;
		}

		// Minimize to system tray when application is minimized.
		protected override void OnStateChanged(EventArgs e)
		{
			if (WindowState == WindowState.Minimized) this.Hide();

			base.OnStateChanged(e);
		}

		// Minimize to system tray when application is closed.
		protected override void OnClosing(CancelEventArgs e)
		{
			// setting cancel to true will cancel the close request
			// so the application is not closed
			e.Cancel = true;
			this.Hide();
			base.OnClosing(e);
		}

		void MainWindow_SourceInitialized(object sender, EventArgs e)
		{
			RegisterForPowerNotifications();
			IntPtr hwnd = new WindowInteropHelper(this).Handle;
			HwndSource.FromHwnd(hwnd).AddHook(new HwndSourceHook(WndProc));

			this.WindowState = WindowState.Minimized;
			this.Hide();

			NTAccount tempAcc = new NTAccount(Environment.UserName);
			currentUserSID = ((SecurityIdentifier)tempAcc.Translate(typeof(SecurityIdentifier))).ToString();
			tempAcc = null;

			notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
			notifyIcon.ToolTipText = "Waiting for lid event...";
		}
		private void RegisterForPowerNotifications()
		{
			IntPtr handle = new WindowInteropHelper(this).Handle;
			IntPtr hLIDSWITCHSTATECHANGE = RegisterPowerSettingNotification(handle, ref GUID_LIDSWITCH_STATE_CHANGE, DEVICE_NOTIFY_WINDOW_HANDLE);
		}
		IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case WM_POWERBROADCAST:
					OnPowerBroadcast(wParam, lParam);
					break;
				default:
					break;
			}
			return IntPtr.Zero;
		}

		private void OnPowerBroadcast(IntPtr wParam, IntPtr lParam)
		{
			if ((int)wParam == PBT_POWERSETTINGCHANGE)
			{
				POWERBROADCAST_SETTING ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(lParam, typeof(POWERBROADCAST_SETTING));
				if (ps.PowerSetting == GUID_LIDSWITCH_STATE_CHANGE)
				{
					bool isLidOpen = ps.Data != 0;

					if (!isLidOpen == _previousLidState)
					{
						LidIs(isLidOpen?1:0);
					}
					_previousLidState = isLidOpen;
				}
			}
		}

		private void LidIs(int lidStatus)
		{
			notifyIcon.ToolTipText = "Lid open: " + loginType[lidStatus];
			using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Authentication\\LogonUI\\UserTile", true)) // Must dispose key or use "using" keyword
			{
				if (key != null)  // Must check for null key
				{
					key.SetValue(currentUserSID, loginUUID[lidStatus], RegistryValueKind.String);
				}
				key.Close();
			}

			return;
		}

	}
	
}

