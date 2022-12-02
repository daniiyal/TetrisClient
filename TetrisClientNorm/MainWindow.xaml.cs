using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TetrisClientNorm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly ImageSource[] tileImages;

        private readonly ImageSource[] blockImages = new ImageSource[]
        {
            new BitmapImage(new Uri("Assets/Block-Empty.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-I.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-J.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-L.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-O.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-S.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-T.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-Z.png", UriKind.Relative))
        };


        public static string FieldSize = "small";

        private Image[,] imageControls;

        private Client client;

        private ImageSource[] InitImages()
        {
            ImageSource[] tileImages = new ImageSource[113];
            tileImages[0] = new BitmapImage(new Uri("Assets/Tiles/Block-Empty.png", UriKind.Relative));

            int tile = 0;

            for (int i = 1; i < tileImages.Length; i++)
            {
                tileImages[i] = new BitmapImage(new Uri($"Assets/Tiles/tile ({i}).png", UriKind.Relative));
            }

            return tileImages;
        }

        public MainWindow()
        {
            InitializeComponent();
            client = new Client();
            tileImages = InitImages();
        }

        public async Task GameLoop()
        {
            while (true)
            {
                try
                {
                    if (!DrawGrid())
                        return;
                    DrawBlock();
                    DrawNextBlock();
                    await Task.Delay(500);
                }
                catch (Exception)
                {
                    throw new SocketException();
                }
            }
        }

        private Image[,] SetupGameCanvas(int rows, int cols)
        {
            Image[,] imageControls = new Image[rows, cols];
            int cellSize = 25;

            GameCanvas.Width = cellSize * cols;
            GameCanvas.Height = cellSize * rows;

            GameCanvas.Children.Clear();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Image imageControl = new Image()
                    {
                        Width = cellSize,
                        Height = cellSize
                    };

                    Canvas.SetTop(imageControl, i * cellSize);
                    Canvas.SetLeft(imageControl, j * cellSize);
                    GameCanvas.Children.Add(imageControl);
                    imageControls[i, j] = imageControl;
                }
            }

            return imageControls;
        }

        private async Task StartGame()
        {
            try
            {
                client.SendMessageAsync($"StartGame-{FieldSize}");

                var response = client.ReceiveResponse();

                if (response.Split('-')[0] == "GameStarted")
                {
                    int rows, cols;
                    Int32.TryParse(response.Split('-')[1], out rows);
                    Int32.TryParse(response.Split('-')[2], out cols);

                    imageControls = SetupGameCanvas(rows, cols);

                    GameScreen.Visibility = Visibility.Visible;

                    StartMenu.Visibility = Visibility.Hidden;
                    GameOverMenu.Visibility = Visibility.Hidden;

                    await GameLoop();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Нет соединения с сервером");

                GameScreen.Visibility = Visibility.Hidden;
                StartMenu.Visibility = Visibility.Visible;
            }

        }

        private bool DrawGrid()
        {
            try
            {
                client.SendMessageAsync("GetGrid" + '\n');
                var response = client.ReceiveResponse();

                if (response.Split('-')[0] == "GameOver")
                {
                    GameScreen.Visibility = Visibility.Hidden;
                    if (response.Split('-')[1] == "Record")
                    {
                        HighScore.Visibility = Visibility.Visible;
                        FinalHighScore.Text = ScoreTest.Text;
                    }
                    else
                    {
                        GameOverMenu.Visibility = Visibility.Visible;
                        FinalScore.Text = ScoreTest.Text;
                    }

                    return false;
                }

                var rows = response.Split("n");

                int id;
                for (int i = 0; i < rows.Length - 1; i++)
                {
                    var cols = rows[i].Split("-");
                    for (int j = 0; j < cols.Length - 1; j++)
                    {
                        Int32.TryParse(cols[j], out id);
                        imageControls[i, j].Source = tileImages[id];
                    }
                }

                int score;
                Int32.TryParse(rows[rows.Length - 1], out score);
                ScoreTest.Text = "Очки: " + score;
                return true;
            }
            catch (Exception)
            {
                throw new Exception();
            }

        }

        private void DrawNextBlock()
        {
            try
            {
                client.SendMessageAsync("GetNextBlock" + '\n');
                var nextBlock = client.ReceiveResponse();

                int blockId;
                Int32.TryParse(nextBlock, out blockId);
                NextImage.Source = blockImages[blockId];
            }
            catch (Exception)
            {
                throw new Exception();
            }

        }
        private void DrawBlock()
        {
            try
            {
                client.SendMessageAsync("GetBlock" + '\n');

                var blockPositions = client.ReceiveResponse().Split('n');


                for (int i = 0; i < blockPositions.Length - 1; i++)
                {
                    int row, col, id;

                    var block = blockPositions[i].Split('-');

                    Int32.TryParse(block[0], out row);
                    Int32.TryParse(block[1], out col);
                    Int32.TryParse(block[2], out id);

                    imageControls[row, col].Source = tileImages[id];
                }
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }

        private void GameCanvas_OnLoaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.KeyDown += KeyEvents;

        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            await StartGame();

        }

        private async void FindServer(object sender, RoutedEventArgs e)
        {
            var list = await client.FindServer();
            if (list != null)
            {
                ServerAddresses.Items.Add(new Address(list[0], list[1]));
            }
            
        }

        private void KeyEvents(object sender, KeyEventArgs e)
        {

            switch (e.Key)
            {
                case Key.Left:
                    client.SendMessageAsync("Left");
                    break;
                case Key.Right:
                    client.SendMessageAsync("Right");
                    break;
                case Key.Up:
                    client.SendMessageAsync("Rotate");
                    break;
                case Key.Space:
                    client.SendMessageAsync("Drop");
                    break;
            }
        }

        private void ServerAddresses_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectedAddress = (Address)ServerAddresses.SelectedItem;
                client.ServerEndPoint = new IPEndPoint(IPAddress.Parse(selectedAddress.IpAddress),
                    Convert.ToInt32(selectedAddress.Port));
                client.ConnectServer();
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось подключиться к серверу");
            }

        }

        private async void StartGameButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await StartGame();
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось подключиться к серверу.");

                GameScreen.Visibility = Visibility.Hidden;
                StartMenu.Visibility = Visibility.Visible;
            }

        }

        private void SettingButton_OnClick(object sender, RoutedEventArgs e)
        {
            StartMenu.Visibility = Visibility.Hidden;
            SettingMenu.Visibility = Visibility.Visible;
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            StartMenu.Visibility = Visibility.Visible;
            SettingMenu.Visibility = Visibility.Hidden;
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            RadioButton li = (sender as RadioButton);
            FieldSize = li.GroupName;
        }

        private void AddScoreName_Button_OnClick(object sender, RoutedEventArgs e)
        {
            var username = Username.Text;

            if (String.IsNullOrEmpty(username))
            {
                MessageBox.Show("Введите имя");
                return;
            }


            TextBlock text = new TextBlock();
            text.FontSize = 30;
            text.FontFamily = new FontFamily("Roboto");
            text.Margin = new Thickness(0, 0, 10, 0);
            text.TextAlignment = TextAlignment.Center;
            text.FontWeight = FontWeights.Bold;

            try
            {
                client.SendMessageAsync($"WriteRecord-{username}");

                var response = client.ReceiveResponse();

                UsernameInput.Visibility = Visibility.Hidden;

                if (response == "success")
                {
                    text.Text = "Вы в таблице";
                    text.Foreground = Brushes.Yellow;
                }
                TopScoreMessage.Children.Add(text);
            }
            catch (Exception)
            {
                text.Text = "Не удалось записать";
                text.Foreground = Brushes.LightCoral;
                TopScoreMessage.Children.Add(text);
            }


        }

        private void BackToMenuButton_OnClick(object sender, RoutedEventArgs e)
        {
            StartMenu.Visibility = Visibility.Visible;

            TopScorers.Visibility = Visibility.Hidden;
            SettingMenu.Visibility = Visibility.Hidden;
            HighScore.Visibility = Visibility.Hidden;
        }

        private void Username_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Username.Text = textBox.Text;

        }

        private void ToggleRecordsButton_OnChecked(object sender, RoutedEventArgs e)
        {
            RadioButton li = (sender as RadioButton);
            FieldSize = li.Name;
        }


        private void RecordsButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (client.ServerEndPoint == null)
                {
                    MessageBox.Show("Нет подключения к серверу. Проверьте настройки");
                    return;
                }

                client.SendMessageAsync($"GetRecords-{FieldSize}");

                TopScorers.Visibility = Visibility.Visible;
                StartMenu.Visibility = Visibility.Hidden;

                var response = client.ReceiveResponse();

                var scores = response.Split("\r");

                TopScore.Items.Clear();

                foreach (var score in scores)
                {
                    if (String.IsNullOrEmpty(score)) continue;

                    Score s = new Score(score.Split('-')[0],
                        score.Split('-')[1],
                        score.Split('-')[2],
                        score.Split('-')[3]);

                    TopScore.Items.Add(s);

                }
            }
            catch (Exception)
            {
                MessageBox.Show("Нет подключения к серверу. Проверьте настройки");
            }

        }
    }
}
