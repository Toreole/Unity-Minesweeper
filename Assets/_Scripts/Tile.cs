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
        public Vector2Int coords;
        
        void Awake()
        {
            image.color = defaultColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(!clicked)
            {
                clicked = true;
                textField.text = GameManager.Click(coords);
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

        public void Reveal(string x)
        {
            clicked = true;
            textField.text = x;
            image.color = revealedColor;
        }

        public void _Reset()
        {
            clicked = false;
            textField.text = "";
            image.color = defaultColor;
        }
    }
}