using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CurtainController : Singleton<CurtainController>
{
    public RectTransform LeftCurtain;
    public RectTransform RightCurtain;
    public float curtainAnimDuration = 0.5f;

    private UIManager uiManager;
    public bool curtainsOpened { get; private set; } = true;
    public bool animating { get; private set; } = false;

    public float CurtainLevelLoadDuration = .5f;

    IEnumerator Start()
    {
        uiManager = UIManager.request();

        CloseCurtainDirectly();
        yield return new WaitForSeconds(.25f);
        OpenCurtain();
    }

    public void CloseCurtainDirectly()
    {
        if (!curtainsOpened) return;

        float aimWidth = GetCanvasWidth() / 2;
        setWidth(RightCurtain, aimWidth);
        setWidth(LeftCurtain, aimWidth);

        RightCurtain.gameObject.SetActive(true);
        LeftCurtain.gameObject.SetActive(true);


        curtainsOpened = false;

        Debug.Log("Curtain is closed!");
    }

    public void CloseCurtain(Callback callback = null)
    {

        if (!curtainsOpened)
        {
            if (callback != null)
                callback.Invoke();
            return;
        }

        RightCurtain.gameObject.SetActive(true);
        LeftCurtain.gameObject.SetActive(true);

        float aimWidth = GetCanvasWidth() / 2;

        setWidth(RightCurtain, 0);
        setWidth(LeftCurtain, 0);

        animating = true;

        DOTween.To(() => RightCurtain.sizeDelta.x, (x) => setWidth(RightCurtain, x), aimWidth, curtainAnimDuration);
        DOTween.To(() => LeftCurtain.sizeDelta.x, (x) => setWidth(LeftCurtain, x), aimWidth, curtainAnimDuration)
            .OnComplete(() =>
            {
                if (callback != null) callback.Invoke();
                animating = false;

            });


        curtainsOpened = false;
    }

    public void OpenCurtain(Callback callback = null, float delay = 0)
    {
        if (curtainsOpened)
        {
            if (callback != null)
                callback.Invoke();
            return;
        }

        float sourceWidth = GetCanvasWidth() / 2;

        setWidth(RightCurtain, sourceWidth);
        setWidth(LeftCurtain, sourceWidth);

        float aimWidth = 0;

        animating = true;

        DOTween.To(() => RightCurtain.sizeDelta.x, (x) => setWidth(RightCurtain, x), aimWidth, curtainAnimDuration).SetDelay(delay);
        DOTween.To(() => LeftCurtain.sizeDelta.x, (x) => setWidth(LeftCurtain, x), aimWidth, curtainAnimDuration).SetDelay(delay)
            .OnComplete(() =>
            {
                if (callback != null) callback.Invoke();

                animating = false;
            });

        curtainsOpened = true;


    }

    private void setWidth(RectTransform rectTransform, float width)
    {

        Vector2 size = rectTransform.sizeDelta;
        size.x = width;
        rectTransform.sizeDelta = size;

    }

    public float GetCanvasWidth()
    {
        return uiManager.GetComponent<RectTransform>().rect.width;
    }
}
