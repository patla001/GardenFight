using UnityEngine;

public class AttackHitboxBehaviour_Fist : StateMachineBehaviour
{
    public float enableTime = 0.12f;
    public float disableTime = 0.28f;

    private FistHitbox fist;
    private bool enabledFired = false;
    private bool disabledFired = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        fist = animator.GetComponentInChildren<FistHitbox>();

        enabledFired = false;
        disabledFired = false;

        fist?.DisableHitbox();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float t = stateInfo.normalizedTime * stateInfo.length;

        if (!enabledFired && t >= enableTime)
        {
            fist?.EnableHitbox();
            enabledFired = true;
        }

        if (!disabledFired && t >= disableTime)
        {
            fist?.DisableHitbox();
            disabledFired = true;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        fist?.DisableHitbox();
    }
}
