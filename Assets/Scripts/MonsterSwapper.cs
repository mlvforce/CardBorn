using UnityEngine;
using TMPro;

public class MonsterPartSwapper : MonoBehaviour
{
   
    // MONSTER PARTS
   

    [Header("Available Monster Parts")]
    [SerializeField] private GameObject[] heads;
    [SerializeField] private GameObject[] bodies;
    [SerializeField] private GameObject[] legs;


   
    // UI TEXT
    

    [Header("UI Text")]
    [SerializeField] private TMP_Text headNumberText;
    [SerializeField] private TMP_Text bodyNumberText;
    [SerializeField] private TMP_Text legNumberText;


    
    // CURRENT SELECTED PARTS
  

    private int currentHead = 0;
    private int currentBody = 0;
    private int currentLegs = 0;


    
    // START
   

    private void Start()
    {
        // Start with the first part in each list.
        currentHead = 0;
        currentBody = 0;
        currentLegs = 0;

        RefreshMonster();

        Debug.Log("Monster Part Swapper started.");
    }


    
    // HEAD BUTTONS
    

    public void NextHead()
    {
        if (heads == null || heads.Length == 0)
            return;

        currentHead++;

        // If we go past the final head,
        // loop back to the first one.
        if (currentHead >= heads.Length)
        {
            currentHead = 0;
        }

        RefreshHeads();
        UpdateUI();

        Debug.Log("Selected Head: " + (currentHead + 1));
    }


    public void PreviousHead()
    {
        if (heads == null || heads.Length == 0)
            return;

        currentHead--;

        // If we go below the first head,
        // loop around to the last one.
        if (currentHead < 0)
        {
            currentHead = heads.Length - 1;
        }

        RefreshHeads();
        UpdateUI();

        Debug.Log("Selected Head: " + (currentHead + 1));
    }


   
    // BODY BUTTONS
    

    public void NextBody()
    {
        if (bodies == null || bodies.Length == 0)
            return;

        currentBody++;

        if (currentBody >= bodies.Length)
        {
            currentBody = 0;
        }

        RefreshBodies();
        UpdateUI();

        Debug.Log("Selected Body: " + (currentBody + 1));
    }


    public void PreviousBody()
    {
        if (bodies == null || bodies.Length == 0)
            return;

        currentBody--;

        if (currentBody < 0)
        {
            currentBody = bodies.Length - 1;
        }

        RefreshBodies();
        UpdateUI();

        Debug.Log("Selected Body: " + (currentBody + 1));
    }


    
    // LEGS BUTTONS
    

    public void NextLegs()
    {
        if (legs == null || legs.Length == 0)
            return;

        currentLegs++;

        if (currentLegs >= legs.Length)
        {
            currentLegs = 0;
        }

        RefreshLegs();
        UpdateUI();

        Debug.Log("Selected Legs: " + (currentLegs + 1));
    }


    public void PreviousLegs()
    {
        if (legs == null || legs.Length == 0)
            return;

        currentLegs--;

        if (currentLegs < 0)
        {
            currentLegs = legs.Length - 1;
        }

        RefreshLegs();
        UpdateUI();

        Debug.Log("Selected Legs: " + (currentLegs + 1));
    }


    
    // RANDOMISE BUTTON
   

    public void RandomiseMonster()
    {
        // Pick a random head.
        if (heads != null && heads.Length > 0)
        {
            currentHead = Random.Range(0, heads.Length);
        }

        // Pick a random body.
        if (bodies != null && bodies.Length > 0)
        {
            currentBody = Random.Range(0, bodies.Length);
        }

        // Pick random legs.
        if (legs != null && legs.Length > 0)
        {
            currentLegs = Random.Range(0, legs.Length);
        }

        RefreshMonster();

        Debug.Log(
            "Monster Randomised! " +
            "Head: " + (currentHead + 1) +
            " | Body: " + (currentBody + 1) +
            " | Legs: " + (currentLegs + 1)
        );
    }


   
    // REFRESH WHOLE MONSTER
   

    private void RefreshMonster()
    {
        RefreshHeads();
        RefreshBodies();
        RefreshLegs();

        UpdateUI();
    }


   
    // REFRESH INDIVIDUAL PART TYPES

    private void RefreshHeads()
    {
        SetActivePart(heads, currentHead);
    }


    private void RefreshBodies()
    {
        SetActivePart(bodies, currentBody);
    }


    private void RefreshLegs()
    {
        SetActivePart(legs, currentLegs);
    }


    
    // ENABLE / DISABLE PARTS
  

    private void SetActivePart(GameObject[] parts, int selectedIndex)
    {
        if (parts == null || parts.Length == 0)
            return;

        for (int i = 0; i < parts.Length; i++)
        {
            // Skip any empty Inspector slots.
            if (parts[i] == null)
                continue;

            // Only the currently selected part should be enabled.
            if (i == selectedIndex)
            {
                parts[i].SetActive(true);
            }
            else
            {
                parts[i].SetActive(false);
            }
        }
    }


    
    // UPDATE NUMBER DISPLAY
   

    private void UpdateUI()
    {
        // HEAD
        if (headNumberText != null)
        {
            if (heads != null && heads.Length > 0)
            {
                headNumberText.text = (currentHead + 1).ToString();
            }
            else
            {
                headNumberText.text = "0";
            }
        }


        // BODY
        if (bodyNumberText != null)
        {
            if (bodies != null && bodies.Length > 0)
            {
                bodyNumberText.text = (currentBody + 1).ToString();
            }
            else
            {
                bodyNumberText.text = "0";
            }
        }


        // LEGS
        if (legNumberText != null)
        {
            if (legs != null && legs.Length > 0)
            {
                legNumberText.text = (currentLegs + 1).ToString();
            }
            else
            {
                legNumberText.text = "0";
            }
        }
    }
}