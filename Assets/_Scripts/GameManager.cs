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
                    }
                }
            }

            this.bombCount = new int[currentRes.width,currentRes.height];
            //reset the thingy
            bombs.Clear();
            firstClick = true;
        }

        void ResetGameFor(int index)
        {
            print("reee");
            //do this first.
            var res = gameSizes[index];

            //adjust the window size:
            int width = (int) Mathf.Max(minWidth, res.width * (tileSize + tileOffset));
            print(width);
            int height = res.height * (int)(tileSize + tileOffset) + marginTop + marginBottom;
            print(height);
            Screen.SetResolution(width, height, false);

            //init the game last.
            bool change = res != currentRes;
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
            bool clickIsBomb = bombs.Contains(coords);
            if(clickIsBomb)
            {
                //lose the game hahahaha noob >:)
                Lose();
                return "B";
            }
            int c = bombCount[coords.x, coords.y];
            if(c == 0)
            {
                //todo: reveal around this one.
                return "";
            }
            return c.ToString(); //c: count of bombs around this location.
        }
        
        private void Lose()
        {
            //TODO: lose the game.
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
                        if (fy == 0 && fy == fx) //is self
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
