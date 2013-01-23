using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using SharedClasses;
using System.IO;

namespace OfficeWeekPlanner
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const string cThisAppName = "OfficeWeekPlanner";
		private const string cCurrentProjectName = "TestProject";//Just hardcoded for now

		private ObservableCollection<OfficeWeekItem> listOfWeekItems = new ObservableCollection<OfficeWeekItem>();

		public MainWindow()
		{
			InitializeComponent();

			this.Height = 800;
			this.Width = 1200;

			CommandBinding pasteCmdBinding = new CommandBinding(
				ApplicationCommands.Paste,
				OnPaste,
				OnCanExecutePaste);
			richTextBox1.CommandBindings.Add(pasteCmdBinding);
		}

		private static void OnPaste(object sender, ExecutedRoutedEventArgs e)
		{
			RichTextBox richTextBox = sender as RichTextBox;
			if (richTextBox == null) return;

			if (Clipboard.ContainsImage())
			{
				string tempFileName = Path.GetTempFileName();
				ImagesInterop.SaveClipboardImageToFile(tempFileName);
				richTextBox.Document.Blocks.Add(new Paragraph(new InlineUIContainer(new Image { Source = ImagesInterop.GetImageFromFile(tempFileName) })));
			}

			/*var dataObj = (IDataObject)Clipboard.GetDataObject();
			if (dataObj == null) { return; }

			if (Clipboard.ContainsImage())
			{
				var img = new Image();
				var imgSrc = Clipboard.GetImage();
				img.Source = imgSrc;
				var buc = new BlockUIContainer(img);
				Figure fig = new Figure(buc, richTextBox.Selection.Start);
				fig.Width = new FigureLength(imgSrc.Width);
				fig.Height = new FigureLength(imgSrc.Height);
				richTextBox.InvalidateVisual();
			}

			e.Handled = true;*/
		}


		private static void OnCanExecutePaste(object target, CanExecuteRoutedEventArgs args)
		{
			if (Clipboard.ContainsImage())
				args.CanExecute = true;//Tells it we want do handle the OnPaste ourselves
			//args.CanExecute = true;
			//RichTextBox richTextBox = target as RichTextBox;
			//args.CanExecute = false;
			//if (richTextBox != null)
			//{
			//    args.CanExecute = richTextBox.IsEnabled;
			//}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			listboxWeekList.ItemsSource = listOfWeekItems;
			listOfWeekItems.Add(new OfficeWeekItem(
				new DateTime(2013, 01, 07),//03),
				5,//2,
				new ObservableCollection<OfficeWeekItem.OfficeCompletedTask>()
				{//Completed items
					new OfficeWeekItem.OfficeCompletedTask(DateTime.Now, "Hallo", TimeSpan.Zero, null,
						new Run("How are you?"), new InlineUIContainer(new Image() { Source = this.Icon })),
					new OfficeWeekItem.OfficeCompletedTask("Epanet results hyperlinks", new DateTime(2013, 01, 07), TimeSpan.FromDays(2), "Click on Node/Pump/Link/FCV/etc inside Epanet status box", 
						new Paragraph(new Run("Hallo sexy")), new BlockUIContainer(new Image() { Source = this.Icon })),
					new OfficeWeekItem.OfficeCompletedTask("Win7 JumpLists", new DateTime(2013, 01, 09, 7, 30, 0), TimeSpan.FromHours(4), "Integrated 'Request new feature' and 'Report a bug' into Windows 7 taskbar JumpList"),
					new OfficeWeekItem.OfficeCompletedTask("Linked MediaWiki", new DateTime(2013, 01, 09, 11, 30, 0), TimeSpan.FromHours(4), "Linked the relevant applications' wiki page into the Help menu, loads the page in embedded browser with a button to be able to load in external browser")
				},
				new ObservableCollection<OfficeWeekItem.OfficeTodoTask>()
				{//Todo items
					new OfficeWeekItem.OfficeTodoTask("Licensing", "Implement licensing into Wadiso6, have a look at the Wadiso5 code. Use ifdef to exclude in ReportingExe."),
					new OfficeWeekItem.OfficeTodoTask("Update all DTM", "When running [DTM->Update Topology for all] it should show notification that Z values where updated, click to update elevation."),
					new OfficeWeekItem.OfficeTodoTask("Finish merge/split MP", "Compete merge/split for Master Plan."),
					new OfficeWeekItem.OfficeTodoTask("Queries", "All queries should be possible as per Wadiso5, especially multiple text per link."),
					new OfficeWeekItem.OfficeTodoTask("Alternative merge/split MP", "Code the alternative method for merge/split for Master Plan, this is not coded in Wadiso5 yet. It shold make use of System_Type and Future_System_Type instead of the 'Numbering.txt' file.")
				}));
		}

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			FocusManager.SetFocusedElement(this, null);
		}

		private void todoTaskItemMainBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			//listboxSelectedFullDescription.ItemsSource = WPFHelper.GetFromObjectSender<OfficeWeekItem.OfficeTodoTask>(sender).DescriptionChuncks;
			richTextBox1.DataContext = null;
			richTextBox1.Document.Blocks.Clear();
			WPFHelper.DoActionIfObtainedItemFromObjectSender<OfficeWeekItem.OfficeTodoTask>(
				sender,
				(tmpTask) =>
				{
					richTextBox1.DataContext = tmpTask;
					var itemDescriptionBlocks = tmpTask.DescriptionBlocks;
					if (itemDescriptionBlocks == null) return;
					foreach (var block in itemDescriptionBlocks)
						richTextBox1.Document.Blocks.Add(block);
				});
		}

		private void richTextBox1_TextChanged(object sender, TextChangedEventArgs e)
		{
			var tmpTask = richTextBox1.DataContext as OfficeWeekItem.OfficeTodoTask;
			if (tmpTask == null) return;
			tmpTask.DescriptionBlocks = new ObservableCollection<Block>(richTextBox1.Document.Blocks);
			//var blocks = richTextBox1.Document.Blocks;
			//string tmpstr = "";
			//int cnt = 1;
			//foreach (var bl in blocks)
			//    if (bl is Paragraph)
			//        tmpstr += (!string.IsNullOrEmpty(tmpstr) ? "," : "") + (++cnt).ToString() + "=" + (bl as Paragraph).Inlines.Count;
			//this.Title = tmpstr;//blocks.Count.ToString() + (blocks.Count == 0 && blocks.FirstBlock is Paragraph ? "" : (blocks.FirstBlock as Paragraph).Inlines.cou
			//if (blocks == null)
			//    return;
		}

		private void saveMenuItem_Click(object sender, RoutedEventArgs e)
		{
			foreach (var weekItem in listOfWeekItems)
			{
				weekItem.SaveItemToFolder(cThisAppName, cCurrentProjectName);
			}
		}
	}

	public class OfficeWeekItem
	{
		public DateTime WeekStart { get; private set; }
		public int NumberOfDaysInThisWeek { get; private set; }
		public ObservableCollection<OfficeCompletedTask> CompletedItems { get; private set; }
		public ObservableCollection<OfficeTodoTask> TodoItems { get; private set; }

		public OfficeWeekItem(DateTime WeekStart, int NumberOfDaysInThisWeek, ObservableCollection<OfficeCompletedTask> CompletedItems, ObservableCollection<OfficeTodoTask> TodoItems)
		{
			int implementBelow_StartOfWeek;//See below commented method 'StartOfWeek' inside 'static class Extensions', implement it
			this.WeekStart = WeekStart;
			this.NumberOfDaysInThisWeek = NumberOfDaysInThisWeek;
			this.CompletedItems = CompletedItems;
			this.TodoItems = TodoItems;
		}

		/*public static class Extensions
		{
			public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
			{
				int diff = dt.DayOfWeek - startOfWeek;
				if (diff < 0)
				{
					diff += 7;
				}
				return dt.AddDays(-1 * diff).Date;
			}
		}*/

		/*public abstract class DescriptionChunk
		{
			public virtual Brush BorderColor { get { return Brushes.Transparent; } }
		}

		public class TextDescriptionChunk : DescriptionChunk
		{
			public string Text { get; set; }
			public TextDescriptionChunk(string Text)
			{
				this.Text = Text;
			}
		}
		public class ImageDescriptionChunk : DescriptionChunk
		{
			public ImageSource Image { get; set; }
			public ImageDescriptionChunk(ImageSource Image)
			{
				this.Image = Image;
			}

			public override Brush BorderColor { get { return Brushes.Gray; } }
		}*/

		public class OfficeTodoTask
		{
			public string Name { get; private set; }
			public string TaskSummary { get; private set; }
			public ObservableCollection<Block> DescriptionBlocks { get; set; }
			public TimeSpan EstimatedDuration { get; private set; }

			public OfficeTodoTask(string Name, string TaskSummary = null, params Block[] DescriptionBlocks)
			{
				this.Name = Name;
				this.TaskSummary = TaskSummary ?? Name;//Use the name as long description if no TaskSummary specified
				this.DescriptionBlocks = new ObservableCollection<Block>(DescriptionBlocks);
			}

			public OfficeTodoTask(string Name, TimeSpan EstimatedDuration, string TaskSummary = null, params Block[] DescriptionBlocks)
				: this(Name, TaskSummary, DescriptionBlocks)
			{
				this.EstimatedDuration = EstimatedDuration;
			}

			/*public string GetValidFolderNameFromName()
			{
				return this.Name;//This should eventually return a VALID foldername, not just this.Name
			}*/

			public override string ToString()
			{
				return Name;
			}
		}

		public class OfficeCompletedTask : OfficeTodoTask
		{
			public DateTime TaskStartTime { get; private set; }
			public TimeSpan TaskDuration { get; private set; }

			public OfficeCompletedTask(string Name, DateTime TaskStartTime, TimeSpan TaskDuration, string TaskSummary = null, params Block[] DescriptionBlocks)
				: base(Name, TaskSummary, DescriptionBlocks)
			{
				this.TaskStartTime = TaskStartTime;
				this.TaskDuration = TaskDuration;
			}

			//We just swop the Name and TaskStartTime parameter otherwise the params Block[] and params Inline[] would be ambiguous
			public OfficeCompletedTask(DateTime TaskStartTime, string Name, TimeSpan TaskDuration, string TaskSummary = null, params Inline[] DescriptionInlines)
				: base(Name, TaskSummary)
			{
				this.TaskStartTime = TaskStartTime;
				this.TaskDuration = TaskDuration;
				if (DescriptionInlines.Length > 0)
				{
					var tmpPar = new Paragraph();
					foreach (var inl in DescriptionInlines)
						tmpPar.Inlines.Add(inl);
					this.DescriptionBlocks.Add(tmpPar);
				}
			}
		}

		private const string cWeekStartDateFoldernameDateFormat = "yyyy-MM-dd";
		private string GetWeekStartDateFolderName()
		{
			return this.WeekStart.ToString(cWeekStartDateFoldernameDateFormat);
		}

		private string GetFilepathForThisDetailsFilename(string applicationName, string projectName)
		{
			return SettingsInterop.GetFullFilePathInLocalAppdata(cDetailsFileName, applicationName, projectName + "\\" + GetWeekStartDateFolderName());
		}

		private void SaveDetailsToTextFile(string applicationName, string projectName)
		{
			string textFilePath = GetFilepathForThisDetailsFilename(applicationName, projectName);
			File.WriteAllLines(textFilePath, new string[]
			{
				"WeekStart" + this.WeekStart.ToString("yyyy-MM-dd"),
				"NumberOfDaysInThisWeek=" + this.NumberOfDaysInThisWeek.ToString(),
			});
		}

		private static bool SaveBlockToFolder(string folderFullPath, Block block)
		{
			/*
			Inline can contain:
			Run
			InlineUIContainer\Image

			Block can contain:
			Paragraph
			BlockUIContainer\Image
			*/

			/*
			File formats
			1.*, 2.*, N.* means Inline1, Inline2, InlineN...
			a.* means this block (only one item in this folder)*/

			try
			{
				if (!Directory.Exists(folderFullPath))
					Directory.CreateDirectory(folderFullPath);

				var tmpPar = block as Paragraph;
				var tmpBlockUIcont = block as BlockUIContainer;

				string error = null;

				if (tmpPar != null)
				{
					var paragraphInlinesArray = tmpPar.Inlines.ToArray();
					for (int i = 0; i < paragraphInlinesArray.Length; i++)
					{
						Inline inline = paragraphInlinesArray[i];
						Run tmpRun = inline as Run;
						InlineUIContainer tmpInlineUIcont = inline as InlineUIContainer;

						if (tmpRun != null)
							File.WriteAllText(Path.Combine(folderFullPath, i.ToString() + ".txt"), tmpRun.Text);
						else if (tmpInlineUIcont != null)
						{
							var tmpImage = tmpInlineUIcont.Child as Image;
							if (tmpImage != null)
							{
								var tmpBitmapSource = tmpImage.Source as BitmapSource;
								if (tmpBitmapSource != null)
									ImagesInterop.SaveBitmapSourceToFile(tmpBitmapSource, Path.Combine(folderFullPath, i.ToString() + ".bmp"));
								else
									error = string.Format("Cannot obtain bitmap from Block.Paragraph.Inlines[{0}].Image.Source", i);
							}
							else
								error = string.Format("Cannot obtain bitmap from Block.Paragraph.Inlines[{0}].Image", i);
						}
						else
						{
							error = "Unknown Inline type, cannot save Inline type = " + inline.GetType().Name;
							break;
						}
					}
				}
				else if (tmpBlockUIcont != null)
				{
					var tmpImage = tmpBlockUIcont.Child as Image;
					if (tmpImage != null)
					{
						var tmpBitmapSource = tmpImage.Source as BitmapSource;
						if (tmpBitmapSource != null)
							ImagesInterop.SaveBitmapSourceToFile(tmpBitmapSource, Path.Combine(folderFullPath, "a.bmp"));
						else
							error = "Cannot obtain bitmap from Block.BlockUIContainer.Image.Source";
					}
					else
						error = "Cannot obtain Image from Block.BlockUIContainer.Image";
				}
				else
					error = "Unknown Block type, cannot save Block type = " + block.GetType().Name;

				if (!string.IsNullOrEmpty(error))
				{
					UserMessages.ShowErrorMessage(error);
					return false;
				}
				else
					return true;
			}
			catch (Exception exc)
			{
				UserMessages.ShowErrorMessage("Error while trying to save Block content to folder: " + exc.Message);
				return false;
			}
		}

		private const string cCompletedFolderName = "Completed";
		private const string cTodoFolderName = "Todo";
		private const string cDetailsFileName = "Details.fjset";
		public void SaveItemToFolder(string applicationName, string projectName)
		{
			bool failedToSaveSomething = false;

			//Format example: ...\MyProject\2013-01-10\Completed\Details.fjset
			//Format example: ...\MyProject\2013-01-10\Completed\1\"DescriptionBlock 1 contents"

			//FN = FolderName
			string weekStartdateFN = GetWeekStartDateFolderName();
			this.SaveDetailsToTextFile(applicationName, projectName);

			for (int i = 0; i < this.CompletedItems.Count; i++)
			{
				string completedItemFolderName = i.ToString();//this.CompletedItems[i].GetValidFolderNameFromName();

				OfficeCompletedTask completedItem = this.CompletedItems[i];
				string folderpathForCompletedItem = SettingsInterop.GetFullFolderPathInLocalAppdata(
					string.Format(@"{0}\{1}", weekStartdateFN, cCompletedFolderName, completedItemFolderName), applicationName, projectName);

				ObservableCollection<Block> blockList1 = completedItem.DescriptionBlocks;
				if (blockList1 == null || blockList1.Count == 0)
					continue;

				for (int j = 0; j < completedItem.DescriptionBlocks.Count; j++)
				{
					string descriptionBlockFolderName = j.ToString();
					string relativePath 
							= string.Format(@"{0}\{1}\{2}\{3}", weekStartdateFN, cCompletedFolderName, completedItemFolderName, descriptionBlockFolderName);
					string curDescriptionBlockFolderPath
							= SettingsInterop.GetFullFolderPathInLocalAppdata(relativePath, applicationName, projectName);
					if (!SaveBlockToFolder(curDescriptionBlockFolderPath, completedItem.DescriptionBlocks[j]))
						failedToSaveSomething = true;
					if (failedToSaveSomething)
						break;
				}
				if (failedToSaveSomething)
					break;
			}

			for (int i = 0; i < this.TodoItems.Count; i++)
			{
				ObservableCollection<Block> blockList2 = this.TodoItems[i].DescriptionBlocks;
				if (blockList2 == null || blockList2.Count == 0)
					continue;

				string todoItemFolderName = i.ToString();// this.TodoItems[i].GetValidFolderNameFromName();
				for (int j = 0; j < this.TodoItems[i].DescriptionBlocks.Count; j++)
				{
					string descriptionBlockFolderName = i.ToString();
					string relativePath
							= string.Format(@"{0}\{1}\{2}\{3}", weekStartdateFN, cTodoFolderName, todoItemFolderName, descriptionBlockFolderName);
					string curDescriptionBlockFolderPath
							= SettingsInterop.GetFullFolderPathInLocalAppdata(relativePath, applicationName, projectName);
					if (!SaveBlockToFolder(curDescriptionBlockFolderPath, this.TodoItems[i].DescriptionBlocks[j]))
						failedToSaveSomething = true;
					if (failedToSaveSomething)
						break;
				}
				if (failedToSaveSomething)
					break;
			}

			if (failedToSaveSomething)
			{
				//Could not save something (maybe unsupported Block type), now delete complete folder
				string projectRootFolder
					= Path.GetDirectoryName(SettingsInterop.GetFullFolderPathInLocalAppdata("subfolder", applicationName, projectName));
				try
				{
					Directory.Delete(projectRootFolder, true);
				}
				catch (Exception exc)
				{
					UserMessages.ShowErrorMessage("Did not save successfully, also unable to delete unfinished project folder '"
						+ projectRootFolder + "', error message:"
						+ Environment.NewLine
						+ exc.Message);
				}
			}
		}
	}
}
