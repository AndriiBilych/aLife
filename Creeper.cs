using System;
using System.Collections.Generic;
using System.Text;

namespace aLife
{
    class Creeper
    {

        //klasa potrzebna do zwrócenia przez metodę checkNewCellules położenia komórki
        //zawierającej najwiecej bakterii lub współrzędnych posX = -1, posY = -1,
        //gdy nie ma sąsiednich komórek zawierających bakterie
        class CelluleLocation
        {
            public int posX, posY;

            public CelluleLocation(int posX, int posY)
            {
                this.posX = posX;
                this.posY = posY;
            }
        }

        private int xPos;
        private int yPos;
        private int creeperEnergy; //energia potrzebna do rozmnażania, przemieszczenia się
                                   //i pozwalająca przetrwać jakiś czas, gdy jest mało pożywienia

        public Creeper(int x, int y)
        {
            xPos = x; yPos = y;
            creeperEnergy = Init.CREEPER_INITIAL_ENERGY;
        }

        public void setXPos(int newXPos) { xPos = newXPos; }
        public void setYPos(int newYPos) { yPos = newYPos; }

        private void addCreeperEnergy(int bactNum)
        {
            //można wprowadzić parametr określający ile energii zyska pełzacz po zjedzeniu każdej bakterii.
            //Np. po zjedzeniu jednej bakterii może zyskać 2 jednostki energii lub 0,5 jednostki energii,
            //czyli this.creeperEnergy += 2*bactNum lub this.creeperEnergy += 0.5*bactNum
            this.creeperEnergy += bactNum;
        }

        public void reduceCreeperEnergy(int energy)
        {
            this.creeperEnergy -= energy;
            if (this.creeperEnergy < 0) this.creeperEnergy = 0;
        }

        public bool creeperHasEnergy()
        {
            return creeperEnergy > 0;
        }

        private bool creeperHasEnoughEnergyToMakeNewCreeper()
        {
            return creeperEnergy >= (Init.CREEPER_ENERGY_PRO_LIFE + Init.CREEPER_ENERGY_RESERVE);
        }

        //    private void goToNewCellule(World w, World tempWorld, int x, int y, int newX, int newY){
        //        //główna akcja dla pełzacza, który nie może urodzić
        //
        //    }

        private CelluleLocation checkNewCellules(World w, World tempWorld)
        {
            //metoda bada czy sąsiednie komórki zawierają bakterie. Jeśli tak, znajduje komórkę
            //zawierającą największą liczbę bakterii. Jeśli jest kilka takich komórek,
            //wybierana jest losowo jedna z nich, pełzacz do niej przechodzi i metoda zwraca true.
            //Jeżeli żadna z sąsiednich komórek nie zawiera bakterii, pełzacz nie przemieszcza się
            //i metotda zwraca false.

            //wczytanie tablicy modyfikatorów położenia - będą z niej losowane, bez powtórzeń,
            // modyfikatory położenia komórek
            List<LocationModifier> locationModifiers = Init.initializedLocationModifiersList();

            int i, newXPos, newYPos, bestXPos, bestYPos, bactNum;

            //Modyfikatory losowane są do momentu wyczerpania listy modyfikatorów położenia,
            //tak aby sprawdzone były wszystkie komórki z listy modyfikatorów w celu znalezienia
            //komórki zawierającej największą liczbę bakterii
            bestXPos = -1;
            bestYPos = -1;
            bactNum = 0;
            while (locationModifiers.Count > 0)
            {
                i = (int)(new Random().NextDouble() * locationModifiers.Count);
                newXPos = xPos + locationModifiers[i].modifyX;
                newYPos = yPos + locationModifiers[i].modifyY;

                if (!World.isCelluleOut(newXPos, newYPos))
                    if (w.board[newXPos, newYPos].getBactNum() > bactNum)
                    {
                        bactNum = w.board[newXPos, newYPos].getBactNum();
                        bestXPos = newXPos;
                        bestYPos = newYPos;
                    }

                locationModifiers.RemoveAt(i);
            }

            //gdy w sąsiednich komórkach nie ma bakterii zwracana jest komórka
            //o współrzędnych posX = -1, posY = -1
            return new CelluleLocation(bestXPos, bestYPos);
        }

        public void creeperAction(World w, World tempWorld)
        {
            // W jednym takcie pełzacz wykonuje jedną z 5 akcji (w podanej niżej kolejności):
            // a) urodzenie co najmniej jednego pełzacza i zmniejszenie swojej energii,
            // b) zjedzenie bakterii i zwiększenie swojej energii,
            // c) przemieszczenie się do sąsiedniej komórki o największej liczbie bakterii i zmniejszenie swojej energii,
            // d) jeśli sąsiednie komórki nie zawierają bakterii, pełzacz czeka i zmniejsza swoją energię,
            // e) pełzacz umiera jeśli nie ma już energii (creeperEnergy = 0) i nie ma baketrii do zjedzenia.

            Creeper tempCreeper; //potrzebny do chwilowego przechowania pełzacza przy przenoszeniu do
                                 //do innej komórki. Ze względu na modyfikację współrzednych
                                 //określających położenie pełzacza, trzeba najpierw pełzacza usunąć
                                 //z aktualnej komórki, a potem dodać do docelowej komórki. Przed jego
                                 //usunięciem z aktualnej komórki jest zachowany w tempCreeper,
                                 //aby można go było dodać do docelowej komórki.
            Cellule currentCellule = w.board[xPos, yPos];
            int currentCelluleBactNum;
            int bestXPos, bestYPos; //współrzędne komórki z największą liczbą bakterii
            CelluleLocation celluleWithHighestBactNum;

            if (creeperHasEnoughEnergyToMakeNewCreeper())
            {
                // a) jeżeli pełzacz ma wystarczającą ilość energii to rodzi co najmnej jednego nowego pełzacza,
                // i jego energia jest zmniejszana o CREEPER_ENERGY_PRO_LIFE na każdego urodzonego pełzacza
                int bornCreepersNum = 0;
                while (creeperHasEnoughEnergyToMakeNewCreeper() && bornCreepersNum <= (Init.MAX_CREEPER_NUM_BORN_PER_TACK - 1))
                {
                    currentCellule.addCreeper(new Creeper(xPos, yPos));
                    reduceCreeperEnergy(Init.CREEPER_ENERGY_PRO_LIFE);
                    bornCreepersNum++;
                }
            }
            // b) jeżeli pełzacz nie ma wystarczającej ilości energii do urodzenia nowego pełzacza
            // to, jeżeli w komórce są bakterie, to je zjada (ale nie więcej niź MAX_BACT_EATEN_BY_CREEPER)
            // i zyskuje ilość energii odpowiadającą liczbie zjedzonych bakterii
            else
            {
                currentCelluleBactNum = currentCellule.getBactNum();
                if (currentCelluleBactNum > 0)
                {
                    if (currentCelluleBactNum >= Init.MAX_BACT_EATEN_BY_CREEPER)
                    {
                        addCreeperEnergy(Init.MAX_BACT_EATEN_BY_CREEPER);
                        currentCellule.reduceBactNum(Init.MAX_BACT_EATEN_BY_CREEPER);
                    }
                    else
                    { //bakterii jest mniej niż MAX_BACT_EATEN_BY_CREEPER
                        addCreeperEnergy(currentCelluleBactNum);
                        currentCellule.reduceBactNum(currentCelluleBactNum);
                    }
                }
                // W komórce nie ma bakterii - są jeszcze 3 ostatnie mozliwości.
                // Jeżeli pełzacz ma jeszcze energię:
                // c) sprawdza sąsiednie komórki i przenosi się to tej, w której jest najwięcej jedzenia
                //    a jego energia zmniejsza się (np. o jedną jednostę energii).
                // d) jeżeli sąsiednie komórki nie zawierają bakterii, to energia pełzacza zmniejszana
                //    jest np. o jeden, ale pełzacz przeżywa i ma szansę na zjedzenie bakterii w następnym takcie.
                // Jeżeli pełzacz nie ma energii:
                // e) ginie.
                else
                {
                    if (creeperHasEnergy())
                    {
                        celluleWithHighestBactNum = checkNewCellules(w, tempWorld);
                        bestXPos = celluleWithHighestBactNum.posX;
                        bestYPos = celluleWithHighestBactNum.posY;
                        if (bestXPos > -1 && bestYPos > -1)
                        { //w sąsiednich komórkach są bakterie
                            reduceCreeperEnergy(1); //energia zużyta na przeniesie pełzacza do nowej komórki
                            tempCreeper = this;
                            w.board[xPos, yPos].removeCreeper(this);
                            tempWorld.board[bestXPos, bestYPos].moveCreeper(tempCreeper);
                        }
                        else
                        { //w sąsiednich komórkach nie ma bakterii
                            reduceCreeperEnergy(1); //energia zużyta na przetrwanie taktu bez jedzenia <- d)
                        }
                    }
                    else currentCellule.removeCreeper(this); // pełzacz ginie <- e)
                }
            }
        }
    }
}
