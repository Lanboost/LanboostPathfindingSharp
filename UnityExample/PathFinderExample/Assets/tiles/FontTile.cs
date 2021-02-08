using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FontTile : Tile
{
	private Sprite newSprite;

	public void SetSpite(Sprite s)
	{
		newSprite = s;
	}

	public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData)
	{
		base.GetTileData(location, tileMap, ref tileData);

		

		//    Change Sprite
		tileData.sprite = newSprite;
	}
}