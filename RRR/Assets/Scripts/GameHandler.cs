using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[Serializable]
public class ObstacleSpawnConfig
{
	public Obstacle obstaclePrefab;
	public int Probability;
}

class SpawnMapping {
	public int Index;
	public int Min;
	public int Max;
}

public class GameHandler : MonoBehaviour
{
	public Lane[] allLanes;
	[SerializeField] private Material _laneMaterial;
	[SerializeField] private ObstacleSpawnConfig[] _obstacleSpawnConfigs;
	[SerializeField] private ObstacleSpawnConfig[] _peoplePartsSpawnConfigs;
	[SerializeField] private Transform _obstacleContainer;
	[SerializeField] private GameObject jetpackGuy = default;

	[SerializeField] private Robot _robotPrefab;

	void Start()
	{
		GameManager.Instance.StartNewGame();
		SceneManager.LoadSceneAsync("GameUI", LoadSceneMode.Additive);
		allLanes = FindObjectsOfType<Lane>();

		var robot = Instantiate(_robotPrefab);
		robot.SetStartLane(allLanes[1]);
		
		StartCoroutine(DamageOverTime());
		/*var robot2 = Instantiate(_robotPrefab);
		robot2.SetStartLane(allLanes[1]);
		
		var robot3 = Instantiate(_robotPrefab);
		robot3.SetStartLane(allLanes[2]);*/
	}

	public void LaunchJetpack()
	{
		Robot robot = FindObjectOfType<Robot>();
		var poorGuy = Instantiate(jetpackGuy);
		poorGuy.transform.position = robot.transform.position;
		poorGuy.GetComponent<Animator>().SetTrigger("HeavensCalling");
	}

	private void Update()
	{
		HandleGameTime();
		AnimateLanes();
		HandleObstacleSpawning();

		if (!GameManager.Instance.HasRobots())
		{
			OnGameOver();
		}
	}
	
	private void HandleObstacleSpawning()
	{
		GameManager.Instance.secondsToNextObstacles -= Time.deltaTime;
		if (GameManager.Instance.secondsToNextObstacles <= 0)
		{
			var indecies = Enumerable.Range(0, allLanes.Length).OrderBy(x => Random.value).Take(Random.Range(1, allLanes.Length - 1)).ToList();
			for (int i = 0; i < indecies.Count; i++)
			{
				var lineDepth = allLanes[indecies[i]].transform.position.z;
				
				var spawnObstacle = Random.Range(0, 2) == 0; // 0 or 1
				var objectsToChooseFrom = spawnObstacle ? _obstacleSpawnConfigs : _peoplePartsSpawnConfigs;
				
				var randomObstacle = PickRandom(objectsToChooseFrom);
				Instantiate(
					randomObstacle.gameObject,
					new Vector3(Config.rightLineLimit, 0, lineDepth),
					Quaternion.Euler(Vector3.zero),
					_obstacleContainer
				);
			}

			GameManager.Instance.secondsToNextObstacles = Random.Range(0.5f, 1f) * Config.levelRunSpeed;
		}
	}

	private Obstacle PickRandom(ObstacleSpawnConfig[] objectsToChooseFrom)
	{
		List<SpawnMapping> probabilityMapping = new List<SpawnMapping>();
		int maxProbability = 0;
		
		for (int i = 0; i < objectsToChooseFrom.Length; i++)
		{
			probabilityMapping.Add(new SpawnMapping()
			{
				Index = i,
				Min = maxProbability,
				Max = maxProbability + objectsToChooseFrom[i].Probability - 1
			});
			maxProbability += objectsToChooseFrom[i].Probability;
		}
		
		int random = Random.Range(0, maxProbability);

		int indexToSpawn = -1;
		foreach(SpawnMapping m in probabilityMapping)
		{
			if (m.Min <= random && m.Max >= random)
			{
				indexToSpawn = m.Index;
			}
		}

		return objectsToChooseFrom[indexToSpawn].obstaclePrefab;
	}

	private void HandleGameTime()
	{
		GameManager.Instance.remainingTime -= Time.deltaTime;

		if (!GameManager.Instance.IsGameOver && GameManager.Instance.remainingTime <= 0)
		{
			OnGameOver();
		}
	}

	private void AnimateLanes()
	{
		_laneMaterial.mainTextureOffset =
			new Vector2((_laneMaterial.mainTextureOffset.x - Time.deltaTime * (Config.levelRunSpeed / Config.laneTilingMagicNr)) % 1, 1);
	}

	private void OnGameOver()
	{
		GameManager.Instance.GameOver();
		SceneManager.LoadSceneAsync("GameOver", LoadSceneMode.Single);
		StopCoroutine(DamageOverTime());
	}

	private void OnDestroy()
	{
		_laneMaterial.mainTextureOffset = Vector2.one;
	}
	
	IEnumerator DamageOverTime()
	{
		while (true)
		{
			GameManager.Instance.DamageAllRobots(Config.damageOverTime);
			yield return Config.damageOverTimePeriod;
		}
	}
}