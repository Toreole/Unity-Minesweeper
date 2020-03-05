using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

namespace Minesweeper
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        [SerializeField]
        protected int marginTop, marginBottom;
        [SerializeField]
        protected float tileSize, tileOffset;
        [SerializeField]
        protected float minWidth;

        [SerializeField]
        protected GameRes[] gameSizes;

        [SerializeField]
        protected int defaultSize = 0;

        [SerializeField]
        protected Image mainBackground;

        [SerializeField]
        protected GameObject tilePrefab;
        [SerializeField]
        protected Transform tileHolder;

        [SerializeField]
        protected TMPro.TMP_Dropdown resDropdown;

        [SerializeField]
        protected GameObject winScreen, loseScreen;

        //runtime.
        private List<Vector2Int> bombs;
        private Tile[,] tileMap;
        private int[,] bombCount;
        private GameRes currentRes;
        private bool firstClick = true;

        private void Start()
        {
            _instance = this;
            Screen.SetResolution(600, 600, false);
            var path = Path.Combine(Application.dataPath, "background.jpg");

            if (File.Exists(path)) //file doesnt exist, abort mission!
            {
                //read the contents.
                byte[] data = File.ReadAllBytes(path);
                // get the texture
                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(data);
                //www.LoadImageIntoTexture(tex);
                mainBackground.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one / 2f, 100);
                //todo ?? adjust the height of the image in the UI:
                //float correctHeight = ((float)tex.height / (float)tex.width )* mainBackground.rectTransform.rect.width;
                //mainBackground.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, correctHeight);
            }
            //setup reset n such:
            resDropdown.onValueChanged.AddListener(ResetGameFor);
            
            currentRes = gameSizes[defaultSize];
            bombs = new List<Vector2Int>();
            InitGame(true);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (firstClick)
                return;
            if (Input.GetKeyDown(KeyCode.F1))
            {
                for (int x = 0; x < currentRes.width; x++)
                {
                    for (int y = 0; y < currentRes.height; y++)
                    {
                        Tile tile = tileMap[x, y];
                        if (bombs.Exists(b => b == new Vector2Int(x, y)))
                            tile.Reveal("B"); //is bomb
                        else //no bomb
                            tile.Reveal(bombCount[x, y]);
                    }
                }
            }
        }
#endif

        void InitGame(bool resChanged)
        {
            if (resChanged)
            {
                var holder = tileHolder as RectTransform;
                //adjust the size so it BARELY fits.
                holder.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentRes.width * (tileOffset + tileSize));
                holder.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currentRes.height * (tileOffset + tileSize));

                //Clear the tiles currently in-game.
                for (int i = tileHolder.childCount - 1; i >= 0; i--)
                {
                    Destroy(tileHolder.GetChild(i).gameObject);
                }
                //instantiate the tiles.
                tileMap = new Tile[currentRes.width, currentRes.height];
                for (int y = 0; y < currentRes.height; y++)
                {
                    for (int x = 0; x < currentRes.width; x++)
                    {
                        //get the tile components and set their things.
                        Tile temp = Instantiate(tilePrefab, tileHolder).GetComponent<Tile>();
                        tileMap[x, y] = temp;
                        temp.coords = new Vector2Int(x, y);
                        temp.clicked = false; //double check
                    }
                }
            }
            else
            {
                //reset the field. 
                for (int y = 0; y < currentRes.height; y++)
                {
                    for (int x = 0; x < currentRes.width; x++)
                    {
                        Tile t = tileMap[x, y];
                        t._Reset();
                    }
                }
            }

            this.bombCount = new int[currentRes.width,currentRes.height];
            //reset the thingy
            bombs.Clear();
            firstClick = true;
        }

        public void ResetGame()
        {
            ResetGameFor(resDropdown.value);
            loseScreen.SetActive(false);
            winScreen.SetActive(false);
        }

        void ResetGameFor(int index)
        {
            print("reee");
            //do this first.
            var res = gameSizes[index];
            bool change = res != currentRes;

            if (change)
            {
                //adjust the window size:
                int width = (int)Mathf.Max(minWidth, res.width * (tileSize + tileOffset));
                print(width);
                int height = res.height * (int)(tileSize + tileOffset) + marginTop + marginBottom;
                print(height);
                Screen.SetResolution(width, height, false);
            }

            //init the game last.
            currentRes = res;
            InitGame(change);
        }

        //static interface for Tiles.
        public static string Click(Vector2Int coords) => _instance._Click(coords);

        /// <summary>
        /// Handles when a tile is clicked.
        /// </summary>
        /// <param name="coords">the grid coordinates of the tile</param>
        /// <returns>count of bombs surrounding this tile as string</returns>
        private string _Click(Vector2Int coords)
        {
            if(firstClick) //is this the first click? then generate the map based on this
            {
                firstClick = false;
                GenerateMap(coords);
            }
            //is the click a bomb??
            bool clickIsBomb = bombs.Exists(x => x == coords);
            if(clickIsBomb)
            {
                print("clicked a bomb");
                //lose the game hahahaha noob >:)
                DoLose();
                return "B";
            }
            int c = bombCount[coords.x, coords.y];
            if(c == 0)
            {
                //reveal around this one.
                FloodReveal(coords);
                //FloodRevealQueue(coords);
                return "";
            }
            //todo: check if the game is over.
            CompletionCheck();
            return c.ToString(); //c: count of bombs around this location.
        }
        
        //FloodReveal using a queue.
        void FloodRevealQueue(Vector2Int start)
        {
            Queue<Vector2Int> tilesToCheck = new Queue<Vector2Int>();
            tilesToCheck.Enqueue(start); //enqueue the first one.

            while(tilesToCheck.Count > 0) //while there are still objects in the queue, continue operating.
            {
                Vector2Int current = tilesToCheck.Dequeue();
                int bc = bombCount[current.x, current.y];
                if(bc == 0)
                {
                    GatherAround(current);
                }
                if(bombs.Exists(x => x == current)) //check if the current check is a bomb
                {
                    continue;
                }
                tileMap[current.x, current.y].Reveal(bc);
            }

            void GatherAround(Vector2Int c)
            {
                for (int x = -1; x <= 1; x++) //offset on x
                {
                    int fx = c.x + x;
                    if (fx < 0 || fx >= currentRes.width) //out of bounds
                        continue;
                    for (int y = -1; y <= 1; y++) //offset on y
                    {
                        int fy = c.y + y;
                        if (fy < 0 || fy >= currentRes.height) //out of bounds
                            continue;
                        if (y == 0 && x == 0) //dont reveal self.
                            continue;
                        if(!tileMap[fx, fy].clicked) //only if it hasnt been revealed yet
                        {
                            tilesToCheck.Enqueue(new Vector2Int(fx, fy));
                        }
                    }
                }
            }
        }

        //recursive reveal.
        void FloodReveal(Vector2Int coords)
        {
            Tile tile;
            int bc = 0;
            for(int x = -1; x <= 1; x++) //offset on x
            {
                int fx = coords.x + x;
                if (fx < 0 || fx >= currentRes.width) //out of bounds
                    continue;
                for(int y = -1; y <= 1; y++) //offset on y
                {
                    int fy = coords.y + y;
                    if (fy < 0 || fy >= currentRes.height) //out of bounds
                        continue;
                    if (y == 0 && x == 0) //dont reveal self.
                        continue;

                    bc = bombCount[fx, fy]; //amount of bombs around this tile.
                    tile = tileMap[fx, fy]; //bordering tile

                    if (tile.clicked) //is already revealed
                        continue;
                    tile.Reveal(bc);
                    //reveal the tile.
                    if (bc == 0)
                    {
                        FloodReveal(new Vector2Int(fx, fy)); //reveal around the next tile.
                    }
                }
            }
        }

        /// <summary>
        /// Run a completion check.
        /// </summary>
        void CompletionCheck()
        {
            Tile t; 
            for(int x = 0; x <= currentRes.width; x++)
            {
                for(int y = 0; y <= currentRes.height; y++)
                {
                    t = tileMap[x, y];
                    //! bomb check doesnt need to be done -> already in Click()
                    if (!t.isBomb) //tile not a bomb
                    {
                        if(t.clicked)
                            continue;//field has been revealed. -> win condition.
                        return;//field has not been revealed -> not over yet.
                    }
                }
            }
            //-> all tiles are revealed, but no bombs -> game is won.
            DoWin();
        }

        private void DoLose()
        {
            loseScreen.SetActive(true);
        }

        private void DoWin()
        {
            winScreen.SetActive(true);
        }

        /// <summary>
        /// Generates the bombs on the map.
        /// </summary>
        /// <param name="start"></param>
        //todo: this apparently freezes unity....
        void GenerateMap(Vector2Int start)
        {
            //bomb count is equal to the amount of tiles divided by some factor.
            int bc = (currentRes.height * currentRes.width) / 8;
            //print(bombCount); 400 / 8 = 50, seems better, but still too easy lets see...
            while(bc > 0)
            {
                //random location for the bomb:
                Vector2Int bombLocation = GetUniqueBombLocationOutsideCenter(start);
                bombs.Add(bombLocation);
                bc--;
                tileMap[bombLocation.x, bombLocation.y].isBomb = true;
                //horizontal offset
                for(int x = -1; x <= 1; x++)
                {
                    int fx = bombLocation.x + x;
                    if (fx < 0 || fx >= currentRes.width) //outside of bounds
                        continue;
                    //vertical offset
                    for(int y = -1; y <= 1; y++)
                    {
                        int fy = bombLocation.y + y;
                        if (fy < 0 || fy >= currentRes.height) //outside of bounds
                            continue;
                        this.bombCount[fx, fy] += 1;
                    }
                }
            }
        }

        Vector2Int GetRandomLocation()
        {
            return new Vector2Int(Random.Range(0, currentRes.width - 1), Random.Range(0, currentRes.height - 1));
        }

        //brute force random locations on the field.
        Vector2Int GetUniqueBombLocationOutsideCenter(Vector2Int center)
        {
            Vector2Int val = new Vector2Int();
            val.x = Random.Range(0, currentRes.width - 1);
            val.y = Random.Range(0, currentRes.height - 1);
            if (bombs.Exists(x => x.Equals(val)) || (InRange(val.x, center.x, 1) && InRange(val.y, center.y, 1)))
                return GetUniqueBombLocationOutsideCenter(center);
            return val;
        }

        bool InRange(int point, int center, int radius) => Mathf.Abs(center - point) <= radius;
    }

    [System.Serializable]
    public class GameRes
    {
        public int width, height;
    }
    public class Int2
    {
        public int x, y;

        public Int2()
        {
            this.x = 0;
            this.y = 0;
        }
        public Int2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
