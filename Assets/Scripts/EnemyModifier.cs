using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyModifier : MonoBehaviour
{
    [HideInInspector]
    public EnemyChaseLogic ECL;
    [HideInInspector]
    public EnemyShootLogic ESL;
    [HideInInspector]
    public EnemyStats ES;

    private List<Modifier> Modifiers = new List<Modifier>();

    public int EnemyDifficulty = 0;

    // Start is called before the first frame update
    void Awake()
    {
        //ger references to external components holding each stat
        ECL = GetComponent<EnemyChaseLogic>();
        ESL = GetComponent<EnemyShootLogic>();
        ES = GetComponent<EnemyStats>();

        //create modifiers
        HealthModifier HM = new HealthModifier();
        ProjectileCountModifier PCM = new ProjectileCountModifier();
        
        Modifiers.Add(HM);
        Modifiers.Add(PCM);

        for (int i = 0; i < EnemyDifficulty; ++i)
        {
            int mod = Random.Range(0, Modifiers.Count);
            ++Modifiers[mod].Tier;
        }

        //apply all modifiers in the list
        foreach(Modifier mod in Modifiers)
            mod.ApplyModifier(this);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

public class Modifier
{
    public int Tier = 0;

    public virtual void ApplyModifier(EnemyModifier script)
    {

    }
}

public class HealthModifier : Modifier
{
    public List<int> HealthTiers = new List<int>();

    public HealthModifier()
    {
        HealthTiers.Add(1);
        HealthTiers.Add(2);
        HealthTiers.Add(3);
        HealthTiers.Add(5);
        HealthTiers.Add(8);
        HealthTiers.Add(15);
        HealthTiers.Add(20);
    }

    public override void ApplyModifier(EnemyModifier script)
    {
        if (Tier > HealthTiers.Count) return;

        script.ES.StartingHealth = HealthTiers[Tier];
    }
}


public class ProjectileCountModifier : Modifier
{
    public List<int> PCTiers = new List<int>();

    public ProjectileCountModifier()
    {
        PCTiers.Add(1);
        PCTiers.Add(2);
        PCTiers.Add(3);
        PCTiers.Add(5);
        PCTiers.Add(8);
        PCTiers.Add(12);
    }

    public override void ApplyModifier(EnemyModifier script)
    {
        if (Tier > PCTiers.Count) return;

        script.ESL.BulletsPerShot = PCTiers[Tier];
    }
}