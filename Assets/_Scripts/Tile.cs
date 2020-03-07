using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Minesweeper
{
    /// <summary>
    /// An interactive Tile for Minesweeper.
    /// </summary>
    public class Tile : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        protected TextMeshProUGUI textField;
        [SerializeField]
        protected Image image;
        [SerializeField]
        protected Color defaultColor, highlightColor, revealedColor;

        [HideInInspector, System.NonSerialized]
        public bool clicked = false;
        [HideInInspector, System.NonSerialized]
        public Vector2Int coords;
        [HideInInspector, System.NonSerialized]
        public bool isBomb = false;

        bool flagged = false;

        void Awake()
        {
            //make sure to reset the default color.
            image.color = defaultColor;
        }

        /// <summary>
        /// Handle the click.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if(!clicked)
            {
                //left click -> reveal.
                if (eventData.button == PointerEventData.InputButton.Left && !flagged)
                {
                    clicked = true;
                    textField.text = GameManager.Click(coords);
                    image.color = revealedColor;
                }
                //right click -> flag this tile.
                else if (eventData.button == PointerEventData.InputButton.Right)
                {
                    flagged = !flagged;
                    textField.text = flagged ? "?" : "";
                    textField.color = flagged ? Color.red : Color.black;
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (clicked)
                return;
            image.color = highlightColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (clicked)
                return;
            image.color = defaultColor;
        }

        /// <summary>
        /// Force reveal.
        /// </summary>
        /// <param name="x">the number of bombs around this tile.</param>
        public void Reveal(int x)
        {
            clicked = true;
            if(x > 0)
                textField.text = x.ToString();
            image.color = revealedColor;
        }

        /// <summary>
        /// Force reveal this tile.
        /// </summary>
        /// <param name="x">string to be displayed.</param>
        public void Reveal(string x)
        {
            clicked = true;
            textField.text = x;
            image.color = revealedColor;
        }

        /// <summary>
        /// Reset the tile's data.
        /// </summary>
        public void _Reset()
        {
            clicked = false;
            textField.text = "";
            image.color = defaultColor;
            textField.color = Color.black;
            flagged = false;
            isBomb = false;
        }
    }
}