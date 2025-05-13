using UnityEngine;

public abstract class State : MonoBehaviour
{
    protected AICharacter aiCharacter;
    private bool usePathfinding;

    public bool UsePathfinding => usePathfinding;

    public void SetStateInfo(AICharacter _aiCharacter, bool _usePathfinding)
    {
        aiCharacter = _aiCharacter;
        usePathfinding = _usePathfinding;
    }

    /// <summary>
    /// This function is called when the state is entered
    /// </summary>
    public abstract void Enter();

    /// <summary>
    /// This function is called when the state is exited
    /// </summary>
    public virtual void Exit() { }

    /// <summary>
    /// This function is called every frame while the state is active
    /// </summary>
    public virtual void Run() { }

}
