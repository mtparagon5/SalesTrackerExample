using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;

namespace SalesTracker
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			#region calendar selected date

			MainCalendar.SelectedDate = todaysDate;

			#endregion

			#region create directories

			CreateDirectoriesExist();

			#endregion
		}

		#region binding objects

		/// <summary>
		/// Data object used for binding
		/// </summary>
		public class SalesData
		{
			public DateTime Date { get; set; }
			public string DateString { get; set; }
			public DayOfWeek DayOfWeek { get; set; }
			public decimal Sales { get; set; }
			public decimal Promo { get; set; }
			public decimal Returns { get; set; }
			public decimal Cash { get; set; }
		}

		#endregion

		#region directories

		/// <summary>
		/// Definitions of Directories that will be created
		/// </summary>
		public string InitialDirectoryPath
		{
			get { return "C:\\Program Files\\SalesTracker_Example"; }
		}
		public string DataDirectoryPath
		{
			get { return InitialDirectoryPath + "\\___Data___"; }
		}
		public string BackupDirectoryPath
		{
			get { return DataDirectoryPath + "\\___Backup___"; }
		}
		public string DayOfWeekDataDirectoryPath
		{
			get { return string.Format("{0}\\__DayOfWeekData__", DataDirectoryPath); }
		}
		public string WeekDataDirectoryPath
		{
			get { return string.Format("{0}\\__WeekData__", DataDirectoryPath); }
		}
		public string MonthDataDirectoryPath
		{
			get { return string.Format("{0}\\__MonthData__", DataDirectoryPath); }
		}
		public string YearDataDirectoryPath
		{
			get { return string.Format("{0}\\__YearData__", DataDirectoryPath); }
		}
		public string MasterDataDirectoryPath
		{
			get { return string.Format("{0}\\__MasterData__", DataDirectoryPath); }
		}
		public string logPath
		{
			get { return BackupDirectoryPath + "\\__Logs__.csv"; }
		}

		#endregion

		#region paths

		/// <summary>
		/// Variables used to define file paths upon saving data
		/// </summary>
		private string dayOfWeekDataPath = "";
		private string weekDataPath = "";
		private string monthDataPath = "";
		private string yearDataPath = "";
		private string masterDataPath = "";
		private string backupDataPath = "";

		#endregion

		#region variables

		/// <summary>
		/// Various variables used
		/// </summary>
		// Day0 was the first day of sales for this example
		private DateTime _day0 { get { return new DateTime(2018, 4, 23); } }
		public DateTime Day0 { get { return _day0; } }
		public DateTime todaysDate { get { return DateTime.Now.ToLocalTime().Date; } }
		public int CurrentWeekNumberIndex { get; set; }
		public DayOfWeek DayOfWeek { get { return DateTime.Now.ToLocalTime().DayOfWeek; } }
		public int Day { get { return DateTime.Now.ToLocalTime().Day; } }
		public int Month { get { return DateTime.Now.ToLocalTime().Month; } }
		public int Year { get { return DateTime.Now.ToLocalTime().Year; } }
		public int Hour { get { return DateTime.Now.ToLocalTime().Hour; } }
		public int Minute { get { return DateTime.Now.ToLocalTime().Minute; } }
		public string DateStamp { get { return string.Format("{0}/{1}/{2},\t{3},\t{4}:{5}\n", Day, Month, Year, DayOfWeek, Hour, Minute); } }
		public DayOfWeek SelectedDayOfWeek { get; set; }
		public DateTime SelectedDate { get; set; }
		
		#endregion

		#region events

		/// <summary>
		/// Calendar single date selection event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SelectedDatesChanged(object sender, RoutedEventArgs e)
		{
			SelectedDate = MainCalendar.SelectedDate.Value.Date;
			SelectedDayOfWeek = MainCalendar.SelectedDate.Value.DayOfWeek;
			DateSelected_TB.Text = SelectedDate.ToString("MM/dd/yyyy");
			DaySelected_TB.Text = SelectedDayOfWeek.ToString();
		}

		/// <summary>
		/// DataGridView row removal button click event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RemoveRowData_Clicked(object sender, RoutedEventArgs e)
		{
			Sales_DG.Items.RemoveAt(Sales_DG.SelectedIndex);
		}

		/// <summary>
		/// Click event to add user defined sales data to the datagrid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AddSalesToDG_Clicked(object sender, RoutedEventArgs e)
		{
			decimal salesValue;
			SalesData sd = new SalesData();
			sd.Date = SelectedDate;
			sd.DateString = SelectedDate.ToString("MM/dd/yyyy");
			sd.DayOfWeek = SelectedDayOfWeek;
			if (Decimal.TryParse(Sales_Input.Text.ToString(), out salesValue))
			{
				sd.Sales = Convert.ToDecimal(Sales_Input.Text);
			}
			else { MessageBox.Show("The Sales value you have listed is incorrectly formatted."); }
			if (Decimal.TryParse(Promo_Input.Text.ToString(), out salesValue))
			{
				sd.Promo = Convert.ToDecimal(Promo_Input.Text);
			}
			else { MessageBox.Show("The Promo value you have listed is incorrectly formatted."); }
			if (Decimal.TryParse(Returns_Input.Text.ToString(), out salesValue))
			{
				sd.Returns = Convert.ToDecimal(Returns_Input.Text);
			}
			else { MessageBox.Show("The Returns value you have listed is incorrectly formatted."); }
			if (Decimal.TryParse(Cash_Input.Text.ToString(), out salesValue))
			{
				sd.Cash = Convert.ToDecimal(Cash_Input.Text);
			}
			else { MessageBox.Show("The Cash value you have listed is incorrectly formatted."); }
			Sales_DG.Items.Add(sd);
		}

		/// <summary>
		/// Click event to save data to various files for organization
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SaveSalesData_Clicked(object sender, RoutedEventArgs e)
		{
			var result = MessageBox.Show("This will add your data to the database. Are you sure you would like to continue?", 
											"Save Files", 
											MessageBoxButton.OKCancel, 
											MessageBoxImage.Question);
			if (result == MessageBoxResult.OK)
			{
				StringBuilder sb = new StringBuilder();
				
				foreach (var sD in Sales_DG.Items)
				{
					sb.Clear();
					var salesData = sD as SalesData;
					var timespan = salesData.Date.Subtract(Day0).TotalDays;
					CurrentWeekNumberIndex = (int)Math.Floor(timespan / 7);

					var monthName = "";
					if (salesData.Date.Month == 1) { monthName = "January"; }
					else if (salesData.Date.Month == 2) { monthName = "February"; }
					else if (salesData.Date.Month == 3) { monthName = "March"; }
					else if (salesData.Date.Month == 4) { monthName = "April"; }
					else if (salesData.Date.Month == 5) { monthName = "May"; }
					else if (salesData.Date.Month == 6) { monthName = "June"; }
					else if (salesData.Date.Month == 7) { monthName = "July"; }
					else if (salesData.Date.Month == 8) { monthName = "August"; }
					else if (salesData.Date.Month == 9) { monthName = "September"; }
					else if (salesData.Date.Month == 10) { monthName = "October"; }
					else if (salesData.Date.Month == 11) { monthName = "November"; }
					else if (salesData.Date.Month == 12) { monthName = "December"; }


					dayOfWeekDataPath = string.Format("{0}\\{1}.csv", DayOfWeekDataDirectoryPath, salesData.DayOfWeek);
					weekDataPath = string.Format("{0}\\Week{1}.csv", WeekDataDirectoryPath, CurrentWeekNumberIndex+1);
					monthDataPath = string.Format("{0}\\{1}.csv", MonthDataDirectoryPath, monthName);
					yearDataPath = string.Format("{0}\\{1}.csv", YearDataDirectoryPath, salesData.Date.Year);
					masterDataPath = string.Format("{0}\\MasterData.csv", MasterDataDirectoryPath);
					backupDataPath = string.Format("{0}\\DataBackup_DoNotEdit.csv", BackupDirectoryPath);

					string dataString = string.Format("{0},\t{1},\t{2},\t{3},\t{4},\t{5}",
															salesData.Date.ToString("MM/dd/yyyy"), salesData.DayOfWeek,
															salesData.Sales, salesData.Promo,
															salesData.Returns, salesData.Cash);
					sb.AppendLine(dataString);
					File.AppendAllText(dayOfWeekDataPath, sb.ToString());
					File.AppendAllText(weekDataPath, sb.ToString());
					File.AppendAllText(monthDataPath, sb.ToString());
					File.AppendAllText(yearDataPath, sb.ToString());
					File.AppendAllText(masterDataPath, sb.ToString());
					File.AppendAllText(backupDataPath, sb.ToString());
				}
				LogUser(logPath);
			}
		}

		#endregion

		#region helper methods

		/// <summary>
		/// Method to log use
		/// </summary>
		/// <param name="path"></param>
		public void LogUser(string path)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(DateStamp);
			File.AppendAllText(path, sb.ToString());
		}

		/// <summary>
		/// Method used to ensure all the necessary 
		/// </summary>
		public void CreateDirectoriesExist()
		{
			if (!Directory.Exists(InitialDirectoryPath))
			{
				Directory.CreateDirectory(InitialDirectoryPath);
			}
			if (!Directory.Exists(DataDirectoryPath))
			{
				Directory.CreateDirectory(DataDirectoryPath);
			}
			if (!Directory.Exists(BackupDirectoryPath))
			{
				Directory.CreateDirectory(BackupDirectoryPath);
			}
			if (!Directory.Exists(DayOfWeekDataDirectoryPath))
			{
				Directory.CreateDirectory(DayOfWeekDataDirectoryPath);
			}
			if (!Directory.Exists(WeekDataDirectoryPath))
			{
				Directory.CreateDirectory(WeekDataDirectoryPath);
			}
			if (!Directory.Exists(MonthDataDirectoryPath))
			{
				Directory.CreateDirectory(MonthDataDirectoryPath);
			}
			if (!Directory.Exists(YearDataDirectoryPath))
			{
				Directory.CreateDirectory(YearDataDirectoryPath);
			}
			if (!Directory.Exists(MasterDataDirectoryPath))
			{
				Directory.CreateDirectory(MasterDataDirectoryPath);
			}
		}

		#endregion
	}
}
