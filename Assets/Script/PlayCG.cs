using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayCG : MonoBehaviour
{

    public VideoPlayer vPlayer;
    public RawImage Image;

    // Start is called before the first frame update
    void Start()
    {
        vPlayer.Play();
    }

    // Update is called once per frame
    void Update()
    {
        Image.texture = vPlayer.texture;
    }
}
