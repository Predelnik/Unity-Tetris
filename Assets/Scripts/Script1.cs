using UnityEngine;
using System.Collections;
using System.Collections.Generic;

enum FigureType
{
	I,
	J,
	L,
	O,
	S,
	T,
	Z,
}

class EnumUtils<T> where T : struct, System.IConvertible
{
	static private void enumCheck ()
	{
		if (!typeof(T).IsEnum) 
		{
			throw new System.ArgumentException("T must be an enumerated type");
		}
	}

	static public int enumCount ()
	{
		enumCheck ();
		return System.Enum.GetNames(typeof(T)).Length;
	}

	static public T randomVal ()
	{
		enumCheck ();
		return (T) System.Enum.GetValues (typeof (T)).GetValue (Random.Range (0, enumCount ()));
	}
}

public class BrickCoords
{
	public int x, y;
	public BrickCoords (int xArg, int yArg)
	{
		x = xArg;
		y = yArg;
	}

	public void rotate90 (Vector2 rotationCenter)
	{
		float xF = x, yF = y;

		xF -= rotationCenter.x; yF -= rotationCenter.y;
		float xFnew = yF, yFnew = -xF;
		x = (int) (xFnew + rotationCenter.x);
		y = (int) (yFnew + rotationCenter.y);
	}
}

public class BBox
{
	public int xMin, xMax, yMin, yMax;
	public BBox () { xMin = 0; xMax = 0; yMin = 0; yMax = 0; }

	public int getHeight () { return yMax - yMin; }
	public int getWidth () { return xMax - xMin; }

	public static BBox calcBBox (List<BrickCoords> list)
	{
		BBox bbox = new BBox ();
		if (list.Count == 0)
			throw new wrongObjectStateException ();
		bbox.xMin = bbox.xMax = list[0].x;
		bbox.yMin = bbox.yMax = list[0].y;
		foreach (var coords in list)
		{
			bbox.xMin = System.Math.Min (coords.x, bbox.xMin);
			bbox.xMax = System.Math.Max (coords.x, bbox.xMax);
			bbox.yMin = System.Math.Min (coords.y, bbox.yMin);
			bbox.yMax = System.Math.Max (coords.y, bbox.yMax);
		}
		return bbox;
	}
}

public class wrongObjectStateException : System.Exception
{
	public wrongObjectStateException()
	{
	}
	
	public wrongObjectStateException(string message)
		: base(message)
	{
	}
	
	public wrongObjectStateException(string message, System.Exception inner)
		: base(message, inner)
	{
	}
}

public class Figure
{
	private FigureType curType = FigureType.I;
	public int posX { get; set;}
	public int posY { get; set;}
	private int curRotation = 0; // number of rotations by 90 degrees
	static readonly int rotationCount = 4;

	public Figure ()
	{
		posX = 0;
		posY = 0;
	}

	static public Figure getRandomFigure () // Creates figure with random rotation and type
	{
		Figure res = new Figure ();
		res.curType = EnumUtils<FigureType>.randomVal ();
		res.curRotation = Random.Range (0, 4);
		return res;
	}

	public int brickCount ()
	{
		return GetRotatedBricksCoordsList ().Count;
	}

	public Figure clone ()
	{
		Figure res = new Figure ();
		res.posX = posX;
		res.posY = posY;
		res.curRotation = curRotation;
		res.curType = curType;
		return res;
	}
	
	public List<BrickCoords> GetBrickCoordsList ()
	{
		var list = GetRotatedBricksCoordsList ();
		foreach (var coords in list)
			{
				coords.x += posX;
				coords.y += posY;
			}
		return list;
	}

	private List<BrickCoords> GetRotatedBricksCoordsList ()
	{
		var list = GetUnmodifiedBricksCoordList ();
		foreach (var coords in list)
		{
			for (int i = 0; i < curRotation; i++)
			{
				coords.rotate90 (getRotationCenter ());
			}
		}
		return list;
	}

	private Vector2 getRotationCenter ()
	{
		switch (curType) {
		case FigureType.O:
		case FigureType.I:
			return new Vector2 (0.5f, 0.5f);
	    case FigureType.L:
		case FigureType.J:
		case FigureType.S:
		case FigureType.Z:
		case FigureType.T:
			return new Vector2 (0.0f, 0.0f);
				}
		return new Vector2 (0.0f, 0.0f);
	}

	private List<BrickCoords> GetUnmodifiedBricksCoordList ()
	{
		int[,] rawCoords = null;
		switch (curType) {
		case FigureType.L:
			rawCoords = new int[,] {{-1, 0}, {0, 0}, {1, 0}, {1, 1}};
			break;
		case FigureType.J:
			rawCoords = new int[,] {{-1, 0}, {0, 0}, {1, 0}, {-1, 1}};
			break;
		case FigureType.O:
			rawCoords = new int[,] {{0, 0}, {1, 0}, {1, 1}, {0, 1}};
			break;
		case FigureType.I:
			rawCoords = new int[,] {{-1, 0}, {0, 0}, {1, 0}, {2, 0}};
			break;
		case FigureType.S:
			rawCoords = new int[,] {{-1, 0}, {0, 0}, {0, 1}, {1, 1}};
			break;
		case FigureType.Z:
			rawCoords = new int[,] {{1, 0}, {0, 0}, {0, 1}, {1, -1}};
			break;
		case FigureType.T:
			rawCoords = new int[,] {{-1, 0}, {0, 0}, {1, 0}, {0, 1}};
			break;
		}
		if (rawCoords == null)
			throw new wrongObjectStateException ();
		var res = new List<BrickCoords> ();
		for (int i = 0; i < rawCoords.GetLength(0); i++) 
			{
			res.Add (new BrickCoords (rawCoords[i, 0], rawCoords[i, 1]));
			}
		return res;
	}

	public void rotate ()
	{
		curRotation = (curRotation + 1) % rotationCount;
	}

	public void fall ()
	{
		posY--;
	}

	public BBox getRotatedBBox ()
	{
		var list = GetRotatedBricksCoordsList ();
		return BBox.calcBBox (list);
	}

	public BBox getBBox ()
	{
		var list = GetBrickCoordsList ();
		return BBox.calcBBox (list);
	}
}

public class Script1 : MonoBehaviour {
	public int fieldWidth = 10;
	public int fieldHeight = 20;
	public float fastFallSpeedTick = 0.1f;
	public GameObject brickSprite;
	public GameObject fieldBackdropSprite;

	private GameObject foreGround;
	GameObject[,] field;
	private float fallSpeedTick = 0.5f;
	private float inputSpeedTick = 0.1f;
	private Figure curFigure;
	private readonly List<GameObject> curFigureParts = new List<GameObject> (); 
	private readonly List<GameObject> nextFigureParts = new List<GameObject> (); 
	private float nextFallTick = 0.0f;
	private float nextFastFallTick = 0.0f;
	private float nextInputTick = 0.0f;
	private Figure nextFigure = null;
	private Vector3 offset;
	private Vector3 fieldAbsSize;
	private int currentLevel = 1;
	private int currentScore = 0;
	private GameObject fieldBackdrop;

	// Use this for initialization
	void Start () {
		field = new GameObject[fieldWidth, fieldHeight];
		for (int i = 0; i < fieldWidth; i++)
			for (int j = 0; j < fieldHeight; j++)
				field [i, j] = null;
		fieldBackdrop = (GameObject) Instantiate (fieldBackdropSprite);
		foreGround = GameObject.Find ("Foreground");
		var fieldSpriteSize = fieldBackdrop.GetComponent<SpriteRenderer> ().bounds.size;
		var brickSpriteSize = brickSprite.GetComponent<SpriteRenderer> ().bounds.size;
		fieldAbsSize = new Vector3 (brickSpriteSize.x * fieldWidth, brickSpriteSize.y * fieldHeight, 0);
		fieldBackdrop.transform.position = new Vector3 (0.0f, 0.0f, 5.0f);
		offset = (-fieldAbsSize * 0.5f);
		fieldBackdrop.transform.localScale = new Vector3 (fieldAbsSize.x / fieldSpriteSize.x, 
		                                                  fieldAbsSize.y / fieldSpriteSize.y, 0);
	}


	void setBrickSpritePos (GameObject sprite, int x, int y)
	{
		var transform = sprite.GetComponent<Transform> ();
		var spriteSize = brickSprite.GetComponent<SpriteRenderer> ().bounds.size;			
		transform.position = new Vector3 ((x + 0.5f) * spriteSize.x, (y + 0.5f) * spriteSize.y, 0.0f) + offset;
	}

	Figure GenerateFigure ()
	{
		if (nextFigure == null)
			nextFigure = Figure.getRandomFigure ();
		var res = nextFigure;
		nextFigure = Figure.getRandomFigure ();
		var bbox = res.getRotatedBBox ();
		res.posY = fieldHeight;
		res.posX = Random.Range (-bbox.xMin, fieldWidth - 1 - bbox.xMax);
		foreach (var part in nextFigureParts)
			Destroy (part);
		foreach (var part in curFigureParts)
			  Destroy (part);
		curFigureParts.Clear ();
		nextFigureParts.Clear ();
		for (int i = 0; i < res.brickCount (); i++)
			curFigureParts.Add ((GameObject) Instantiate (brickSprite));
		for (int i = 0; i < nextFigure.brickCount (); i++)
			nextFigureParts.Add ((GameObject) Instantiate (brickSprite));
		return res;
	}

	void updateFigurePartsPosition (Figure figure,List<GameObject> figureParts, bool hideOutOfBounds)
	{
		var list = figure.GetBrickCoordsList ();
		for (int i = 0; i < list.Count; i++) {
			if (hideOutOfBounds)
				figureParts[i].GetComponent<SpriteRenderer> ().enabled = inBounds (list[i]);
			setBrickSpritePos (figureParts[i], list[i].x, list[i].y);
		}
	}

	void updateCurFigurePartsPosition ()
	{
		updateFigurePartsPosition (curFigure, curFigureParts, true);
	}

	void updateNextFigurePartsPosition ()
	{
		var bbox = nextFigure.getRotatedBBox ();
		var nextFigureClone = nextFigure.clone ();
		nextFigureClone.posX = fieldWidth + 2 - bbox.xMin;
		nextFigureClone.posY = fieldHeight - 1 - bbox.yMax;
		updateFigurePartsPosition (nextFigureClone, nextFigureParts, false);
	}

	void generateFigureIfNeeded ()
	{
		if (curFigure == null) {
			curFigure = GenerateFigure ();
			updateCurFigurePartsPosition ();
			updateNextFigurePartsPosition ();
		}
	}

	bool inBounds (BrickCoords coords)
	{
		return (coords.x >= 0 && coords.y >= 0 && coords.x < fieldWidth && coords.y < fieldHeight);
	}

	bool isCollided (Figure figure)
	{
		var bbox = figure.getBBox ();
		if (bbox.xMin < 0 || bbox.yMin < 0 || bbox.xMax >= fieldWidth)
			return true;
		var list = figure.GetBrickCoordsList ();
		foreach (var coords in list) {
					if (inBounds (coords) && field[coords.x, coords.y] != null)
						return true;
				}
		return false;
	}

	void gameLost ()
	{
		for (int i = 0; i < fieldWidth; i++)
						for (int j = 0; j < fieldHeight; j++) {
								if (field[i, j] != null)
									{
										Destroy (field[i, j]);
										field [i, j] = null;
									}
						}
		currentScore = 0;
		currentLevel = 0;
		updateSpeedTick ();
	}

	void stamp ()
	{
		var list = curFigure.GetBrickCoordsList ();
		foreach (var coords in list) {
				if (!inBounds (coords))
					{
						gameLost ();
						return;
					}
				var newSprite = (GameObject) Instantiate (brickSprite);
				field[coords.x, coords.y] = newSprite;
				setBrickSpritePos (newSprite, coords.x, coords.y);
			}
	}

	void checkAndCollapseIfNeeded ()
	{
		System.Func<int, bool> rowFilled = (int i) => { for (int j = 0; j < fieldWidth; j++) if (field[j, i] == null) { return false;} return true; };
		int linesDestroyed = 0;
		for (int i = 0; i < fieldHeight; i++) {
					if (rowFilled (i))
						{
				linesDestroyed++;
							for (int t = 0; t < fieldWidth; t++) 
							{
								Destroy (field[t, i]);
								field[t, i] = null;
							}
								
							for (int j = i + 1; j < fieldHeight; j++)
								{
									for (int t = 0; t < fieldWidth; t++) 
										{
											if (field[t, j] != null)
												setBrickSpritePos (field[t, j], t, j - 1);

											field[t, j - 1] = field[t, j];
										}
								}
							for (int t = 0; t < fieldWidth; t++) 
								field[t, fieldHeight - 1] = null;
							i--;
						}
				}

		currentScore += (int) ((Mathf.Pow (linesDestroyed, 1.5f) * 10.0f) * Mathf.Pow (1.5f, currentLevel));
		if (currentScore > (int) Mathf.Pow(currentLevel, 1.5f) * 200) {
					currentLevel++;
			updateSpeedTick ();
				}
	}

	void updateSpeedTick ()
	{
		fallSpeedTick = 0.5f /  Mathf.Pow (1.2f, (currentLevel - 1));
	}
	
	void processFigureMovement ()
	{
		bool dropFast = Input.GetButton ("DropFast");
		if ((!dropFast && Time.time > nextFallTick) ||
		    (dropFast && Time.time > nextFastFallTick)
		    )
		    {
			nextFallTick = Time.time + fallSpeedTick;
			nextFastFallTick = Time.time + fastFallSpeedTick;
			Figure newFigure = curFigure.clone ();
			newFigure.fall ();
			if (isCollided (newFigure))
			{
				stamp ();
				curFigure = null;
			}
			else
			{
				curFigure = newFigure;
				updateCurFigurePartsPosition ();
			}
			checkAndCollapseIfNeeded ();
		}

		System.Action updateInputTick = () => { nextInputTick = Time.time + inputSpeedTick; };
		if (Time.time > nextInputTick && curFigure != null) {					
			if (Input.GetButtonDown ("RotateRight")) {
					var newFigure = curFigure.clone ();
					newFigure.rotate ();
					var bbox = newFigure.getBBox ();
					if (bbox.xMin < 0)
						newFigure.posX -= bbox.xMin;	
					if (bbox.xMax >= fieldWidth)
					newFigure.posX -= (bbox.xMax + 1 - fieldWidth);
					if (!isCollided (newFigure)) {
						curFigure = newFigure;
						updateCurFigurePartsPosition ();
					}
				    updateInputTick ();
				}
			int movement = 0;
			if (Input.GetButton ("MoveLeft")) 
				movement = -1;
			else if (Input.GetButton ("MoveRight")) 
				movement = 1;
			if (movement != 0)
			{
				var newFigure = curFigure.clone ();
				newFigure.posX += movement;
				if (!isCollided (newFigure)) {
					curFigure = newFigure;
					updateCurFigurePartsPosition ();
				}
				updateInputTick ();
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		generateFigureIfNeeded ();
		if (curFigure != null) {
			processFigureMovement ();
				}
	}

	void OnGUI () {
		// Make a background box
		float xPos = Screen.width * 0.5f - Screen.height * 0.1f * fieldAbsSize.x * 0.5f - 90.0f;
		float yPos = Screen.height * (0.5f - 0.1f * fieldAbsSize.y * 0.5f);
		GUI.Box(new Rect(xPos, yPos, 80.0f, 100.0f), "Status");
		GUI.Label (new Rect (xPos + 5.0f, yPos + 25.0f, 80.0f, 100.0f), "Level: " + currentLevel);
		GUI.Label (new Rect (xPos + 5.0f, yPos + 40.0f, 80.0f, 100.0f), "Score: " + currentScore);
	}

}
