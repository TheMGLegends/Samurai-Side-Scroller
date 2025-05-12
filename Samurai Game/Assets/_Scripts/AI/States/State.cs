using UnityEngine;

public abstract class State : MonoBehaviour
{
    protected AICharacter aiCharacter;
    private bool usePathfinding;

    public bool UsePathfinding => usePathfinding;

    public void SetAICharacter(AICharacter _aiCharacter)
    {
        aiCharacter = _aiCharacter;
    }

    public void SetUsePathfinding(bool _usePathfinding)
    {
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
