using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class PlanetCapture : MonoBehaviour
{
    [SerializeField] RenderTexture renderTexture;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<SpriteRenderer>().sprite = ConvertRenderTextureToSprite(renderTexture);
    }

    public static Sprite ConvertRenderTextureToSprite(RenderTexture renderTexture){
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        return sprite;
    }
}
