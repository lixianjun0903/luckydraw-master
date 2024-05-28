using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pillow.Lucky
{
    public partial class FruitMachineDialog : UIBase
    {
        [SerializeField] Button btnClose;
        [SerializeField] Button btnClick;
        // [SerializeField] GameObject fruitItem;
        [SerializeField] Transform itemRoot;
        [SerializeField] Sprite[] itemImgs;


        [Header("物体数量")][SerializeField] int itemCount = 16;
        [Header("水果图标大小")][SerializeField] float itemSize = 1f;


        private int startIndexs;
        private RewardData[] rewardDatas;
        private List<GameObject> cursorLists = new List<GameObject>();
        private List<GameObject> selectLists = new List<GameObject>();

        private bool rollStyle2 = false;
        private bool rollStyle3 = false;


        protected internal override void OnInit(UIView uIView)
        {
            base.OnInit(uIView);

            btnClick.onClick.RemoveAllListeners();
            btnClick.onClick.AddListener(OnClickSpin);
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(OnCloseEvent);

            rewardDatas = LuckyManager.Instance.GetDefaultFruitData(itemCount);
            CreateItems();
        }
        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            startIndexs = 0;
            ChangeSelectEanble(0, true);
            ChangeCursorEanble(0, true);

        }
        protected internal override void OnClose(object userData)
        {
            base.OnClose(userData);
        }

        private void OnClickSpin()
        {
            if (IsRolling)
            {
                return;
            }

            StartCoroutine(StartSpin(SpinComplete));
        }


        private void CreateItems()
        {
            for (int i = 0; i < rewardDatas.Length; i++)
            {
                var reward_data = rewardDatas[i];
                GameObject fruitOjb;
                if (i < itemRoot.childCount)
                {
                    fruitOjb = itemRoot.GetChild(i).gameObject;
                }
                else
                {
                    fruitOjb = Instantiate(itemRoot.GetChild(0).gameObject, itemRoot);
                }
                fruitOjb.SetActive(true);
                var fruitImg = fruitOjb.transform.Find("Icon").GetComponent<Image>();
                fruitImg.sprite = itemImgs[reward_data.type - 1];
                fruitImg.SetNativeSize();
                fruitImg.transform.localScale = Vector3.one * itemSize;

                var amountTxt = fruitOjb.GetComponentInChildren<TextMeshProUGUI>();
                amountTxt.text = string.Format("{0}", reward_data.amount);

                fruitOjb.transform.localScale = Vector3.one * itemSize;

                cursorLists.Add(fruitOjb.transform.Find("Cursor").gameObject);
                selectLists.Add(fruitOjb.transform.Find("Select").gameObject);
            }
        }


        private IEnumerator StartSpin(Action<List<RewardData>> onSpinCompleted)
        {

            IsRolling = true;

            ChangeSelectEanble(0, true);
            ChangeCursorEanble(0, true);

            GetSelectIndex();


            int[] rewardIdLists = LuckyManager.Instance.CalculateRewardId(rewardDatas, GetRewardCount());

            int spinCount = rewardIdLists.Length;

            List<RewardData> rewardDataList = new List<RewardData>();


            if (rollStyle3)
            {
                yield return StaraThreeSpine();
            }


            for (int i = 0; i < rewardIdLists.Length; i++)
            {
                var rewardDt = LuckyManager.Instance.GetDataById(rewardDatas, rewardIdLists[i], out int rewardIndex);
                if (rewardDt == null)
                {
                    Debug.LogErrorFormat("水果机表中不存在id:{0}", rewardIdLists[i]);
                    continue;
                }
                int totalLen = itemCount * 3 + rewardIndex;

                if (rollStyle3 || rollStyle2)
                    totalLen = itemCount * 3 - i + rewardIndex;


                int curIndex = startIndexs;
                float spinTime = totalLen * 0.06f;
                var moveAct = DOTween.To(() => curIndex, (x) => curIndex = x, totalLen, spinTime);
                moveAct.SetEase(Ease.Linear);
                moveAct.onUpdate = () =>
                {
                    int fixNum = curIndex % itemCount;
                    startIndexs = fixNum;
                    ChangeCursorEanble(fixNum);
                };
                moveAct.OnComplete(() =>
                {
                    rewardDataList.Add(rewardDt);
                    ChangeSelectEanble(rewardIndex);
                    if (--spinCount <= 0)
                    {
                        IsRolling = false;
                        onSpinCompleted?.Invoke(rewardDataList);
                    }
                });
                if (rollStyle3 || rollStyle2)
                    yield return new WaitForSeconds(spinTime);
            }
        }



        IEnumerator StaraThreeSpine()
        {
            int totalLen = itemCount * 2;
            int curIndex = 0;
            float time = totalLen * 0.05f;
            var moveAct = DOTween.To(() => curIndex, (x) => curIndex = x, totalLen, time);
            moveAct.SetEase(Ease.Linear);
            moveAct.onUpdate = () =>
            {
                int fixNum = curIndex % itemCount;
                startIndexs = fixNum;
                ChangeThreeCursorEanble(fixNum);
            };

            yield return new WaitForSeconds(time);
        }


        void ChangeThreeCursorEanble(int index)
        {
            for (int i = 0; i < cursorLists.Count; i++)
            {
                int intervalcount = cursorLists.Count / 3 + 1;

                int nextindex = (index - intervalcount) < 0 ? cursorLists.Count - (intervalcount - index) : index - intervalcount;

                int threeindex = (nextindex - intervalcount) < 0 ? cursorLists.Count - (intervalcount - nextindex) : nextindex - intervalcount;

                if (i == index || i == nextindex || i == threeindex)
                    cursorLists[i].SetActive(true);
                else
                    cursorLists[i].SetActive(false);
            }
        }

        void ChangeCursorEanble(int index, bool isAll = false)
        {

            for (int i = 0; i < cursorLists.Count; i++)
            {
                if (isAll)
                    cursorLists[i].SetActive(false);
                else if (i == index)
                    cursorLists[i].SetActive(true);
                else
                    cursorLists[i].SetActive(false);
            }
        }

        void ChangeSelectEanble(int index, bool isAll = false)
        {

            if (isAll)
            {
                foreach (var item in selectLists)
                {
                    item.SetActive(false);
                }
            }
            else
            {
                for (int i = 0; i < selectLists.Count; i++)
                {
                    if (i == index)
                        selectLists[i].SetActive(true);
                }
            }
        }








        void SpinComplete(List<RewardData> rewardData)
        {
            UIManager.Instance.ShowRewardDialog(rewardData, itemImgs);
        }




        public int GetRewardCount()
        {
            if (rollStyle2)
                return 3;
            else if (rollStyle3)
                return 3;

            return 1;
        }



        private void GetSelectIndex()
        {
            int index = GetSelectLayoutIndex();

            switch (index)
            {
                case 2:
                    rollStyle2 = true;
                    rollStyle3 = false;
                    break;
                case 3:
                case 4:
                case 5:
                case 6:
                    rollStyle2 = false;
                    rollStyle3 = true;
                    break;


                case 1:
                default:
                    rollStyle2 = false;
                    rollStyle3 = false;
                    break;
            }
        }

    }
}


