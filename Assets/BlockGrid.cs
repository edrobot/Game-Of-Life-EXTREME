using UnityEngine;
using System.Collections;
using System.Collections.Generic;



[ExecuteInEditMode]
public class BlockGrid : MonoBehaviour {

	public int size_x = 0;
	public int size_y = 0;
	public int current_x = 0;
	public int current_y = 0;

	public List<Invader> DefaultInvaders = new List<Invader>();

	public bool GameMode = false;

	public List< List <LifeBlock> > BlockList = new List< List<LifeBlock> >();
	public List< List <bool> > AliveList = new List< List<bool> >();
	public List< List <int> > RuleList = new List< List<int> >();

	public LifeBlock blockObjectTemplate;

	public bool isPlaying = false;

	public float timeInterval = 1.0f;
	public float currentInterval = 1.0f;
	public int stepcount = 0;

	public float Level = 1;
	public float VelocityPerLevel = 0.4f;
	public int EnemiesPerLevel = 5;
	public int EnemiesLeft = 0;

	public float NextEnemyTimer = 0;
	public float NextEnemyTimerMin = 1;
	public float NextEnemyTimerMax = 3;

	public float DefaultTimeToDraw = 30;

	//private float BonusTime = 0;

	public float timeLeftToDraw = 0;

	public string customInput = "Enter Here";

	// Use this for initialization
	void Start () {
		GenerateGrid();
		if(GameMode)
		{
			NextEnemyTimer = Random.Range(NextEnemyTimerMin,NextEnemyTimerMax);
			EnemiesLeft = EnemiesPerLevel;
		}
	
	}

	void GenerateGrid()
	{
		foreach (List<LifeBlock> BL in BlockList)
		{
			foreach(LifeBlock B in BL)
			{
				DestroyImmediate(B.gameObject);
			}
		}
		BlockList.Clear();
		AliveList.Clear();
		RuleList.Clear();

		
		List<GameObject> children = new List<GameObject>();
		foreach (Transform child in transform) children.Add(child.gameObject);
		children.ForEach(child => DestroyImmediate(child));
		
		
		for (int n = 0; n < size_y; n++)
		{
			List<LifeBlock> newBlockList = new List<LifeBlock>();
			List<bool> newAliveList = new List<bool>();
			List<int> newRuleList = new List<int>();

			List<Dictionary<int,int>> newNextStateList = new List<Dictionary<int,int>>();
			
			for (int i = 0; i < size_x; i++)
			{
				//GameObject newObject = new GameObject();
				LifeBlock newBlock = Instantiate(blockObjectTemplate,new Vector3((i-((float) size_x/2)) * blockObjectTemplate.renderer.bounds.extents.x * 2,(n-(size_y/2))*blockObjectTemplate.renderer.bounds.extents.y * 2,0) + gameObject.transform.position, Quaternion.identity) as LifeBlock;
				newBlock.transform.parent = transform;
				newBlock.transform.localScale = new Vector3(1,1,1);
				newBlock.transform.localPosition = new Vector3((i-((float) size_x/2)) * blockObjectTemplate.renderer.bounds.extents.x * 2,(n-(size_y/2))*blockObjectTemplate.renderer.bounds.extents.y * 2,0);
				newBlock.xCoordinate = i;
				newBlock.yCoordinate = n;
				newBlock.mainGrid = this;
				newBlockList.Add(newBlock);
				newAliveList.Add(false);
				newRuleList.Add(1);

				
				
			}
			
			BlockList.Add(newBlockList);
			AliveList.Add(newAliveList);
			RuleList.Add(newRuleList);

			
			
		}
		current_x = size_x;
		current_y = size_y;

	}

	void GenerateAlien()
	{

		Invader newInvader = Instantiate(DefaultInvaders[Mathf.RoundToInt(Random.Range(0,DefaultInvaders.Count))]) as Invader;

		Vector3 newPos;

		if(Random.value >= 0.5f)
		{
			newPos = new Vector3(-2.5f,Random.Range(-0.5f,1f),transform.position.z);
			newInvader.velocityX = Level * VelocityPerLevel;
		}
		else
		{
			newPos = new Vector3(2.5f,Random.Range(-0.5f,1f),transform.position.z);
			newInvader.velocityX = -Level * VelocityPerLevel;
		}
		newInvader.transform.position = newPos;
	}

	// Update is called once per frame
	void Update () 
	{
		if(Application.isEditor)
		{

		}
		if (Application.isPlaying)
		{
			if(Input.GetKeyDown(KeyCode.Escape))
			{
				Application.Quit();
			}

			if(isPlaying)
			{
				currentInterval -= Time.deltaTime;

				if (currentInterval < 0)
				{
					GridStep();
					currentInterval = timeInterval;
				}
			}

			if(GameMode)
			{
				NextEnemyTimer -= Time.deltaTime;
				if(NextEnemyTimer < 0 && EnemiesLeft > 0)
				{
					GenerateAlien();
					NextEnemyTimer = Random.Range(NextEnemyTimerMin,NextEnemyTimerMax);
					EnemiesLeft --;
				}

				if (EnemiesLeft <= 0)
				{
					Level++;
					print ("Level " + Level.ToString());
					EnemiesPerLevel ++;
					EnemiesLeft = EnemiesPerLevel;
				}
			}
		}
	
	}

	void GridStep()
	{

		/*So a qick note on exactly what happens in a step.

		First, it figures out what the rules are going to be in each cell.
		 	* It does this first by checking what rules would cause the cell to "give birth".
		 	* If the rule would cause the cell to "give birth", it checks how many living cells, with that rule
		 	surrounds the empty cell.
		 	* The rule that will both "give birth" and has the most neighbors moves onto the next step.
		 	* If there's a tie, the cell will be assigned a "dead rule" that won't produce living cells.
		 	* If the cell has no neighbors at all, it keeps the same rule it currently has.

		Second, now that we know what rules are going to be in play we go through each cell and use the rule
		to determine if it lives or dies.

		Third, now that we're done we overwrite the old grid with the new grid, and update the grid squares accordingly.

		If there is only one type of automata on the grid, it should act indentical to a normal version of 
		Conway's Game of Life.

		As far as I can tell this entire process runs in O(n) time, so feel free to use a larger grid!

		*/


		stepcount++;
		List< List <bool> > NextAliveList = new List< List<bool> >();
		List< List <int> > NextRuleList = new List< List <int> >();

		//Figure out the rules
		Dictionary <int, Dictionary <int, Dictionary<int,int> > > NextStateList = new Dictionary <int, Dictionary <int, Dictionary<int,int> > >();
		for (int n = 0; n < AliveList.Count; n++)
		{
			List<int> newRuleList = new List<int>();

			for (int i = 0; i < AliveList[n].Count; i++)
			{
				int nextRule = NextRule(AliveList, i,n,-1);
				newRuleList.Add(nextRule);
				BlockList[n][i].currentRule = nextRule;
			}
			NextRuleList.Add(newRuleList);
		}

		//Figures out whether the cells live or die by the rules.
		//And update the grid accordingly.
		for (int n = 0; n < AliveList.Count; n++)
		{
			List <bool> NewAliveList = new List<bool>();


			for (int i = 0; i < AliveList[n].Count; i++)
			{
				bool isThisAlive = false;

				int neighbors = Neighbors(AliveList, i,n,-2);

				if(LifeBlock.RuleTypesLife[NextRuleList[n][i]].ContainsKey(neighbors))
				{
					if(LifeBlock.RuleTypesLife[NextRuleList[n][i]][neighbors] == 1)
					{
						isThisAlive = !AliveList[n][i];
					}
					if(LifeBlock.RuleTypesLife[NextRuleList[n][i]][neighbors] == 2)
					{
						isThisAlive = AliveList[n][i];
					}
					if(LifeBlock.RuleTypesLife[NextRuleList[n][i]][neighbors] == 3)
					{
						isThisAlive = true;
					}

				}

				else
				{
					isThisAlive = false;
				}

				NewAliveList.Add(isThisAlive);
			}

			NextAliveList.Add(NewAliveList);
		}
		AliveList = NextAliveList;
		RuleList = NextRuleList;
		UpdateGridParts();

	}
	
	//Fi
	void UpdateGridParts()
	{
		for (int n = 0; n < BlockList.Count; n++)
		{
			for (int i = 0; i < BlockList[n].Count; i++)
			{
				BlockList[n][i].isAlive = AliveList[n][i];
				BlockList[n][i].currentRule = RuleList[n][i];
			}
		}
	}

	int Neighbors(List< List< bool > > grid, int x, int y, int ofRuleTypeOnly)
	{
		int totalNeighbors = 0;

		for(int i = -1; i <= 1; i++)
		{
			for(int n = -1; n <= 1; n++)
			{
				if(y+i >= 0 && y+i < grid.Count)
				{
					if(x+n >= 0 && x+n < grid[y+i].Count)
					{
						if (grid[y+i][x+n]) 
						{
							if(i != 0 || n != 0) 
							{
								if (ofRuleTypeOnly == -2 || ofRuleTypeOnly == RuleList[y+i][x+n])
								{
									totalNeighbors++;
								}
							}
						}
					}
				}

			}
		}
		return totalNeighbors;
	}

	int NextRule(List< List< bool > > grid, int x, int y, int neighbors)
	{
		int returnRule = -1;
		int frequency = -1;

		foreach(KeyValuePair<int,Dictionary<int,int> > rule in LifeBlock.RuleTypesLife)
		{
			int q = Neighbors(grid,x,y,rule.Key);
			if(q > frequency && q > 0)
			{
				frequency = q;
				returnRule = rule.Key;
			}
			else if (q == frequency)
			{
				frequency = q;
				returnRule = -1;
			}
		}
		if (frequency == -1)
		{
			returnRule = RuleList[y][x];
		}
		return returnRule;


	}

	void OnGUI()
	{
		//GUI stuff. Sorry it's not centered.

		if(GameMode == false)
		{
			if (GUI.Button (new Rect (10,10,150,50), "Step")) 
			{
				GridStep();
			}

			if (!isPlaying)
			{
				if (GUI.Button (new Rect (160,10,150,50), "Play")) 
				{
					PlayToggle();
				}
			}
			else
			{
				if (GUI.Button (new Rect (160,10,150,50), "Stop")) 
				{
					PlayToggle();
				}
			}
			if (GUI.Button (new Rect (310,10,150,50), "Random")) 
			{
				if(Event.current.button == 0)
				{
					random(LifeBlock.LeftRule);
				}

				if(Event.current.button == 1)
				{
					random(LifeBlock.RightRule);
				}
			}

			if (GUI.Button (new Rect (160,Screen.height - 60,150,50), "Reset")) 
			{
				reset();
			}

			//To Fix: I either need to make this a switch case or make a list of names so I can 
			//just use one button.

			if(LifeBlock.RightRule == 1)
			{
				if (GUI.Button (new Rect (310,Screen.height - 60,150,50), "RightClick:\nDay and Night")) 
				{
					LifeBlock.RightRule = 2;
				}
			}

			else if(LifeBlock.RightRule == 2)
			{
				if (GUI.Button (new Rect (310,Screen.height - 60,150,50), "RightClick:\nLife Without Death")) 
				{
					LifeBlock.RightRule = 3;
				}
			}

			else if(LifeBlock.RightRule == 3)
			{
				if (GUI.Button (new Rect (310,Screen.height - 60,150,50), "RightClick:\nDiamoeaba")) 
				{
					LifeBlock.RightRule = 4;
				}
			}

			else if(LifeBlock.RightRule == 4)
			{
				if (GUI.Button (new Rect (310,Screen.height - 60,150,50), "RightClick:\nSeeds")) 
				{
					LifeBlock.RightRule = 999;
				}
			}

			else if(LifeBlock.RightRule == 999)
			{
				if (GUI.Button (new Rect (310,Screen.height - 60,150,50), "RightClick:\nCustom")) 
				{
					LifeBlock.RightRule = 1;
				}
			}

			if (GUI.Button (new Rect (460,Screen.height - 60,150,50), "Set Custom" + LifeBlock.currentCustomInput)) 
			{
				LifeBlock.RuleTypesLife[999] = LifeBlock.ParseRule(customInput);
			}


			customInput = GUI.TextField(new Rect (460,Screen.height - 80,150,20),customInput);


			if (GUI.Button (new Rect (10,Screen.height - 60,150,50), "Steps " + stepcount.ToString())) 
			{

			}
		}

		else
		{
			if(timeLeftToDraw > 0)
			{
				GUI.TextArea(new Rect(0,0,100,50),"Time To Draw:" + timeLeftToDraw.ToString());
				isPlaying = false;
			}
			else
			{
				isPlaying = true;
			}
		}

	}

	void PlayToggle()
	{
		if (isPlaying)
		{
			isPlaying = false;
		}
		else
		{
			isPlaying = true;
		}
	}

	void random(int rule)
	{
		for(int i = 0; i < AliveList.Count; i++)
		{
			for(int n = 0; n <AliveList[i].Count; n++)
			{
				if(Random.Range(0,100) > 50)
				{
					AliveList[i][n] = true;
					RuleList[i][n] = rule;
				}
				else
				{
					AliveList[i][n] = false;
					RuleList[i][n] = rule;
				}

			}
		}

		UpdateGridParts();
	}

	void reset()
	{
		for(int i = 0; i < AliveList.Count; i++)
		{
			for(int n = 0; n < AliveList[i].Count; n++)
			{

				AliveList[i][n] = false;
				
			}
		}
		stepcount = 0;
		UpdateGridParts();
	}
}
