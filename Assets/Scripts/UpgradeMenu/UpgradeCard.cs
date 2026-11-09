
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCard : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text upgradeNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image upgradeImage;

    private FishingUpgrade upgrade;

    public void SetUpgrade(FishingUpgrade upgrade)
    {
        if (upgrade == null)
            return;

        this.upgrade = upgrade;

        upgradeNameText.text = upgrade.upgradeName;
        descriptionText.text = upgrade.description;
        upgradeImage.sprite = upgrade.sprite;
    }
}
