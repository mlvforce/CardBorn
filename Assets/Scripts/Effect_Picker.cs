using UnityEngine;
using TMPro;

public class EffectPicker : MonoBehaviour
{
    // MONSTER STATS

    [Header("Monster Stats")]
    [SerializeField] private MonsterStats monsterStats;


    // EFFECT CATEGORY PANELS

    [Header("Effect Category Panels")]

    [SerializeField] private GameObject effectsHP;
    [SerializeField] private GameObject effectsDEF;
    [SerializeField] private GameObject effectsINT;
    [SerializeField] private GameObject effectsATT;


    // BATTLE HARDENED

    [Header("Battle Hardened")]

    [SerializeField] private GameObject battleHardenedButton;

    [SerializeField] private GameObject battleHardenedInfo;

    [SerializeField] private TMP_Text battleHardenedInfoText;


    // EFFECT SLOT TEXT

    [Header("Effect Slot Text")]

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


        HideEffectLists();


        HideAllEffectInfo();
    }


    // EFFECT SLOT 1

    public void OpenEffectSlot1()
    {
        activeSlot = 1;

        OpenEffectList();
    }


    // EFFECT SLOT 2

    public void OpenEffectSlot2()
    {
        activeSlot = 2;

        OpenEffectList();
    }


    // OPEN EFFECT LIST

    private void OpenEffectList()
    {
        previewedEffect = "";

        HideAllEffectInfo();

        ShowEffectLists();

        RefreshAvailableEffects();
    }


    // SHOW EFFECT LISTS

    private void ShowEffectLists()
    {
        if (effectsHP != null)
            effectsHP.SetActive(true);

        if (effectsDEF != null)
            effectsDEF.SetActive(true);

        if (effectsINT != null)
            effectsINT.SetActive(true);

        if (effectsATT != null)
            effectsATT.SetActive(true);
    }


    // HIDE EFFECT LISTS

    private void HideEffectLists()
    {
        if (effectsHP != null)
            effectsHP.SetActive(false);

        if (effectsDEF != null)
            effectsDEF.SetActive(false);

        if (effectsINT != null)
            effectsINT.SetActive(false);

        if (effectsATT != null)
            effectsATT.SetActive(false);
    }


    // CHECK AVAILABLE EFFECTS

    public void RefreshAvailableEffects()
    {
        if (monsterStats == null)
        {
            Debug.LogWarning(
                "EffectPicker: MonsterStats has not been assigned."
            );

            return;
        }


        // BATTLE HARDENED
        
        // Requires 50 HP.

        bool battleHardenedUnlocked =
            monsterStats.Health >= 50;


        bool battleHardenedAlreadySelected =
            selectedEffect1 == "Battle Hardened" ||
            selectedEffect2 == "Battle Hardened";


        if (battleHardenedButton != null)
        {
            battleHardenedButton.SetActive(
                battleHardenedUnlocked &&
                !battleHardenedAlreadySelected
            );
        }


    }


    // PREVIEW BATTLE HARDENED

    public void PreviewBattleHardened()
    {
        Debug.Log("Preview Battle Hardened clicked.");


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


        HideEffectLists();


        HideAllEffectInfo();


        if (battleHardenedInfo != null)
        {
            battleHardenedInfo.SetActive(true);
        }


        
        if (battleHardenedInfoText != null)
        {
            battleHardenedInfoText.text =
                "BATTLE HARDENED\n\n" +

                 "Once Per Game This Monster\n"+
                 
                 "Can Survive a Fatal Attack on 1 HP";
        }
    }


    // ADD CURRENT EFFECT

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
        {
            Debug.Log(
                "No effect is currently being previewed."
            );

            return;
        }


       
        if (selectedEffect1 == previewedEffect ||
            selectedEffect2 == previewedEffect)
        {
            Debug.Log(
                previewedEffect +
                " has already been equipped."
            );

            return;
        }


        // EFFECT SLOT 1

        if (activeSlot == 1)
        {
            selectedEffect1 = previewedEffect;

            if (effect1Text != null)
            {
                effect1Text.text = selectedEffect1;
            }
        }


        // EFFECT SLOT 2

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

        previewedEffect = "";


        HideAllEffectInfo();
        HideEffectLists();


        
        RefreshAvailableEffects();
    }


    // REMOVE EFFECT 1

    public void RemoveEffect1()
    {
        selectedEffect1 = "";

        if (effect1Text != null)
        {
            effect1Text.text = "";
        }


        previewedEffect = "";

        HideAllEffectInfo();

        RefreshAvailableEffects();
    }


    // REMOVE EFFECT 2

    public void RemoveEffect2()
    {
        selectedEffect2 = "";

        if (effect2Text != null)
        {
            effect2Text.text = "";
        }


        previewedEffect = "";

        HideAllEffectInfo();

        RefreshAvailableEffects();
    }


    // BACK FROM EFFECT INFO

    public void BackFromEffectInfo()
    {
        previewedEffect = "";

        HideAllEffectInfo();


        if (activeSlot != 0)
        {
            ShowEffectLists();
            RefreshAvailableEffects();
        }
    }


    // HIDE ALL EFFECT INFO

    private void HideAllEffectInfo()
    {
        if (battleHardenedInfo != null)
        {
            battleHardenedInfo.SetActive(false);
        }


        
    }


    // SAVING ACCESS

    public string GetEffect1()
    {
        return selectedEffect1;
    }


    public string GetEffect2()
    {
        return selectedEffect2;
    }
}