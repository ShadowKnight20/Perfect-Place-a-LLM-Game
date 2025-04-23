using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerMoney : MonoBehaviour
{
    public int currentMoney = 0;
    public Text moneyDisplay;

    public Color flashColor = Color.green;
    public float flashDuration = 0.5f;
    public Vector3 flashScale = new Vector3(1.3f, 1.3f, 1f);

    private Color originalColor;
    private Vector3 originalScale;

    void Start()
    {
        if (moneyDisplay != null)
        {
            originalColor = moneyDisplay.color;
            originalScale = moneyDisplay.rectTransform.localScale;
        }
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        Debug.Log($"Received ${amount}, total now ${currentMoney}");

        if (moneyDisplay != null)
        {
            moneyDisplay.text = "$" + currentMoney;
            StartCoroutine(FlashEffect());
        }
    }

    private IEnumerator FlashEffect()
    {
        // Flash green + scale up
        moneyDisplay.color = flashColor;
        moneyDisplay.rectTransform.localScale = flashScale;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return to original color and scale
        moneyDisplay.color = originalColor;
        moneyDisplay.rectTransform.localScale = originalScale;
    }
}
