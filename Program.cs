using java.lang;
using System;
using System.Collections.Generic;

namespace aLife
{
    class Init
    {
        public static int SIZE_WORLD = 10; //rozmiar świata - NIE ZMIENIAĆ
        public static int NUM_TACT = 100; //całkowita liczba taktów
        public static int VEW_NUM_TACT = 5; //co ile taktów wyświetla wyniki
        public static int START_NUM_CREEPERS = 2000; //początkowa liczba pełzaczy
        public static int START_NUM_BACT = 2000; //początkowa liczba bakterii
        public static int CREEPER_ENERGY_PRO_LIFE = 1;  //ilość energii potrzebna do urodzenia nowego pełzacza
        public static int CREEPER_INITIAL_ENERGY = 1; //zapas eneergii nowo urodzonego pełzacza
        public static int CREEPER_ENERGY_RESERVE = 4; //rezerwa energii zostawiana podczas rodzenia nowego pełzacza
                                                     //potrzebna do przetrwania, gdy jest mało pożywienia
        public static int MAX_CREEPER_NUM_BORN_PER_TACK = 5; //maksymalna liczba pełzaczy rodzonych przez jednego
                                                            //pełzacza w jednym takcie.
                                                            // Liczba pełzaczy, które mogą się urodzić w jednym takcie jest ograniczona przez ilość energii pełzacza po
                                                            // odjęciu CREEPER_ENERGY_RESERVE. Pełzacz nie może zgromadzić zbyt dużo energii, ponieważ gdy tylko
                                                            // przekracza poziom energii równy CREEPER_ENERGY_PRO_LIFE + CREEPER_ENERGY_RESERVE w następnym takcie
                                                            // rodzi co najmniej jednego pełzacza i jego poziom energii jest zmniejszany o CREEPER_ENERGY_PRO_LIFE
                                                            // na każdego urodzonego pełzacza.

        public static int MAX_BACT_EATEN_BY_CREEPER = 15; //Maksymalna liczba bakterii zjadanych przez pełzacza
                                                         //w jednym takcie
        public static double BACT_MULTIPLICATION_RATE = 0.6; //współczynnik rozmnażania bakterii - tyle nowych bakterii
                                                            //powstaje z jednej backterii w każdym takcie.
                                                            //MOŻE PRZYJMOWAĆ WARTOŚCI UŁAMKOWE.
        public static double BACT_SPREAD_RATE = 0.4; //współczynnik rozprzestrzeniania nowo urodzonych bakterii.
                                                    //DOPUSZCZALNY ZAKRES: od 0 do 1
                                                    //Np. przy wsp. = 0.7, 70% zostaje w komórce,
                                                    //w której się urodziła, a 30% przenosi się
                                                    //do sąsiednich losowo wybranych
        public static int BACT_NUM_LIMIT = 1000000; //graniczna liczba bakterii dla całego świata.
                                                   //Po przekroczeniu tej liczby komórki umierają/koniec symulacji.

        //lista modyfikatorów pozycji - w niej określamy, które miejsca (względem obecnego położenia)
        //mają być sprawdzane/uwzględniane przy przemieszczaniu się pełzaczy lub bakterii z obecnego położenia
        public static List<LocationModifier> initializedLocationModifiersList()
        {
            List<LocationModifier> locationModifiers = new List<LocationModifier>(4);
            locationModifiers.Add(new LocationModifier(-1, 0));
            locationModifiers.Add(new LocationModifier(1, 0));
            locationModifiers.Add(new LocationModifier(0, -1));
            locationModifiers.Add(new LocationModifier(0, 1));

            //możliwość sprawdzania dodatkowo komórek w narożnikach
            //        locationModifiers.add(new LocationModifier(-1, -1));
            //        locationModifiers.add(new LocationModifier(1, 1));
            //        locationModifiers.add(new LocationModifier(-1, 1));
            //        locationModifiers.add(new LocationModifier(1, -1));
            return locationModifiers;
        }
    }

    class LocationModifier
    {
        public int modifyX, modifyY;

        public LocationModifier(int x, int y)
        {
            modifyX = x;
            modifyY = y;
        }
    }

    class Program
    {
        private static List<int> totallyCreepers = new List<int>();
        private static List<int> totallyBacteria = new List<int>();
        //powyżej kolekcje pomocnicze do zapamiętania liczby bakterii
        //i pełzaczy w każdym takcie, celem późniejszego wyświetlenia

        private static void bacteriaTest(World w)
        {
            //wyświetla konsolowo liczbę bakterii w danym takcie
            //w układzie tablicy
            for (int i = 0; i < Init.SIZE_WORLD; i++)
            {
                for (int j = 0; j < Init.SIZE_WORLD; j++)
                {
                    Console.WriteLine(w.board[i, j].getBactNum()); // format string
                }
                Console.WriteLine();
            }
        }

        private static void creepersTest(World w)
        {
            //wyświetla konsolowo liczbę pełzaczy w danym takcie
            //w układzie tablicy
            for (int i = 0; i < Init.SIZE_WORLD; i++)
            {
                for (int j = 0; j < Init.SIZE_WORLD; j++)
                {
                    Console.WriteLine(w.board[i, j].getCreepersNum());
                }
                Console.WriteLine();
            }
        }

        private static void mainTest(World w)
        {
            //wyświetla konsolowo w układzie tablicy
            //liczbę bakterii i pełzaczy w danym takcie
            for (int i = 0; i < Init.SIZE_WORLD; i++)
            {
                for (int j = 0; j < Init.SIZE_WORLD; j++)
                {
                    Console.Write(w.board[i, j].getBactNum() + " | " + w.board[i, j].getCreepersNum() + "\t");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private static int totalNum(World w, string what)
        {
            //zwraca sumaryczną liczbę bakterii, lub pełzaczy, w całym świecie
            int sum = 0;
            for (int i = 0; i < Init.SIZE_WORLD; i++)
                for (int j = 0; j < Init.SIZE_WORLD; j++)
                    switch (what)
                    {
                        case "BACTERIA":
                            sum += w.board[i, j].getBactNum();
                            break;
                        case "CREEPERS":
                            sum += w.board[i, j].getCreepersNum();
                            break;
                    }
            return sum;
        }

        private static void addNewBornOrganismsToMainWorldCellules(World mainWorld, World tempWorld)
        {
            for (int i = 0; i < Init.SIZE_WORLD; i++)
            {
                for (int j = 0; j < Init.SIZE_WORLD; j++)
                {
                    mainWorld.board[i, j].addBactNum(tempWorld.board[i, j].getBactNum());
                    mainWorld.board[i, j].creepers.AddRange(tempWorld.board[i, j].creepers);
                }
            }
        }

        public static void Main(string[] args)
        {

            Console.WriteLine(new Random().NextDouble());

            World mainWorld = new World();
            World tempWorld; // dodatkowy świat potrzebny czasowo w trakcie creepersAndBacteriaAction
                             // do przechowywania nowo urodzonych bakterii (wszystkich) i pełzaczy (tylko tych
                             // które przemieszczają się do sąsiednich komórek), aby nie zwiększały populacji
                             // w niewylosowanych jeszcze komórkach.
                             // Po zakończaniu akcji pełzaczy i bakterii dla wszystkich komórek,
                             // bakterie i pełzacze z tempWorld są dodawane do odpowiednich komórek mainWorld.
                             // Przed każdym creepersAndBacteriaAction wykonywanym dla wszystkich komórek mainWorld,
                             // tworzony jest nowy, pusty tempWorld.

            //--------------------------------------------------------
            //kod do testowania - nie używany w standardowej symulacji
            //--------------------------------------------------------
            //        for (int i = 0; i < Init.SIZE_WORLD; i++) {
            //            for (int j=0; j < Init.SIZE_WORLD; j++) {
            //                mainWorld.setBacteriaNumAtPosition(5, i, j);
            //            }
            //        }
            //        mainWorld.setOneCreeperAtPosition(5, 5);
            //--------------------------------------------------------
            //kod do testowania - nie używany w standardowej symulacji
            //--------------------------------------------------------

            mainWorld.sowBacteries(Init.START_NUM_BACT);
            mainWorld.sowCreepers(Init.START_NUM_CREEPERS);
            int numTact = 0, num, totalBactNum;

            Console.WriteLine("Stan początkowy");
            mainTest(mainWorld);

            totallyCreepers.Add(totalNum(mainWorld, "CREEPERS"));
            totallyBacteria.Add(totalNum(mainWorld, "BACTERIA"));

            bool prematureEndOfSimulation = false;
            while (numTact < Init.NUM_TACT && !prematureEndOfSimulation)
            {
                num = 0;
                while (num < Init.VEW_NUM_TACT && !prematureEndOfSimulation)
                {
                    tempWorld = new World();
                    mainWorld.creepersAndBacteriaAction(mainWorld, tempWorld);
                    addNewBornOrganismsToMainWorldCellules(mainWorld, tempWorld);
                    totallyCreepers.Add(totalNum(mainWorld, "CREEPERS"));
                    totalBactNum = totalNum(mainWorld, "BACTERIA");
                    totallyBacteria.Add(totalBactNum);

                    if (totalBactNum > Init.BACT_NUM_LIMIT) prematureEndOfSimulation = true;

                    num++;
                    numTact++;
                }
                Console.WriteLine("Przebieg " + numTact);
                mainTest(mainWorld);
            }

            Console.WriteLine();
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("Bacterias");
            for (int i = 0; i < totallyCreepers.Count; i++)
            {
                Console.WriteLine(totallyBacteria[i]);
                //System.out.println(i + "  " + totallyCreepers.get(i) + "  " + totallyBacteria.get(i));
            }
            Console.WriteLine("Creepers");
            for (int i = 0; i < totallyCreepers.Count; i++)
            {
                Console.WriteLine(totallyCreepers[i]);
                //System.out.println(i + "  " + totallyCreepers.get(i) + "  " + totallyBacteria.get(i));
            }
            if (prematureEndOfSimulation) Console.WriteLine("Sumaryczna liczba bakterii przekroczyła " + Init.BACT_NUM_LIMIT
                    + " - komórki umierają/koniec symulacji.");
            Console.WriteLine();
        }
    }
}
