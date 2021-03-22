using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Disaster_ShortCircuit : DisasterObject {

	private static Sprite[] ObjectSprites = null;
	private const string RESOURCE_PATH = "Sprite/Disaster/ShortCircuit";

	protected override void Start() {
		if (ObjectSprites == null)
			ObjectSprites = Resources.LoadAll<Sprite>(RESOURCE_PATH);

		sprites = ObjectSprites;
		frameInterval = 0.5f;
		base.Start();
	}

	protected override void Active() {
		TileMgr.Instance.CreateElectric(Pos);
	}
}
