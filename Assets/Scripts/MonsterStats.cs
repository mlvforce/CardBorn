using UnityEngine;
using TMPro;

public class MonsterStats : MonoBehaviour
{
   
    // MONSTER STATS
    // Every monster begins with 1 in each stat.
    // These 4 points are FREE and do not use the 100 point pool.
    

    [Header("Monster Stats")]
    [SerializeField] private int health = 1;
    [SerializeField] private int defence = 1;
    [SerializeField] private int attack = 1;
    [SerializeField] private int intelligence = 1;


    
    // POINT SETTINGS
    

    [Header("Point Settings")]

    // Player has 100 EXTRA points to spend.
    [SerializeField] private int startingPoints = 100;

    // No stat can ever go below 1.
    [SerializeField] private int minimumStat = 1;

    private int pointsRemaining;


   
    // UI TEXT

    [Header("Stat Number Text")]
    [SerializeField] private TMP_Text healthNumber;
    [SerializeField] private TMP_Text defenceNumber;
    [SerializeField] private TMP_Text attackNumber;
    [SerializeField] private TMP_Text intelligenceNumber;

    [Header("Points and Cost")]
    [SerializeField] private TMP_Text pointsNumber;
    [SerializeField] private TMP_Text costNumber;


   
   
   

    public int Health => health;
    public int Defence => defence;
    public int Attack => attack;
    public int Intelligence => intelligence;

    public int PointsRemaining => pointsRemaining;

    // Number of the player's 100 points that have been spent.
    public int PointsSpent => startingPoints - pointsRemaining;

    // Includes the four free starting points.
    public int TotalStats =>
        health +
        defence +
        attack +
        intelligence;

    public int SummonCost => CalculateSummonCost();


  
    // START
   

    private void Start()
    {
        // Reset stats to their minimum values.
        health = minimumStat;
        defence = minimumStat;
        attack = minimumStat;
        intelligence = minimumStat;

        // Give the player their full 100 point pool.
        pointsRemaining = startingPoints;

        UpdateUI();
    }


   
    // HEALTH


    public void IncreaseHealth()
    {
        if (CanSpendPoint())
        {
            health++;
            pointsRemaining--;

            UpdateUI();
        }
    }

    public void DecreaseHealth()
    {
        if (health > minimumStat)
        {
            health--;
            pointsRemaining++;

            UpdateUI();
        }
    }


    
    // DEFENCE
    

    public void IncreaseDefence()
    {
        if (CanSpendPoint())
        {
            defence++;
            pointsRemaining--;

            UpdateUI();
        }
    }

    public void DecreaseDefence()
    {
        if (defence > minimumStat)
        {
            defence--;
            pointsRemaining++;

            UpdateUI();
        }
    }


    
    // ATTACK
  

    public void IncreaseAttack()
    {
        if (CanSpendPoint())
        {
            attack++;
            pointsRemaining--;

            UpdateUI();
        }
    }

    public void DecreaseAttack()
    {
        if (attack > minimumStat)
        {
            attack--;
            pointsRemaining++;

            UpdateUI();
        }
    }


   
    // INTELLIGENCE
    

    public void IncreaseIntelligence()
    {
        if (CanSpendPoint())
        {
            intelligence++;
            pointsRemaining--;

            UpdateUI();
        }
    }

    public void DecreaseIntelligence()
    {
        if (intelligence > minimumStat)
        {
            intelligence--;
            pointsRemaining++;

            UpdateUI();
        }
    }


   
    // POINT CHECK
    

    private bool CanSpendPoint()
    {
        // Player can only increase a stat if they
        // still have points available.
        return pointsRemaining > 0;
    }


    
    // SUMMON COST
    

    private int CalculateSummonCost()
    {
       

        return Mathf.CeilToInt(PointsSpent / 5f);
    }


   
    // UPDATE UI


    private void UpdateUI()
    {
        // Update stat numbers.
        healthNumber.text = health.ToString();
        defenceNumber.text = defence.ToString();
        attackNumber.text = attack.ToString();
        intelligenceNumber.text = intelligence.ToString();

        // Update remaining allocation points.
        pointsNumber.text = pointsRemaining.ToString();

        // Update summon cost.
        costNumber.text = SummonCost + " SP";
    }
}