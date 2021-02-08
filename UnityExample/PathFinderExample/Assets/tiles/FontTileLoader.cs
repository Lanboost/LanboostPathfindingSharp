using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FontTileLoader : MonoBehaviour
{
	public Sprite[] sprites;
	public FontTile[] tiles;
    // Start is called before the first frame update
    void Start()
    {
        sprites = Resources.LoadAll<Sprite>("font");
		tiles = new FontTile[10];
		for(int i=0; i< tiles.Length; i++)
		{
			tiles[i] = new FontTile();
			tiles[i].SetSpite(sprites[i + 52]);
		}
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
