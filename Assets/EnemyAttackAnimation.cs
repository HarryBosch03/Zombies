using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zombies.Runtime.Enemies.Common;

public class EnemyAttackAnimation : StateMachineBehaviour
{
    public float damageTime;

    private List<EnemyAttackinator> attackingAnimators = new();

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.TryGetComponent(out EnemyAttackinator attackinator))
        {
            attackingAnimators.Add(attackinator);
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime * stateInfo.length > damageTime)
        {
            var attackinator = attackingAnimators.FirstOrDefault(e => e.gameObject == animator.gameObject);
            if (attackinator != null && !attackinator.hasLanded) attackinator.LandAttack();
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var attackinator = attackingAnimators.FirstOrDefault(e => e.gameObject == animator.gameObject);
        if (attackinator != null)
        {
            attackinator.EndAttack();
            attackingAnimators.Remove(attackinator);
        }
    }
}