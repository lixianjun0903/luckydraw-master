using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;


namespace Pillow.Lucky
{
    public partial class LuckyCardDialog : UIBase
    {
        [SerializeField] Button btnClose;
        [SerializeField] Sprite[] itemImgs;
        [SerializeField] Transform itemParent;


        [Header("物体数量")][SerializeField] int ItemCount = 6;
        [Header("可以翻几张")][SerializeField] int ItemFlipCount = 3;

        int[] rewardIds;
        RewardData[] rewardDatas;
        private List<RewardData> reward_data_list;


        Vector3[] initPosArr;

        bool isSwapCard = false;

        private int mCurClaimIndex;
        private int CurClaimIndex
        {
            get { return mCurClaimIndex; }
            set
            {
                mCurClaimIndex = value;
                RefreshCardADState();
            }
        }
        private bool mCardSelectable;
        private bool CardSelectable
        {
            get
            {
                return mCardSelectable;
            }
            set
            {
                mCardSelectable = value;
                IsRolling = !mCardSelectable;
            }
        }


        protected internal override void OnInit(UIView view)
        {
            base.OnInit(view);

            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(OnCloseEvent);


            initPosArr = new Vector3[ItemCount];
            for (int i = 0; i < ItemCount; i++)
            {
                var card = itemParent.GetChild(i);
                initPosArr[i] = card.localPosition;
                var bt = card.GetComponent<Button>();
                bt.onClick.RemoveAllListeners();
                bt.onClick.AddListener(() => { OnClickSpin(bt); });
            }
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            CurClaimIndex = 0;


            rewardDatas = LuckyManager.Instance.GetDefaultLuckyCardData(ItemCount);
            rewardIds = LuckyManager.Instance.CalculateRewardId(rewardDatas, ItemFlipCount);

            CreateItems();
        }

        protected override void OnCloseEvent()
        {
            SpinComplete();
            base.OnCloseEvent();
        }


        void CreateItems()
        {
            reward_data_list = reward_data_list ?? new List<RewardData>();
            reward_data_list.Clear();


            for (int i = 0; i < ItemCount; i++)
            {

                GameObject item;
                if (i + 1 <= itemParent.childCount)
                {
                    item = itemParent.GetChild(i).gameObject;
                }
                else
                {
                    item = Instantiate(itemParent.GetChild(0).gameObject, itemParent);
                }

                item.transform.localPosition = initPosArr[i];
                item.transform.localScale = Vector3.one;
                var cardGraphics = item.transform.Find("Graphics").GetComponent<RectTransform>();

                RefreshCardData(cardGraphics, false, rewardDatas[i]);
                SetCardOpen(false, item.GetComponent<Button>(), true, rewardDatas[i]);
            }

            StartAnim(rewardDatas);
        }

        private void RefreshCardData(RectTransform cardGraphics, bool isOpen, RewardData rewardData)
        {

            var cardItem = cardGraphics.transform.Find("Item");

            cardGraphics.transform.Find("Marker").gameObject.SetActive(!isOpen);
            cardGraphics.transform.Find("Bg").gameObject.SetActive(isOpen);
            cardItem.gameObject.SetActive(isOpen);

            var size = cardGraphics.localScale;
            size.x = (isOpen ? 1 : -1) * Mathf.Abs(cardGraphics.localScale.x);
            cardGraphics.localScale = size;

            cardItem.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("+{0}", rewardData.amount);
            var reward_img = cardItem.GetComponentInChildren<Image>();
            reward_img.sprite = itemImgs[rewardData.type - 1];
            reward_img.SetNativeSize();
        }

        private void RefreshCardADState(bool isOneFree = false)
        {

            //foreach (Transform item in cardsRoot)
            //{
            //    var bt = item.GetComponent<Button>();
            //    var adIconEanble = (bt.interactable && CurClaimIndex > 0);
            //}
        }




        private void OnClickSpin(Button cardBt)
        {

            if (rewardIds == null || CurClaimIndex >= rewardIds.Length || !CardSelectable)
            {
                return;
            }

            var rewardDt = LuckyManager.Instance.GetDataById(rewardDatas, rewardIds[CurClaimIndex], out int index);

            reward_data_list.Add(rewardDt);
            SetCardOpen(true, cardBt, true, rewardDt, 0.4f, () =>
            {
                if (++CurClaimIndex >= rewardIds.Length)
                {
                    var seqDelay = DOTween.Sequence();
                    seqDelay.AppendInterval(1f);
                    seqDelay.AppendCallback(() =>
                    {
                        SpinComplete();
                    });
                }
            });

        }


        void SpinComplete()
        {
            // OnCloseEvent();
            if (reward_data_list.Count > 0)
            {
                UIManager.Instance.ShowRewardDialog(new List<RewardData>(reward_data_list), itemImgs);
                reward_data_list.Clear();
            }
        }



        /// <summary>
        /// 明牌，洗牌动画
        /// </summary>
        private void StartAnim(RewardData[] rewardsArr)
        {
            CardSelectable = false;

            if (rewardsArr.Length != itemParent.childCount)
            {
                Debug.LogErrorFormat("rewards length is not equals cards count:{0}", itemParent.childCount);
                return;
            }

            isSwapCard = true;
            var seqAnim = DOTween.Sequence();
            seqAnim.AppendInterval(2f);
            seqAnim.AppendCallback(() =>
            {
                int overCount = itemParent.childCount;
                for (int i = 0; i < itemParent.childCount; i++)
                {
                    SetCardOpen(false, itemParent.GetChild(i).GetComponent<Button>(), false, null, 0.2f, () =>
                    {
                        //所有卡片翻回去之后开始洗牌
                        if (--overCount <= 0)
                        {
                            List<int> cardsList = new List<int>();

                            for (int childIndex = 0; childIndex < itemParent.childCount; childIndex++)
                            {
                                cardsList.Add(childIndex);
                            }


                            SwapCards(cardsList, 10, () =>
                            {
                                RefreshCardADState(false);
                                CardSelectable = true;
                            });
                        }
                    });
                }
            });
        }

        private void SwapCards(List<int> cardsList, int swapCount, Action onComplete)
        {
            if (swapCount <= 0)
            {
                onComplete?.Invoke();
                return;
            }

            SwapCardAnim(cardsList, () =>
            {
                ListRandom(cardsList);
                SwapCards(cardsList, --swapCount, onComplete);
            });
        }

        public static void ListRandom<T>(List<T> sources)
        {
            int index;
            T temp;
            for (int i = 0; i < sources.Count; i++)
            {
                index = UnityEngine.Random.Range(0, sources.Count);
                if (index != i)
                {
                    temp = sources[i];
                    sources[i] = sources[index];
                    sources[index] = temp;
                }
            }
        }
        /// <summary>
        /// 洗牌动画
        /// </summary>
        /// <param name="onSwapOver"></param>
        private void SwapCardAnim(List<int> cardsindexList, Action onSwapOver)
        {
            if (itemParent.childCount % 2 != 0)
            {
                Debug.LogError("cardsRoot 卡片个数不能为奇数");
                return;
            }

            int moveMission = itemParent.childCount;
            TweenCallback onMoveOver = () =>
            {
                if (--moveMission <= 0)
                {
                    onSwapOver?.Invoke();
                }
            };
            for (int i = 0; i < cardsindexList.Count; i += 2)
            {
                var indexA = cardsindexList[i];
                var indexB = cardsindexList[i + 1];
                var cardA = itemParent.GetChild(indexA);
                var cardB = itemParent.GetChild(indexB);
                float moveDuration = Vector2.Distance(initPosArr[indexA], initPosArr[indexB]) / 1500;
                moveDuration = Mathf.Clamp(moveDuration, 0, 0.18f);

                cardA.DOLocalMove(initPosArr[indexB], moveDuration).onComplete = onMoveOver;
                cardB.DOLocalMove(initPosArr[indexA], moveDuration).onComplete = onMoveOver;
            }
        }

        private void SetCardOpen(bool isUser, Button cardBt, bool isOpen, RewardData reward = null, float duration = 0.2f, Action onCardAnimOver = null)
        {
            if (isOpen && reward == null)
            {
                Debug.LogError("翻卡传入奖励数据为null");
                return;
            }
            if (isUser && !CardSelectable)
            {
                return;
            }
            CardSelectable = false;
            int texIndex = 0;
            int colorIndex = 0;
            string tmStr = string.Empty;
            Vector3 halfRotate = new Vector3(0, 90, 0);
            if (isOpen)
            {
                texIndex = reward.type;
                colorIndex = reward.type - 1;
                tmStr = reward.amount.ToString();
                halfRotate.y = 270;
            }

            cardBt.interactable = !isOpen;

            var card = cardBt.transform;
            var cardGraphics = card.Find("Graphics").GetComponent<RectTransform>();

            var seqAnim = DOTween.Sequence();
            seqAnim.Append(cardGraphics.DOLocalRotate(halfRotate, duration).SetEase(Ease.Linear));
            seqAnim.AppendCallback(() =>
            {
                RefreshCardData(cardGraphics, isOpen, reward);
            });
            seqAnim.Append(cardGraphics.DOLocalRotate(halfRotate + Vector3.up * 90, duration).SetEase(Ease.Linear));
            seqAnim.onComplete = () =>
            {
                if (!isUser)
                {
                    //CardSelectable = true;
                    onCardAnimOver?.Invoke();
                }
            };
            if (isUser)
            {
                card.SetSiblingIndex(itemParent.childCount);
                //float moveDuration = Vector2.Distance(card.transform.localPosition, Vector3.zero) / 400;
                //moveDuration = Mathf.Clamp(moveDuration, 0, 0.4f);
                //card.DOMove(Vector3.zero, moveDuration);
                //card.DOScale(1.4f, moveDuration).onComplete = () =>
                //{
                //    var moveSeq = DOTween.Sequence();
                //    moveSeq.AppendInterval(1.0f);
                //    moveSeq.AppendCallback(() =>
                //    {
                //        var smallCard = smallCardsRoot.GetChild(CurClaimIndex);
                //        moveDuration = Mathf.Clamp(Vector2.Distance(card.position, smallCard.position) / 10, 0, 0.4f);
                //        card.DOScale(smallCard.GetComponent<RectTransform>().sizeDelta.x / card.GetComponent<RectTransform>().sizeDelta.x, moveDuration);
                //        card.DOMove(smallCard.position, moveDuration).onComplete = () =>
                //        {
                //            CardSelectable = true;
                //            GF.Sound.PlaySound("poker_end.wav", false);

                //            //加奖励
                //            GF.UserData.ClaimReward(UserDataType.LuckpokerRedpacket, reward, GF.UserData.GameUIForm.gameMainView.levelSocreTxt.transform);
                //            onCardAnimOver?.Invoke();
                //        };
                //    });
                //};


                CardSelectable = true;
                onCardAnimOver?.Invoke();
            }
        }




    }
}
