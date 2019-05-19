using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;

namespace EvoConsole
{
    class Mob
    {
        static Random rand = new Random();  // объект для генерации случайных чисел

        static int healthPerFood = 40;

        // 0....7  - сделать шаг
        // 8....15 - посмотреть
        // 16...23 - съесть
        // 24 - фотосинтез
        // 25...32 - поворот
        // 33...63 - сдвиг команды

        private byte[] commands = new byte[64];      // массив команд
        private byte pointer = 0;                    // номер текущей команды

        private int health = 160;
        private int age = 0;

        // [0] - строка
        // [1] - столбец
        private int[] coords = new int[2];
        byte nextObject;                         // на что посмотрел
        byte viewsCount = 0;                     // сколько раз посмотрел (лимит 4)
        byte shiftCount = 0;                     // сколько раз сдвинул команду (лимит 4)
        byte direction = 0;

        public Mob()
        {
            for (byte temp = 0; temp < 64; temp++)
            {
                //commands[temp] = 24;
                //commands[temp] = (byte)(rand.Next(64));
            }
            commands = new byte[64] { 46, 61, 50, 41, 46, 5, 8, 27, 54, 57, 53, 8, 16, 6, 36, 58, 13, 55, 61, 37, 24, 60, 3, 62, 34, 4, 49, 11, 53, 23, 61, 47, 1, 61, 3, 56, 62, 16, 63, 1, 33, 41, 55, 25, 20, 45, 16, 15, 17, 36, 5, 24, 56, 17, 46, 37, 50, 37, 25, 2, 27, 23, 11, 8 };

            coords = World.GetFreeCoords();
            World.WriteInfo(coords, World.MOB);
        }

        public Mob(byte[] commands)
        {
            this.commands = commands;
            coords = World.GetFreeCoords();
            World.WriteInfo(coords, World.MOB);
        }

        public void Main()
        {
            if (health <= 0)  // если моб умер
            {
                Program.deadMobs.Add(this);  // добавление в список мертвых мобов
                Program.mobsStatistic.Add(this);
                World.WriteInfo(coords, World.CORPSE);  // записываем труп
                return;  // конец хода
            }

        Begin:
            
            if (commands[pointer] <= 7)  // команда переместиться
            {
                switch ((commands[pointer] + direction) % 8)
                {
                    case 0:
                        // смотрим туда, куда хотим переместиться
                        nextObject = World.GetInfo(new int[2] { coords[0] - 1, coords[1] });
                        // перемещаемся, если там пусто
                        if (nextObject == World.VOID)
                        {
                            // записываем новые координаты моба в мир
                            World.WriteInfo(new int[2] { coords[0] - 1, coords[1] }, coords, World.MOB);
                            // меняем координаты моба
                            coords[0]--;
                            // сдвигаем команду на 1
                            pointer++;
                        }
                        else
                        if (nextObject == World.FOOD || nextObject == World.CORPSE)
                        {
                            // записываем новые координаты моба в мир
                            World.WriteInfo(new int[2] { coords[0] - 1, coords[1] }, coords, World.MOB);
                            // меняем координаты моба
                            coords[0]--;
                            // увеличиваем здровье моба
                            health = health + healthPerFood;
                            // сдвигаем команду на 2
                            pointer = (byte)(pointer + 2);
                        }
                        // сдвигаем команду в зависимости от того, что было в выбранной клетке
                        else Shift(nextObject);
                        break;
                    case 1:
                        nextObject = World.GetInfo(new int[2] { coords[0] - 1, coords[1] + 1 });
                        if (nextObject == World.VOID)
                        {
                            World.WriteInfo(new int[2] { coords[0] - 1, coords[1] + 1 }, coords, World.MOB);
                            coords[0]--;
                            coords[1]++;
                            pointer++;
                        }
                        else
                        if (nextObject == World.FOOD || nextObject == World.CORPSE)
                        {
                            World.WriteInfo(new int[2] { coords[0] - 1, coords[1] + 1 }, coords, World.MOB);
                            coords[0]--;
                            coords[1]++;
                            health = health + healthPerFood;
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 2:
                        nextObject = World.GetInfo(new int[2] { coords[0], coords[1] + 1 });
                        if (nextObject == World.VOID)
                        {
                            World.WriteInfo(new int[2] { coords[0], coords[1] + 1 }, coords, World.MOB);
                            coords[1]++;
                            pointer++;
                        }
                        else
                        if (nextObject == World.FOOD || nextObject == World.CORPSE)
                        {
                            World.WriteInfo(new int[2] { coords[0], coords[1] + 1 }, coords, World.MOB);
                            coords[1]++;
                            health = health + healthPerFood;
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 3:
                        nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] + 1 });
                        if (nextObject == World.VOID)
                        {
                            World.WriteInfo(new int[2] { coords[0] + 1, coords[1] + 1 }, coords, World.MOB);
                            coords[0]++;
                            coords[1]++;
                            pointer++;
                        }
                        else
                        if (nextObject == World.FOOD || nextObject == World.CORPSE)
                        {
                            World.WriteInfo(new int[2] { coords[0] + 1, coords[1] + 1 }, coords, World.MOB);
                            coords[0]++;
                            coords[1]++;
                            health = health + healthPerFood;
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 4:
                        nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] });
                        if (nextObject == World.VOID)
                        {
                            World.WriteInfo(new int[2] { coords[0] + 1, coords[1] }, coords, World.MOB);
                            coords[0]++;
                            pointer++;
                        }
                        else
                        if (nextObject == World.FOOD || nextObject == World.CORPSE)
                        {
                            World.WriteInfo(new int[2] { coords[0] + 1, coords[1] }, coords, World.MOB);
                            coords[0]++;
                            health = health + healthPerFood;
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 5:
                        nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] - 1 });
                        if (nextObject == World.VOID)
                        {
                            World.WriteInfo(new int[2] { coords[0] + 1, coords[1] - 1 }, coords, World.MOB);
                            coords[0]++;
                            coords[1]--;
                            pointer++;
                        }
                        else
                        if (nextObject == World.FOOD || nextObject == World.CORPSE)
                        {
                            World.WriteInfo(new int[2] { coords[0] + 1, coords[1] - 1 }, coords, World.MOB);
                            coords[0]++;
                            coords[1]--;
                            health = health + healthPerFood;
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 6:
                        nextObject = World.GetInfo(new int[2] { coords[0], coords[1] - 1 });
                        if (nextObject == World.VOID)
                        {
                            World.WriteInfo(new int[2] { coords[0], coords[1] - 1 }, coords, World.MOB);
                            coords[1]--;
                            pointer++;
                        }
                        else
                        if (nextObject == World.FOOD || nextObject == World.CORPSE)
                        {
                            World.WriteInfo(new int[2] { coords[0], coords[1] - 1 }, coords, World.MOB);
                            coords[1]--;
                            health = health + healthPerFood;
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 7:
                        nextObject = World.GetInfo(new int[2] { coords[0] - 1, coords[1] - 1 });
                        if (nextObject == World.VOID)
                        {
                            World.WriteInfo(new int[2] { coords[0] - 1, coords[1] - 1 }, coords, World.MOB);
                            coords[0]--;
                            coords[1]--;
                            pointer++;
                        }
                        else
                        if (nextObject == World.FOOD || nextObject == World.CORPSE)
                        {
                            World.WriteInfo(new int[2] { coords[0] - 1, coords[1] - 1 }, coords, World.MOB);
                            coords[0]--;
                            coords[1]--;
                            health = health + healthPerFood;
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                }
                pointer = (byte)(pointer % 64);
            }
            else
            if (commands[pointer] >= 8 & commands[pointer] <= 15)  // команда посмотреть
            {
                // если моб задейтсвовал эту команду меньше 4 раз
                if (viewsCount < 4)
                {
                    switch ((commands[pointer] + direction) % 8)
                    {
                        case 0:
                            // смотрим в выбранном напрвалении
                            nextObject = World.GetInfo(new int[2] { coords[0] - 1, coords[1] });
                            // сдвигаем команду в зависимости от того, что увидили
                            Shift(nextObject);
                            break;
                        case 1:
                            nextObject = World.GetInfo(new int[2] { coords[0] - 1, coords[1] + 1 });
                            Shift(nextObject);
                            break;
                        case 2:
                            nextObject = World.GetInfo(new int[2] { coords[0], coords[1] + 1 });
                            Shift(nextObject);
                            break;
                        case 3:
                            nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] + 1 });
                            Shift(nextObject);
                            break;
                        case 4:
                            nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] });
                            Shift(nextObject);
                            break;
                        case 5:
                            nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] - 1 });
                            Shift(nextObject);
                            break;
                        case 6:
                            nextObject = World.GetInfo(new int[2] { coords[0], coords[1] - 1 });
                            Shift(nextObject);
                            break;
                        case 7:
                            nextObject = World.GetInfo(new int[2] { coords[0] - 1, coords[1] - 1 });
                            Shift(nextObject);
                            break;
                    }
                    pointer = (byte)(pointer % 64);
                }
                else goto End;

                goto Begin;
            }
            else
            if (commands[pointer] >= 16 & commands[pointer] <= 23)  // команда съесть
            {
                switch ((commands[pointer] + direction) % 8)
                {
                    case 0:
                        // смотрим на то, что хотим съесть
                        nextObject = World.GetInfo(new int[2] { coords[0] - 1, coords[1] });
                        // если там еда
                        if (nextObject == World.FOOD || nextObject == World.CORPSE)
                        {
                            // на месте еды записываем пустоту
                            World.WriteInfo(new int[2] { coords[0] - 1, coords[1] }, World.VOID);
                            health = health + healthPerFood;
                            // сдвигаем команду на 2
                            pointer = (byte)(pointer + 2);
                        }
                        // сдвигаем команду в зависимости от того, что было в выбранной клетке
                        else Shift(nextObject);
                        break;
                    case 1:
                        nextObject = World.GetInfo(new int[2] { coords[0] - 1, coords[1] + 1 });
                        if (nextObject == World.FOOD || nextObject == World.CORPSE)
                        {
                            World.WriteInfo(new int[2] { coords[0] - 1, coords[1] + 1 }, World.VOID);
                            health = health + healthPerFood;
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 2:
                        nextObject = World.GetInfo(new int[2] { coords[0], coords[1] + 1 });
                        if (nextObject == World.FOOD || nextObject == World.CORPSE)
                        {
                            World.WriteInfo(new int[2] { coords[0], coords[1] + 1 }, World.VOID);
                            health = health + healthPerFood;
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 3:
                        nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] + 1 });
                        if (nextObject == World.FOOD || nextObject == World.CORPSE)
                        {
                            World.WriteInfo(new int[2] { coords[0] + 1, coords[1] + 1 }, World.VOID);
                            health = health + healthPerFood;
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 4:
                        nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] });
                        if (nextObject == World.FOOD || nextObject == World.CORPSE)
                        {
                            World.WriteInfo(new int[2] { coords[0] + 1, coords[1] }, World.VOID);
                            health = health + healthPerFood;
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 5:
                        nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] - 1 });
                        if (nextObject == World.FOOD || nextObject == World.CORPSE)
                        {
                            World.WriteInfo(new int[2] { coords[0] + 1, coords[1] - 1 }, World.VOID);
                            health = health + healthPerFood;
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 6:
                        nextObject = World.GetInfo(new int[2] { coords[0], coords[1] - 1 });
                        if (nextObject == World.FOOD || nextObject == World.CORPSE)
                        {
                            World.WriteInfo(new int[2] { coords[0], coords[1] - 1 }, World.VOID);
                            health = health + healthPerFood;
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 7:
                        nextObject = World.GetInfo(new int[2] { coords[0] - 1, coords[1] - 1 });
                        if (nextObject == World.FOOD || nextObject == World.CORPSE)
                        {
                            World.WriteInfo(new int[2] { coords[0] - 1, coords[1] - 1 }, World.VOID);
                            health = health + healthPerFood;
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                }
                pointer = (byte)(pointer % 64);
            }
            else
            if (commands[pointer] == 24)  // команда фотосинтез
            {
                health = health + 1;
                pointer++;
                pointer = (byte)(pointer % 64);
            }
            else
            if (commands[pointer] >= 25 & commands[pointer] <= 32)  // команда повернуться
            {
                switch (commands[pointer])
                {
                    case 25:
                        direction = (byte)(direction + 0);
                        break;
                    case 26:
                        direction = (byte)(direction + 1);
                        break;
                    case 27:
                        direction = (byte)(direction + 2);
                        break;
                    case 28:
                        direction = (byte)(direction + 3);
                        break;
                    case 29:
                        direction = (byte)(direction + 4);
                        break;
                    case 30:
                        direction = (byte)(direction + 5);
                        break;
                    case 31:
                        direction = (byte)(direction + 6);
                        break;
                    case 32:
                        direction = (byte)(direction + 7);
                        break;
                }
                direction = (byte)(direction % 8);
                pointer++;
                pointer = (byte)(pointer % 64);
            }
            else
            if (commands[pointer] >= 33)  // сдвиг команды
            {
                if (shiftCount < 4)
                {
                    pointer = (byte)((pointer + commands[pointer]) % 64);
                    shiftCount++;
                    goto Begin;
                }
                else goto End;
            }

        End:

            health = health - 2;
            viewsCount = 0;
            shiftCount = 0;
            age++;
        }

        private void Shift(byte viewObject)
        {
            switch (viewObject)
            {
                case World.VOID:
                    pointer = (byte)(pointer + 1);
                    break;
                case World.FOOD:
                    pointer = (byte)(pointer + 2);
                    break;
                case World.MOB:
                    pointer = (byte)(pointer + 3);
                    break;
                case World.CORPSE:
                    pointer = (byte)(pointer + 4);
                    break;
                case World.WALL:
                    pointer = (byte)(pointer + 5);
                    break;
            }
        }

        public static void MutateCommands(byte[] commands)
        {
            commands[rand.Next(64)] = (byte)rand.Next(64);
            Program.mobs.Add(new Mob(commands));
        }

        public int GetAge()
        {
            return age;
        }

        public byte[] GetCommands()
        {
            return commands;
        }
    }

    class World
    {
        // 0 - пусто
        // 1 - еда
        // 2 - моб
        // 3 - труп
        // 4 - стена

        // константы для обозначения соответствующих объектов
        public const byte VOID = 0;
        public const byte FOOD = 1;
        public const byte MOB = 2;
        public const byte CORPSE = 3;
        public const byte WALL = 4;

        // размеры мира
        public const int HEIGHT = 40;
        public const int WIDTH = 100;

        // создаем мир заданных размеров
        private static byte[,] world = new byte[HEIGHT, WIDTH];

        private static int allFood = 0;

        static Random rand = new Random();  // объект для генерации случайных чисел

        public static void GenerateWorld()
        {
            // заполняем все нулями, т.е. пустотой
            for (byte height = 0; height < HEIGHT; height++)
            {
                for (byte width = 0; width < WIDTH; width++)
                {
                    world[height, width] = VOID;
                }
            }
            // создаем границы
            for (byte temp = 0; temp < WIDTH; temp++)
            {
                world[0, temp] = WALL;
                world[HEIGHT - 1, temp] = WALL;
            }
            for (byte temp = 0; temp < HEIGHT; temp++)
            {
                world[temp, 0] = WALL;
                world[temp, WIDTH - 1] = WALL;
            }
            // в пустом мире нет еды - логично
            allFood = 0;
        }

        public static void GenerateFood()
        {
            int foodCount = 0;  // количество сгенерированной за ход еды

            // генерирует 8 еды за ход
            while ((foodCount < 8) && (allFood < 600))
            {
                // получаем пустые координаты
                int[] freeCoords = GetFreeCoords();
                // если нет свободного места
                if ((freeCoords[0] == 0) || (freeCoords[1] == 0))
                {
                    return;
                }
                // ставим на них еду
                world[freeCoords[0], freeCoords[1]] = FOOD;
                foodCount++;
                allFood++;
            }
        }

        public static int[] GetFreeCoords()
        {
            List<int[]> freeCoords = new List<int[]>();

            for (int i = 1; i < (HEIGHT - 1); i++)
            {
                for (int j = 1; j < (WIDTH - 1); j++)
                {
                    if (world[i, j] == VOID)
                    {
                        freeCoords.Add(new int[] { i, j });
                    }
                }
            }

            if (freeCoords.Count != 0)
            {
                return freeCoords[rand.Next(freeCoords.Count)];
            }
            else return new int[2] { 0, 0 };
        }

        public static byte GetInfo(int[] coordsToInfo)
        {
            return world[coordsToInfo[0], coordsToInfo[1]];
        }

        /* вот тут можно реализовать полиморфизм для моба и трупа
         * когда добавлю минералы, можно записать их количество в
         * другое измерение матрицы, когда умрет моб, владевший ими
         * будет второй аргумент у функции для трупа, и все так же 1
         * для живого моба
         */
        public static void WriteInfo(int[] coordsToInfo, byte curObject)
        {
            if (world[coordsToInfo[0], coordsToInfo[1]] == FOOD)
            {
                allFood--;
            }
            world[coordsToInfo[0], coordsToInfo[1]] = curObject;
        }

        public static void WriteInfo(int[] coordsToInfo, int[] currentCoords, byte curObject)
        {
            if (world[coordsToInfo[0], coordsToInfo[1]] == FOOD)
            {
                allFood--;
            }
            world[coordsToInfo[0], coordsToInfo[1]] = curObject;
            world[currentCoords[0], currentCoords[1]] = VOID;
        }

        public static void DrawWorld()
        {
            for (int height = 0; height < HEIGHT; height++)
            {
                for (int width = 0; width < WIDTH; width++)
                {
                    Console.CursorVisible = false;
                    switch (world[height, width])
                    {
                        case VOID:
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.SetCursorPosition(width, height);
                            Console.Write(" ");
                            break;
                        case FOOD:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.SetCursorPosition(width, height);
                            Console.Write(".");
                            break;
                        case MOB:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.SetCursorPosition(width, height);
                            Console.Write("O");
                            break;
                        case CORPSE:
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.SetCursorPosition(width, height);
                            Console.Write("x");
                            break;
                        case WALL:
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.SetCursorPosition(width, height);
                            Console.Write("#");
                            break;
                    }
                }
            }
        }
    }

    class Program
    {
        static string writePath = @"..\..\..\mob.txt";

        public static List<Mob> mobs = new List<Mob>();
        public static List<Mob> deadMobs = new List<Mob>();
        public static List<Mob> mobsStatistic = new List<Mob>();
        public static List<Mob> topMob = new List<Mob>();

        public static void RemoveMobs()  // функция удаления мертвых мобов
        {
            foreach (Mob mob in deadMobs)  // берем каждого моба из списка мертвых
            {
                mobs.Remove(mob);  // удаляем его из списка живых
            }
            deadMobs.Clear();  // очищаем список мертвых мобов
        }

        static void Main(string[] args)
        {
            // создаем главный поток и запускаем
            Thread mainThread = new Thread(MainThread);
            mainThread.Start();
            // создаем поток управления и запускаем
            Thread controlThread = new Thread(ControlThread);
            controlThread.Start();
        }
        
        static void MainThread()
        {
            World.GenerateWorld();  // создаем пустой мир

            // генерируем начальную популяцию
            for (byte count = 0; count < 64; count++)
            {
                mobs.Add(new Mob());
            }

            uint generation = 0;

        Begin:

            uint turn = 0;  // считаем ходы текущей популяции

            // пока мобы живы
            while (mobs.Count != 0)  
            {
                // даем ход каждому мобу
                for (int temp = 0; temp < mobs.Count; temp++)
                {
                    mobs[temp].Main();
                }
                World.GenerateFood();                       // генерируем еду
                RemoveMobs();                               // удаление мертвых мобов;
                turn++;                                     // проешл ход

                // если не ввели d, отрисовывать мир
                if (command != "d")
                {
                    World.DrawWorld();
                    DrawStatistic(turn, generation);
                }
            }
            generation++;
            if (mobs.Count == 0)
            {
                World.GenerateWorld();

                IEnumerable<Mob> sortedMobsStatistic = from mob in mobsStatistic orderby mob.GetAge() descending select mob;

                for (byte count = 0; count < 8; count++)
                {
                    for (byte countCopies = 0; countCopies < 6; countCopies++)
                    {
                        mobs.Add(new Mob(sortedMobsStatistic.ElementAt(count).GetCommands()));
                    }
                    Mob.MutateCommands(sortedMobsStatistic.ElementAt(count).GetCommands());
                    Mob.MutateCommands(sortedMobsStatistic.ElementAt(count).GetCommands());
                }

                if (topMob.Count != 0)
                {
                    if (sortedMobsStatistic.ElementAt(0).GetAge() > topMob.ElementAt(0).GetAge())
                    {
                        topMob.RemoveAt(0);
                        topMob.Add(sortedMobsStatistic.ElementAt(0));

                        try
                        {
                            using (StreamWriter sw = new StreamWriter(writePath, true, System.Text.Encoding.Default))
                            {
                                sw.Write(topMob.ElementAt(0).GetAge() + " -- ");
                                for (int x = 0; x < 64; x++)
                                {
                                    sw.Write(topMob.ElementAt(0).GetCommands()[x] + ", ");
                                }
                                sw.WriteLine();
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
                else topMob.Add(sortedMobsStatistic.ElementAt(0));

                DrawStatistic(turn, generation, sortedMobsStatistic);
                mobsStatistic.Clear();

                /*List<Mob> mobsTemp = new List<Mob>();

                for (byte count = 0; count < 8; count++)
                {
                    for (byte countCopies = 0; countCopies < 6; countCopies++)
                    {
                        mobs.Add(new Mob(sortedMobsStatistic.ElementAt(count).GetCommands()));
                    }
                    Mob.MutateCommands(sortedMobsStatistic.ElementAt(count).GetCommands());
                    Mob.MutateCommands(sortedMobsStatistic.ElementAt(count).GetCommands());
                    mobsTemp.Add(sortedMobsStatistic.ElementAt(count));
                }

                DrawStatistic(turn, generation, mobsTemp);

                mobsStatistic.Clear();
                mobsStatistic = mobsTemp;
                mobsTemp = null;*/

                goto Begin;
            }
        }

        static string command;  // буфер для команды управления

        // в отдельном потоке
        static void ControlThread()
        {
            // всегда считываем команду управления
            while(true)
            {
                command = Console.ReadLine();
            }
        }

        static void DrawStatistic(uint turn, uint generation)
        {
            for (int i = 1; i < 30; i++)
            {
                Console.SetCursorPosition(World.WIDTH + i, 0);
                Console.Write(" ");
                Console.SetCursorPosition(World.WIDTH + i, 1);
                Console.Write(" ");
            }
            Console.SetCursorPosition(World.WIDTH + 1, 0);
            Console.Write("Current turn: " + turn);
            Console.SetCursorPosition(World.WIDTH + 1, 1);
            Console.Write("Generation: " + generation);
        }

        /*static void DrawStatistic(uint turn, uint generation, List<Mob> mobsTemp)
        {
            DrawStatistic(turn, generation);

            for (int i = 0; i < 8; i++)
            {
                for (int x = 0; x < 20; x++)
                {
                    Console.SetCursorPosition(World.WIDTH + 1, 3 + i);
                    Console.Write(" ");
                }
                Console.SetCursorPosition(World.WIDTH + 1, 3 + i);
                Console.Write(i + ": " + mobsTemp.ElementAt(i).GetAge());
            }
        }*/

        static void DrawStatistic(uint turn, uint generation, IEnumerable<Mob> sortedMobsStatistic)
        {
            DrawStatistic(turn, generation);

            for (int i = 0; i < 8; i++)
            {
                for (int x = 0; x < 20; x++)
                {
                    Console.SetCursorPosition(World.WIDTH + 1 + x, 3 + i);
                    Console.Write(" ");
                }
                Console.SetCursorPosition(World.WIDTH + 1, 3 + i);
                Console.Write(i + ": " + sortedMobsStatistic.ElementAt(i).GetAge());
            }
        }
    }
}
