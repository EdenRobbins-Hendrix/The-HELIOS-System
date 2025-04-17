using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupBoostEffect : MonoBehaviour
{
    private ParticleSystem particleSystem;
    
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        
        if (particleSystem != null)
        {
            // Configure the particle system
            var main = particleSystem.main;
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.3f, 1.0f, 0.3f, 1.0f), 
                new Color(0.8f, 1.0f, 0.5f, 0.5f)
            );
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            var emission = particleSystem.emission;
            emission.rateOverTime = 20;
            
            var shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;
            
            var colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            
            // Generate a gradient that fades out
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(Color.white, 0.0f), 
                    new GradientColorKey(Color.white, 1.0f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0.0f), 
                    new GradientAlphaKey(0.0f, 1.0f) 
                }
            );
            colorOverLifetime.color = grad;
            
            // Make it burst just once
            var burst = new ParticleSystem.Burst(0.0f, 15, 25);
            emission.SetBurst(0, burst);
            
            // Start the particle system
            particleSystem.Play();
        }
    }
}