using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : SingletonMonoBehaviour<LoadingScreen>
{
    [SerializeField] private GameObject loadingObjRoot;
    [SerializeField] private Image fullscreenFadeImage;
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private Image progressBar;

    //progress should be between 0 and 1
    public void SetProgressBar(float progress)
    {
        progressBar.fillAmount = Mathf.Clamp01(progress);
    }

    public IEnumerator FadeInRoutine()
    {
        fullscreenFadeImage.gameObject.SetActive(true); 

        yield return FadeImage(fullscreenFadeImage, 0f, 1f, fadeTime);
        
        loadingObjRoot.SetActive(true); 
    }

    public IEnumerator FadeOutRoutine()
    {
        loadingObjRoot.SetActive(false); //TODO: animate loading screen end before disabling

        yield return FadeImage(fullscreenFadeImage, 1f, 0f, fadeTime);

        fullscreenFadeImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeImage(Image image, float startOpacity, float targetOpacity, float time)
    {
        if(!image)
        {
            yield break;
        }

        var col = image.color;
        if(time <= 0f)
        {
            col.a = targetOpacity;
            image.color = col;
            yield break;
        }

        var t = 0f;
        do
        {
            t += Time.deltaTime / time;
            col.a = Mathf.Lerp(startOpacity, targetOpacity, t);
            image.color = col;
        }
        while (t < 1f);
    }
}
