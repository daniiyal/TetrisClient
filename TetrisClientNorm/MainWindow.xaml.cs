using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TetrisClientNorm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly ImageSource[] tileImages = new ImageSource[]
        {
            new BitmapImage(new Uri("Assets/TileEmpty.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileCyan.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileOrange.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileRed.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileYellow.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileGreen.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TilePurple.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileBlue.png", UriKind.Relative))
        };

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

        public MainWindow()
        {
            InitializeComponent();
            imageControls = SetupGameCanvas();
            StartGame();
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
            client = new Client();
            client.ConnectServer("127.0.0.1", 333);

            client.SendMessage($"StartGame {Row} {Column}" + '\n');
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

                    Canvas.SetTop(imageControl, i*cellSize);
                    Canvas.SetLeft(imageControl, j*cellSize);
                    GameCanvas.Children.Add(imageControl);
                    imageControls[i, j] = imageControl;
                }
            }

            return imageControls;
        }


        private void DrawGrid()
        {
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Column; j++)
                {
                    client.SendMessage($"GetGrid {i} {j}" + '\n');
                    var response = client.ReceiveResponse();
                    if (response == "GameOver")
                    {
                        GameOverMenu.Visibility = Visibility.Visible;
                        return;
                    }
                    int id = Convert.ToInt32(response);
                    imageControls[i, j].Source = tileImages[id];
                }
            }
        }

        private void DrawBlock()
        {
            client.SendMessage($"GetBlock" + '\n');

            string blockPositions = client.ReceiveResponse();

            var id = Convert.ToInt32(blockPositions.Split('n').Last());
            for (int i = 0; i < blockPositions.Split('n').Length-1; i++)
            {
                var block = blockPositions.Split('n')[i];
                var row = Convert.ToInt32(block.Split('-')[0]);
                var col = Convert.ToInt32(block.Split('-')[1]);

                imageControls[row, col].Source = tileImages[id];
            }
        }



        private async void GameCanvas_OnLoaded(object sender, RoutedEventArgs e)
        {
            await GameLoop();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            client.SendMessage($"StartGame {Row} {Column}" + '\n');
            GameOverMenu.Visibility = Visibility.Hidden;
        }
    }
}
