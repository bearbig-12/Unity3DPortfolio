
using System;
using UnityEngine;

public static class FuzzyBossAttack
{
    public struct Scores
    {
        public float melee;
        public float ranged;
        public float aoe;

    }
    public static Scores AttackProb(float relatedHP, float playerStamina, float distance, bool currentPhase)
    {
        relatedHP = Mathf.Clamp(relatedHP, -100f, 100f);
        playerStamina = Mathf.Clamp(playerStamina, 0f, 100f);
        distance = Mathf.Clamp(distance, 0f, 30f);

        // HP memberships
        float enemyAdv = LShoulder(relatedHP, -80, -20);
        float even = Tri(relatedHP, -40, 0f, 40);
        float playerAdv = RShoulder(relatedHP, 20, 80);

        // Stamina memberships 
        float staLow = LShoulder(playerStamina, 0f, 45f);
        float staMid = Tri(playerStamina, 20f, 50f, 80f);
        float staHigh = RShoulder(playerStamina, 65f, 100f);

        // Distance memberships
        float close = LShoulder(distance, 1.5f, 3.0f);
        float mid = Tri(distance, 2.5f, 5.5f, 8.0f);
        float far = RShoulder(distance, 6.5f, 11.0f);

        Scores s = new Scores();
        float aoePriority = currentPhase ? 1f : 0f;

        if (currentPhase)
        {
            // AOE rules
            Rule(ref s.aoe, mid, staLow, 1.2f);
            Rule(ref s.aoe, mid, playerAdv, 1.0f);
            Rule(ref s.aoe, close, playerAdv, 0.8f);
        }

        // Melee rules
        Rule(ref s.melee, close, 0.9f);
        Rule(ref s.melee, close, enemyAdv, 1.0f);
        Rule(ref s.melee, close, even, 0.9f);
        Rule(ref s.melee, close, staLow, 0.7f);
        Rule(ref s.melee, close, staHigh, 0.6f);


        // Ranged rules
        Rule(ref s.ranged, far, 1.0f);
        Rule(ref s.ranged, mid,  0.7f);
        Rule(ref s.ranged, mid, enemyAdv, 0.8f);

        if (currentPhase && s.aoe >= aoePriority)
        {
            s.melee = 0f;
            s.ranged = 0f;
        }

        return s;
    }

    static void Rule(ref float score, float a, float b = 1f, float c = 1f, float weight = 1f)
    {
        float w = Mathf.Min(a, Mathf.Min(b, c)) * weight;
        score += w;
    }

    static float LShoulder(float x, float a, float b)
    {
        if (x <= a)
        {
            return 1f;
        }
        if (x >= b)
        {
            return 0f;
        }
        return (b - x) / (b - a);
    }

    static float RShoulder(float x, float a, float b)
    {
        if (x <= a)
        {
            return 0f;
        }
        if (x >= b)
        {
            return 1f;
        }
        return (x - a) / (b - a);
    }

    static float Tri(float x, float a, float b, float c)
    {
        if (x <= a || x >= c)
        {
            return 0f;
        }
        // 근사값 처리
        if (Mathf.Approximately(x, b))
        {
            return 1f;
        }
        return (x < b) ? (x - a) / (b - a) : (c - x) / (c - b);
    }
}
