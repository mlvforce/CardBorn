using UnityEngine;
using TMPro;

public class MonsterSwapper : MonoBehaviour
{
    // DATA CLASSES

    [System.Serializable]
    public class HeadPart
    {
        public GameObject head;

        public Transform neckAttachPoint;
    }

    [System.Serializable]
    public class BodyPart
    {
        public GameObject body;

        public Transform headSocket;

        public Transform legsSocket;
    }

    [System.Serializable]
    public class LegsPart
    {
        public GameObject legs;

        public Transform hipAttachPoint;
    }


    // MONSTER PARTS

    [Header("Available Monster Parts")]

    [SerializeField] private HeadPart[] heads;
    [SerializeField] private BodyPart[] bodies;
    [SerializeField] private LegsPart[] legs;


    // UI TEXT

    [Header("UI Text")]

    [SerializeField] private TMP_Text headNumberText;
    [SerializeField] private TMP_Text bodyNumberText;
    [SerializeField] private TMP_Text legNumberText;


    // CURRENT SELECTION
    

    private int currentHead = 0;
    private int currentBody = 0;
    private int currentLegs = 0;


    // START

    private void Start()
    {
        UpdateVisibleParts();

        SnapMonsterTogether();

        
        UpdateUI();
    }


    // HEAD BUTTONS

    public void NextHead()
    {
        if (heads == null || heads.Length == 0)
            return;

        currentHead++;

        if (currentHead >= heads.Length)
            currentHead = 0;

        UpdateVisibleParts();
        SnapMonsterTogether();
        UpdateUI();
    }


    public void PreviousHead()
    {
        if (heads == null || heads.Length == 0)
            return;

        currentHead--;

        if (currentHead < 0)
            currentHead = heads.Length - 1;

        UpdateVisibleParts();
        SnapMonsterTogether();
        UpdateUI();
    }


    // BODY BUTTONS

    public void NextBody()
    {
        if (bodies == null || bodies.Length == 0)
            return;

        currentBody++;

        if (currentBody >= bodies.Length)
            currentBody = 0;

        UpdateVisibleParts();

        
        SnapMonsterTogether();

        UpdateUI();
    }


    public void PreviousBody()
    {
        if (bodies == null || bodies.Length == 0)
            return;

        currentBody--;

        if (currentBody < 0)
            currentBody = bodies.Length - 1;

        UpdateVisibleParts();
        SnapMonsterTogether();
        UpdateUI();
    }


    // LEGS BUTTONS

    public void NextLegs()
    {
        if (legs == null || legs.Length == 0)
            return;

        currentLegs++;

        if (currentLegs >= legs.Length)
            currentLegs = 0;

        UpdateVisibleParts();
        SnapMonsterTogether();
        UpdateUI();
    }


    public void PreviousLegs()
    {
        if (legs == null || legs.Length == 0)
            return;

        currentLegs--;

        if (currentLegs < 0)
            currentLegs = legs.Length - 1;

        UpdateVisibleParts();
        SnapMonsterTogether();
        UpdateUI();
    }


    // RANDOMISE

    public void RandomiseMonster()
    {
        if (heads != null && heads.Length > 0)
            currentHead = Random.Range(0, heads.Length);

        if (bodies != null && bodies.Length > 0)
            currentBody = Random.Range(0, bodies.Length);

        if (legs != null && legs.Length > 0)
            currentLegs = Random.Range(0, legs.Length);

        UpdateVisibleParts();
        SnapMonsterTogether();
        UpdateUI();
    }


    // ENABLE / DISABLE PARTS

    private void UpdateVisibleParts()
    {
        // HEADS
        for (int i = 0; i < heads.Length; i++)
        {
            if (heads[i].head != null)
            {
                heads[i].head.SetActive(i == currentHead);
            }
        }


        // BODIES
        for (int i = 0; i < bodies.Length; i++)
        {
            if (bodies[i].body != null)
            {
                bodies[i].body.SetActive(i == currentBody);
            }
        }


        // LEGS
        for (int i = 0; i < legs.Length; i++)
        {
            if (legs[i].legs != null)
            {
                legs[i].legs.SetActive(i == currentLegs);
            }
        }
    }


    // SNAP COMPLETE MONSTER TOGETHER

    private void SnapMonsterTogether()
    {
        if (heads.Length == 0 ||
            bodies.Length == 0 ||
            legs.Length == 0)
        {
            return;
        }

        BodyPart selectedBody = bodies[currentBody];
        HeadPart selectedHead = heads[currentHead];
        LegsPart selectedLegs = legs[currentLegs];


        // SNAP HEAD

        if (selectedHead.head != null &&
            selectedHead.neckAttachPoint != null &&
            selectedBody.headSocket != null)
        {
            SnapPart(
                selectedHead.head.transform,
                selectedHead.neckAttachPoint,
                selectedBody.headSocket
            );
        }
        else
        {
            Debug.LogWarning(
                "Cannot snap head. Check Head, Neck Attach Point and Head Socket."
            );
        }


        // SNAP LEGS

        if (selectedLegs.legs != null &&
            selectedLegs.hipAttachPoint != null &&
            selectedBody.legsSocket != null)
        {
            SnapPart(
                selectedLegs.legs.transform,
                selectedLegs.hipAttachPoint,
                selectedBody.legsSocket
            );
        }
        else
        {
            Debug.LogWarning(
                "Cannot snap legs. Check Legs, Hip Attach Point and Legs Socket."
            );
        }
    }


    // SNAP INDIVIDUAL PART

    private void SnapPart(
        Transform partRoot,
        Transform attachPoint,
        Transform targetSocket)
    {
        if (partRoot == null ||
            attachPoint == null ||
            targetSocket == null)
        {
            return;
        }


        
        

        Quaternion rotationDifference =
            targetSocket.rotation *
            Quaternion.Inverse(attachPoint.rotation);

        partRoot.rotation =
            rotationDifference *
            partRoot.rotation;



        Vector3 positionDifference =
            targetSocket.position -
            attachPoint.position;


       
        

        partRoot.position += positionDifference;
    }


     
    // UPDATE UI
     
    private void UpdateUI()
    {
        if (headNumberText != null)
        {
            headNumberText.text =
                (currentHead + 1).ToString();
        }

        if (bodyNumberText != null)
        {
            bodyNumberText.text =
                (currentBody + 1).ToString();
        }

        if (legNumberText != null)
        {
            legNumberText.text =
                (currentLegs + 1).ToString();
        }
    }


    
    // GET CURRENT SELECTIONS
    

    public int GetSelectedHead()
    {
        return currentHead;
    }

    public int GetSelectedBody()
    {
        return currentBody;
    }

    public int GetSelectedLegs()
    {
        return currentLegs;
    }
}