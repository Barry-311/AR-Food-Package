using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayButton : MonoBehaviour
{
    public ParticleSystem CoffeBeans;

    public void OnButtonClicked()
    {
        Debug.Log("Button Clicked");

        if (CoffeBeans != null)
        {
            CoffeBeans.Play();
        }
    }
}
