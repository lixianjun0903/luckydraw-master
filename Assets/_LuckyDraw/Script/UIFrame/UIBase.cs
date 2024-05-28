using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Pillow.Lucky
{

    public class UIBase : MonoBehaviour
    {
        protected UIView UIView;
        private bool m_Visible = false;

        protected bool IsRolling { get; set; }
        protected Canvas m_Canvas = null;
        protected RectTransform m_rectTransform = null;
        protected Dictionary<string, object> UIParms;
        protected SelectLayout selectLayout = null;

        public string Name
        {
            get
            {
                return gameObject.name;
            }
            set
            {
                gameObject.name = value;
            }
        }

        public bool Visible
        {
            get
            {
                return m_Visible;
            }
            set
            {
                if (m_Visible == value)
                {
                    return;
                }

                m_Visible = value;
                InternalSetVisible(value);
            }
        }


        protected internal virtual void OnInit(UIView view)
        {
            UIView = view;
            Name = view.ToString();

            m_rectTransform = GetComponent<RectTransform>();
            m_rectTransform.anchorMin = Vector2.zero;
            m_rectTransform.anchorMax = Vector2.one;
            m_rectTransform.sizeDelta = Vector2.zero;
            m_rectTransform.localPosition = Vector3.zero;
            m_rectTransform.anchoredPosition = Vector2.zero;

            m_Canvas = this.AddComponent<Canvas>();
            m_Canvas.overrideSorting = true;
            m_Canvas.sortingOrder = (int)view;
            this.AddComponent<GraphicRaycaster>();

        }


        protected internal virtual void OnOpen(object userData)
        {
            Visible = true;

            IsRolling = false;

            if (userData != null)
                UIParms = (Dictionary<string, object>)userData;
            if (UIParms == null)
                UIParms = new Dictionary<string, object>();
        }


        protected internal virtual void OnClose(object userData)
        {
            if (IsRolling)
                return;
            Visible = false;

        }



        protected virtual void InternalSetVisible(bool visible)
        {
            if (null == this)
                return;
            gameObject.SetActive(visible);
        }




        protected virtual void OnCloseEvent()
        {
            UIManager.Instance.CloseDialog(this.UIView);
        }


        protected void ShowDialog(UIView uIView)
        {
            UIManager.Instance.ShowDialog(uIView);
        }


        protected int GetSelectLayoutIndex()
        {
            selectLayout = selectLayout ?? this.GetComponentInChildren<SelectLayout>();

            if (selectLayout == null)
                return -1;

            return selectLayout.GetSelectedToggle();
        }


    }
}