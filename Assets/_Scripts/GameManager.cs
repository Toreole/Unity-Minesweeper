using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

namespace Minesweeper
{
    /// <summary>
    /// The main component that runs the game.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        //private instance for static callbacks.
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
            InitGame(true);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Testing generation. Not used anymore.
        /// </summary>
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
                        if (tile.isBomb)
                            tile.Reveal("B"); //is bomb
                        else //no bomb
                            tile.Reveal(bombCount[x, y]);
                    }
                }
            }
        }
#endif

        /// <summary>
        /// Initialize the game.
        /// </summary>
        /// <param name="resChanged">Has the resolution / gamesize changed?</param>
        void InitGame(bool resChanged)
        {
            if (resChanged) //res has changed
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
                        temp.isBomb = false;
                    }
                }
            }
            else //size has not changed.
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
            firstClick = true;
        }

        /// <summary>
        /// Reset the Game.
        /// </summary>
        public void ResetGame()
        {
            ResetGameFor(resDropdown.value);
            loseScreen.SetActive(false);
            winScreen.SetActive(false);
        }

        /// <summary>
        /// Reset the game using the resolution and the given index.
        /// </summary>
        /// <param name="index">index of the resolution</param>
        void ResetGameFor(int index)
        {
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

        /// <summary>
        /// Process a click on a tile.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
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
            //bool clickIsBomb = bombs.Exists(x => x == coords);
            if(tileMap[coords.x, coords.y].isBomb)
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
                //FloodRevealQueue(coords); outdated and weirdly slow
                CompletionCheck();
                return "";
            }
            //todo: check if the game is over.
            CompletionCheck();
            return c.ToString(); //c: count of bombs around this location.
        }
        
        /// <summary>
        /// Flood Reveal with a queue. Slow'n'Stupid.
        /// </summary>
        /// <param name="start">the position at which so start the fill</param>
        //[System.Obsolete] <- no actual attribute to avoid warnings lol
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
                if(tileMap[current.x, current.y].isBomb) //check if the current check is a bomb
                {
                    continue;
                }
                tileMap[current.x, current.y].Reveal(bc);
            }

            ///Gather the next bunch of tiles.
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

        /// <summary>
        /// Flood Reveal all connected "0" tiles and their surroundings. Uses Recursion.
        /// </summary>
        /// <param name="coords">the start coordinates.</param>
        void FloodReveal(Vector2Int coords)
        {
            Tile tile; //tile
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
                    tile.Reveal(bc); //reveal this tile.
                    if (bc == 0)//this is a "0" tile! reveal it's surroundings.
                    {
                        FloodReveal(new Vector2Int(fx, fy));
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
            for(int x = 0; x < currentRes.width; x++)
            {
                for(int y = 0; y < currentRes.height; y++)
                {
                    t = tileMap[x, y];
                    //! bomb check doesnt need to be done -> already in Click()
                    if (!t.isBomb) //tile not a bomb
                    {
                        if(t.clicked) //NOTE: could be rewritten do if(!t.clicked) return; but i like the way it looks rn.
                            continue;//field has been revealed. -> win condition.
                        return;//field has not been revealed -> not over yet.
                    }
                }
            }
            //-> all tiles are revealed, but no bombs -> game is won.
            DoWin();
        }

        /// <summary>
        /// Show the losing screen.
        /// </summary>
        private void DoLose()
        {
            loseScreen.SetActive(true);
        }


        /// <summary>
        /// Show the win screen.
        /// </summary>
        private void DoWin()
        {
            winScreen.SetActive(true);
        }

        /// <summary>
        /// Generates the bombs on the map. Can be relatively slow!
        /// </summary>
        /// <param name="start"></param>
        //TODO: could there be a better way to do this?
        void GenerateMap(Vector2Int start)
        {
            //bomb count is equal to the amount of tiles divided by some factor.
            int bc = (currentRes.height * currentRes.width) / 8;
            //print(bombCount); 400 / 8 = 50, seems better, but still too easy lets see...
            while(bc > 0)
            {
                //random location for the bomb:
                Vector2Int bombLocation = GetUniqueBombLocationOutsideCenter(start);

                bc--; //one less bomb to generate.
                tileMap[bombLocation.x, bombLocation.y].isBomb = true;
                
                //increase the "surrounding bombs" data of all tiles touching the bombLocation.
                for(int x = -1; x <= 1; x++)//horizontal offset.
                {
                    int fx = bombLocation.x + x;
                    if (fx < 0 || fx >= currentRes.width) //outside of bounds
                        continue;
                    //vertical offset
                    for(int y = -1; y <= 1; y++)//vertical offset.
                    {
                        int fy = bombLocation.y + y;
                        if (fy < 0 || fy >= currentRes.height) //outside of bounds
                            continue;
                        this.bombCount[fx, fy] += 1;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a random location on the map.
        /// </summary>
        /// <returns>the location</returns>
        Vector2Int GetRandomLocation()
        {
            return new Vector2Int(Random.Range(0, currentRes.width - 1), Random.Range(0, currentRes.height - 1));
        }

        /// <summary>
        /// Brute force a new unique bomb location recursively.
        /// Might take a bunch of attempts.
        /// </summary>
        /// <param name="center">the center to avoid.</param>
        /// <returns>a unique location</returns>
        Vector2Int GetUniqueBombLocationOutsideCenter(Vector2Int center)
        {
            Vector2Int val = new Vector2Int();
            val.x = Random.Range(0, currentRes.width - 1);
            val.y = Random.Range(0, currentRes.height - 1);
            if (tileMap[val.x, val.y].isBomb || (InRange(val.x, center.x, 1) && InRange(val.y, center.y, 1)))
                return GetUniqueBombLocationOutsideCenter(center);
            return val;
        }

        //small helper.
        bool InRange(int point, int center, int radius) => Mathf.Abs(center - point) <= radius;
    }

    [System.Serializable]
    public class GameRes
    {
        public int width, height;
    }
}
