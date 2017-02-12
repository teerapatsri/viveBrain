using UnityEngine;
using System.Collections;

public class ParticleController : MonoBehaviour
{
    [Header(" [Controlling Wand]")]
    [Tooltip("Controlling Wand")]
    public WandController wand;
    [Header(" [Target]")]
    [Tooltip("Shooting Target")]
    public Transform target;
    private ParticleSystem particlesys;
    private bool on;
    // Use this for initialization
    void Start()
    {
        on = false;
        particlesys = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        on = wand.IsGripping();
        if (on)
        {
            particlesys.Play();
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[GetComponent<ParticleSystem>().particleCount];
            particlesys.GetParticles(particles);
            for (int i = 0; i < particles.Length; i++)
            {
                Vector3 dir = target.position - particles[i].position;
                if (dir.sqrMagnitude < 1) particles[i].remainingLifetime = 0;
                dir.Normalize();
                particles[i].velocity += dir * Time.deltaTime * 10f;
            }
            particlesys.SetParticles(particles, particles.Length);
        }
        else
        {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[GetComponent<ParticleSystem>().particleCount];
            particlesys.GetParticles(particles);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].remainingLifetime = 0;
            }
            particlesys.SetParticles(particles, particles.Length);
            particlesys.Stop();
        }
    }
}
