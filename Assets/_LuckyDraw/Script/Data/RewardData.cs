namespace Pillow.Lucky
{
    public class RewardData
    {
        public int id;

        public int amount;

        /// <summary>
        /// 奖励类型
        /// </summary>
        public int type;
        /// <summary>
        /// 奖励权重
        /// </summary>
        public int weight;

    
    }


    public class SlotsData : RewardData
    {
        /// <summary>
        /// 此项为true 抽到此选项时代表未中奖
        /// </summary>
        public bool unusable;
    }

}
