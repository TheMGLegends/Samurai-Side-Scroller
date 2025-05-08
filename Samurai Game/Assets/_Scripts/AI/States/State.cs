using UnityEngine;

public abstract class State : MonoBehaviour
{
    protected AICharacter aiCharacter;

    public void SetAICharacter(AICharacter _aiCharacter)
    {
        aiCharacter = _aiCharacter;
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
