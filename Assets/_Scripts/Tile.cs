using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Minesweeper
{
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
        public int surroundingBombs;
        [HideInInspector, System.NonSerialized]
        public bool isBombSelf;
        [HideInInspector, System.NonSerialized]
        public Vector2Int coords;
        
        void Awake()
        {
            image.color = defaultColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(clicked)
            {
                clicked = true;
                if (isBombSelf)
                {
                    //todo: lose game.
                }
                else
                {
                    if(surroundingBombs == 0)
                    {
                        //todo: Game.RevealAround(Vector2 coords);
                    }
                    else
                        textField.text = surroundingBombs.ToString();
                    image.color = revealedColor;
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

        public void Reveal()
        {
            clicked = true;
            if(surroundingBombs > 0)
                textField.text = surroundingBombs.ToString();
            image.color = revealedColor;
        }
    }
}