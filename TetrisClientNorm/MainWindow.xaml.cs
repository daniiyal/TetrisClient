using System;
using System.Linq;
using System.Net;
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


        public static int Column = 10;
        public static int Row = 20;

        private readonly Image[,] imageControls;

        private Client client;

        private ImageSource[] InitImages()
        {
            ImageSource[] tileImages =  new ImageSource[113];
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
            imageControls = SetupGameCanvas();
            tileImages = InitImages();
            //ShowStartMenu();
        }

        public async Task GameLoop()
        {
            while (true)
            {
                DrawGrid();
                DrawBlock();
                await Task.Delay(500);
            }
        }

        private void StartGame()
        {
            client.ConnectServer();

        }


        private Image[,] SetupGameCanvas()
        {
            Image[,] imageControls = new Image[Row, Column];
            int cellSize = 25;

            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Column; j++)
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


        private void DrawGrid()
        {
            client.SendMessage($"GetGrid" + '\n');
            var response = client.ReceiveResponse();

            if (response == "GameOver")
            {
                GameOverMenu.Visibility = Visibility.Visible;
                FinalScore.Text = ScoreTest.Text;
                return;
            }

            var rows = response.Split("n");

            for (int i = 0; i < rows.Length-1; i++)
            {
                var cols = rows[i].Split("-");
                for (int j = 0; j < cols.Length-1; j++)
                {
                    int id;
                    Int32.TryParse(cols[j], out id);
                    imageControls[i, j].Source = tileImages[id];
                }
            }

            ScoreTest.Text = "Очки: " + rows[rows.Length - 1];
        }

        private void DrawBlock()
        {
            client.SendMessage($"GetBlock" + '\n');

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

        private void GameCanvas_OnLoaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.KeyDown += KeyEvents;
  
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            client.SendMessage($"StartGame {Row} {Column}");
            StartMenu.Visibility = Visibility.Hidden;
            GameOverMenu.Visibility = Visibility.Hidden;
            await GameLoop();
        }

        private async void FindServer(object sender, RoutedEventArgs e)
        {
            var list = await client.FindServer();
            
            ServerAddresses.Items.Add(new Address(list[0], list[1]) );
        }

        private void KeyEvents(object sender, KeyEventArgs e)
        {

            switch (e.Key)
            {
                case Key.Left:
                    client.SendMessage("Left");
                    break;
                case Key.Right:
                    client.SendMessage("Right");
                    break;
                case Key.Up:
                    client.SendMessage("Rotate");
                    break;
                case Key.Space:
                    client.SendMessage("Drop");
                    break;
            }
        }

        private void ServerAddresses_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedAddress = (Address) ServerAddresses.SelectedItem;
            client.serverEndPoint = new IPEndPoint(IPAddress.Parse(selectedAddress.IpAddress), Convert.ToInt32(selectedAddress.Port));
        }

        private async void StartGameButton_OnClick(object sender, RoutedEventArgs e)
        {
            StartGame();
            client.SendMessage($"StartGame {Row} {Column}");
            StartMenu.Visibility = Visibility.Hidden;
            GameOverMenu.Visibility = Visibility.Hidden;
            await GameLoop();
        }
    }
}
