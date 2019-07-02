﻿namespace WGJ.PuppetShadow
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Puppet : Character
    {

        [SerializeField]
        protected bool isPathing = false;
        protected Vector3 targetPos;

        [Header("Movements")]

        [Header("Pursuit movement parameters")]
        [SerializeField]
        protected float closeRangeValue = 0.3f;
        [SerializeField]
        [Range(0.5f, 5f)]
        protected float amplitude = 1f;
        [SerializeField]
        [Range(1f, 8f)]
        protected float frequency = 0.3f;


        [Header("Idle movement parameters")]
        [SerializeField]
        [Range(0.5f, 10f)]
        protected float idleAmplitude = 1f;
        [SerializeField]
        [Range(1f, 8f)]
        protected float idleFrequency = 3f;
        
        private float randomOffsetCos;
        private float randomOffsetSin;

        protected Coroutine movementCoroutine = null;

        protected void MoveTo(Vector3 targetWorldPos)
        {
            TilePath path = LevelManager.Instance.Pathfinder.GetPath(transform.position, targetWorldPos);

            if (path.IsEmpty)
                return;

            movementCoroutine = StartCoroutine(_FollowPath(path.SimplifiedPath));
            StopCoroutine(movementCoroutine);
        }

        public void StopMovement()
        {

            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
        private IEnumerator _FollowPath(TilePath path)
        {
            isPathing = true;
            Vector3 previousPos = transform.position;
            GenerateNewRandomOffset();

            for (int i = 0; i < path.Size; i++)
            {
                targetPos = path[i].CenterWorld;
                rb.velocity = GetGhostVelocityToward(previousPos, targetPos);
                while (!IsVeryCloseTo(path[i].CenterWorld))
                {
                    yield return null;
                    rb.velocity = GetGhostVelocityToward(previousPos, targetPos);
                }
                previousPos = transform.position;
            }
            rb.velocity = Vector2.zero;
            isPathing = false;

            movementCoroutine = null;
            
        }

        protected IEnumerator _IdleFloating(float maxTime = 5f)
        {
            float timer = 0f;
            GenerateNewRandomOffset();
            
            while (timer < maxTime)
            {
                rb.velocity = new Vector2(
                    Cosinus(Time.time, idleAmplitude, idleFrequency),
                    Sinus(Time.time, idleAmplitude, idleFrequency));
                yield return null;
                timer += Time.deltaTime;
            }
            rb.velocity = Vector2.zero;


            movementCoroutine = null;
            

        }

        protected bool IsVeryCloseTo(Vector3 worldPos)
        {
            return Vector3.Distance(transform.position, worldPos) < closeRangeValue;
        }

        private Vector2 GetGhostVelocityToward(Vector3 previousPos, Vector3 targetPos)
        {
            float adjustedSpeed = speed;
            float perpendicularCoeff = 1f;

            float distanceFromNext = Vector3.Distance(transform.position, targetPos);
            float distanceFromPrevious = Vector3.Distance(transform.position, previousPos);
            float distanceTotal = Vector3.Distance(targetPos, previousPos);

            float minDistance = Mathf.Min(distanceFromNext, distanceFromPrevious);
            if (minDistance < 1f)
            {
                perpendicularCoeff = Mathf.Lerp(0.5f, 1f, Mathf.Clamp01(minDistance - closeRangeValue)); 
            }

            float t = distanceFromNext / distanceTotal;
            
            Vector3 direction = (targetPos - transform.position).normalized * adjustedSpeed;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;


            Vector3 calculatedVelocity = direction + (perpendicular * Cosinus(Time.time, amplitude, frequency) *  perpendicularCoeff);

            
            return calculatedVelocity;


        }
        
        
        private float Cosinus(float t, float amplitude, float frequency)
        {
            return amplitude * Mathf.Cos( (t + randomOffsetCos) * frequency);
        }
        private float Sinus(float t, float amplitude, float frequency)
        {
            return amplitude * Mathf.Sin((t + randomOffsetSin) * frequency);
        }

        private void GenerateNewRandomOffset()
        {

            randomOffsetSin = UnityEngine.Random.Range(0f, 1f); //to avoid synchronized different ghosts
            randomOffsetCos = UnityEngine.Random.Range(0f, 1f); 
        }

        private void OnDrawGizmos()
        {
            if (targetPos != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, targetPos);
            }
        }
    }
}
