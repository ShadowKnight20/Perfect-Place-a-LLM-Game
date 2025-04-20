using UnityEngine;
using UnityEngine.UI;
using LLMUnity;
using System.Collections.Generic;
using LLMUnitySamples; 

public class CustomerManager : MonoBehaviour
{
    public GameObject[] customerPrefabs; // Now supports multiple prefabs
    public Transform spawnPoint;
    public Transform targetPoint;
    public Transform exitPoint;
    public Transform counterPoint;

    private GameObject currentCustomer;
    private CustomerMovement movementScript;

    private List<MultipleCharactersInteraction> activeInteractions = new List<MultipleCharactersInteraction>();

    public void SpawnCustomer()
    {
        if (currentCustomer != null) return;

        GameObject prefab = customerPrefabs[Random.Range(0, customerPrefabs.Length)];
        currentCustomer = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        movementScript = currentCustomer.GetComponent<CustomerMovement>();
        movementScript.MoveTo(counterPoint.position);

        // Setup AI Input-Output connection
        var llm = currentCustomer.GetComponentInChildren<LLMCharacter>();
        var input = currentCustomer.GetComponentInChildren<InputField>();
        var output = currentCustomer.GetComponentInChildren<Text>();

        if (llm != null && input != null && output != null)
        {
            input.onSubmit.RemoveAllListeners(); // safety check
            input.onSubmit.AddListener((msg) =>
            {
                input.interactable = false;
                output.text = "...";
                _ = llm.Chat(msg, response =>
                {
                    output.text = response;
                    input.interactable = true;
                    input.text = "";
                    input.Select();
                });
            });
        }
    }


    public void SendCustomerAway()
    {
        if (currentCustomer == null) return;
        var input = currentCustomer.GetComponentInChildren<InputField>();
        if (input != null)
        {
            input.onSubmit.RemoveAllListeners();
        }

        var llm = currentCustomer.GetComponentInChildren<LLMCharacter>();
        if (llm != null)
        {
            //llm.Reset();
            llm.CancelRequests(); // Cancel previous AI request
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
