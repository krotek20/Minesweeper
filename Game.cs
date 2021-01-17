using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace solb
{public partial class Game : Form
    {
        static MyButton myb;
        int ROWS = 20;
        int COLS = 35;
        static Timer timer;
        static int seconds = 0;
        public Game()
        {
            // Ghost button to be selected after every single mouse click
            Button select = new Button();
            select.Location = new Point(0, 0);
            select.Size = new Size(0, 0);
            this.Controls.Add(select);
            this.BackColor = Color.Gray;

            InitializeComponent();
            
            // Game board generator
            GenerateMineSweeper();

            // Setting up timer
            pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox1.BackgroundImage = Image.FromFile(Application.StartupPath + @"\clock.png");
            tb_time.BackColor = Color.RoyalBlue;
            timer = new Timer();
            timer.Interval = 1000;
            timer.Start();
            timer.Tick += new EventHandler(timer_Tick);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            // Time increases by one second whenever the game starts.
            seconds++;
            tb_time.Text = seconds.ToString("00");
        }

        private void GenerateMineSweeper()
        {
            // New MyButton instance
            myb = new MyButton(ROWS, COLS, this, lbl_bmbFound);

            // Random bombs generator
            GenerateBombsLocations();
        }

        // New button class
        // It inherits from Button superclass.
        public class MyButton : Button
        {
            private Label bmbFound;
            private Form game;
            public List<List<MyButton>> btns { get; set; }
            public int row { get; set; }
            public int col { get; set; }
            public bool bomb { get; set; }
            private bool isLeftClicked { get; set; }
            public bool isRightClicked { get; set; }
            private bool inQueue { get; set; }
            private bool FirstButton = true;
            private bool gameOver = false;
            private int contor = 700; // 700 cells
            
            // Default constructor 
            public MyButton() { }

            // Constructor overloading to generate the cells
            public MyButton(int rows, int cols, Form game, Label bmbFound)
            {
                this.bmbFound = bmbFound; // number of set flags
                this.game = game;

                btns = new List<List<MyButton>>();

                Point p = new Point(0, 0);
                for (int row = 0; row < rows; ++row)
                {
                    List<MyButton> newRow = new List<MyButton>();
                    btns.Add(newRow);
                    for (int col = 0; col < cols; ++col)
                    {
                        MyButton btn = new MyButton();
                        btn.Name = "btn" + row + col;
                        btn.Location = p;
                        btn.Size = new Size(25, 25);
                        btn.BackColor = Color.RoyalBlue;
                        btn.FlatStyle = FlatStyle.Popup;
                        btn.row = row;
                        btn.col = col;

                        // No button is clicked
                        btn.isLeftClicked = false;
                        btn.isRightClicked = false;

                        // Handling mouse click (left or right) events
                        btn.Click += new EventHandler(btn_Click);
                        btn.MouseUp += new MouseEventHandler(btn_MouseUp);

                        newRow.Add(btn);

                        // Adding button in form's control
                        game.Controls.Add(btn);
                        p.X += 25;
                    }
                    p.Y += 25;
                    p.X = 0;
                }
            }

            // Left click event handler
            public void btn_Click(object sender, EventArgs e)
            {
                MyButton btn = sender as MyButton;
                btn.isLeftClicked = true;
                if (btn.isRightClicked == false)
                {
                    // At the beggining, the first cell clicked won't have any bombs around (for a better start).
                    while ((btn.bomb == true || BombsAround(btn.row, btn.col) != 0) && FirstButton == true)
                    {
                        GenerateBombsLocations();
                    }
                    FirstButton = false;

                    // If the pressed button is a bomb, the game is over.
                    if (btn.bomb == true)
                    {
                        btn.Enabled = false;
                        btn.BackColor = Color.Red;
                        btn.BackgroundImageLayout = ImageLayout.Stretch;
                        btn.BackgroundImage = Image.FromFile(Application.StartupPath + @"\bomb2.png");
                        if (gameOver == false)
                        {
                            gameOver = true;
                            EndGame(btn.row, btn.col);
                        }
                    }
                    // Otherwise, compute the sum of nearby bombs and display it.
                    else
                    {
                        btn.Enabled = false;
                        int bombsAround = BombsAround(btn.row, btn.col);
                        btn.BackColor = Color.LightGray;
                        btn.Text = bombsAround.ToString();
                        btn.TextAlign = ContentAlignment.MiddleCenter;
                        btn.Font = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold);
                        if (bombsAround == 0) btn.Text = "";

                        // If the pressed cell has no bombs around it and there are more
                        // cells alike around, those will also be opened.
                        if (btn.bomb == false && bombsAround == 0)
                        {
                            NoneBombCellsAround(btn.row, btn.col);
                        }
                        contor--;
                        // If there are 99 cells left unpressed it means the player won.
                        if (contor == 99 && gameOver == false) WinGame();
                    }
                }

                // When the game is over all cells will be opened.
                // It means that every untriggered left click event will be triggered.
                else if (gameOver == true)
                {
                    if (btn.bomb == true)
                    {
                        btn.Enabled = false;
                        btn.BackColor = Color.LightGray;
                        btn.BackgroundImageLayout = ImageLayout.Stretch;
                        btn.BackgroundImage = Image.FromFile(Application.StartupPath + @"\bomb2.png");
                    }
                    else
                    {
                        btn.BackgroundImage = null;
                        btn.Enabled = false;
                        btn.Text = BombsAround(btn.row, btn.col).ToString();
                        btn.BackColor = Color.Red;
                        btn.TextAlign = ContentAlignment.MiddleCenter;
                        btn.Font = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold);
                    }
                }

                // Focusing the ghost button
                game.Controls[0].Focus();
            }

            // Right click event handler
            public void btn_MouseUp(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                {
                    MyButton btn = sender as MyButton;
                    BombFound(btn.row, btn.col, bmbFound);
                    if (btn.isRightClicked == false)
                    {
                        btn.isRightClicked = true;
                        btn.BackgroundImageLayout = ImageLayout.Stretch;
                        btn.BackgroundImage = Image.FromFile(Application.StartupPath + @"\flag.png");
                    }
                    else
                    {
                        btn.isRightClicked = false;
                        btn.BackgroundImage = null; // flag wipe
                    }
                }
            }

            int[] dx = new int[] { 1, 1, 1, -1, -1, -1, 0, 0 };
            int[] dy = new int[] { 1, 0, -1, 1, 0, -1, 1, -1 };
            
            // Compute the number of bombs around a cell
            public int BombsAround(int row, int col)
            {
                int bombs = 0;
                for (int dir = 0; dir < 8; ++dir)
                {
                    int nextRow = row + dy[dir];
                    int nextCol = col + dx[dir];
                    if ((nextRow >= 0 && nextRow <= 19) && (nextCol >= 0 && nextCol <= 34)) // If its neighbor is on board
                        if (myb.btns[nextRow][nextCol].bomb == true) bombs++;
                }
                return bombs;
            }

            // Searching (Lee's algorithm) cells with none bombs around nearby the currently pressed cell.
            public void NoneBombCellsAround(int row, int col)
            {
                int nextRow = row, nextCol = col;

                Queue<MyButton> queue = new Queue<MyButton>();

                myb.btns[row][col].inQueue = true;
                queue.Enqueue(myb.btns[row][col]);

                while (queue.Count != 0)
                {
                    queue.Dequeue();
                    row = nextRow; col = nextCol;
                    for (int dir = 0; dir < 8; ++dir)
                    {
                        nextRow = row + dy[dir];
                        nextCol = col + dx[dir];
                        if (OK(nextRow, nextCol) == true)
                        {
                            myb.btns[nextRow][nextCol].inQueue = true;
                            queue.Enqueue(myb.btns[nextRow][nextCol]);
                            myb.btns[nextRow][nextCol].PerformClick();
                            OpenAround(nextRow, nextCol);
                        }
                    }
                }
            }

            public bool OK(int row, int col)
            {
                if ((row < 0 || row > 19) || (col < 0 || col > 34)) return false; // Out of board
                if (BombsAround(row, col) != 0) return false;
                if (myb.btns[row][col].bomb == true) return false;
                if (!isOpenedAround(row, col)) return false;
                if (myb.btns[row][col].inQueue == true) return false; // Daca elementul este deja in coada

                return true;
            }

            // If cells around are already opened, there is no need to push it in the queue.
            public bool isOpenedAround(int row, int col)
            {
                for (int dir = 0; dir < 8; ++dir)
                {
                    int nextRow = row + dy[dir];
                    int nextCol = col + dx[dir];
                    if ((nextRow >= 0 && nextRow <= 19) 
                        && (nextCol >= 0 && nextCol <= 34) 
                        && myb.btns[nextRow][nextCol].isLeftClicked == true) 
                            return true;
                }
                return false;
            }

            // Opens every cell around
            public void OpenAround(int row, int col)
            {
                for (int dir = 0; dir < 8; ++dir)
                {
                    int nextRow = row + dy[dir];
                    int nextCol = col + dx[dir];
                    if ((nextRow >= 0 && nextRow <= 19) && (nextCol >= 0 && nextCol <= 34)) 
                        myb.btns[nextRow][nextCol].PerformClick();
                }
            }
        }

        // There are 20 * 35 = 700 cells, so there will be 99 bombs placed.
        // Initially, all the bombs are erased, then the new ones will be added.
        public static void GenerateBombsLocations()
        {
            int row, col;
            for (row = 0; row < 20; ++row)
            {
                for (col = 0; col < 35; ++col)
                {
                    myb.btns[row][col].bomb = false;
                }
            }

            Random r = new Random();
            for (int i = 1; i <= 99; ++i)
            {
                row = r.Next(0, 20);
                col = r.Next(0, 35);
                while (myb.btns[row][col].bomb == true)
                {
                    row = r.Next(0, 20);
                    col = r.Next(0, 35);
                }
                myb.btns[row][col].bomb = true;
            }
        }

        // Number of flags placed.
        public static void BombFound(int row, int col, Label bmbFound)
        {
            if (myb.btns[row][col].isRightClicked == false)
            {
                int bombsNumber = Convert.ToInt32(bmbFound.Text);
                bombsNumber++;
                bmbFound.Text = bombsNumber.ToString("00");
            }
            else
            {
                int bombsNumber = Convert.ToInt32(bmbFound.Text);
                bombsNumber--;
                bmbFound.Text = bombsNumber.ToString("00");
            }
        }
    
        // Lost
        public static void EndGame(int btnRow, int btnCol)
        {
            timer.Stop();

            for (int row = 0; row < 20; ++row)
            {
                for (int col = 0; col < 35; ++col)
                {
                    myb.btns[row][col].PerformClick();
                }
            }
            if (MessageBox.Show("Ai pierdut! Doresti sa incepi un joc nou?", "Infrangere!", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                Application.Restart();
                Environment.Exit(0);
            }
        }

        // Victory
        public static void WinGame()
        {
            if (MessageBox.Show("Ai castigat! Doresti sa-ti salvezi scorul?", "Victorie!", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                SaveScore ss = new SaveScore(seconds);
                ss.Show();
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }

}
