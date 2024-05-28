using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EraseMask : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public bool isEnable = true;  //是否启用
    [Header("毛刷大小")] public int brushSize = 50; //毛刷大小
    [Header("擦拭比例")] public int rate = 90;      //可以设置当到达一定比例后全部显示

    Action eraserEvent;         //擦拭事件

    Action eraserCompleteEvent; //擦拭完成事件
    RawImage uiTex;             //渲染对象

    Texture2D MyTex;            //渲染材质
    int mWidth, mHeight;        //宽，高
    int maxColor, startColor;   //最大颜色量，开始颜色量
    bool twoPoints = false;     //判断鼠标是否松开
    Vector2 startPos, endPos;   //开始点，结束点
    float radius = 12f;         //平滑的段数
    float distance = 1f;        //鼠标滑动的距离
    double fate;                //进度

    Texture2D lastTexture2D;

    void Awake()
    {
        if (!isEnable) { return; }
        uiTex = GetComponentInChildren<RawImage>();
        lastTexture2D = (Texture2D)uiTex.mainTexture;

        eraserEvent = getTransparentPercent;
    }



    public void Init(Action action)
    {
        MyTex = new Texture2D(lastTexture2D.width, lastTexture2D.height, TextureFormat.ARGB32, false);
        mWidth = MyTex.width;
        mHeight = MyTex.height;
        MyTex.SetPixels(lastTexture2D.GetPixels());
        MyTex.Apply();
        uiTex.texture = MyTex;
        maxColor = MyTex.GetPixels().Length;
        startColor = 0;

        eraserCompleteEvent = action;

        uiTex.gameObject.SetActive(true);

        isEnable = true;
    }


    #region 事件
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isEnable) { return; }
        startPos = eventData.position;
        CheckPoint(startPos);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isEnable) { return; }
        if (twoPoints && Vector2.Distance(eventData.position, endPos) > distance)//如果两次记录的鼠标坐标距离大于一定的距离，开始记录鼠标的点
        {
            Vector2 pos = eventData.position;
            float dis = Vector2.Distance(endPos, pos);

            CheckPoint(eventData.position);
            int segments = (int)(dis / radius);//计算出平滑的段数                                              
            segments = segments < 1 ? 1 : segments;
            if (segments >= 10) { segments = 10; }
            Vector2[] points = Beizier(startPos, endPos, pos, segments);//进行贝塞尔平滑
            for (int i = 0; i < points.Length; i++)
            {
                CheckPoint(points[i]);
            }
            endPos = pos;
            if (points.Length > 2)
                startPos = points[points.Length - 2];
        }
        else
        {
            twoPoints = true;
            endPos = eventData.position;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isEnable) { return; }
        twoPoints = false;
    }
    void CheckPoint(Vector3 pScreenPos)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(pScreenPos);
        Vector3 localPos = uiTex.gameObject.transform.InverseTransformPoint(worldPos);

        if (localPos.x > -mWidth / 2 && localPos.x < mWidth / 2 && localPos.y > -mHeight / 2 && localPos.y < mHeight / 2)
        {
            for (int i = (int)localPos.x - brushSize; i < (int)localPos.x + brushSize; i++)
            {
                for (int j = (int)localPos.y - brushSize; j < (int)localPos.y + brushSize; j++)
                {
                    if (Mathf.Pow(i - localPos.x, 2) + Mathf.Pow(j - localPos.y, 2) > Mathf.Pow(brushSize, 2))
                        continue;
                    if (i < 0) { if (i < -mWidth / 2) { continue; } }
                    if (i > 0) { if (i > mWidth / 2) { continue; } }
                    if (j < 0) { if (j < -mHeight / 2) { continue; } }
                    if (j > 0) { if (j > mHeight / 2) { continue; } }

                    Color col = MyTex.GetPixel(i + (int)mWidth / 2, j + (int)mHeight / 2);
                    if (col.a != 0f)
                    {
                        col.a = 0.0f;
                        startColor++;
                        MyTex.SetPixel(i + (int)mWidth / 2, j + (int)mHeight / 2, col);
                    }
                }
            }
            //开始刮的时候 去判断进度
            if (isEnable)
            {
                eraserEvent.Invoke();
            }
            MyTex.Apply();
        }
    }
    #endregion

    /// <summary> 
    /// 检测当前刮刮卡 进度
    /// </summary>
    /// <returns></returns>
    public void getTransparentPercent()
    {
        if (!isEnable) { return; }
        fate = (float)startColor / maxColor * 100;
        fate = (float)Math.Round(fate, 2);
      //  Debug.Log("当前百分比: " + fate);
        if (fate >= rate)
        {
            isEnable = false;

            uiTex.gameObject.SetActive(false);
            eraserCompleteEvent?.Invoke();
        }
    }


    /// <summary>
    /// 贝塞尔平滑
    /// </summary>
    /// <param name="start">起点</param>
    /// <param name="mid">中点</param>
    /// <param name="end">终点</param>
    /// <param name="segments">段数</param>
    /// <returns></returns>
    public Vector2[] Beizier(Vector2 start, Vector2 mid, Vector2 end, int segments)
    {
        float d = 1f / segments;
        Vector2[] points = new Vector2[segments - 1];
        for (int i = 0; i < points.Length; i++)
        {
            float t = d * (i + 1);
            points[i] = (1 - t) * (1 - t) * mid + 2 * t * (1 - t) * start + t * t * end;
        }
        List<Vector2> rps = new List<Vector2>();
        rps.Add(mid);
        rps.AddRange(points);
        rps.Add(end);
        return rps.ToArray();
    }
}
