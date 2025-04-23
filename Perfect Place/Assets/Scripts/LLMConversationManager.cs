using UnityEngine;
using UnityEngine.UI;
using LLMUnity;
using System.Threading.Tasks;

public class LLMConversationManager : MonoBehaviour
{
    public InputField playerInput;

    public LLMCharacter characterA;
    public Text characterAText;

    public LLMCharacter characterB;
    public Text characterBText;

    private string lastResponseA = "";
    private string lastResponseB = "";

    void Start()
    {
        playerInput.onSubmit.RemoveAllListeners();
        playerInput.onSubmit.AddListener(HandlePlayerInput);
    }

    void HandlePlayerInput(string message)
    {
        playerInput.interactable = false;
        characterAText.text = "...";
        characterBText.text = "...";

        _ = RunConversation(message);
    }

    async Task RunConversation(string message)
    {
        string inputForA = $"Player: {message}\n{(string.IsNullOrEmpty(lastResponseB) ? "" : "Other: " + lastResponseB)}";
        string inputForB = $"Player: {message}\n{(string.IsNullOrEmpty(lastResponseA) ? "" : "Other: " + lastResponseA)}";

        Task<string> taskA = characterA.Chat(inputForA);
        await Task.Delay(500); // slight delay to mimic thought
        Task<string> taskB = characterB.Chat(inputForB);

        lastResponseA = await taskA;
        characterAText.text = lastResponseA;

        lastResponseB = await taskB;
        characterBText.text = lastResponseB;

        playerInput.text = "";
        playerInput.interactable = true;
        playerInput.Select();
    }

    public void CancelAllRequests()
    {
        characterA.CancelRequests();
        characterB.CancelRequests();
    }
}
