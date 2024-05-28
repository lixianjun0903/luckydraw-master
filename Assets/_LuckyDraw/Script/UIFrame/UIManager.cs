using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pillow.Lucky
{

    public enum UIView
    {
        DefaultScreen = 0,
        WheelDialog = 10,
        FruitMachineDialog = 20,
        SlotMachineDialog = 30,
        LuckyCardDialog = 40,
        ScratchCardDialog = 50,


        RewardDialog = 100,
    }

    public class UIManager : MonoBehaviour
    {

        public static UIManager Instance;

        public Transform canvasRoot;

        public List<GameObject> prefabLists;

        private Dictionary<UIView, UIBase> uiCacheDics = new Dictionary<UIView, UIBase>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            ShowDialog(UIView.DefaultScreen);
        }


        public void ShowRewardDialog(RewardData rewardData, Sprite[] rewardIcons)
        {
            if (rewardData == null) return;

            var list = new List<RewardData>();
            list.Add(rewardData);
            ShowRewardDialog(list, rewardIcons);
        }


        public void ShowRewardDialog(List<RewardData> rewardDatas, Sprite[] rewardIcons)
        {
            if (rewardDatas == null || rewardDatas.Count < 1) return;

            var parms = new Dictionary<string, object>();
            parms.Add("RewardData", rewardDatas);
            parms.Add("RewardIcons", rewardIcons);
            ShowDialog(UIView.RewardDialog, parms);
        }







        public UIBase ShowDialog(UIView view, object userData = null)
        {
            if (userData == null)
                userData = new Dictionary<string, object>();

            UIBase panel = null;
            if (!uiCacheDics.TryGetValue(view, out panel))
            {
                var panelPrefab = GetPrefab(view.ToString());
                var panelObject = Instantiate(panelPrefab, canvasRoot, false);
                panel = panelObject.GetComponent<UIBase>();
                panel.OnInit(view);
                uiCacheDics.Add(view, panel);
            }
            panel.OnOpen(userData);
            return panel;
        }


        public void CloseDialog(UIView view, object userData = null)
        {
            if (uiCacheDics.TryGetValue(view, out UIBase panel))
            {
                panel.OnClose(userData);
            }
        }






        private GameObject GetPrefab(string name)
        {
            foreach (var item in prefabLists)
            {
                if (item.name == name)
                    return item;
            }
            return null;
        }
    }
}
