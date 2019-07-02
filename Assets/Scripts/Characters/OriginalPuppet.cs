﻿namespace WGJ.PuppetShadow
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class OriginalPuppet : Puppet
    {
        [Header("Original Puppet")]
        [SerializeField]
        private GameObject shadowPuppetPrefab;

        private ShadowPuppet shadowPuppet;

        protected override void Awake()
        {
            base.Awake();
            OnDeath += DestroyShadowPuppet;
        }
        private void Start()
        {
            CreateShadowPuppet();
        }

        private void Update()
        {

            if (movementCoroutine == null)
            {
                movementCoroutine = StartCoroutine(_IdleFloating());
            }
        }

        private void DestroyShadowPuppet()
        {
            shadowPuppet.SpawnLightPuppet();
            Destroy(shadowPuppet.gameObject);
        }

        private void CreateShadowPuppet()
        {
            shadowPuppet = Instantiate(shadowPuppetPrefab, transform.parent).GetComponent<ShadowPuppet>();
            shadowPuppet.transform.position += Vector3.right * 3f;

            shadowPuppet.gameObject.SetActive(true);


        }
    }

}
