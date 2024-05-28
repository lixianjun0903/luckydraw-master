using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pillow.Lucky
{
    public class LuckyManager : MonoBehaviour
    {


        public static LuckyManager Instance;



        private void Awake()
        {
            Instance = this;
        }


        public void Start()
        {

        }












        /// <summary>
        /// 获取转盘数据
        /// </summary>
        /// <param name="count">物品数量</param>
        /// <returns></returns>
        public RewardData[] GetDefaultWheelData(int dataCount)
        {
            var lists = new RewardData[dataCount];
            for (int i = 0; i < dataCount; i++)
            {
                var data = new RewardData();
                data.id = i;
                data.type = (i + 1);
                data.amount = (i + 1) * 100;
                data.weight = (dataCount - i) * 100;
                lists[i] = data;
            }
            return lists;
        }

        /// <summary>
        /// 获取老虎机数据
        /// </summary>
        /// <param name="count">物品数量</param>
        /// <returns></returns>
        public SlotsData[] GetDefaultSlotData(int dataCount)
        {
            var lists = new SlotsData[dataCount];
            for (int i = 0; i < dataCount; i++)
            {
                var data = new SlotsData();
                data.id = i;
                data.type = (i + 1);
                data.amount = (i + 1) * 100;
                data.weight = (dataCount - i) * 100;
                lists[i] = data;
            }
            return lists;
        }

        /// <summary>
        /// 获取水果机数据
        /// </summary>
        /// <param name="count">物品数量</param>
        /// <returns></returns>
        public RewardData[] GetDefaultFruitData(int dataCount)
        {

            int type = 1;
            var lists = new RewardData[dataCount];
            for (int i = 0; i < dataCount; i++)
            {
                var data = new RewardData();
                data.id = i;
                data.type = type;
                data.amount = (i + 1) * 100;
                data.weight = (dataCount - i) * 100;
                lists[i] = data;

                type++;
                if (type > 8)
                {
                    type = 1;
                }
            }
            return lists;
        }

        /// <summary>
        /// 获取刮刮乐数据
        /// </summary>
        /// <param name="count">物品数量</param>
        /// <returns></returns>
        public RewardData[] GetDefaultScrathcCardData(int dataCount)
        {


            var lists = new RewardData[dataCount];
            for (int i = 0; i < dataCount; i++)
            {
                var data = new RewardData();
                data.id = i;
                data.type = (i + 1);
                data.amount = (i + 1) * 100;
                data.weight = (dataCount - i) * 100;
                lists[i] = data;
            }
            return lists;
        }

        /// <summary>
        /// 获取翻卡数据
        /// </summary>
        /// <param name="count">物品数量</param>
        /// <returns></returns>
        public RewardData[] GetDefaultLuckyCardData(int dataCount)
        {
            var lists = new RewardData[dataCount];
            for (int i = 0; i < dataCount; i++)
            {
                var data = new RewardData();
                data.id = i;
                data.type = (i + 1);
                data.amount = (i + 1) * 100;
                data.weight = (dataCount - i) * 100;
                lists[i] = data;
            }
            return lists;
        }









        /// <summary>
        /// 根据权重获得中奖信息ID
        /// </summary>
        /// <param name="reward_list"></param>
        /// <returns></returns>
        public int CalculateRewardId(RewardData[] rewardDatas)
        {
            int reward_id = 0;

            var reward_list = rewardDatas.ToList();
            reward_list.Sort((x, y) => { return x.weight.CompareTo(y.weight); });
            int weight_sum = 0;
            foreach (var item in reward_list)
            {
                weight_sum += item.weight;
            }
            int random_value = UnityEngine.Random.Range(1, weight_sum + 1);
            int sum = 0;
            foreach (var item in reward_list)
            {
                sum += item.weight;
                if (random_value <= sum)
                {
                    reward_id = item.id;
                    break;
                }
            }
            return reward_id;
        }

        /// <summary>
        /// 根据权重获得不同的中奖信息ID
        /// </summary>
        /// <param name="reward_list"></param>
        /// <returns></returns>
        public int[] CalculateRewardId(RewardData[] rewardDatas, int rewardCount)
        {
            int[] reward_id_list = new int[rewardCount];

            var reward_list = rewardDatas.ToList();

            for (int i = 0; i < rewardCount; i++)
            {
                reward_list.Sort((x, y) => { return x.weight.CompareTo(y.weight); });
                int weight_sum = 0;
                foreach (var item in reward_list)
                {
                    weight_sum += item.weight;
                }
                int random_value = UnityEngine.Random.Range(1, weight_sum + 1);
                int sum = 0;
                for (int j = 0; j < reward_list.Count; j++)
                {
                    var item = reward_list[j];
                    sum += item.weight;
                    if (random_value <= sum)
                    {
                        reward_id_list[i] = item.id;
                        reward_list.RemoveAt(j);
                        break;
                    }
                }
            }

            return reward_id_list;
        }

        public T GetDataById<T>(T[] reward_list, int rewardId, out int index) where T : RewardData
        {
            index = 0;
            for (int i = 0; i < reward_list.Length; i++)
            {
                var dt = reward_list[i];
                if (dt.id == rewardId)
                {
                    index = i;
                    return dt;
                }
            }
            return null;
        }



    }
}
