using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoAlg
{
    class Mob
    {
        // 0....7  - сделать шаг
        // 8....15 - посмотреть
        // 16...23 - съесть
        // 24...31 - фотосинтез
        // 32...63 - сдвиг команды

        private byte[] commands = new byte[64];  // массив команд
        private byte pointer = 0;                // номер текущей команды

        private byte health = 200;
        /* энергия дается тогда, когда и здоровье,
         * но не расходуется в конце хода, используется для размножения
         * при достижении 80 энергии и сбрасывается
         */
        private byte energy = 0;  

        // [0] - строка
        // [1] - столбец
        private int[] coords = new int[2];
        byte nextObject;                         // на что посмотрел
        byte viewsCount = 0;                     // сколько раз посмотрел (лимит 4)
        byte shiftCount = 0;

        private const byte VOID = 0;             // константа для проверки направления на пустоту
        private const byte FOOD = 1;             // константа для проверки направления на еду
        private const byte MOB = 2;              // константа для записи моба в мир
        private const byte CORPSE = 3;           // константа для записи трупа в мир

        public Mob()
        {
            Random rand = new Random();

            for(byte temp = 0; temp < 64; temp++)
            {
                commands[temp] = (byte)(rand.Next(64));
            }

            coords = World.GetFreeCoords();
            World.WriteInfo(coords, MOB);

            rand = null;
        }

        public Mob(byte[] commands, int[] coords)
        {
            this.commands = commands;
            this.coords = coords;
            World.WriteInfo(coords, MOB);
        }

        public void Main()
        {
            if(health < 0)  // если моб умер
            {
                Program.deadMobs.Add(this);  // добавление в список мертвых мобов
                return;  // конец хода
            }

            Begin:

            if(commands[pointer] <= 7)  // команда переместиться
            {
                switch(commands[pointer])
                {
                    case 0:
                        // смотрим туда, куда хотим переместиться
                        nextObject = World.GetInfo(new int[2] { coords[0] - 1, coords[1] });
                        // перемещаемся, если там пусто
                        if (nextObject == VOID)
                        {
                            // записываем новые координаты моба в мир
                            World.WriteInfo(new int[2] { coords[0] - 1, coords[1] }, new int[2] { coords[0], coords[1] }, MOB);
                            // меняем координаты моба
                            coords[0]--;
                            // сдвигаем команду на 1
                            pointer++;
                        }
                        // сдвигаем команду в зависимости от того, что было в выбранной клетке
                        else Shift(nextObject);
                        break;
                    case 1:
                        nextObject = World.GetInfo(new int[2] { coords[0] - 1, coords[1] + 1 });
                        if (nextObject == VOID)
                        {
                            World.WriteInfo(new int[2] { coords[0] - 1, coords[1] + 1 }, new int[2] { coords[0], coords[1] }, MOB);
                            coords[0]--;
                            coords[1]++;
                            pointer++;
                        }
                        else Shift(nextObject);
                        break;
                    case 2:
                        nextObject = World.GetInfo(new int[2] { coords[0], coords[1] + 1 });
                        if (nextObject == VOID)
                        {
                            World.WriteInfo(new int[2] { coords[0], coords[1] + 1 }, new int[2] { coords[0], coords[1] }, MOB);
                            coords[1]++;
                            pointer++;
                        }
                        else Shift(nextObject);
                        break;
                    case 3:
                        nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] + 1 });
                        if (nextObject == VOID)
                        {
                            World.WriteInfo(new int[2] { coords[0] + 1, coords[1] + 1 }, new int[2] { coords[0], coords[1] }, MOB);
                            coords[0]++;
                            coords[1]++;
                            pointer++;
                        }
                        else Shift(nextObject);
                        break;
                    case 4:
                        nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] });
                        if (nextObject == VOID)
                        {
                            World.WriteInfo(new int[2] { coords[0] + 1, coords[1] }, new int[2] { coords[0], coords[1] }, MOB);
                            coords[0]++;
                            pointer++;
                        }
                        else Shift(nextObject);
                        break;
                    case 5:
                        nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] - 1 });
                        if (nextObject == VOID)
                        {
                            World.WriteInfo(new int[2] { coords[0] + 1, coords[1] - 1 }, new int[2] { coords[0], coords[1] }, MOB);
                            coords[0]++;
                            coords[1]--;
                            pointer++;
                        }
                        else Shift(nextObject);
                        break;
                    case 6:
                        nextObject = World.GetInfo(new int[2] { coords[0], coords[1] - 1 });
                        if (nextObject == VOID)
                        {
                            World.WriteInfo(new int[2] { coords[0], coords[1] - 1 }, new int[2] { coords[0], coords[1] }, MOB);
                            coords[1]--;
                            pointer++;
                        }
                        else Shift(nextObject);
                        break;
                    case 7:
                        nextObject = World.GetInfo(new int[2] { coords[0] - 1, coords[1] - 1 });
                        if (nextObject == VOID)
                        {
                            World.WriteInfo(new int[2] { coords[0] - 1, coords[1] - 1 }, new int[2] { coords[0], coords[1] }, MOB);
                            coords[0]--;
                            coords[1]--;
                            pointer++;
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
                    switch (commands[pointer])
                    {
                        case 8:
                            // смотрим в выбранном напрвалении
                            nextObject = World.GetInfo(new int[2] { coords[0] - 1, coords[1] });
                            // сдвигаем команду в зависимости от того, что увидили
                            Shift(nextObject);
                            break;
                        case 9:
                            nextObject = World.GetInfo(new int[2] { coords[0] - 1, coords[1] + 1 });
                            Shift(nextObject);
                            break;
                        case 10:
                            nextObject = World.GetInfo(new int[2] { coords[0], coords[1] + 1 });
                            Shift(nextObject);
                            break;
                        case 11:
                            nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] + 1 });
                            Shift(nextObject);
                            break;
                        case 12:
                            nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] });
                            Shift(nextObject);
                            break;
                        case 13:
                            nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] - 1 });
                            Shift(nextObject);
                            break;
                        case 14:
                            nextObject = World.GetInfo(new int[2] { coords[0], coords[1] - 1 });
                            Shift(nextObject);
                            break;
                        case 15:
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
                switch (commands[pointer])
                {
                    case 16:
                        // смотрим на то, что хотим съесть
                        nextObject = World.GetInfo(new int[2] { coords[0] - 1, coords[1] });
                        // если там еда
                        if (nextObject == FOOD)
                        {
                            // записываем новые координаты моба в мир
                            World.WriteInfo(new int[2] { coords[0] - 1, coords[1] }, new int[2] { coords[0], coords[1] }, MOB);
                            // меняем координаты моба
                            coords[0]--;
                            health = (byte)(health + 20);
                            energy = (byte)(energy + 20);
                            // сдвигаем команду на 2
                            pointer = (byte)(pointer + 2);
                        }
                        // сдвигаем команду в зависимости от того, что было в выбранной клетке
                        else Shift(nextObject);
                        break;
                    case 17:
                        nextObject = World.GetInfo(new int[2] { coords[0] - 1, coords[1] + 1 });
                        if (nextObject == FOOD)
                        {
                            World.WriteInfo(new int[2] { coords[0] - 1, coords[1] + 1 }, new int[2] { coords[0], coords[1] }, MOB);
                            coords[0]--;
                            coords[1]++;
                            health = (byte)(health + 20);
                            energy = (byte)(energy + 20);
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 18:
                        nextObject = World.GetInfo(new int[2] { coords[0], coords[1] + 1 });
                        if (nextObject == FOOD)
                        {
                            World.WriteInfo(new int[2] { coords[0], coords[1] + 1 }, new int[2] { coords[0], coords[1] }, MOB);
                            coords[1]++;
                            health = (byte)(health + 20);
                            energy = (byte)(energy + 20);
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 19:
                        nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] + 1 });
                        if (nextObject == FOOD)
                        {
                            World.WriteInfo(new int[2] { coords[0] + 1, coords[1] + 1 }, new int[2] { coords[0], coords[1] }, MOB);
                            coords[0]++;
                            coords[1]++;
                            health = (byte)(health + 20);
                            energy = (byte)(energy + 20);
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 20:
                        nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] });
                        if (nextObject == FOOD)
                        {
                            World.WriteInfo(new int[2] { coords[0] + 1, coords[1] }, new int[2] { coords[0], coords[1] }, MOB);
                            coords[0]++;
                            health = (byte)(health + 20);
                            energy = (byte)(energy + 20);
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 21:
                        nextObject = World.GetInfo(new int[2] { coords[0] + 1, coords[1] - 1 });
                        if (nextObject == FOOD)
                        {
                            World.WriteInfo(new int[2] { coords[0] + 1, coords[1] - 1 }, new int[2] { coords[0], coords[1] }, MOB);
                            coords[0]++;
                            coords[1]--;
                            health = (byte)(health + 20);
                            energy = (byte)(energy + 20);
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 22:
                        nextObject = World.GetInfo(new int[2] { coords[0], coords[1] - 1 });
                        if (nextObject == FOOD)
                        {
                            World.WriteInfo(new int[2] { coords[0], coords[1] - 1 }, new int[2] { coords[0], coords[1] }, MOB);
                            coords[1]--;
                            health = (byte)(health + 20);
                            energy = (byte)(energy + 20);
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                    case 23:
                        nextObject = World.GetInfo(new int[2] { coords[0] - 1, coords[1] - 1 });
                        if (nextObject == FOOD)
                        {
                            World.WriteInfo(new int[2] { coords[0] - 1, coords[1] - 1 }, new int[2] { coords[0], coords[1] }, MOB);
                            coords[0]--;
                            coords[1]--;
                            health = (byte)(health + 20);
                            energy = (byte)(energy + 20);
                            pointer = (byte)(pointer + 2);
                        }
                        else Shift(nextObject);
                        break;
                }
                pointer = (byte)(pointer % 64);
            }
            else
            if (commands[pointer] >= 24 & commands[pointer] <= 31)  // команда фотосинтез
            {
                health = (byte)(health + 6);
                energy = (byte)(energy + 2);
                pointer++;
                pointer = (byte)(pointer % 64);
            }
            else
            if (commands[pointer] >= 32)  // сдвиг команды
            {
                if(shiftCount < 4)
                {
                    pointer = (byte)((pointer + commands[pointer]) % 64);
                    shiftCount++;
                    goto Begin;
                }
                else goto End;
            }

            if (energy >= 200)  // размножение, если накопилась энергия
            {
                if (World.GetInfo(new int[] { coords[0] - 1, coords[1] }) == VOID)
                {
                    Multiply(commands, new int[] { coords[0] - 1, coords[1] });
                    energy = (byte)(energy - 80);
                }
                else 
                if (World.GetInfo(new int[] { coords[0] - 1, coords[1] + 1 }) == VOID)
                {
                    Multiply(commands, new int[] { coords[0] - 1, coords[1] + 1 });
                    energy = (byte)(energy - 80);
                }
                else
                if (World.GetInfo(new int[] { coords[0], coords[1] + 1 }) == VOID)
                {
                    Multiply(commands, new int[] { coords[0], coords[1] + 1 });
                    energy = (byte)(energy - 80);
                }
                else
                if (World.GetInfo(new int[] { coords[0] + 1, coords[1] + 1 }) == VOID)
                {
                    Multiply(commands, new int[] { coords[0] + 1, coords[1] + 1 });
                    energy = (byte)(energy - 80);
                }
                else
                if (World.GetInfo(new int[] { coords[0] + 1, coords[1] }) == VOID)
                {
                    Multiply(commands, new int[] { coords[0] + 1, coords[1] });
                    energy = (byte)(energy - 80);
                }
                else
                if (World.GetInfo(new int[] { coords[0] + 1, coords[1] - 1 }) == VOID)
                {
                    Multiply(commands, new int[] { coords[0] + 1, coords[1] - 1 });
                    energy = (byte)(energy - 80);
                }
                else
                if (World.GetInfo(new int[] { coords[0], coords[1] - 1 }) == VOID)
                {
                    Multiply(commands, new int[] { coords[0], coords[1] - 1 });
                    energy = (byte)(energy - 80);
                }
                else
                if (World.GetInfo(new int[] { coords[0] - 1, coords[1] - 1 }) == VOID)
                {
                    Multiply(commands, new int[] { coords[0] - 1, coords[1] - 1 });
                    energy = (byte)(energy - 80);
                }
                pointer++;
                pointer = (byte)(pointer % 64);
            }

        End:

            health = (byte)(health - 5);
            viewsCount = 0;
            shiftCount = 0;
        }

        private void Shift(byte viewObject)
        {
            switch(viewObject)
            {
                case 0:
                    pointer = (byte)(pointer + 1);
                    break;
                case 1:
                    pointer = (byte)(pointer + 2);
                    break;
                case 2:
                    pointer = (byte)(pointer + 3);
                    break;
                case 3:
                    pointer = (byte)(pointer + 4);
                    break;
                case 4:
                    pointer = (byte)(pointer + 5);
                    break;
            }
        }

        private static void Multiply(byte[] commands, int[] coords)
        {
            Random rand = new Random();

            byte probability = 20;

            if (rand.Next(100) < probability)
            {
                commands[rand.Next(64)] = (byte)rand.Next(64);
            }
            Program.mobs.Add(new Mob(commands, coords));
        }
    }

    class World
    {
        // 0 - пусто
        // 1 - еда
        // 2 - моб
        // 3 - труп
        // 4 - стена

        private const byte VOID = 0;  // константа для обозначения пустой клетки
        private const byte FOOD = 1;

        private const int HEIGHT = 75;
        private const int WEIGHT = 100;

        // создаем мир заданных размеров
        private static byte[,] world = new byte[HEIGHT, WEIGHT];

        private static int allFood = 0;

        public static void GenerateWorld()
        {
            // создаем границы
            for (byte temp = 0; temp < 100; temp++)
            {
                world[0, temp] = 4;
                world[74, temp] = 4;
            }
            for (byte temp = 0; temp < 75; temp++)
            {
                world[temp, 0] = 4;
                world[temp, 99] = 4;
            }
            
        }

        public static void GenerateFood()
        {
            int foodCount = 0;  // количество сгенерированной за ход еды

            // генерирует 2 еды за ход
            while ((foodCount < 2) && (allFood < 100))
            {
                // получаем пустые координаты
                int[] freeCoords = GetFreeCoords();
                // если нет свободного места
                if((freeCoords[0] == 0) || (freeCoords[1] == 0))
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

            for(int i = 1; i < (HEIGHT - 1); i++)
            {
                for(int j = 1; j < (WEIGHT - 1); j++)
                {
                    if(world[i, j] == VOID)
                    {
                        freeCoords.Add(new int[] { i, j });
                    }
                }
            }

            if (freeCoords.Count != 0)
            {
                Random rand = new Random();  // объект для генерации случайных чисел

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
        
        public static void WriteWorld()
        {
            for(int i = 0; i < HEIGHT; i++)
            {
                for(int j = 0; j < WEIGHT; j++)
                {
                    Console.Write(world[i, j]);
                }
                Console.WriteLine();
            }
        }
    }                                                      

    class Program
    {
        public static List<Mob> mobs = new List<Mob>();
        public static List<Mob> deadMobs = new List<Mob>();
        
        public static void RemoveMobs()  // функция удаления мертвых мобов
        {
            foreach(Mob mob in deadMobs)  // берем каждого моба из списка мертвых
            {
                mobs.Remove(mob);  // удаляем его из списка живых
            }
            deadMobs.Clear();  // очищаем список мертвых мобов
        }

        static void Main(string[] args)
        {
            World.GenerateWorld();  // создаем стены

            // генерируем начальную популяцию
            for(byte count = 0; count < 8; count++)
            {
                mobs.Add(new Mob());
            }
            uint turn = 0;
            while (turn < 10000)
            {
                // даем ход каждому мобу
                for(int temp = 0; temp < mobs.Count; temp++)
                {
                    mobs[temp].Main();
                }
                World.GenerateFood();  // генерируем еду
                RemoveMobs();  // удаление мертвых мобов
                turn++;
                Console.WriteLine("Конец хода №" + turn);
                Console.WriteLine("Мобов: " + mobs.Count);
            }
            World.WriteWorld();
            Console.ReadKey();
        }
    }
}
