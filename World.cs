using System;
using System.Collections.Generic;
using System.Text;

namespace aLife
{
    class World
    {
        public Cellule[,] board; // deklaracja zmiennej przechowującej tablicę komórek świata

        public World()
        { // konstruktor świata
            board = new Cellule[Init.SIZE_WORLD, Init.SIZE_WORLD];
            for (int i = 0; i < Init.SIZE_WORLD; i++)
            {
                for (int j = 0; j < Init.SIZE_WORLD; j++)
                {
                    board[i, j] = new Cellule();
                    board[i, j].setXPos(i);
                    board[i, j].setYPos(j);
                }
            }
        }

        public static bool isCelluleOut(int x, int y)
        {
            // bada czy komórka o wskazanej pozycji
            // nie jest poza tablicą komórek
            if ((x < 0) || (y < 0) || (x == Init.SIZE_WORLD) || (y == Init.SIZE_WORLD))
                return true;
            else return false;
        }

        //metoda do testowania - nie używana w standardowej symulacji
        void setBacteriaNumAtPosition(int bactNum, int posX, int posY)
        {
            board[posX, posY].setBactNum(bactNum);
        }

        //metoda do testowania - nie używana w standardowej symulacji
        void setOneCreeperAtPosition(int posX, int posY)
        {
            board[posX, posY].creepers.Add(new Creeper(posX, posY));
        }

        public void sowBacteries(int bactNumToSow)
        {
            // rozsiewa losowo bakterie w komórkach świata
            // w łącznej ilości równej ok. START_NUM_BACT (nie mniej niż)
            int allNumBact = 0;
            int randomNumBact;
            int iPos;
            int jPos;
            while (allNumBact < bactNumToSow)
            {
                iPos = new Random().Next(0, Init.SIZE_WORLD);
                jPos = new Random().Next(0, Init.SIZE_WORLD);
                randomNumBact = new Random().Next(0, Init.SIZE_WORLD);
                board[iPos, jPos].addBactNum(randomNumBact);
                allNumBact += randomNumBact;
            }
        }

        public void sowCreepers(int creepersNumToSow)
        {
            // rozsiewa losowo w komórkach świata początkową 
            // ilość pełzaczy określoną w START_NUM_CREEPERS
            int actualNumCreep = 0;
            int iPos;
            int jPos;
            while (actualNumCreep < creepersNumToSow)
            {
                iPos = new Random().Next(0, Init.SIZE_WORLD);
                jPos = new Random().Next(0, Init.SIZE_WORLD);
                board[iPos, jPos].creepers.Add(new Creeper(iPos, jPos));
                actualNumCreep++;
            }
        }

        public void creepersAndBacteriaAction(World w, World tempWorld)
        {
            //metoda realizuje akcje creeperAction() dla każdego pojedynczego 
            //pełzacza wywołując komórki świata losowo
            int allCellules = Init.SIZE_WORLD * Init.SIZE_WORLD;
            int numCellules = 0;
            int iPos;
            int jPos;
            // poniżej przygotowanie komórek do losowych odwiedzin
            for (int i = 0; i < Init.SIZE_WORLD; i++)
                for (int j = 0; j < Init.SIZE_WORLD; j++)
                    w.board[i, j].setOld(false);
            // poniżej pętla losowego wyboru komórek świata
            // z wywołaniem akcji creepersAction() dla jednej komórki i rozmnożeniem się pozostałych bakterii
            // w tej komórce lub najpierw rozmnożeniem się bakterii w tej komórce, a następnie
            // wywołaniem akcji creepersAction() dla jednej komórki (kolejność określana jest losowo
            // z prawdopodobieństwem 50%)
            while (numCellules < allCellules)
            {
                iPos = new Random().Next(0, Init.SIZE_WORLD);
                jPos = new Random().Next(0, Init.SIZE_WORLD);
                Cellule cell = w.board[iPos, jPos];

                if (!cell.getOld())
                { //komórka nie była odwiedzona
                    if (new Random().NextDouble() > 0.5)
                    {
                        cell.oneCelluleCreepersAction(w, tempWorld);
                        cell.oneCelluleBacteriaMultiplication(tempWorld);
                    }
                    else
                    {
                        cell.oneCelluleBacteriaMultiplication(tempWorld);
                        cell.oneCelluleCreepersAction(w, tempWorld);
                    }

                    cell.setOld(true); numCellules++;
                }
            }
        }
    }
}
