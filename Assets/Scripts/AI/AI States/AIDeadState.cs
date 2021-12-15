using Mirror;

public class AIDeadState : AIBaseState
{
    public override void EnterState(AIController aiController)
    {
        var soundInstance = aiController.InstantiateSound();
        NetworkServer.Spawn(soundInstance);
        soundInstance.GetComponent<SoundPrefab>().RpcPlayRobotExplosionSound();
        aiController.DestroyBot();
    }
}