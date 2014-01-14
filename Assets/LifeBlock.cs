using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class LifeBlock : MonoBehaviour {

	const bool ALIVE = true;
	const bool DEAD = false;

	public BlockGrid mainGrid;

	public SpriteRenderer CurrentRenderer;
	public Sprite emptySprite;
	public Sprite fullSprite;

	public bool isAlive = false;
	public int xCoordinate = 0;
	public int yCoordinate = 0;

	public Color activated;
	public Color deactivated;

	public static int LeftRule = 0;
	public static int RightRule = 2;

	public int currentRule = 0;
	public static string currentCustomInput = "";
	public static bool drawMode = true;

	//Cell Rules Defintions:
	//No Entry means "Death Only"
	//1 means "Birth Only"
	//2 means "Sustain Only"
	//3 means "Birth and Sustain"
	
	public static Dictionary<int,Dictionary<int,int> > RuleTypesLife = 
		new Dictionary<int, Dictionary<int, int>>
	{
		//"Dead" rule, used in case of ties.
		{-1, new Dictionary<int,int>
			{
			}
		},
		//Conway's Game Of Life
		{0, new Dictionary<int,int>
			{
				{3,3},
				{2,2}
			}
		},
		//Day And Night
		{1, new Dictionary<int,int>
			{
				{3,3},
				{4,2},
				{6,3},
				{7,3},
				{8,3}
			}
		},
		//Life Without Death
		{2, new Dictionary<int,int>
			{
				{3,3},
				{0,2},
				{1,2},
				{2,2},
				{4,2},
				{5,2},
				{6,2},
				{7,2},
				{8,2}
			}
		},
		//Diamoeba
		{3, new Dictionary<int,int>
			{
				{3,1},
				{5,3},
				{6,3},
				{7,3},
				{8,3}
			}
		},
		//Seeds
		{4, new Dictionary<int,int>
			{
				{2,1}
			}
		},

		//Custom: Input your own string.
		{999, new Dictionary<int,int>
			{

			}
		}
	};

	public static Dictionary<int,Color> ruleColors = new Dictionary<int,Color>
	{
		//This class tells the cell what color it should be based on what rule it is assigned.

		{0,Color.red},
		{1,Color.yellow},
		{2,Color.white},
		{3,Color.blue},
		{4,Color.green},
		{999,Color.magenta},

	};

	// Use this for initialization
	void Start () 
	{
		//if(BirthNumbers.Find(3));
	}

	public static Dictionary<int, int> ParseRule(string input_string)
	{
		/*
		 * Parses out a rule based on the Golly notion for Life-Like Automata (thanks, Wikipedia!)
		 * 
		 * Golly notation is in the form "B[...]/S[...]", where [...] represents a set of numbers from 1-8
		 * 
		 * B means "Birth", which means an dead cell becomes alive if it surrounded by that many neighbors.
		 * S means "Sustain, which means a alive cell remains alive if it surrounded by that many neighbors.
		 * 
		 * As an example, Conway's game of Life is represented by "B3/S23"
		 * This is because it "gives birth" only if surrounded by 3 neighbors, 
		 * and "sustains" if surrounded by 2 or 3 neighbors.
		 * 
		 * Note that the B0 rule is disabled because it would spawn 
		 * itself even when it wasn't on the grid, which would make it
		 * impossible for any other rules to run.
		 */

		string regex = "^(B[1-8]*\\/)?(S[0-8]*)?$";
		//One problem down, 99 to go.

		//Regular Expression for if someone tries to use a "0" in B.
		string wrongRegex = "^(B[0-8]*\\/)?(S[0-8]*)?$";


		Dictionary<int,int> newRule = new Dictionary<int,int>();

		if(input_string == "")
		{
			currentCustomInput = "\n";
			return newRule;
		}

		if (System.Text.RegularExpressions.Regex.IsMatch(input_string,regex,System.Text.RegularExpressions.RegexOptions.IgnoreCase))
		{
			string[] words = input_string.Split('/');

			for(int i = 1; i < words[0].Length;i++)
			{
				if(!newRule.ContainsKey((int)char.GetNumericValue(words[0][i])))
				{
					newRule.Add((int)char.GetNumericValue(words[0][i]),1);
					//print("Rule:"+words[0][i]+"Births");
				}
			}
			for(int i = 1; i < words[1].Length;i++)
			{
				if(!newRule.ContainsKey((int)char.GetNumericValue(words[1][i])))
				{
					newRule.Add((int)char.GetNumericValue(words[1][i]),2);
				}
				else if(newRule[(int)char.GetNumericValue(words[1][i])] == 1)
				{
					newRule[(int)char.GetNumericValue(words[1][i])] = 3;
				}
			}

			print ("Birth Only");

			foreach(KeyValuePair<int,int> k in newRule)
			{
				if(k.Value == 1)
				{
					print (k.Key);
					if(k.Key == 0)
					{

					}
				}
			}

			print ("Sustain Only");
			
			foreach(KeyValuePair<int,int> k in newRule)
			{
				if(k.Value == 2)
				{
					print (k.Key);
				}
			}

			print ("Birth/Sustain");
			
			foreach(KeyValuePair<int,int> k in newRule)
			{
				if(k.Value == 3)
				{
					print (k.Key);
				}
			}
			currentCustomInput = "\n"+input_string;
		}
		else if(System.Text.RegularExpressions.Regex.IsMatch(input_string,wrongRegex,System.Text.RegularExpressions.RegexOptions.IgnoreCase))
		{
			currentCustomInput = "\n";
			Debug.LogError("Invalid Rule! You cannot have a cell give birth if it has 0 neighbors.");
		}
		else
		{
			currentCustomInput = "\n";
			Debug.LogError("Invalid Rule!");
		}

		return newRule;
	}

	// Update is called once per frame
	void Update () 
	{
		if (isAlive)
		{
			CurrentRenderer.sprite = fullSprite;
			if(ruleColors.ContainsKey(currentRule))
			{
				CurrentRenderer.color = ruleColors[currentRule];
			}
			else
			{
				CurrentRenderer.color = Color.grey;
			}
			//gameObject.collider2D.enabled = true;

		}
		else
		{
			CurrentRenderer.sprite = emptySprite;
			CurrentRenderer.color = deactivated;
			//gameObject.collider2D.enabled = false;
		}
	}
	
	void OnMouseOver()
	{
		if (Input.GetMouseButtonDown(0))
		{
			ToggleCell(LeftRule);
		}
		if (Input.GetMouseButtonDown(1))
		{
			ToggleCell(RightRule);
		}

		if (Input.GetMouseButton(0))
		{
			SetCell(LeftRule,drawMode);
		}
		if (Input.GetMouseButton(1))
		{
			SetCell(RightRule,drawMode);
		}
	}

	void SetCell(int rule, bool state)
	{
		drawMode = state;
		isAlive = state;

		if(isAlive)
		{
			CurrentRenderer.sprite = fullSprite;
		}
		else
		{
			CurrentRenderer.sprite = emptySprite;
		}

		mainGrid.AliveList[yCoordinate][xCoordinate] = isAlive;
		mainGrid.RuleList[yCoordinate][xCoordinate] = rule;
		currentRule = rule;
	}

	void ToggleCell(int rule)
	{
		if(isAlive)
		{
			drawMode = false;
			isAlive = false;
			CurrentRenderer.sprite = emptySprite;
		}
		else
		{
			drawMode = true;
			isAlive = true;
			CurrentRenderer.sprite = fullSprite;
		}
		mainGrid.AliveList[yCoordinate][xCoordinate] = isAlive;
		mainGrid.RuleList[yCoordinate][xCoordinate] = rule;
		currentRule = rule;
	}

}
