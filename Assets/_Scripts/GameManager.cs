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
        protected float marginTop, marginBottom;
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
        private HashSet<Vector2Int> revealedTiles;
        private Tile[,] tileMap;
        private GameRes currentRes;

        private void Start()
        {
            _instance = this;

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

            revealedTiles = new HashSet<Vector2Int>();
            currentRes = gameSizes[defaultSize];
            InitGame(true);
        }

        void InitGame(bool resChanged)
        {
            if (resChanged)
            {
                //Clear the tiles currently in-game.
                for (int i = tileHolder.childCount - 1; i >= 0; i--)
                {
                    Destroy(tileHolder.GetChild(i).gameObject);
                }
                //instantiate the tiles.
                tileMap = new Tile[currentRes.width, currentRes.height];
                for (int y = 0; y < currentRes.height; y++)
                {
                    for(int x = 0; x < currentRes.width; x++)
                    {
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
            }
            //reset the thingy
            revealedTiles.Clear();
        }

        void ResetGameFor(int index)
        {
            print("reee");
            //do this first.
            var res = gameSizes[index];

            //adjust the window size:
            int width = (int) Mathf.Max(minWidth, res.width * (tileSize + tileOffset));
            int height = res.height * (int)(tileSize + tileOffset);
            Screen.SetResolution(width, height, false);

            //init the game last.
            bool change = res != currentRes;
            currentRes = res;
            InitGame(change);
        }
    }

    [System.Serializable]
    public class GameRes
    {
        public int width, height;
    }
}
