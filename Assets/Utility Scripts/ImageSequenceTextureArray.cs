using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ImageSequenceTextureArray : MonoBehaviour
{
    public float delay;
    public List<Texture> textures;
    //With this Material object, a reference to the game object Material can be stored  
    private Material goMaterial;
    public string[] textureNames = { "_MainTex" };
    //An integer to advance frames  
    private int frameCounter = 0;

    void Awake()
    {
        //Get a reference to the Material of the game object this script is attached to  
        this.goMaterial = GetComponent<Renderer>().material;
    }
    [ContextMenu("SortImageSequence")]
    public void SortImageSequence() {
        this.textures = this.textures.OrderBy(s => s.name).ToList();
    }

    void Start()
    {
        //Cast each Object to Texture and store the result inside the Textures array  
        for (int i = 0; i < textures.Count; i++)
        {
            this.textures[i] = textures[i];
        }
    }

    public bool loop;

    public void SetLoop(bool b)
    {
        loop = b;
    }

    void Update()
    {
        //Call the 'PlayLoop' method as a coroutine with a 0.04 delay  
        if (loop)
            StartCoroutine("PlayLoop", delay);
        else
            StartCoroutine("Play", delay);
        //Set the material's texture to the current value of the frameCounter variable  
        foreach (var texName in textureNames)
        {
            goMaterial.SetTexture(texName, textures[frameCounter]);
        }

    }

    //The following methods return a IEnumerator so they can be yielded:  
    //A method to play the animation in a loop  
    IEnumerator PlayLoop(float delay)
    {
        //Wait for the time defined at the delay parameter  
        yield return new WaitForSeconds(delay);

        //Advance one frame  
        frameCounter = (++frameCounter) % textures.Count;

        //Stop this coroutine  
        StopCoroutine("PlayLoop");
    }

    //A method to play the animation just once  
    IEnumerator Play(float delay)
    {
        //Wait for the time defined at the delay parameter  
        yield return new WaitForSeconds(delay);

        //If the frame counter isn't at the last frame  
        if (frameCounter < textures.Count - 1)
        {
            //Advance one frame  
            ++frameCounter;
        }

        //Stop this coroutine  
        StopCoroutine("Play");
    }
}
