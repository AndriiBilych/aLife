using Newtonsoft.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using java.lang;
using System;
using System.Collections.Generic;
using System.IO;

namespace aLife
{
    class Init
    {
        //rozmiar świata - NIE ZMIENIAĆ
        public static int SIZE_WORLD = 10; 

        //całkowita liczba taktów
        public static int NUM_TACT = 100; 

        //co ile taktów wyświetla wyniki
        public static int VEW_NUM_TACT = 10;

        //początkowa liczba pełzaczy
        public static int START_NUM_CREEPERS = 500;

        //początkowa liczba bakterii
        public static int START_NUM_BACT = 500;

        //1  //ilość energii potrzebna do urodzenia nowego pełzacza
        public static int CREEPER_ENERGY_PRO_LIFE = 1;

        //1 //zapas eneergii nowo urodzonego pełzacza
        public static int CREEPER_INITIAL_ENERGY = 1; 

        //rezerwa energii zostawiana podczas rodzenia nowego pełzacza
        //potrzebna do przetrwania, gdy jest mało pożywienia
        public static int CREEPER_ENERGY_RESERVE = 4;

//        //5   maksymalna liczba pełzaczy rodzonych przez jednego
//                pełzacza w jednym takcie.
//                Liczba pełzaczy, które mogą się urodzić w jednym takcie jest ograniczona przez ilość energii pełzacza po
//                odjęciu CREEPER_ENERGY_RESERVE. Pełzacz nie może zgromadzić zbyt dużo energii, ponieważ gdy tylko
//                przekracza poziom energii równy CREEPER_ENERGY_PRO_LIFE + CREEPER_ENERGY_RESERVE w następnym takcie
//                rodzi co najmniej jednego pełzacza i jego poziom energii jest zmniejszany o CREEPER_ENERGY_PRO_LIFE
//                na każdego urodzonego pełzacza. 
                
        public static int MAX_CREEPER_NUM_BORN_PER_TACK = 5;

        //Maksymalna liczba bakterii zjadanych przez pełzacza
        //w jednym takcie
        public static int MAX_BACT_EATEN_BY_CREEPER = 12;

        //0.8 //współczynnik rozmnażania bakterii - tyle nowych bakterii
        //powstaje z jednej backterii w każdym takcie.
        //MOŻE PRZYJMOWAĆ WARTOŚCI UŁAMKOWE.
        public static double BACT_MULTIPLICATION_RATE = 1;

        //0.5 //współczynnik rozprzestrzeniania nowo urodzonych bakterii.
            //DOPUSZCZALNY ZAKRES: od 0 do 1
            //Np. przy wsp. = 0.7, 70% zostaje w komórce,
            //w której się urodziła, a 30% przenosi się
            //do sąsiednich losowo wybranych
        public static double BACT_SPREAD_RATE = 0.2;

        //graniczna liczba bakterii dla całego świata.
            //Po przekroczeniu tej liczby komórki umierają/koniec symulacji.
        public static int BACT_NUM_LIMIT = 1000000; 

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

        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };

        static readonly string ApplicationName = "aLife";

        static readonly string SpreadsheetId = "1ZZ5zZkgYlniNOsYBLtw1idzzabBmWU16oazJePBSURA";

        static readonly string sheet = "Sheet2";

        static SheetsService service;

        static string worldData = "";

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

        public static double RandomNumberBetween(double minValue, double maxValue)
        {
            var next = new Random().NextDouble();

            return minValue + (next * (maxValue - minValue));
        }

        private static List<IList<object>> WorldData(World w)
        {
            var oblist = new List<object>();
            List<IList<object>> values = new List<IList<object>>();

            //wyświetla konsolowo w układzie tablicy
            //liczbę bakterii i pełzaczy w danym takcie
            for (int i = 0; i < Init.SIZE_WORLD; i++)
            {
                oblist.Clear();
                oblist.TrimExcess();

                for (int j = 0; j < Init.SIZE_WORLD; j++)
                {
                    worldData += w.board[i, j].getBactNum() + " | " + w.board[i, j].getCreepersNum() + "\t";

                    oblist.Add(w.board[i, j].getBactNum() + " | " + w.board[i, j].getCreepersNum());
                }
                worldData += "\n";
                values.Add(new List<object>(oblist));

            }
            worldData += "\n";

            return values;
        }

        //Test function
        //static void ReadEntries()
        //{
        //    var range = $"{sheet}!A:F";
        //    SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

        //    var response = request.Execute();
        //    IList<IList<object>> values = response.Values;
        //    if (values != null && values.Count > 0)
        //    {
        //        foreach (var row in values)
        //        {
        //            // Print columns A to F, which correspond to indices 0 and 4.
        //            Console.WriteLine("{0} | {1} | {2} | {3} | {4} | {5}", row[0], row[1], row[2], row[3], row[4], row[5]);
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine("No data found.");
        //    }
        //}

        static void CreateEntry(List<IList<object>> values, string sheetRange)
        {
            var range = $"{sheet}!" + sheetRange;
            var valueRange = new ValueRange();

            valueRange.Values = values;

            var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendReponse = appendRequest.Execute();
        }

        static void UpdateEntry(List<IList<object>> values)
        {
            var valueRange = new ValueRange();
            
            valueRange.Values = values;

            var updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, $"{sheet}!A1:J10");
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var updateReponse = updateRequest.Execute();
        }

        static void DeleteEntry()
        {
            var deleteRequest = service.Spreadsheets.Values.Clear(new ClearValuesRequest(), SpreadsheetId, $"{sheet}!A1:M1500");
            var deleteReponse = deleteRequest.Execute();
        }

        public static void Main(string[] args)
        {

            GoogleCredential credential;
            using (var stream = new FileStream("aLife-5238216c8adf.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(Scopes);
            }

            // Create Google Sheets API service.
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            World mainWorld = new World();
            World tempWorld; // dodatkowy świat potrzebny czasowo w trakcie creepersAndBacteriaAction
                             // do przechowywania nowo urodzonych bakterii (wszystkich) i pełzaczy (tylko tych
                             // które przemieszczają się do sąsiednich komórek), aby nie zwiększały populacji
                             // w niewylosowanych jeszcze komórkach.
                             // Po zakończaniu akcji pełzaczy i bakterii dla wszystkich komórek,
                             // bakterie i pełzacze z tempWorld są dodawane do odpowiednich komórek mainWorld.
                             // Przed każdym creepersAndBacteriaAction wykonywanym dla wszystkich komórek mainWorld,
                             // tworzony jest nowy, pusty tempWorld.

            List<IList<object>> valuesForParsing = new List<IList<object>>(); //Data for parsing to spreadsheets

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

            DeleteEntry();

            worldData += "Stan początkowy\n";
            valuesForParsing.Add(new List<object> { "Stan początkowy" });
            valuesForParsing.AddRange(WorldData(mainWorld));

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
                worldData += "Przebieg " + numTact + "\t\t" + "Bacteria | Creepers\n";
                valuesForParsing.Add(new List<object> { "Przebieg " + numTact , "Bacteria | Creepers" });
                valuesForParsing.AddRange(WorldData(mainWorld));
            }

            Console.WriteLine(worldData);

            //Parsing runs
            CreateEntry(valuesForParsing, "A:J");

            valuesForParsing.Clear();

            string consoleOutput;

            
            Console.WriteLine("---------------------------------------");

            consoleOutput = "Run\tBacterias\tCreepers";
            valuesForParsing.Add(new List<object> { "Run", "Bacterias", "Creepers" });

            for (int i = 0; i < totallyCreepers.Count; i++)
            {
                consoleOutput += "\n" + i + "\t\t" + totallyBacteria[i] + "\t\t" + totallyCreepers[i];
                valuesForParsing.Add(new List<object> { i, totallyBacteria[i], totallyCreepers[i]});
            }

            Console.WriteLine(consoleOutput);

            CreateEntry(valuesForParsing, "K:L");

            if (prematureEndOfSimulation)
            {
                Console.WriteLine("Sumaryczna liczba bakterii przekroczyła "
                      + Init.BACT_NUM_LIMIT
                      + " - komórki umierają/koniec symulacji.");
                valuesForParsing.Add(new List<object> { "Sumaryczna liczba bakterii przekroczyła "
                      + Init.BACT_NUM_LIMIT
                      + " - komórki umierają/koniec symulacji." });
            }
            Console.WriteLine();

        }
    }
}
