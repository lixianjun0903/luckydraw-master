using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

namespace Pillow.Lucky
{
    public class ScratchCardDialog : UIBase
    {
        [SerializeField] EraseMask eraseMask;
        [SerializeField] Button btnClose;

        [SerializeField] Transform itemRoot;
        [SerializeField] Sprite[] itemImgs;

        [Header("物体数量")][SerializeField] int itemCount = 9;

        private RewardData[] rewardDatas;

        private List<RewardData> reward_data_list;

        protected internal override void OnInit(UIView view)
        {
            base.OnInit(view);

            btnClose.onClick.AddListener(OnCloseEvent);
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            eraseMask.Init(EraseComplete);

            rewardDatas = LuckyManager.Instance.GetDefaultScrathcCardData(itemCount);

            IsRolling = false;

            CreateItems();
        }


        void CreateItems()
        {
            var rewardLists = LuckyManager.Instance.CalculateRewardId(rewardDatas, Random.Range(4, 5));

            reward_data_list = reward_data_list ?? new List<RewardData>();
            reward_data_list.Clear();


            for (int i = 0; i < itemCount; i++)
            {
                Transform item = null;
                if (i + 1 <= itemRoot.childCount)
                {
                    item = itemRoot.GetChild(i);
                }
                else
                {
                    item = Instantiate(itemRoot.GetChild(0), itemRoot);
                }

                int rewardId = -1;

                foreach (var index in rewardLists)
                {
                    if (i == index)
                    {
                        rewardId = index; break;
                    }
                }

                var reward_img = item.Find("Icon").GetComponent<Image>();
                var reward_value = item.Find("Value").GetComponent<TextMeshProUGUI>();


                if (rewardId < 0)
                {
                    reward_img.gameObject.SetActive(false);
                    reward_value.gameObject.SetActive(false);
                    continue;
                }

                var reward_data = LuckyManager.Instance.GetDataById(rewardDatas, rewardId, out int wheel_index);
                reward_data_list.Add(reward_data);

                reward_img.sprite = itemImgs[reward_data.type - 1];
                reward_img.SetNativeSize();
                reward_value.text = string.Format("{0}", reward_data.amount);

                reward_img.gameObject.SetActive(true);
                reward_value.gameObject.SetActive(true);

            }


        }







        void EraseComplete()
        {
            UIManager.Instance.ShowRewardDialog(reward_data_list, itemImgs);
        }



    }
}