﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(NavMeshAgent))]
public class PanicController : MonoBehaviour
{
    public float PanicLevel;
    public float PanicThreshold = 300;
    public float PanicIncrease = 10;
    public float PanicIncreaseNoFlash = 30;
    public float PanicDecrease = 10;
    public GameObject ReturnTo;
    public AudioSource heartbeatSource;

    // Update is called once per frame
    void Update()
    {
        heartbeatSource.volume = Math.Min(PanicLevel / PanicThreshold, 1);
        var candles = GameObject.FindGameObjectsWithTag("Candle");
        var inDarkness = !candles.Any(IsInLight);
        var playerCtrl = GetComponent<PlayerController>();
        var hasFlashLight = playerCtrl.Carry().carryLight;
        var hasTeddy = playerCtrl.Carry().carryTeddy;

        float increase;
        if (inDarkness)
        {
            if (!hasTeddy)
            {
                increase = float.MaxValue;
            }
            else
            {
                increase = hasFlashLight ? PanicIncrease : PanicIncreaseNoFlash;
            }
        }
        else
        {
            increase = -PanicDecrease;
        }
        PanicLevel = Math.Max(PanicLevel + increase * Time.deltaTime, 0f);

        if (!playerCtrl.Panic)
        {
            if (PanicLevel > PanicThreshold)
            {
                if (hasTeddy)
                {
                    TextController.Get().ShowText("I am sure I saw something move in the dark!", Color.red, 4f);
                }
                else
                {
                    TextController.Get().ShowText("It's too dark. I need my Teddy!", Color.red, 4f);
                }
                playerCtrl.ForceMovement(ReturnTo.transform.position);
                playerCtrl.Panic = true;
            }
        }
        else
        {
            var navAgent = GetComponent<NavMeshAgent>();
            if (DidAgentReachDestination(navAgent))
            {
                playerCtrl.Panic = false;
                playerCtrl.StopForceMovement();
                PanicLevel = 0;
            }
        }
    }

    private static bool DidAgentReachDestination(NavMeshAgent agent)
    {
        var distance = Vector3.Distance(agent.gameObject.transform.position, agent.destination);
        return distance <= agent.stoppingDistance;
    }

    private bool IsInLight(GameObject candle)
    {
        var cc = candle.GetComponentInParent<CandleController>();
        if (cc != null && !cc.lit)
        {
            return false;
        }
        var range = candle.GetComponentInChildren<Light>().range;
        var direction = transform.position - candle.transform.position;
        var distance = Vector3.Magnitude(direction);
        if (distance <= range)
        {
            RaycastHit hit;
            if (!Physics.Raycast(candle.transform.position, direction, out hit, distance))
            {
                return false;
            }
            return hit.collider.GetComponentInParent<PlayerController>();
        }
        return false;
    }
}