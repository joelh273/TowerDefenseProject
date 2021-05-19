using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Game : MonoBehaviour
{
	[SerializeField]
	GameScenario scenario = default;

	[SerializeField]
	Vector2Int boardSize = new Vector2Int(11, 11);

	[SerializeField]
	GameBoard board = default;

	[SerializeField]
	GameTileContentFactory tileContentFactory = default;

	[SerializeField, Range(0, 100)]
	int startingPlayerHealth = 10;

	[SerializeField, Range(1f, 10f)]
	float playSpeed = 1f;

	[SerializeField]
	WarFactory warFactory = default;

	GameScenario.State activeScenario;

	int playerHealth;

	bool defeat = false;

	public TextMeshProUGUI healthText;

	const float pausedTimeScale = 0f;

	GameBehaviorCollection enemies = new GameBehaviorCollection();
	GameBehaviorCollection nonEnemies = new GameBehaviorCollection();

	Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

	SelectedObjectType selectedObjectType;

	void Awake()
	{
		playerHealth = startingPlayerHealth;
		setHealthText();
		board.Initialize(boardSize, tileContentFactory);
		board.ShowGrid = true;
		activeScenario = scenario.Begin();
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			HandleTouch();
		}
		if (Input.GetKeyDown(KeyCode.V))
		{
			board.ShowPaths = !board.ShowPaths;
		}
		if (Input.GetKeyDown(KeyCode.G))
		{
			board.ShowGrid = !board.ShowGrid;
		}
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			if (selectedObjectType !=SelectedObjectType.Wall)
            {
				SelectObjectTypeClick(selectedObjectType, SelectedObjectType.Wall);
            }
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			if (selectedObjectType != SelectedObjectType.Laser)
			{
				SelectObjectTypeClick(selectedObjectType, SelectedObjectType.Laser);
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			if (selectedObjectType != SelectedObjectType.Mortar)
			{
				SelectObjectTypeClick(selectedObjectType, SelectedObjectType.Mortar);
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			if (selectedObjectType != SelectedObjectType.Spawn)
			{
				SelectObjectTypeClick(selectedObjectType, SelectedObjectType.Spawn);
			}
		}
		else if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			if (selectedObjectType != SelectedObjectType.Destination)
			{
				SelectObjectTypeClick(selectedObjectType, SelectedObjectType.Destination);
			}
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Time.timeScale = Time.timeScale > pausedTimeScale ? pausedTimeScale : playSpeed;
			var n = FindObjectOfType<Canvas>().transform.GetChild(6).gameObject;
			n.SetActive(!n.activeSelf);
		}
		else if (Time.timeScale > pausedTimeScale)
		{
			Time.timeScale = playSpeed;
		}
		if (Input.GetKeyDown(KeyCode.B))
		{
			BeginNewGame();
		}
		if (playerHealth <= 0 && startingPlayerHealth > 0 && defeat == false)
		{
			defeat = true;
			FindObjectOfType<AudioManager>().Play("Defeat");
			Time.timeScale = Time.timeScale > pausedTimeScale ? pausedTimeScale : playSpeed;
			var n = FindObjectOfType<Canvas>().transform.GetChild(8).gameObject;
			n.SetActive(!n.activeSelf);
		}
		if (!activeScenario.Progress() && enemies.IsEmpty)
		{
			FindObjectOfType<AudioManager>().Play("Victory");
			Time.timeScale = Time.timeScale > pausedTimeScale ? pausedTimeScale : playSpeed;
			var n = FindObjectOfType<Canvas>().transform.GetChild(7).gameObject;
			n.SetActive(!n.activeSelf);
			activeScenario.Progress();
		}

		enemies.GameUpdate();
		Physics.SyncTransforms();
		board.GameUpdate();
		nonEnemies.GameUpdate();
	}

	void setHealthText()
    {
		healthText.text = "Health: " + playerHealth.ToString();
    }

	void SelectObjectTypeClick(SelectedObjectType previousType, SelectedObjectType newType)
	{
		selectedObjectType = newType;
		var n = FindObjectOfType<Canvas>().transform.GetChild((int) newType +1).GetComponent<RectTransform>();
		n.anchoredPosition = new Vector3(n.anchoredPosition.x, n.anchoredPosition.y + 20);
		var old = FindObjectOfType<Canvas>().transform.GetChild((int)previousType + 1).GetComponent<RectTransform>();
		old.anchoredPosition = new Vector3(old.anchoredPosition.x, old.anchoredPosition.y - 20);
	}

	void HandleTouch()
	{
		GameTile tile = board.GetTile(TouchRay);
		if (tile != null)
		{
			if (selectedObjectType == SelectedObjectType.Wall)
			{
				board.ToggleWall(tile);
			}
			if (selectedObjectType == SelectedObjectType.Laser)
			{
				board.ToggleTower(tile, TowerType.Laser);
			}
			if (selectedObjectType == SelectedObjectType.Mortar)
			{
				board.ToggleTower(tile, TowerType.Mortar);
			}
			if (selectedObjectType == SelectedObjectType.Spawn)
			{
				board.ToggleSpawnPoint(tile);
			}
			if (selectedObjectType == SelectedObjectType.Destination)
			{
				board.ToggleDestination(tile);
			}
		}
	}

	public static void SpawnEnemy(EnemyFactory factory, EnemyType type)
	{
		GameTile spawnPoint =
			instance.board.GetSpawnPoint(Random.Range(0, instance.board.SpawnPointCount));
		Enemy enemy = factory.Get(type);
		enemy.SpawnOn(spawnPoint);

		instance.enemies.Add(enemy);
	}

	static Game instance;

	public static Shell SpawnShell()
	{
		Shell shell = instance.warFactory.Shell;
		instance.nonEnemies.Add(shell);
		return shell;
	}

	public static Explosion SpawnExplosion()
	{
		Explosion explosion = instance.warFactory.Explosion;
		instance.nonEnemies.Add(explosion);
		return explosion;
	}

	void BeginNewGame()
	{
		playerHealth = startingPlayerHealth;
		setHealthText();
		enemies.Clear();
		nonEnemies.Clear();
		board.Clear();
		activeScenario = scenario.Begin();
	}

	public static void EnemyReachedDestination()
	{
		instance.playerHealth -= 1;
		instance.setHealthText();
	}

	void OnEnable()
	{
		instance = this;
	}

	public enum SelectedObjectType
	{
		Wall, Laser, Mortar, Spawn, Destination
	}

}