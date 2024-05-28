using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;
using TMPro;


namespace Pillow.Lucky
{
    public class WheelDialog : UIBase
    {
        [SerializeField] Button btnClick;
        [SerializeField] Button btnClose;
        [SerializeField] Sprite[] itemImgs;

        [SerializeField] Transform itemParent;
        Transform itemSpinRoot;

        [Header("物体数量")][SerializeField] int ItemCount = 8;

        private float item_rangle;

        private RewardData[] rewardDatas;

        protected internal override void OnInit(UIView view)
        {
            base.OnInit(view);

            item_rangle = 360f / ItemCount;

           //转动的根目录为itemSpinRoot
            itemSpinRoot = itemParent.parent;

            btnClick.onClick.RemoveAllListeners();
            btnClick.onClick.AddListener(OnClickSpin);
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(OnCloseEvent);

            rewardDatas = LuckyManager.Instance.GetDefaultWheelData(ItemCount);
            //创建对应的item
            CreateItems(rewardDatas);
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            ResetWheels();
        }



        private void ResetWheels()
        {

        }

        private void CreateItems(RewardData[] wheel_tb)
        {
            Vector3 init_pos = itemParent.GetChild(0).transform.localPosition;
            if (wheel_tb.Length != ItemCount)
            {
                Debug.LogError("wheel_tb.Count !=  wheel_count !!!");
            }

            for (int i = 0; i < wheel_tb.Length; i++)
            {
                var reward_data = wheel_tb[i];
                var next_rot = Quaternion.Euler(0, 0, i * item_rangle);
                var next_pos = next_rot * init_pos;
                GameObject item;
                if (i + 1 <= itemParent.childCount)
                {
                    item = itemParent.GetChild(i).gameObject;
                }
                else
                {
                    item = Instantiate(itemParent.GetChild(0).gameObject, itemParent);
                }
                //每个item转动的角度
                item.transform.localPosition = next_pos;
                item.transform.localRotation = next_rot;

                var reward_text = item.GetComponentInChildren<TextMeshProUGUI>();
                var reward_img = item.GetComponentInChildren<Image>();
                reward_img.sprite = itemImgs[reward_data.type - 1];
                reward_img.SetNativeSize();
                reward_text.text = string.Format("{0}", reward_data.amount);

                item.GetComponentInChildren<ParticleSystem>().GetComponent<Renderer>().sortingOrder = m_Canvas.sortingOrder + 1;
            }
        }

        private void OnClickSpin()
        {
            if (IsRolling)
            {
                return;
            }

            StartSpin(SpinComplete);
        }

        private void StartSpin(Action<RewardData> onSpinCompleted)
        {
            IsRolling = true;
            int rewardId = LuckyManager.Instance.CalculateRewardId(rewardDatas);
            var reward_data = LuckyManager.Instance.GetDataById(rewardDatas, rewardId, out int wheel_index);

            //初始角度
            float cur_angle = itemSpinRoot.localRotation.eulerAngles.z;
            int round_count = UnityEngine.Random.Range(4, 5);
            //最后要转动的角度
            float final_angle = -item_rangle * wheel_index + 360 * round_count;

            float duration = 2.5f;
            // DOTween实现转动角度方法实现1
            var roll_act = DOTween.To(() => cur_angle, (x) => cur_angle = x, final_angle, duration);
           //转动模式设置
            roll_act.SetEase(GetEase());
            roll_act.onUpdate = () =>
            {
                // DOTween实现转动角度方法实现2
                itemSpinRoot.localRotation = Quaternion.Euler(0, 0, cur_angle);
            };
            roll_act.OnComplete(() =>
            {
                IsRolling = false;

                onSpinCompleted.Invoke(reward_data);
            });
        }

        void SpinComplete(RewardData rewardData)
        {
            UIManager.Instance.ShowRewardDialog(rewardData, itemImgs);
        }


        private Ease GetEase()
        {
            int index = GetSelectLayoutIndex();

            switch (index)
            {
                case 2:
                    return Ease.OutBack;
                case 3:
                    return Ease.OutQuad;
                case 4:
                    return Ease.OutQuart;
                case 5:
                    return Ease.OutCubic;
                case 6:
                    return Ease.OutCirc;
                case 1:
                default:
                    return Ease.Linear;
            }
        }

    }

}