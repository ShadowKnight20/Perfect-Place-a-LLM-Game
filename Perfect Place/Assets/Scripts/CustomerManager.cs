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

    private GameObject currentCustomer;
    private CustomerMovement movementScript;

    private List<MultipleCharactersInteraction> activeInteractions = new List<MultipleCharactersInteraction>();
    private bool hasPaidCustomer = false;

    public void SpawnCustomer()
    {
        if (currentCustomer != null) return;

        hasPaidCustomer = false;

        GameObject prefab = customerPrefabs[Random.Range(0, customerPrefabs.Length)];
        currentCustomer = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        movementScript = currentCustomer.GetComponent<CustomerMovement>();
        movementScript.MoveTo(counterPoint.position);

        var llms = currentCustomer.GetComponentsInChildren<LLMCharacter>();
        var input = currentCustomer.GetComponentInChildren<InputField>();
        var outputs = currentCustomer.GetComponentsInChildren<Text>();

        if (llms.Length >= 2 && input != null && outputs.Length >= 2)
        {
            var motherLLM = llms[0];
            var childLLM = llms[1];
            var motherOutput = outputs[0];
            var childOutput = outputs[1];

            input.onSubmit.RemoveAllListeners();
            input.onSubmit.AddListener((msg) =>
            {
                input.interactable = false;
                motherOutput.text = "...";

                _ = motherLLM.Chat(msg, motherResponse =>
                {
                    motherOutput.text = motherResponse;
                    input.interactable = true;
                    input.text = "";
                    input.Select();

                    if (!hasPaidCustomer && motherResponse.Contains("$"))
                    {
                        int gold = ExtractMoneyAmount(motherResponse);
                        if (gold > 0)
                        {
                            Object.FindAnyObjectByType<PlayerMoney>().AddMoney(gold);
                            hasPaidCustomer = true;
                        }
                    }

                    if (motherResponse.Contains("[talk to child]"))
                    {
                        string toChild = motherResponse.Replace("[talk to child]", "").Trim();
                        _ = childLLM.Chat("Mom says: " + toChild, childResponse =>
                        {
                            childOutput.text = childResponse;
                        });
                    }
                    else if (Random.value < 0.5f) // child sometimes replies independently
                    {
                        _ = childLLM.Chat("The player said: " + msg, childResponse =>
                        {
                            childOutput.text = childResponse;
                        });
                    }
                });
            });
        }
    }

    int ExtractMoneyAmount(string response)
    {
        var match = Regex.Match(response, @"\\$(\\d+)");
        if (match.Success)
        {
            return int.Parse(match.Groups[1].Value);
        }
        return 0;
    }

    public void SendCustomerAway()
    {
        if (currentCustomer == null) return;

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
        foreach (var interaction in activeInteractions)
        {
            interaction.AIReplyComplete();
        }

        var llms = Object.FindObjectsByType<LLMCharacter>(FindObjectsSortMode.None);
        foreach (var llm in llms)
        {
            llm.CancelRequests();
        }

        activeInteractions.Clear();
    }
}
