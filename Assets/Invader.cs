using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * NOTE: this class is really, really broken.
 * If you're dead set on seeing how bad it is, 
 * try running the "invader game" scene.
 * 
 * At some point I plan on scrapping the whole thing
 * and starting over withsomething that isn't based
 * on collision detection.
 */

public class Invader : MonoBehaviour {

	public float maxHealth = 10;
	public float health = 10;
	///public Sprite selfSprite;
	public SpriteRenderer selfRenderer;

	public float velocityX = 0;
	public float velocityY = 0;

	public List<Sprite> SpriteList = new List<Sprite>();

	// Use this for initialization
	void Start () 
	{
		selfRenderer.sprite = SpriteList[Mathf.RoundToInt(Random.Range(0,SpriteList.Count))];
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector3 move = new Vector2(velocityX,velocityY);

		if (health < 0)
		{
			Destroy(gameObject);
		}

		transform.position += move*Time.deltaTime;
	}

	void OnTriggerStay2D(Collider2D hit)
	{
		//print("Triggered Entered");
		if(hit.gameObject.tag == "GridOn")
		{

			LifeBlock L = hit.gameObject.GetComponent<LifeBlock>();
			if(L.isAlive)
			{
				L.isAlive = false;
				L.mainGrid.AliveList[L.yCoordinate][L.xCoordinate] = false;
				health --;

				selfRenderer.color = Color.Lerp(Color.red,Color.white,health/maxHealth);
				//animation["InvaderColorChange"].time = 15f;
			}

		}
	}
}
