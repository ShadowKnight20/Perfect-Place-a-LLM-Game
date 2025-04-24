using UnityEngine;
using UnityEngine.UI;
using LLMUnity;
using System.Collections.Generic;
using LLMUnitySamples;
using System.Text.RegularExpressions;

public class CustomerManager : MonoBehaviour
{
    public GameObject[] customerPrefabs;
    public Transform spawnPoint;
    public Transform targetPoint;
    public Transform exitPoint;
    public Transform counterPoint;
    public Button submitButton; // Button for submitting input

    private GameObject currentCustomer;
    private CustomerMovement movementScript;

    private List<MultipleCharactersInteraction> activeInteractions = new List<MultipleCharactersInteraction>();
    private bool hasPaidCustomer = false;
    private bool isConversationActive = false;

    private InputField currentInput;
    private LLMCharacter llmPrimary;
    private LLMCharacter llmSecondary;
    private Text outputPrimary;
    private Text outputSecondary;

    public void SpawnCustomer()
    {
        if (currentCustomer != null) return;

        hasPaidCustomer = false;
        isConversationActive = true;

        GameObject prefab = customerPrefabs[Random.Range(0, customerPrefabs.Length)];
        currentCustomer = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        movementScript = currentCustomer.GetComponent<CustomerMovement>();
        movementScript.MoveTo(counterPoint.position);

        var llms = currentCustomer.GetComponentsInChildren<LLMCharacter>();
        var inputs = currentCustomer.GetComponentsInChildren<InputField>();
        var outputs = currentCustomer.GetComponentsInChildren<Text>();

        if (llms.Length >= 1 && inputs.Length >= 1 && outputs.Length >= 1)
        {
            llmPrimary = llms[0];
            llmSecondary = llms.Length > 1 ? llms[1] : null;
            currentInput = inputs[0];
            outputPrimary = outputs[0];
            outputSecondary = outputs.Length > 1 ? outputs[1] : null;

            currentInput.onSubmit.RemoveAllListeners();
            currentInput.onSubmit.AddListener((msg) =>
            {
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    SubmitMessage(msg);
                }
            });

            if (submitButton != null)
            {
                submitButton.onClick.RemoveAllListeners();
                submitButton.onClick.AddListener(() =>
                {
                    if (!string.IsNullOrWhiteSpace(currentInput.text))
                    {
                        SubmitMessage(currentInput.text);
                    }
                });
            }
        }
    }
    private bool childResponding = false;

    private void SubmitMessage(string msg)
    {
        if (!isConversationActive || llmPrimary == null) return;

        currentInput.interactable = false;
        outputPrimary.text = "...";

        _ = llmPrimary.Chat(msg, response =>
        {
            if (!isConversationActive) return;

            outputPrimary.text = response;
            currentInput.text = "";
            currentInput.interactable = true;
            currentInput.Select();

            if (!hasPaidCustomer && response.Contains("$"))
            {
                int gold = ExtractMoneyAmount(response);
                if (gold > 0)
                {
                    Object.FindAnyObjectByType<PlayerMoney>().AddMoney(gold);
                    hasPaidCustomer = true;
                }
            }

            // CHILD CHAT SAFEGUARD
            if (llmSecondary != null && outputSecondary != null && Random.value < 0.5f && !childResponding)
            {
                childResponding = true;

                _ = llmSecondary.Chat("They said: " + msg, childResponse =>
                {
                    if (isConversationActive)
                    {
                        outputSecondary.text = childResponse;
                    }

                    //Reset after response
                    childResponding = false;
                });
            }
        });
    }

    int ExtractMoneyAmount(string response)
    {
        var match = Regex.Match(response, @"\$(\d+)");
        if (match.Success)
        {
            return int.Parse(match.Groups[1].Value);
        }
        return 0;
    }

    public void SendCustomerAway()
    {
        if (currentCustomer == null) return;

        isConversationActive = false;

        var input = currentCustomer.GetComponentInChildren<InputField>();
        if (input != null)
        {
            input.onSubmit.RemoveAllListeners();
        }

        var llms = currentCustomer.GetComponentsInChildren<LLMCharacter>();
        foreach (var llm in llms)
        {
            llm.CancelRequests();
        }

        movementScript.MoveTo(exitPoint.position);
        movementScript.OnDestinationReached = () =>
        {
            Destroy(currentCustomer);
            currentCustomer = null;
        };
    }

    public void CancelAllRequests()
    {
        isConversationActive = false;

        foreach (var interaction in activeInteractions)
        {
            interaction.AIReplyComplete();
        }

        // Clone list before iterating to avoid modification exception
        var llms = new List<LLMCharacter>(Object.FindObjectsByType<LLMCharacter>(FindObjectsSortMode.None));
        foreach (var llm in llms)
        {
            if (llm != null)
            {
                llm.CancelRequests();
            }
        }

        activeInteractions.Clear();
    }
}
