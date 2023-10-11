using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[CreateAssetMenu(menuName = "FadeFunctions")]
public class FadeFunctions : ScriptableObject
{
    [Header("Assign")]
    public GameObject fadeScreen;
    private Image fadeImage;
    private EasyTween easyTween;
    public static IEnumerator FadeSprite(SpriteRenderer sprite, float targetA, float duration, bool unscaledTime = false)
    {
        float currentTime = 0;
        float start = sprite.color.a;
        Color spriteColor = sprite.color;
        while (currentTime < duration)
        {
            if (unscaledTime)
            {
                currentTime += Time.unscaledDeltaTime;
            }
            else
            {
                currentTime += Time.deltaTime;
            }
            spriteColor.a = Mathf.Lerp(start, targetA, currentTime / duration);
            sprite.color = spriteColor;
            yield return null;
        }
        yield break;
    }
    public static IEnumerator FadeImage(Image uiImage, float targetA, float duration, bool unscaledTime = false)
    {
        float currentTime = 0;
        float start = uiImage.color.a;
        Color spriteColor = uiImage.color;
        while (currentTime < duration)
        {
            if (unscaledTime)
            {
                currentTime += Time.unscaledDeltaTime;
            }
            else
            {
                currentTime += Time.deltaTime;
            }
            spriteColor.a = Mathf.Lerp(start, targetA, currentTime / duration);
            uiImage.color = spriteColor;
            yield return null;
        }
        yield break;
    }
    public static IEnumerator FadeText(TextMeshPro text, float targetA, float duration, bool unscaledTime = false)
    {
        float currentTime = 0;
        float start = text.color.a;
        Color spriteColor = text.color;
        while (currentTime < duration)
        {
            if (unscaledTime)
            {
                currentTime += Time.unscaledDeltaTime;
            }
            else
            {
                currentTime += Time.deltaTime;
            }
            spriteColor.a = Mathf.Lerp(start, targetA, currentTime / duration);
            text.color = spriteColor;
            yield return null;
        }
        yield break;
    }
    public static IEnumerator FadeAudio(AudioSource audioSource, float duration, float targetVolume, bool unscaledTime = false)
    { 
        float currentTime = 0;
        float start = audioSource.volume;
        while (currentTime < duration)
        {
            if (unscaledTime)
            {
                currentTime += Time.unscaledDeltaTime;
            }
            else
            {
                currentTime += Time.deltaTime;
            }

            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        yield break;
    }

    public IEnumerator FadeScreen(float targetA, float duration, bool unscaledTime = false)
    {
        //Instantiates the fade screen if it hasn't already
        if (fadeImage == null)
        {
            //Instaniates the fader
            GameObject obj = Instantiate(fadeScreen);

            //Keeps it persistent
            DontDestroyOnLoad(obj);

            //Gets the image & colour
            fadeImage = obj.GetComponent<Image>();
            Color color = fadeImage.color;
            color.a = 1; 
            fadeImage.color = color;
            
            //Gets the tween 
            easyTween = obj.GetComponent<EasyTween>();
        }

        // //Enables the fade image
        // fadeImage.enabled = true;

        // //Fades the screen
        // float currentTime = 0;
        // float start = fadeImage.color.a;
        // Color spriteColor = fadeImage.color;
        // while (currentTime < duration)
        // {
        //     if (unscaledTime)
        //     {
        //         currentTime += Time.unscaledDeltaTime;
        //     }
        //     else
        //     {
        //         currentTime += Time.deltaTime;
        //     }
        //     spriteColor.a = Mathf.Lerp(start, targetA, currentTime / duration);
        //     fadeImage.color = spriteColor;
        //     yield return null;
        // }

        // //Disables the fade image
        // if (targetA == 0) {fadeImage.enabled = false;}

        // yield break;

        //Gets the start value
        float start = fadeImage.color.a;

        //Sets the fade values
        if (targetA == start)
        {
            yield break;
        }
        else if (targetA < start)
        {
            easyTween.SetFadeStartEndValues(targetA, start);
        }
        else
        {
            easyTween.SetFadeStartEndValues(start, targetA);
        }

        //Sets tween options
        easyTween.SetAnimatioDuration(duration);
        easyTween.SetUnscaledTimeAnimation(unscaledTime);

        //starts the tween 
        easyTween.OpenCloseObjectAnimation();

        yield return new WaitForSeconds(duration);

    }
}