using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerSlashedAnimator : MonoBehaviour
{
    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        StartCoroutine(Coroutine());
        IEnumerator Coroutine()
        {
            AudioSystem.Instance?.RequestSound("EnemyAttack01");
            yield return new WaitForSeconds(.05f);
            anim.Play("slashPlaying");
            yield return new WaitForSeconds(.25f);
            AudioSystem.Instance?.RequestSound("Death01");
            yield return new WaitForSeconds(.5f);
            gameObject.SetActive(false);

            //TODO: Trigger the hurt/slain animation once it exists
            
        }
    }
}
