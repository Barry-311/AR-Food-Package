using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayButton : MonoBehaviour
{
    public ParticleSystem CoffeBeans;
    private bool wasPressing = false;
    public void OnButtonClicked()
    {
        Debug.Log("Button Clicked");
        if (CoffeBeans == null)
            return;

        // ¿ª
        if (!wasPressing)
        {
            CoffeBeans.Play();
            wasPressing = true;
        }
        else // ¹Ø
        {
            CoffeBeans.Stop();
            wasPressing = false;
        }
    }
}
