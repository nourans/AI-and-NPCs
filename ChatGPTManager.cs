/*

CS4490Z (thesis): chatgpt to drive npc behaviour


this is the relevent code for the project
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;
using UnityEngine.Events;
using System.IO;


public class ChatGPTManager : MonoBehaviour {

    [TextArea(5,20)]
    public string personality;
    [TextArea(5,20)]
    public string scene;
    public int maxResponseWordLimit = 15;

    public List<NPCAction> actions;

    [System.Serializable]
    public struct NPCAction{
        [TextArea(1,5)]
        public string actionKeyword;
        [TextArea(2,5)]
        public string actionDescription;


        public UnityEvent actionEvent;
    }

    public OnResponseEvent OnResponse;
    
    [System.Serializable]
    public class OnResponseEvent : UnityEvent<string> { }

    private OpenAIApi openAI = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();
     private string logFilePath = "/Users/nouransakr/Downloads/Pets Projects 2.0/Assets/ChatGPTConversationLog.txt"; // don't forget to change this or use:
     // string logFilePath = Path.Combine(Application.persistentDataPath, "ChatGPTConversationLog.txt");

    public string getInstruction(){
        string instructions = "you are a video game character and will answer to the messages that the player sends you. \n" +
        "you must only reply to the player messages using the information in your Personality and the Scene that I provide. \n" +
        "please do not break character \n" +
        "you must answer in less than " + maxResponseWordLimit + " words. \n" +
        "your personality is " + personality + " and the Scene is " + scene + ".\n" + 
        buildActionInstructions() + 
        "here is the player's message: \n";

        return instructions;
    }

    public string buildActionInstructions(){
        string instructions = "";

        foreach (var item in actions){
            instructions += "if I imply that i want you to do the following: " + item.actionDescription 
            + ". You must add to your answer the following keyword: " + item.actionKeyword + ". \n";
            // used only so the appropriate action will be invoked. keyword will later be removed in AskChatGPT(). 
        }
        return instructions;
    }

    public async void AskChatGPT(string newText)
    {
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = getInstruction() + newText;
        newMessage.Role = "user";

        messages.Add(newMessage);
        LogMessage("Player: " + newText); 
        //LogMessage("Formatted message sent to ChatGPT: " + newMessage.Content); // commented out bc it's no longer needed. use for debugging

        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        request.Model = "gpt-3.5-turbo";

        var response = await openAI.CreateChatCompletion(request);

        if(response.Choices != null && response.Choices.Count > 0){
            var chatResponse = response.Choices[0].Message;
            //LogMessage("Response from ChatGPT: " + chatResponse.Content); // commented out bc it's no longer needed. use for debugging
 
            foreach(var item in actions){ // to trigger action
                if(chatResponse.Content.Contains(item.actionKeyword)){
                    string textNoKeyword = chatResponse.Content.Replace(item.actionKeyword, "");
                    chatResponse.Content = textNoKeyword;
                    item.actionEvent.Invoke();
                }
            }

            messages.Add(chatResponse);
            //LogMessage("Final response displayed to player: " + chatResponse.Content); // commented out bc it's no longer needed. use for debugging

            LogMessage("ChatGPT: " + chatResponse.Content); 

            Debug.Log(chatResponse.Content);

            OnResponse.Invoke(chatResponse.Content);
        }
    }
    public void MoveCharacter(Vector3 targetPosition, bool smooth = true){
        if (smooth){
            StartCoroutine(SmoothMove(targetPosition));
        }
        else{
            transform.position = targetPosition;
        }
    }

    private IEnumerator SmoothMove(Vector3 targetPosition){
        float duration = 1.0f; // Duration of the movement
        Vector3 startPosition = transform.position;
        float elapsed = 0;

        while (elapsed < duration){
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }

   private void LogMessage(string message){
    Debug.Log("Logging message: " + message);
    try{
        Debug.Log("Attempting to write to log file.");
        using (StreamWriter writer = new StreamWriter(logFilePath, true)){  // Ensure 'logFilePath' is defined and correct
            writer.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + message);
        }
        Debug.Log("Message logged successfully.");
    }
    catch (System.Exception ex){
        Debug.LogError("Failed to log message: " + ex.Message);
    }
}

    public void catMoveToEat(){
        MoveCharacter(new Vector3(-1779, -454, 1), smooth: true); // 
    }
    public void catMoveToSleep(){
        MoveCharacter(new Vector3(-1188, -96, -1), smooth: true); // 
    }
    public void catMoveToPlay(){
        MoveCharacter(new Vector3(1686, -290, 1), smooth: true); // 
    }

    public void catMoveToTalk(){
        MoveCharacter(new Vector3(123, -600, 1), smooth: true); // 
    }

    public void catMoveToHide(){
        MoveCharacter(new Vector3(-1140, -236, 1), smooth: true); // 
    }
    public void dogMoveToEat(){
        MoveCharacter(new Vector3(-2140, -600, 1), smooth: true); // 
    }
    public void dogMoveToSleep(){
        MoveCharacter(new Vector3(-680, -70, -1), smooth: true); // 
    }
    public void dogMoveToPlay(){
        MoveCharacter(new Vector3(1720, -230, 1), smooth: true); // 
    }

    public void dogMoveToTalk(){
        MoveCharacter(new Vector3(1220, -555, 1), smooth: true); // 
    }

    public void dogMoveToHide(){
        MoveCharacter(new Vector3(-2060, -750, 1), smooth: true); // 
    }
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
