using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ConwaysSoldiers
{
	public partial class MainWindow : Window
	{
		bool isSettingUp = true;
		readonly int GRID_WIDTH = 12;
		int currLevel = 1;
		Dictionary<int, CellButton> cells = new Dictionary<int, CellButton>();

		RoutedEventHandler moveClick;
		CellButton? selectedCell;

		public MainWindow()
		{
			InitializeComponent();
			moveClick = new RoutedEventHandler(MoveClick);
			for (int i = 0; i < GRID_WIDTH; i++)
			{
				GameGrid.ColumnDefinitions.Add(new ColumnDefinition());
				GameGrid.RowDefinitions.Add(new RowDefinition());
			}
			LoadGrid();
			SetupGrid(currLevel);
		}

		public void LoadGrid()
		{
			for (int  i=0; i<GRID_WIDTH*GRID_WIDTH; i++)
			{
				CellButton newButton = new CellButton(i, i % GRID_WIDTH, i / GRID_WIDTH);
				cells.Add(i, newButton);
				GameGrid.Children.Add(newButton);
			}
		}

		public void ToggleSetup(object sender, RoutedEventArgs e)
		{
			if (isSettingUp)
			{
				foreach (CellButton button in cells.Values)
				{
					button.Click -= button.setupClick;
					button.Click += moveClick;
				}
				isSettingUp = false;
				SetupButton.Content = "Enter Setup Phase";
				FlavorText.Text = "Jumping Phase: Get to the top row! \n\nPlease note: Going back to the setup phase will deactivate any squares in the unplaceable zone... \n\n" +
					"No Cheating!";
				return;
			} 
			else
			{
				foreach (CellButton button in cells.Values)
				{
					if (button.y < currLevel)
					{
						button.SetOn(false);
					}
					button.Click -= moveClick;
					button.Click += button.setupClick;
				}
				isSettingUp = true;
				SetupButton.Content = "End Setup Phase";
				FlavorText.Text = "Setup Phase: Place as many squares as you need to get to the top row.";
				return;
			}
		}

		public void MoveClick(object sender, RoutedEventArgs e)
		{
			CellButton realSender = (CellButton)sender;
			if (selectedCell == null && realSender.isOn)
			{
				selectedCell = realSender;
				selectedCell.Background = new SolidColorBrush(Colors.DarkBlue);
				return;
			} 
			else if (selectedCell == null)
			{
				return;
			} 
			else if (IsValidMove(realSender)) {
				CellButton middleCell = cells[(selectedCell.id + realSender.id) / 2];
				selectedCell.SetOn(false);
				middleCell.SetOn(false);
				realSender.SetOn(true);
				selectedCell.SetColor();
				selectedCell = null;
				if (realSender.y == 0)
				{
					FlavorText.Text = "You did it! Click above to go to the next level!";
					NextLevelButton.IsEnabled = true;
				}
				return;
			}
			else
			{
				selectedCell.SetColor();
				selectedCell = null;
				return;
			}
		}

		public void GetNextLevel(object sender, RoutedEventArgs e)
		{
			currLevel += 1;
			SetupButton.Content = "End Setup Phase";
			FlavorText.Text = "Setup Phase: Place as many squares as you need to get to the top row.";
			NextLevelButton.IsEnabled = false;
			ToggleSetup(sender, e);
			SetupGrid(currLevel);
		}

		public bool IsValidMove(CellButton moveCell)
		{
			// If we haven't selected a cell, then there are no valid moves!  We shouldn't ever be calling this if there's no selectedCell, but it doesn't hurt to check
			if (selectedCell == null) { return false; }
			// A move is valid if it "lines up correctly" (they are in the same row/column with exactly one space between) and the space between is on.  Each of the following just checks 
			// if that is the cases
			if ((moveCell.x == selectedCell.x) && (moveCell.y - selectedCell.y == 2))
			{
				return !moveCell.isOn && cells[(selectedCell.id + moveCell.id)/2].isOn;
			}
			if ((moveCell.x == selectedCell.x) && (moveCell.y - selectedCell.y == -2))
			{
				return !moveCell.isOn && cells[(selectedCell.id + moveCell.id) / 2].isOn;
			}
			if ((moveCell.y == selectedCell.y) && (moveCell.x - selectedCell.x == 2))
			{
				return !moveCell.isOn && cells[(selectedCell.id + moveCell.id) / 2].isOn;
			}
			if ((moveCell.y == selectedCell.y) && (moveCell.x - selectedCell.x == -2))
			{
				return !moveCell.isOn && cells[(selectedCell.id + moveCell.id) / 2].isOn;
			}
			return false;
		}

		public void SetupGrid(int level = 1)
		{
			LevelLabel.Content = "Level " + level.ToString();
			foreach (int buttonId in cells.Keys)
			{
				cells[buttonId].isOn = false;
				if (buttonId < level * GameGrid.ColumnDefinitions.Count)
				{
					cells[buttonId].SetPlaceable(false);
				} else
				{
					cells[buttonId].SetPlaceable(true);
				}
			}
		}
	}

	public partial class CellButton : Button
	{
		public bool isOn;
		public bool isPlaceable;
		public int x;
		public int y;
		public int id;

		public RoutedEventHandler setupClick;
		public CellButton(int id, int x, int y, bool isOn = false, bool isPlaceable = true)
		{
			// Setting starting values for fields
			this.id = id;
			this.x = x;
			this.y = y;
			this.isOn = isOn;
			this.isPlaceable = isPlaceable;
			this.setupClick = new RoutedEventHandler(SetupClickHandler);
			// Other initialization
			SetColor();
			Grid.SetRow(this, y);
			Grid.SetColumn(this,x);
			this.Click += this.setupClick;
		}

		public void SetColor()
		{
			if (isOn)
			{
				Background = new SolidColorBrush(Colors.Blue);
				return;
			} 
			else if (isPlaceable)
			{
				Background = new SolidColorBrush(Colors.LightBlue);
				return;
			} 
			else
			{
				Background = new SolidColorBrush(Colors.WhiteSmoke);
				return;
			}
		}

		public void SetupClickHandler(object sender, RoutedEventArgs e)
		{
			if (isPlaceable)
			{
				SetOn(!isOn);
			}
		}

		public void SetOn(bool newVal)
		{
			isOn = newVal;
			SetColor();
		}

		public void SetPlaceable(bool placeable)
		{
			isPlaceable = placeable;
			SetColor();
		}
	}
}