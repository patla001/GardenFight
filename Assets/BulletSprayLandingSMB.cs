
using UnityEngine;

public class BulletSprayLandingSMB : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        BossAI boss = animator.GetComponent<BossAI>();
        if (boss != null)
        {
            boss.RpcPlayLandingSound();

        }
    }
}
