using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationHelper : MonoBehaviour
{
    /*
    * Contains methods to be used in animator events. Also controls some animation activations
    */

    [SerializeField] private ParticleSystem respawnParticles;
    [SerializeField] private ParticleSystem deathParticles;
    private Animator playerAnimator;

    private void OnEnable()
    {
        playerAnimator = GetComponent<Animator>();
        PlayerHealth.onDeath += PlayDeath;
        LevelManager.onPlayerRespawn += PlayRespawn;
    }

    private void OnDisable()
    {
        PlayerHealth.onDeath -= PlayDeath;
        LevelManager.onPlayerRespawn -= PlayRespawn;
    }
    private void Update()
    {

    }

    private void PlayDeath()
    {
        playerAnimator.SetTrigger("IsDead");
    }

    private void DeathStart()
    {
        if (deathParticles != null)
        {
            deathParticles.transform.position = transform.position;
            deathParticles.Play();
        }
    }

    private void RepositionDeathParticles()
    {
        if (deathParticles != null)
        {
            deathParticles.transform.position = gameObject.MyLevelManager().Player.Position;
        }
    }

    private void DeathEnd()
    {
        gameObject.MyLevelManager().GiveSoulControl();
    }

    private void PlayRespawn()
    {
        playerAnimator.SetBool("IsRespawning", true);
    }
    private void RespawnStart()
    {
        if (respawnParticles != null)
        {
            respawnParticles.Play();
        }
    }
    private void RespawnEnd()
    {
        gameObject.MyLevelManager().RespawnControl();
        playerAnimator.SetBool("IsRespawning", false);
        playerAnimator.ResetTrigger("IsDead");
    }
}
