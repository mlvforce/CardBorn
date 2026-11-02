
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EffectPicker : MonoBehaviour
{
    // =====================================================
    // EFFECT DEFINITION
    // =====================================================

    [Serializable]
    public class EffectEntry
    {
        public string effectName;

        [Header("Stat Requirements")]
        public int requiredHP;
        public int requiredDEF;
        public int requiredATK;
        public int requiredINT;

        [Header("UI References")]
        public Button selectionButton;
        public GameObject infoPanel;

        // Manually connected in the Unity Inspector.
        public Button addButton;
        public Button backButton;
    }

    // =====================================================
    // INSPECTOR REFERENCES
    // =====================================================

    [Header("Monster Stats")]
    [SerializeField] private MonsterStats monsterStats;

    [Header("Available Effects")]
    [SerializeField] private GameObject availableEffectsList;

    [Header("Effect Information")]
    [SerializeField] private GameObject effectInfoContainer;

    [Header("Effect Slot Buttons")]
    [SerializeField] private Button slot1Plus;
    [SerializeField] private Button slot1Minus;
    [SerializeField] private Button slot2Plus;
    [SerializeField] private Button slot2Minus;

    [Header("Effect Slot Text")]
    [SerializeField] private TMP_Text effect1Text;
    [SerializeField] private TMP_Text effect2Text;

    [Header("Effect Definitions")]
    [SerializeField] private EffectEntry[] effects;

    // =====================================================
    // LAYOUT SETTINGS
    // =====================================================

    [Header("Automatic Effect Spacing")]
    [SerializeField] private float buttonSpacing = 10f;
    [SerializeField] private float buttonHeight = 35f;

    // =====================================================
    // CURRENT SELECTION
    // =====================================================

    // 0 = Nothing selected
    // 1 = Effect slot 1
    // 2 = Effect slot 2

    private int activeSlot = 0;
    private int previewedEffectIndex = -1;

    private string selectedEffect1 = "";
    private string selectedEffect2 = "";

    private bool listOpen = false;

    private readonly HashSet<Button> registeredButtons =
        new HashSet<Button>();

    private VerticalLayoutGroup verticalLayout;
    private RectTransform listRect;

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        // Automatically connect effect slot buttons.

        RegisterButton(
            slot1Plus,
            OpenEffectSlot1,
            "Slot 1 Plus"
        );

        RegisterButton(
            slot1Minus,
            RemoveEffect1,
            "Slot 1 Minus"
        );

        RegisterButton(
            slot2Plus,
            OpenEffectSlot2,
            "Slot 2 Plus"
        );

        RegisterButton(
            slot2Minus,
            RemoveEffect2,
            "Slot 2 Minus"
        );

        // Automatically connect effect selection buttons.

        if (effects != null)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                int index = i;
                EffectEntry effect = effects[i];

                if (effect == null)
                    continue;

                RegisterButton(
                    effect.selectionButton,
                    () => PreviewEffect(index),
                    effect.effectName + " Selection"
                );
            }
        }

        // Add and Back buttons are NOT registered here.
        // Keep their manual On Click connections.

        SetupAutomaticLayout();
    }

    // =====================================================
    // REGISTER BUTTON
    // =====================================================

    private void RegisterButton(
        Button button,
        UnityEngine.Events.UnityAction action,
        string label)
    {
        if (button == null)
        {
            Debug.LogWarning(
                "Missing button reference: " + label
            );
            return;
        }

        if (!registeredButtons.Add(button))
        {
            Debug.LogWarning(
                "Duplicate button reference: " + button.name
            );
            return;
        }

        button.onClick.AddListener(() =>
        {
            Debug.Log("BUTTON CLICKED: " + label);
            action();
        });
    }

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        UpdateSlotText();
        CloseSelection();
    }

    // =====================================================
    // AUTOMATIC EFFECT LAYOUT
    // =====================================================

    private void SetupAutomaticLayout()
    {
        if (availableEffectsList == null)
        {
            Debug.LogError(
                "AvailableEffectsList is not assigned!"
            );
            return;
        }

        listRect =
            availableEffectsList.GetComponent<RectTransform>();

        if (listRect == null)
        {
            Debug.LogError(
                "AvailableEffectsList needs a RectTransform!"
            );
            return;
        }

        verticalLayout =
            availableEffectsList.GetComponent<VerticalLayoutGroup>();

        if (verticalLayout == null)
        {
            verticalLayout =
                availableEffectsList
                .AddComponent<VerticalLayoutGroup>();
        }

        verticalLayout.spacing = buttonSpacing;
        verticalLayout.childAlignment = TextAnchor.UpperLeft;

        // Preserve the widths you designed in Unity.
        verticalLayout.childControlWidth = false;

        // Arrange each row using its preferred height.
        verticalLayout.childControlHeight = true;

        verticalLayout.childForceExpandWidth = false;
        verticalLayout.childForceExpandHeight = false;

        verticalLayout.padding =
            new RectOffset(8, 8, 8, 8);

        // Give each effect button a consistent height.
        if (effects != null)
        {
            foreach (EffectEntry effect in effects)
            {
                if (effect == null ||
                    effect.selectionButton == null)
                    continue;

                GameObject buttonObject =
                    effect.selectionButton.gameObject;

                // Buttons must be direct children of the list
                // for this layout to arrange them.
                if (buttonObject.transform.parent !=
                    availableEffectsList.transform)
                {
                    Debug.LogWarning(
                        buttonObject.name +
                        " should be a direct child of " +
                        "AvailableEffectsList."
                    );
                }

                LayoutElement element =
                    buttonObject.GetComponent<LayoutElement>();

                if (element == null)
                {
                    element =
                        buttonObject.AddComponent<LayoutElement>();
                }

                element.preferredHeight = buttonHeight;
                element.flexibleHeight = 0f;
            }
        }

        Debug.Log("Automatic effect layout configured.");
    }

    // =====================================================
    // REBUILD EFFECT LIST LAYOUT
    // =====================================================

    private void RebuildEffectLayout()
    {
        if (listRect == null)
            return;

        LayoutRebuilder.MarkLayoutForRebuild(listRect);

        if (availableEffectsList.activeInHierarchy)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                listRect
            );
        }
    }

    // =====================================================
    // OPEN EFFECT SLOT 1
    // =====================================================

    public void OpenEffectSlot1()
    {
        activeSlot = 1;
        OpenEffectList();
    }

    // =====================================================
    // OPEN EFFECT SLOT 2
    // =====================================================

    public void OpenEffectSlot2()
    {
        activeSlot = 2;
        OpenEffectList();
    }

    // =====================================================
    // OPEN EFFECT LIST
    // =====================================================

    private void OpenEffectList()
    {
        previewedEffectIndex = -1;
        listOpen = true;

        HideAllInfo();
        RefreshAvailableEffects();

        Debug.Log(
            "Opening effect list for slot " + activeSlot
        );
    }

    // =====================================================
    // CHECK STAT REQUIREMENTS
    // =====================================================

    private bool MeetsRequirements(EffectEntry effect)
    {
        if (monsterStats == null || effect == null)
            return false;

        return
            monsterStats.Health >= effect.requiredHP &&
            monsterStats.Defence >= effect.requiredDEF &&
            monsterStats.Attack >= effect.requiredATK &&
            monsterStats.Intelligence >= effect.requiredINT;
    }

    // =====================================================
    // CHECK IF EFFECT IS ALREADY EQUIPPED
    // =====================================================

    private bool IsAlreadyEquipped(string effectName)
    {
        return selectedEffect1 == effectName ||
               selectedEffect2 == effectName;
    }

    // =====================================================
    // CHECK EQUIPPED EFFECT VALIDITY
    // =====================================================

    private bool IsValidEquippedEffect(string effectName)
    {
        if (string.IsNullOrEmpty(effectName))
            return true;

        if (effects == null)
            return false;

        foreach (EffectEntry effect in effects)
        {
            if (effect != null &&
                effect.effectName == effectName)
            {
                return MeetsRequirements(effect);
            }
        }

        return false;
    }

    // =====================================================
    // REFRESH AVAILABLE EFFECTS
    // =====================================================

    public void RefreshAvailableEffects()
    {
        if (monsterStats == null || effects == null)
        {
            Debug.LogWarning(
                "EffectPicker is missing stats or effects."
            );
            return;
        }

        // Remove effects if stats no longer qualify.

        if (!IsValidEquippedEffect(selectedEffect1))
            selectedEffect1 = "";

        if (!IsValidEquippedEffect(selectedEffect2))
            selectedEffect2 = "";

        UpdateSlotText();

        // Enable the list before arranging its children.

        if (availableEffectsList != null)
        {
            availableEffectsList.SetActive(listOpen);
        }

        int availableCount = 0;

        // Enable ALL eligible effects, not just one.

        foreach (EffectEntry effect in effects)
        {
            if (effect == null ||
                effect.selectionButton == null)
                continue;

            bool unlocked = MeetsRequirements(effect);

            bool alreadyEquipped =
                IsAlreadyEquipped(effect.effectName);

            bool shouldShow =
                listOpen &&
                unlocked &&
                !alreadyEquipped;

            effect.selectionButton.gameObject.SetActive(
                shouldShow
            );

            if (shouldShow)
                availableCount++;
        }

        // Reposition visible buttons immediately.
        RebuildEffectLayout();

        Debug.Log(
            "Available effects: " + availableCount
        );
    }

    // =====================================================
    // PREVIEW EFFECT
    // =====================================================

    public void PreviewEffect(int index)
    {
        if (effects == null ||
            index < 0 ||
            index >= effects.Length)
        {
            return;
        }

        EffectEntry effect = effects[index];

        if (effect == null)
            return;

        if (activeSlot == 0)
        {
            Debug.LogWarning(
                "Choose an effect slot first."
            );
            return;
        }

        if (!MeetsRequirements(effect))
        {
            Debug.LogWarning(
                effect.effectName + " is locked."
            );
            return;
        }

        if (IsAlreadyEquipped(effect.effectName))
        {
            Debug.LogWarning(
                effect.effectName + " is already equipped."
            );
            return;
        }

        if (effect.infoPanel == null ||
            effectInfoContainer == null)
        {
            Debug.LogError(
                "Info panel missing for " + effect.effectName
            );
            return;
        }

        previewedEffectIndex = index;
        listOpen = false;

        RefreshAvailableEffects();
        HideAllInfo();

        effectInfoContainer.SetActive(true);

        // Activate any disabled parent groups.
        Transform parent = effect.infoPanel.transform.parent;

        while (parent != null &&
               parent != effectInfoContainer.transform)
        {
            parent.gameObject.SetActive(true);
            parent = parent.parent;
        }

        effect.infoPanel.SetActive(true);

        Debug.Log("Previewing " + effect.effectName);
    }

    // =====================================================
    // ADD PREVIEWED EFFECT
    // MANUAL UNITY ON CLICK
    // =====================================================

    public void AddPreviewedEffect()
    {
        Debug.Log(
            "ADD CLICKED - Slot: " + activeSlot +
            " Index: " + previewedEffectIndex
        );

        if (activeSlot != 1 && activeSlot != 2)
            return;

        if (effects == null ||
            previewedEffectIndex < 0 ||
            previewedEffectIndex >= effects.Length)
        {
            Debug.LogWarning("No effect previewed.");
            return;
        }

        EffectEntry effect = effects[previewedEffectIndex];

        if (effect == null ||
            !MeetsRequirements(effect))
        {
            Debug.LogWarning("Effect is not unlocked.");
            return;
        }

        // Prevent duplicates in the opposite slot.

        string otherEffect =
            activeSlot == 1 ? selectedEffect2 : selectedEffect1;

        if (otherEffect == effect.effectName)
        {
            Debug.LogWarning(
                "Effect already equipped in other slot."
            );
            return;
        }

        // Equip effect.

        if (activeSlot == 1)
        {
            selectedEffect1 = effect.effectName;
        }
        else
        {
            selectedEffect2 = effect.effectName;
        }

        Debug.Log(
            "Equipped " + effect.effectName +
            " in slot " + activeSlot
        );

        UpdateSlotText();
        CloseSelection();
    }

    // =====================================================
    // BACK FROM EFFECT INFO
    // MANUAL UNITY ON CLICK
    // =====================================================

    public void BackFromEffectInfo()
    {
        Debug.Log("INFO BACK CLICKED");

        previewedEffectIndex = -1;

        HideAllInfo();

        if (activeSlot != 0)
        {
            listOpen = true;
            RefreshAvailableEffects();
        }
        else
        {
            CloseSelection();
        }
    }

    // =====================================================
    // REMOVE EFFECT 1
    // =====================================================

    public void RemoveEffect1()
    {
        selectedEffect1 = "";

        UpdateSlotText();
        CloseSelection();

        Debug.Log("Effect 1 removed.");
    }

    // =====================================================
    // REMOVE EFFECT 2
    // =====================================================

    public void RemoveEffect2()
    {
        selectedEffect2 = "";

        UpdateSlotText();
        CloseSelection();

        Debug.Log("Effect 2 removed.");
    }

    // =====================================================
    // HIDE INFORMATION PANELS
    // =====================================================

    private void HideAllInfo()
    {
        if (effects != null)
        {
            foreach (EffectEntry effect in effects)
            {
                if (effect != null &&
                    effect.infoPanel != null)
                {
                    effect.infoPanel.SetActive(false);
                }
            }
        }

        if (effectInfoContainer != null)
        {
            effectInfoContainer.SetActive(false);
        }
    }

    // =====================================================
    // CLOSE SELECTION
    // =====================================================

    public void CloseSelection()
    {
        activeSlot = 0;
        previewedEffectIndex = -1;
        listOpen = false;

        HideAllInfo();
        RefreshAvailableEffects();
    }

    // =====================================================
    // UPDATE EFFECT SLOT TEXT
    // =====================================================

    private void UpdateSlotText()
    {
        if (effect1Text != null)
            effect1Text.text = selectedEffect1;

        if (effect2Text != null)
            effect2Text.text = selectedEffect2;
    }

    // =====================================================
    // ACCESS EFFECTS FOR SAVING
    // =====================================================

    public string GetEffect1()
    {
        return selectedEffect1;
    }

    public string GetEffect2()
    {
        return selectedEffect2;
    }
}
