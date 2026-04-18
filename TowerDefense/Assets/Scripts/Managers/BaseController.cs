using UnityEngine;
using System;

public class BaseController : MonoBehaviour
{
    public static event Action<int, int> OnHPChanged;

    [SerializeField] private int maxHP = 20;
    [SerializeField] private ParticleSystem damageParticles;

    public int CurrentHP { get; private set; }
    public int MaxHP => maxHP;
    public float HPRatio => (float)CurrentHP / maxHP;

    private bool halfHPReached = false;

    void Awake()
    {
        CurrentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
        OnHPChanged?.Invoke(CurrentHP, maxHP);

        AudioManager.Instance?.PlaySFX(SFXType.BaseHit, transform.position);
        VFXManager.Instance?.Play(VFXType.BaseHit, transform.position);
        CameraShake.Instance?.Shake(0.12f, 0.18f);

        if (CurrentHP <= maxHP / 2 && !halfHPReached)
        {
            halfHPReached = true;
            if (damageParticles != null)
            {
                damageParticles.Play();
            }
        }

        if (CurrentHP <= 0)
            GameManager.Instance?.TriggerGameOver(false);
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
        OnHPChanged?.Invoke(CurrentHP, maxHP);
    }
}
