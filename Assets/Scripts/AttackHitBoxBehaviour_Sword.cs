using UnityEngine;

public class AttackHitboxBehaviour_Sword : StateMachineBehaviour
{
    public float enableTime = 0.12f;
    public float disableTime = 0.28f;

    private SwordHitbox sword;
    private bool enabledFired = false;
    private bool disabledFired = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        sword = animator.GetComponentInChildren<SwordHitbox>();

        enabledFired = false;
        disabledFired = false;

        sword?.DisableHitbox();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float t = stateInfo.normalizedTime * stateInfo.length;

        if (!enabledFired && t >= enableTime)
        {
            sword?.EnableHitbox();
            enabledFired = true;
        }

        if (!disabledFired && t >= disableTime)
        {
            sword?.DisableHitbox();
            disabledFired = true;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        sword?.DisableHitbox();
    }
}
