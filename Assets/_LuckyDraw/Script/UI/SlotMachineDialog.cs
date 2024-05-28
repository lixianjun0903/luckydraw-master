using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pillow.Lucky
{
    public class SlotMachineDialog : UIBase
    {
        [SerializeField] Button btnClick;
        [SerializeField] Button btnClose;
        [SerializeField] Toggle toggleUnusable;
        [SerializeField] GameObject slot_Item;
        [SerializeField] Transform slotLayout;
        [SerializeField] Sprite[] itemImgs;


        [Header("每列物体数量")][SerializeField] int itemCount = 8;
        [Header("物体间隔")][SerializeField] float itemPadding = 100f;
        [Header("物体大小")][SerializeField] float itemSize = 1f;
        [Header("需要显示出的数量")][SerializeField] int frontItemCount = 2;



        private Transform[] slotWheels;
        private int[][] slotWheelsIds;
        private int[] slotsStopIdx;
        private Vector2 slotEdge;
        private bool isInited;
        private bool isUnusable;


        private SlotsData[] rewardDatas;
        private List<SlotsData> slotsPrizeList;


        private bool rollStyle2 = false;
        private bool rollStyle3 = false;
        private bool rollStyle4 = false;
        protected internal override void OnInit(UIView view)
        {
            base.OnInit(view);

            slot_Item.SetActive(false);
            toggleUnusable.isOn = isUnusable = false;

            slotWheels = new Transform[slotLayout.childCount];
            for (int i = 0; i < slotLayout.childCount; i++)
            {
                slotWheels[i] = slotLayout.GetChild(i);
            }


            btnClick.onClick.RemoveAllListeners();
            btnClick.onClick.AddListener(OnClickSpin);
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(OnCloseEvent);
            toggleUnusable.onValueChanged.RemoveAllListeners();
            toggleUnusable.onValueChanged.AddListener(RefreshSlotData);


            rewardDatas = LuckyManager.Instance.GetDefaultSlotData(itemCount);
            CreateItems(rewardDatas);
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            IsRolling = false;
        }


        /// <summary>
        /// Demo 测试用
        /// </summary>
        void RefreshSlotData(bool status)
        {

            if (!isInited || IsRolling)
            {
                return;
            }

            isUnusable = status;

            if (isUnusable)
            {
                rewardDatas[rewardDatas.Length - 1].unusable = true;
                rewardDatas[rewardDatas.Length - 1].weight = 9000;
            }
            else
            {
                rewardDatas[rewardDatas.Length - 1].unusable = false;
                rewardDatas[rewardDatas.Length - 1].weight = (itemCount - 1) * 100;
            }
        }



        public void CreateItems(SlotsData[] slotsData)
        {
            isInited = false;
            slotsPrizeList = new List<SlotsData>();
            for (int i = 0; i < slotsData.Length; i++)
            {
                var item = slotsData[i];
                slotsPrizeList.Add(item);
            }

            float bottomY = -itemPadding * frontItemCount;
            slotEdge = new Vector2(bottomY + itemPadding * slotsPrizeList.Count, bottomY);

            slotWheelsIds = new int[slotWheels.Length][];
            slotsStopIdx = new int[slotWheels.Length];

            for (int i = 0; i < slotWheelsIds.Length; i++)
            {
                slotWheelsIds[i] = new int[slotsPrizeList.Count];
                for (int j = 0; j < slotWheelsIds[i].Length; j++)
                {
                    slotWheelsIds[i][j] = slotsPrizeList[j].id;
                }
                int halfLen = slotsPrizeList.Count / 2;
                for (int j = 0; j < halfLen; j++)
                {
                    int randSwapIdx = UnityEngine.Random.Range(0, halfLen + 1);
                    int swapIdx2 = slotsPrizeList.Count - randSwapIdx - 1;
                    int tmpPrizeId = slotWheelsIds[i][swapIdx2];
                    slotWheelsIds[i][swapIdx2] = slotWheelsIds[i][randSwapIdx];
                    slotWheelsIds[i][randSwapIdx] = tmpPrizeId;
                }
            }

            for (int i = 0; i < slotWheels.Length; i++)
            {
                var wheel = slotWheels[i];
                for (int j = 0; j < slotWheelsIds[i].Length; j++)
                {
                    var prizeId = slotWheelsIds[i][j];
                    var reward_data = LuckyManager.Instance.GetDataById(rewardDatas, prizeId, out int index);
                    var item = Instantiate(slot_Item, wheel);
                    var pos = item.transform.localPosition;
                    pos.y = slotEdge.y + j * itemPadding;
                    item.transform.localPosition = pos;
                    item.transform.localScale = Vector3.one * itemSize;

                    var reward_img = item.GetComponentInChildren<Image>();
                    reward_img.sprite = itemImgs[reward_data.type - 1];
                    reward_img.SetNativeSize();
                    item.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("{0}", reward_data.amount);
                    item.SetActive(true);
                }
            }
            isInited = true;
        }



        private void OnClickSpin()
        {
            if (!isInited || IsRolling)
            {
                return;
            }

            GetSelectIndex();
            RollSlots(SpinComplete);
        }

        public void RollSlots(Action<bool, SlotsData> onSpinCompleted)
        {

            IsRolling = true;

            int rewardId = LuckyManager.Instance.CalculateRewardId(rewardDatas);
            var reward_data = LuckyManager.Instance.GetDataById(rewardDatas, rewardId, out int slot_index);


            bool isMatched = CalculateSlotStopIndex(reward_data, out slotsStopIdx);

            int slotWheelsCount = slotWheels.Length;
            int roundNum = 5;

            for (int i = 0; i < slotWheels.Length; i++)
            {
                var wheel = slotWheels[i];
                var stopIdx = slotsStopIdx[i];
                float rollDistance = roundNum * (slotEdge.x - slotEdge.y);
                var offsetY = wheel.GetChild(stopIdx).localPosition.y;
                rollDistance += offsetY < 0 ? Mathf.Abs(offsetY - slotEdge.y) + slotEdge.x : offsetY;

                float preframePosY = 0f;
                float curPosY = 0f;
                float rollTime = 2.5f;
                if (rollStyle2)
                {
                    rollTime = 2.5f + i * 0.5f;
                }
                else if (rollStyle3)
                {
                    rollTime = 4.5f - i * 0.5f;
                }
                else if (rollStyle4)
                {
                    rollTime = 2.5f + i * 2.5f;
                }

                var rollAnim = DOTween.To(() => curPosY, (x) => curPosY = x, rollDistance, rollTime);
                rollAnim.SetEase(Ease.OutQuart);
                rollAnim.onUpdate = () =>
                {
                    float deltaY = curPosY - preframePosY;
                    for (int j = 0; j < wheel.childCount; j++)
                    {
                        var item = wheel.GetChild(j);
                        float nextPosY = item.transform.localPosition.y - deltaY;
                        var localPosition = item.transform.localPosition;
                        localPosition.y = nextPosY < slotEdge.y ? slotEdge.x - (slotEdge.y - nextPosY) : nextPosY;
                        item.transform.localPosition = localPosition;
                    }
                    preframePosY = curPosY;
                };
                rollAnim.onComplete = () =>
                {
                    if (--slotWheelsCount <= 0)
                    {
                        IsRolling = false;
                        onSpinCompleted.Invoke(isMatched, reward_data);
                    }
                };
            }
        }









        private bool CalculateSlotStopIndex(SlotsData prize, out int[] resultStopIdx)
        {
            resultStopIdx = new int[slotWheelsIds.Length];
            bool isMatched = (prize != null && !prize.unusable);
            if (isMatched)
            {
                for (int i = 0; i < slotWheelsIds.Length; i++)
                {
                    for (int j = 0; j < slotWheelsIds[i].Length; j++)
                    {
                        if (slotWheelsIds[i][j] == prize.id)
                        {
                            slotsStopIdx[i] = j;
                            break;
                        }
                    }
                }
            }
            else
            {
                int halfLen = slotsPrizeList.Count / 2;
                int[] resultIds = new int[slotsStopIdx.Length];
                for (int i = 0; i < slotsStopIdx.Length; i++)
                {
                    int randIdx;
                    if (i % 2 == 0)
                    {
                        randIdx = UnityEngine.Random.Range(0, halfLen);
                    }
                    else
                    {
                        randIdx = UnityEngine.Random.Range(halfLen, slotsPrizeList.Count);
                    }
                    resultIds[i] = slotsPrizeList[randIdx].id;
                }
                for (int i = 0; i < slotWheelsIds.Length; i++)
                {
                    for (int j = 0; j < slotWheelsIds[i].Length; j++)
                    {
                        if (slotWheelsIds[i][j] == resultIds[i])
                        {
                            slotsStopIdx[i] = j;
                            break;
                        }
                    }
                }
            }
            return isMatched;
        }



        void SpinComplete(bool isMatched, RewardData rewardData)
        {
            if (isMatched)
                UIManager.Instance.ShowRewardDialog(rewardData, itemImgs);
            else
                Debug.LogError("未中奖!!!");
        }

        private void GetSelectIndex()
        {
            int index = GetSelectLayoutIndex();

            switch (index)
            {
                case 2:
                    rollStyle2 = true;
                    rollStyle3 = false;
                    rollStyle4 = false;
                    break;
                case 3:
                    rollStyle2 = false;
                    rollStyle3 = true;
                    rollStyle4 = false;
                    break;
                case 4:
                case 5:
                case 6:
                    rollStyle3 = false;
                    rollStyle2 = false;
                    rollStyle4 = true;
                    break;

                case 1:
                default:
                    rollStyle2 = false;
                    rollStyle3 = false;
                    rollStyle4 = false;
                    break;
            }
        }

    }
}