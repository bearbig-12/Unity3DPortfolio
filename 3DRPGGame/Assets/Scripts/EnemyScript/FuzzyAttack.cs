
using UnityEngine;

public static class FuzzyAttack
{
    public static float AttackProb(float relatedHP, float playerStamina)
    {
        relatedHP = Mathf.Clamp(relatedHP, -100f, 100f);
        playerStamina = Mathf.Clamp(playerStamina, 0f, 100f);

        // HP memberships
        float enemyAdv = LShoulder(relatedHP, -80, -20);
        float even = Tri(relatedHP, -40, 0f, 40);
        float playerAdv = RShoulder(relatedHP, 20, 80);

        // Stamina memberships 
        float staLow = LShoulder(playerStamina, 0f, 45f);
        float staMid = Tri(playerStamina, 20f, 50f, 80f);
        float staHigh = RShoulder(playerStamina, 65f, 100f);

        // 강공격 점수
        float hardScore = 0f;
        // 기본 공격 점수
        float basicScore = 0f;

        void RuleHard(float hpM, float staM)
        {
            float w = Mathf.Min(hpM, staM);
            hardScore += w;
        }

        void RuleBasic(float hpM, float staM)
        {
            float w = Mathf.Min(hpM, staM);
            basicScore += w;
        }

        //// enemy의 우세
        //RuleHard(enemyAdv, staLow);
        //RuleHard(enemyAdv, staMid);
        //RuleHard(enemyAdv, staHigh);

        //// HP 균등 상태
        //RuleHard(even, staLow);
        //RuleBasic(even, staMid);
        //RuleBasic(even, staHigh);

        //// player의 우세
        //RuleHard(playerAdv, staLow);
        //RuleHard(playerAdv, staMid);
        //RuleBasic(playerAdv, staHigh);


        // enemy의 우세 -> 기본 공격
        RuleBasic(enemyAdv, staLow);
        RuleBasic(enemyAdv, staMid);
        RuleBasic(enemyAdv, staHigh);

        // HP 균등 상태 (약화)
        RuleBasic(even, staLow);
        RuleBasic(even, staMid);
        RuleHard(even, staHigh);

        // player의 우세 -> 강공격
        RuleHard(playerAdv, staLow);
        RuleHard(playerAdv, staMid);
        RuleHard(playerAdv, staHigh);

        float total = hardScore + basicScore;

        // 혹은 값이 없을 경우 기본값
        if (total <= 0.0001f)
        {
            return 0.5f;

        }
       
        return Mathf.Clamp01(hardScore / total);
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
        // 꼭짓값 처리
        if (Mathf.Approximately(x, b))
        {
            return 1f;
        }
        return (x < b) ? (x - a) / (b - a) : (c - x) / (c - b);
    }

}
