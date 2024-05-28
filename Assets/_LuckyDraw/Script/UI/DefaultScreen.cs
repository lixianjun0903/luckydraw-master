
using UnityEngine;
using UnityEngine.UI;

namespace Pillow.Lucky
{
    public class DefaultScreen : UIBase
    {
        public Button[] buttons;




        protected internal override void OnInit(UIView view)
        {
            base.OnInit(view);

            buttons[0].onClick.AddListener(() => { ShowDialog(UIView.WheelDialog); });
            buttons[1].onClick.AddListener(() => { ShowDialog(UIView.FruitMachineDialog); });
            buttons[2].onClick.AddListener(() => { ShowDialog(UIView.SlotMachineDialog); });
            buttons[3].onClick.AddListener(() => { ShowDialog(UIView.LuckyCardDialog); });
            buttons[4].onClick.AddListener(() => { ShowDialog(UIView.ScratchCardDialog); });
        }
    }
}