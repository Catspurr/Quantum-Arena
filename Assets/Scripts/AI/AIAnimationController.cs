using Mirror;
using UnityEngine;

//This script executes commands to change character animations
[RequireComponent (typeof (NetworkAnimator))]
public class AIAnimationController : NetworkBehaviour 
{
	#region Variables
		//Caching all the animator parameters to not search for string each time we start a new animation.
		private NetworkAnimator _networkAnimator;
        private readonly int _crouchIdle = Animator.StringToHash("CrouchIdle");
        private readonly int _crouchIdleAim = Animator.StringToHash("CrouchIdleAim");
        private readonly int _crouchMove = Animator.StringToHash("CrouchMove");
        private readonly int _idle1 = Animator.StringToHash("Idle1");
        private readonly int _idleAim = Animator.StringToHash("IdleAim");
        private readonly int _idle2 = Animator.StringToHash("Idle2");
        private readonly int _idle3 = Animator.StringToHash("Idle3");
        private readonly int _kick = Animator.StringToHash("Kick");
        private readonly int _walk = Animator.StringToHash("Walk");
        private readonly int _walkBack = Animator.StringToHash("WalkBack");
        private readonly int _run = Animator.StringToHash("Run");
        private readonly int _runAim = Animator.StringToHash("RunAim");
        private readonly int _turnLeft = Animator.StringToHash("TurnLeft");
        private readonly int _turnRight = Animator.StringToHash("TurnRight");
        private readonly int _strafeLeft = Animator.StringToHash("StrafeLeft");
        private readonly int _strafeRight = Animator.StringToHash("StrafeRight");
        private readonly int _hit1 = Animator.StringToHash("Hit1");
        private readonly int _hit2 = Animator.StringToHash("Hit2");
        private readonly int _hit3 = Animator.StringToHash("Hit3");
        private readonly int _hit4 = Animator.StringToHash("Hit4");
        private readonly int _death1 = Animator.StringToHash("Death1");
        private readonly int _death2 = Animator.StringToHash("Death2");
        private readonly int _death3 = Animator.StringToHash("Death3");
        private readonly int _death4 = Animator.StringToHash("Death4");
        private readonly int _armIdle = Animator.StringToHash("ArmIdle");
        private readonly int _armAim = Animator.StringToHash("ArmAim");
        private readonly int _armShot = Animator.StringToHash("ArmShot");

	#endregion	
	
    private void Awake() => _networkAnimator = GetComponent<NetworkAnimator>();
    public void CrouchIdle() => _networkAnimator.SetTrigger(_crouchIdle);
    public void CrouchIdleAim() => _networkAnimator.SetTrigger(_crouchIdleAim);
    public void CrouchMove() => _networkAnimator.SetTrigger(_crouchMove);
    public void Idle1() => _networkAnimator.SetTrigger(_idle1);
    public void IdleAim() => _networkAnimator.SetTrigger(_idleAim);
    public void Idle2() => _networkAnimator.SetTrigger(_idle2);
    public void Idle3() => _networkAnimator.SetTrigger(_idle3);
    public void Kick() => _networkAnimator.SetTrigger(_kick);
    public void Walk() => _networkAnimator.SetTrigger(_walk);
    public void WalkBack() => _networkAnimator.SetTrigger(_walkBack);
	public void Run() => _networkAnimator.SetTrigger(_run);
    public void RunAim() => _networkAnimator.SetTrigger(_runAim);
    public void TurnLeft() => _networkAnimator.SetTrigger(_turnLeft);
    public void TurnRight() => _networkAnimator.SetTrigger(_turnRight);
	public void StrafeLeft() => _networkAnimator.SetTrigger(_strafeLeft);
	public void StrafeRight() => _networkAnimator.SetTrigger(_strafeRight);
	public void Hit1() => _networkAnimator.SetTrigger(_hit1);
	public void Hit2() => _networkAnimator.SetTrigger(_hit2);
	public void Hit3() => _networkAnimator.SetTrigger(_hit3);
	public void Hit4() => _networkAnimator.SetTrigger(_hit4);
	public void Death1() => _networkAnimator.SetTrigger(_death1);
	public void Death2() => _networkAnimator.SetTrigger(_death2);
	public void Death3() => _networkAnimator.SetTrigger(_death3);
	public void Death4() => _networkAnimator.SetTrigger(_death4);
	public void ArmIdle() => _networkAnimator.SetTrigger(_armIdle);
	public void ArmAim() => _networkAnimator.SetTrigger(_armAim);
    public void ArmShot() => _networkAnimator.SetTrigger(_armShot);
}