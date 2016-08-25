using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Timers;

namespace ASCII_game_1
{
    class Program
    {
        static int TimeCount = 0;
        static Player player = new Player();
        static bool jump = false;
        static Timer jumpTimer = new Timer(150);
        static char[,] levelArray = new char[50, 50];
        public static int TestCountInt { get; private set; }

        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            var path = "levels.txt";
            var selValue = "start";
            while (true)
            {
                switch (selValue)
                {
                    case "start":
                        selValue = startScreen();
                        break;
                    case "play":
                        Console.Clear();
                        printLevel(readLevel(path));
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(0, 21);
                        Console.Write("Score: 0");
                        playGame(player);
                        break;
                    case "edit":
                        Console.Clear();
                        Console.WriteLine("Edit mode");
                        Console.ReadLine();
                        break;
                    default:
                        break;
                };
            }
        }

        private static void playGame(Player pl)
        {
            Timer jumpAndFallTimer = new Timer(100);
            jumpAndFallTimer.Start();
            jumpAndFallTimer.Elapsed += new ElapsedEventHandler(UpDownRoutine);
            jumpTimer.Elapsed += new ElapsedEventHandler(jumpOff);
            player.posX = 7;
            player.posY = 15;
            player.walk = true;
            Console.SetCursorPosition(player.posX, player.posY);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("X");
            bool playing = true;


            while (playing)
            {
                readKeys();
            }
        }

        private static void readKeys()
        {
            ConsoleKeyInfo info = Console.ReadKey();
            if (!jump)
            {
                if (info.Key == ConsoleKey.Spacebar && player.walk) { jump = true; jumpTimer.Start(); readKeys(); }
            }

            if (info.Key == ConsoleKey.LeftArrow && player.walk)
            {
                if (!jump)
                {
                    if (movePlayer(-1, 0)) { player.posX--; player.jumpDir = Dir.left; }
                }
                else      //jump left
                {
                    player.jumpDir = Dir.left; debug("left");
                }
            }

            if (info.Key == ConsoleKey.RightArrow && player.walk)
            {
                if (!jump)
                {
                    if (movePlayer(1, 0)) { player.posX++; player.jumpDir = Dir.right;  }
                }
                else      //jump right
                {
                    player.jumpDir = Dir.right;
                }
            }

            if (jump) { player.jumpDir = Dir.middle; }  //jump up


            if (info.Key == ConsoleKey.DownArrow)
            {
                if (climbPlayer(0, 1)) { player.posY++; }
            }
            if (info.Key == ConsoleKey.UpArrow)
            {
                if (climbPlayer(0, -1)) { player.posY--; }
            }
        }

        private static void UpDownRoutine(object sender, ElapsedEventArgs e)  // kind of interupt routine
        {
            TimeCount++;
            if (TimeCount > 3) { displayCounter(); TestCountInt++; TimeCount = 0; }
            if (player.falling) { fallingDown();}
            if (jump && !player.falling) { jumpingUp(player.jumpDir); }  
        }

        private static void displayCounter()
        {
            debug(TestCountInt.ToString());
        }

        private static void jumpingUp(Dir d)
        {
            if(d == Dir.left)
            {
                    if (movePlayer(-1, -1)) { player.posY--; player.posX--; }
                    if (movePlayer(-2, -1)) { player.posY--; player.posX -= 2; }
                    else if(movePlayer(-1, -1)) {player.posY--; player.posX --;}
            }
            if (d == Dir.middle)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (movePlayer(0, -1)) { player.posY--; }
                }
            }
            if(d == Dir.right)
            {
                if (movePlayer(1, -1)) { player.posY--; player.posX++; }
                if (movePlayer(2, -1)) { player.posY--; player.posX += 2; }
                else if (movePlayer(1, -1)) { player.posY--; player.posX++; }
            }
        }

        

        private static void jumpOff(object sender, ElapsedEventArgs e)
        {
            jump = false;
            jumpTimer.Stop();
        }

        private static bool movePlayer(int mX, int mY)
        {
            int newX = player.posX + mX;
            int newY = player.posY + mY;
            if (levelArray[newX, newY] == '0' || levelArray[newX, newY] == '2')  // 0 = empty tile  2 = ladder
            {
                moveGraphicRoutine(newX, newY);
                if (levelArray[newX, (newY + 1)] == '0')
                {
                    player.falling = true;
                    player.walk = true;  // if set to 'false' player cant move when falling down
                }
                Console.CursorVisible = false;
                return true;
            }
            else if(levelArray[newX, newY] == '8')    // 8 = gold star
            {
                moveGraphicRoutine(newX, newY);
                levelArray[newX, newY] = '0';
                player.score++;
                if (levelArray[newX, (newY + 1)] == '0')
                {
                    player.falling = true;
                    player.walk = true;  // if set to 'false' player cant move when falling down
                }
                Console.CursorVisible = false;
                return true;
            }
            else
            {
                Console.CursorVisible = false;
                return false;
            }
        }

        private static bool climbPlayer(int mX, int mY)
        {
            int newX = player.posX + mX;
            int newY = player.posY + mY;
            if(levelArray[newX, newY] == '2')
            {
                //jumpReady = true;
                moveGraphicRoutine(newX, newY);
                return true;
            }
            else return false;
        }

        private static void moveGraphicRoutine(int newX, int newY)
        {
            Console.SetCursorPosition(player.posX, player.posY);
            Console.Write(getTile(levelArray[player.posX, player.posY]));
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(newX, newY);
            Console.Write("X");
            Console.SetCursorPosition(7, 21);
            Console.Write(player.score);
        }


        private static void fallingDown()
        {
            //jumpReady = false;
            Console.SetCursorPosition(player.posX, player.posY);
            Console.Write(getTile(levelArray[player.posX, player.posY]));
            player.posY++;
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(player.posX, player.posY);
            Console.Write("X");
            Console.SetCursorPosition(7, 21);
            Console.Write(player.score);
            if (levelArray[player.posX, (player.posY + 1)] != '0')
            {
                player.falling = false;
                //jumpReady = true;
                player.walk = true;
            }
            return;
        }

        
        
        private static void printLevel(string level)
        {
            string[] lines = level.Split('\n');
            for (int i = 0; i < lines.Count(); i++)
            {
                char[] column = lines[i].ToCharArray();
                for (int j = 0; j < column.Count(); j++)
                {
                    levelArray[j, i] = column[j];
                    Console.Write(getTile(column[j]));
                }
                Console.WriteLine();
            }
            return;
        }

        private static string readLevel(string path)
        {
            StreamReader sr = new StreamReader(path);
            var level = sr.ReadToEnd();
            sr.Close();
            return level; 
        }
        
        private static string getTile(char ti)
        {
            switch (ti)
            {
                case '0':
                    Console.BackgroundColor = ConsoleColor.Black;
                    return " ";

                case '1':
                    Console.BackgroundColor = ConsoleColor.Blue;
                    return " ";

                case '2':
                    Console.BackgroundColor = ConsoleColor.Black; Console.ForegroundColor = ConsoleColor.DarkGreen;
                    return "H";

                case '8':
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    return "*";

                case '9':
                    Console.BackgroundColor = ConsoleColor.Red;
                    return " ";

            }
            return " ";
        }
        private static string startScreen()
        {
            Console.Clear();
            Console.WriteLine("#####################################################");
            Console.WriteLine("#                                                   #");
            Console.WriteLine("#           T H E     M A D    M I N E R            #");
            Console.WriteLine("#                                                   #");
            Console.WriteLine("#         collect all gold, but watch out...        #");
            Console.WriteLine("#         the mining robots is huntíng you!         #");
            Console.WriteLine("#                                                   #");
            Console.WriteLine("#    control the 'x' with the arrowkeys and jump    #");
            Console.WriteLine("#                   Good Luck!!!                    #");
            Console.WriteLine("#                                                   #");
            Console.WriteLine("#    S: Start the game             E: Edit room     #");
            Console.WriteLine("#                                                   #");
            Console.WriteLine("#####################################################");
            if (Console.ReadKey(true).Key == ConsoleKey.E)
            {
                return "edit";
            }
            else
            {
                return "play";
            }
        }

        private static void debug(string v)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(0, 23);
            Console.Write(v);
        }
    }

    class Player
    {
        public int score { get; set; }
        public int posX { get; set; }
        public int posY { get; set; }
        public bool jumpReady { get; set; }
        public bool jumping { get; set; }
        public bool falling { get; set; }
        public bool walk { get; set; }
        public Dir jumpDir { get; set; }
    }

    enum Dir
    {
        left,
        right,
        middle
    }

}
