using System;
using System.Collections.Generic;
using System.Text;

namespace aLife
{
    class Cellule
    {
        private int xPos;
        private int yPos;
        private int bacteriaNum;
        private bool old;
        public List<Creeper> creepers; //lista pełzaczy w komórce

        public Cellule()
        {
            creepers = new List<Creeper>();
        }

        public void setXPos(int i) { xPos = i; }
        public void setYPos(int j) { yPos = j; }

        public bool getOld() { return old; }
        public void setOld(bool old) { this.old = old; }

        //public void clearAux(){auxList.clear();}
        public int getBactNum() { return bacteriaNum; }

        public void setBactNum(int bactNum) { bacteriaNum = bactNum; }

        public void addBactNum(int bactNum) { bacteriaNum += bactNum; }

        public void reduceBactNum(int bactNum)
        {
            bacteriaNum -= bactNum;
            if (bacteriaNum < 0)
            {
                bacteriaNum = 0;
                Console.WriteLine("UWAGA - miała miejsce próba usunięcia z komórki większej liczby bakterii " +
                        "niż w niej się znajdowało");
                //ten komunikat może ułatwić znalezienie błędów powstałych podczas poprawiania algorytmu
                //nie może pojawić się nigdy, gdy symulacja przebiega prawidłowo
            }
        }

        public int getCreepersNum() { return creepers.Count; }

        public void addCreeper(Creeper newCreeper) { creepers.Add(newCreeper); }

        public void moveCreeper(Creeper existingCreeper)
        {
            existingCreeper.setXPos(this.xPos);
            existingCreeper.setYPos(this.yPos);
            creepers.Add(existingCreeper);
        }

        public void removeCreeper(Creeper creeper)
        {
            if (!(creepers.Count == 0))
            {
                creepers.Remove(creeper);
            }
            else
            {
                Console.WriteLine("UWAGA - miała miejsce próba usunięcia pełzacza z komórki, w której nie ma pełzaczy");
                //ten komunikat może ułatwić znalezienie błędów powstałych podczas poprawiania algorytmu
                //nie może pojawić się nigdy, gdy symulacja przebiega prawidłowo
            }
        }

        public void oneCelluleCreepersAction(World w, World tempWorld)
        {
            for (int i = creepers.Count - 1; i >= 0; i--) //iterowanie od ostatniego do pierwszego pełzacza
                creepers[i].creeperAction(w, tempWorld);
        }

        public void oneCelluleBacteriaMultiplication(World tempWorld)
        {
            int newBacteriaNumber, newBacteriaStayingInCell, remainingNewBacteriaMovingToNewCells;
            //nowo urodzone bakterie nie będą mogły być zjedzone w tym takcie, w którym się urodziły
            //ponieważ trafiają do tempWorld. Dzięki temu symualcja jest stabilniejsza.

            if (bacteriaNum > 0)
            {
                newBacteriaNumber = (int)Math.Round(bacteriaNum * Init.BACT_MULTIPLICATION_RATE);
                //liczba nowo urodzonych bakterii

                newBacteriaStayingInCell = (int)(newBacteriaNumber * Init.BACT_SPREAD_RATE);
                tempWorld.board[xPos, yPos].addBactNum(newBacteriaStayingInCell);
                //część nowo urodzonych bakterii określona przez BACT_SPREAD_RATE, która pozostaje
                //w komórce macierzystej (jej odpowiedniku w tempWorld)

                remainingNewBacteriaMovingToNewCells = newBacteriaNumber - newBacteriaStayingInCell;
                //pozostałe nowourodzone bakterie, które maja się przenieść do sąsiednich komórek

                goToNewCellules(tempWorld, remainingNewBacteriaMovingToNewCells);
            }
        }

        private void goToNewCellules(World tempWorld, int remainingNewBacteriaMovingToNewCells)
        {
            int newXPos, newYPos, newBacteriaPartMovingToNewCell;
            List<LocationModifier> locationModifiers = Init.initializedLocationModifiersList();

            List<LocationModifier> existingCellulesLocationModifiers = new List<LocationModifier>();
            //powyżej lista modyfikatorów położenia dających tylko komórki należące do świata

            for (int it = 0; it < locationModifiers.Count; it++)
            {
                newXPos = xPos + locationModifiers[it].modifyX;
                newYPos = yPos + locationModifiers[it].modifyY;

                if (!World.isCelluleOut(newXPos, newYPos))
                    existingCellulesLocationModifiers.Add(locationModifiers[it]);
            }

            //losowanie bez powtórzeń
            int i;
            while (existingCellulesLocationModifiers.Count > 0 && remainingNewBacteriaMovingToNewCells > 0)
            {
                i = (int)(new Random().NextDouble() * existingCellulesLocationModifiers.Count);
                newXPos = xPos + existingCellulesLocationModifiers[i].modifyX;
                newYPos = yPos + existingCellulesLocationModifiers[i].modifyY;

                if (existingCellulesLocationModifiers.Count == 1)
                {
                    tempWorld.board[newXPos, newYPos].addBactNum(remainingNewBacteriaMovingToNewCells);
                    // jeżeli jest to ostatnia komórka, do której moga się przenieść bakterie,
                    // które mają się przenieść poza komórkę macierzystą, to przenoszą się do niej wszystkie
                    // bakterie, które nie przeniosły się jeszcze do żadnej z sąsiadujących komórek
                    remainingNewBacteriaMovingToNewCells = 0;
                }
                else
                {
                    newBacteriaPartMovingToNewCell = (int)(new Random().NextDouble() * remainingNewBacteriaMovingToNewCells);
                    // losowo określona część nowo urodzonych bakterii, która przeniesie się do nowej komórki
                    tempWorld.board[newXPos, newYPos].addBactNum(newBacteriaPartMovingToNewCell);

                    remainingNewBacteriaMovingToNewCells -= newBacteriaPartMovingToNewCell;
                    // od pozostałych nowo urodzonych bakterii, które mają się przenieść do nowych komórek
                    // odejmujemy te, które właśnie przeniosły się do nowej komórki
                }
                existingCellulesLocationModifiers.RemoveAt(i);
            }
        }
    }
}
