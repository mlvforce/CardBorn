using UnityEngine;
using TMPro;

public class EffectPicker : MonoBehaviour
{
    // MONSTER STATS

    [Header("Monster Stats")]
    [SerializeField] private MonsterStats monsterStats;


    // EFFECT LIST

    [Header("Effect List")]

    [SerializeField] private GameObject effectsHPPanel;

    [SerializeField] private GameObject battleHardenedButton;


    // BATTLE HARDENED INFORMATION

    [Header("Battle Hardened Info")]

    [SerializeField] private GameObject battleHardenedInfoPanel;

    [SerializeField] private TMP_Text battleHardenedInfoText;

    [SerializeField] private GameObject addButton;


    // EFFECT SLOT TEXT

    [Header("Selected Effect Text")]

    [SerializeField] private TMP_Text effect1Text;
    [SerializeField] private TMP_Text effect2Text;


    // CURRENT EFFECT PICKER STATE
   
    private int activeSlot = 0;


   
    private string previewedEffect = "";


  
    private string selectedEffect1 = "";
    private string selectedEffect2 = "";


    // START

    private void Start()
    {
        
        if (effect1Text != null)
            effect1Text.text = "";

        if (effect2Text != null)
            effect2Text.text = "";


        
        CloseEffectInformation();


        
        if (effectsHPPanel != null)
            effectsHPPanel.SetActive(false);
    }


    // OPEN EFFECT SLOT 1

    public void OpenEffectSlot1()
    {
        activeSlot = 1;

        OpenAvailableEffects();
    }


    // OPEN EFFECT SLOT 2

    public void OpenEffectSlot2()
    {
        activeSlot = 2;

        OpenAvailableEffects();
    }


    // OPEN AVAILABLE EFFECTS

    private void OpenAvailableEffects()
    {
        CloseEffectInformation();


        if (effectsHPPanel != null)
            effectsHPPanel.SetActive(true);


        RefreshAvailableEffects();
    }


    // CHECK STAT REQUIREMENTS

    public void RefreshAvailableEffects()
    {
        if (monsterStats == null)
        {
            Debug.LogWarning(
                "EffectPicker: MonsterStats has not been assigned."
            );

            return;
        }


        
        bool hasEnoughHealth =
            monsterStats.Health >= 50;


        
        bool alreadySelected =
            selectedEffect1 == "Battle Hardened" ||
            selectedEffect2 == "Battle Hardened";


        if (battleHardenedButton != null)
        {
            battleHardenedButton.SetActive(
                hasEnoughHealth && !alreadySelected
            );
        }
    }


    // PREVIEW BATTLE HARDENED

    public void PreviewBattleHardened()
    {
        if (monsterStats == null)
            return;


       
        if (monsterStats.Health < 50)
        {
            Debug.Log(
                "Battle Hardened requires at least 50 HP."
            );

            return;
        }


        previewedEffect = "Battle Hardened";


        
        if (effectsHPPanel != null)
            effectsHPPanel.SetActive(false);


        
        if (battleHardenedInfoPanel != null)
            battleHardenedInfoPanel.SetActive(true);


        if (battleHardenedInfoText != null)
        {
            battleHardenedInfoText.text =
                "BATTLE HARDENED\n\n" +
                "Requirement: 50 HP\n\n" +
                "Once per battle, this monster can resist an attack " +
                "that would otherwise defeat it.\n\n" +
                "Instead of being defeated, the monster remains " +
                "on the field with 1 HP.\n\n" +
                "This effect can activate once per card.";
        }


        if (addButton != null)
            addButton.SetActive(true);
    }


    // ADD PREVIEWED EFFECT

    public void AddPreviewedEffect()
    {
        
        if (activeSlot == 0)
        {
            Debug.Log(
                "Select Effect Slot 1 or Effect Slot 2 first."
            );

            return;
        }


        if (string.IsNullOrEmpty(previewedEffect))
            return;


        
        if (selectedEffect1 == previewedEffect ||
            selectedEffect2 == previewedEffect)
        {
            Debug.Log(
                previewedEffect +
                " has already been added to this monster."
            );

            return;
        }


        // ADD TO EFFECT SLOT 1

        if (activeSlot == 1)
        {
            selectedEffect1 = previewedEffect;

            if (effect1Text != null)
            {
                effect1Text.text = selectedEffect1;
            }
        }


        // ADD TO EFFECT SLOT 2

        else if (activeSlot == 2)
        {
            selectedEffect2 = previewedEffect;

            if (effect2Text != null)
            {
                effect2Text.text = selectedEffect2;
            }
        }


        Debug.Log(
            previewedEffect +
            " added to Effect Slot " +
            activeSlot
        );


      
        activeSlot = 0;

       
        CloseEffectInformation();

       
        RefreshAvailableEffects();
    }


    // REMOVE EFFECT SLOT 1
    

    public void RemoveEffect1()
    {
        selectedEffect1 = "";

        if (effect1Text != null)
            effect1Text.text = "";


        CloseEffectInformation();

        RefreshAvailableEffects();
    }


    // REMOVE EFFECT SLOT 2

    public void RemoveEffect2()
    {
        selectedEffect2 = "";

        if (effect2Text != null)
            effect2Text.text = "";


        CloseEffectInformation();

        RefreshAvailableEffects();
    }


    // BACK FROM EFFECT INFORMATION

    public void BackToEffectList()
    {
        CloseEffectInformation();


       
        if (activeSlot != 0)
        {
            if (effectsHPPanel != null)
                effectsHPPanel.SetActive(true);

            RefreshAvailableEffects();
        }
    }


    // CLOSE EFFECT INFO

    private void CloseEffectInformation()
    {
        previewedEffect = "";


        if (battleHardenedInfoPanel != null)
            battleHardenedInfoPanel.SetActive(false);


        if (addButton != null)
            addButton.SetActive(false);
    }


 
    // VALUES FOR SAVING LATER
   

    public string GetEffect1()
    {
        return selectedEffect1;
    }


    public string GetEffect2()
    {
        return selectedEffect2;
    }
}