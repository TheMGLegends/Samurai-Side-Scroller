using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "ScriptableObjects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public GameObject enemyPrefab;
    public bool isGrounded = true;

    [Tooltip("Used when enemy is a ground type, to prevent it from being spawned in the ground")]
    public Vector2 groundOffset = Vector2.zero;
}
