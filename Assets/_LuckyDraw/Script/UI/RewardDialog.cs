using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Pillow.Lucky
{
    public class RewardDialog : UIBase
    {


        public Transform rewardNodeParent;

        public Button reward_Btn;

        List<RewardData> rewardDatas;
        Sprite[] rewardIcons;


        protected internal override void OnInit(UIView view)
        {
            base.OnInit(view);
            reward_Btn.onClick.AddListener(OnCloseEvent);
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);



            if (UIParms.TryGetValue("RewardData", out object data))
            {
                rewardDatas = (List<RewardData>)data;
            }

            if (UIParms.TryGetValue("RewardIcons", out object icon))
            {
                rewardIcons = (Sprite[])icon;
            }


            for (int i = rewardDatas.Count; i < rewardNodeParent.childCount; i++)
            {
                rewardNodeParent.GetChild(i).gameObject.SetActive(false);
            }

            for (int i = 0; i < rewardDatas.Count; i++)
            {
                Transform target = null;
                if (i < rewardNodeParent.childCount)
                {
                    target = rewardNodeParent.GetChild(i);
                }
                else
                {
                    target = Instantiate(rewardNodeParent.GetChild(0), rewardNodeParent);
                }

                var reward_Icon = target.GetComponentInChildren<Image>();
                reward_Icon.sprite = rewardIcons[rewardDatas[i].type - 1];
                reward_Icon.SetNativeSize();
                target.GetComponentInChildren<TextMeshProUGUI>().text = rewardDatas[i].amount.ToString();
                reward_Icon.GetComponentInChildren<ParticleSystem>().GetComponent<Renderer>().sortingOrder
                    = m_Canvas.sortingOrder + 1;
                target.gameObject.SetActive(true);
            }

        }



        protected override void OnCloseEvent()
        {
            base.OnCloseEvent();
        }
    }
}
