
public abstract class PlayerBaseState
{
    //Current Context
    private PlayerStateMachine _ctx;
    //factory
    private PlayerStateFactory _factory;

    //Sub / Superstate variables
    private PlayerBaseState _currentSubState;
    private PlayerBaseState _currentSuperState;

    private bool _isRootState = false;

    //Getters and setters
    protected bool IsRootState { set { _isRootState = value; } get { return _isRootState; } }
    protected PlayerStateMachine Ctx { get { return _ctx; } }
    protected PlayerStateFactory Factory { get { return _factory; } }

    //Constructor
    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    {
        _ctx = currentContext;
        _factory = playerStateFactory;
    }

    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();

    public abstract void CheckSwitchStates();

    public abstract void InitialiseSubState();

    public void UpdateStates() 
    {
        UpdateState();
        if (_currentSubState != null)
        {
            _currentSubState.UpdateStates();
        }
    }

    public void ExitStates()
    {
        ExitState();
        if (_currentSubState != null)
        {
            _currentSubState.ExitStates();
        }
    }

    protected void SwitchState(PlayerBaseState newState) 
    {
        //Exit current state
        ExitState();

        //Enter new one
        newState.EnterState();

        if (_isRootState)
        {
            //Switch current state of context
            _ctx.CurrentState = newState;
        } else if (_currentSuperState != null)
        {
            //Set the current super state's sub state to the new sub state
            //Jesus
            _currentSuperState.SetSubState(newState);
        }

    }

    protected void SetSuperState(PlayerBaseState newSuperState) 
    {
        _currentSuperState = newSuperState;
    }

    protected void SetSubState(PlayerBaseState newSubState) 
    {
        _currentSubState = newSubState;
        newSubState.SetSuperState(this);
    }

}
