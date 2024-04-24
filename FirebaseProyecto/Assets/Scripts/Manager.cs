using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class Manager : MonoBehaviour
{
    public TextMeshProUGUI ClicksTotalText;
    public TextMeshProUGUI CountdownText;
    public float clickCooldownDuration = 5f;
    private float clickCooldownTimer = 0f;
    private int TotalClicks = 0;
    public Button ClickButton; // Reference to your UI button
    public GameObject ExitButton; // Reference to the second button GameObject
    public GameObject LeaderboardButton;
    public GameObject GamePanel; // Reference to your panel GameObject
    public AuthHandler Auth;
    private string Username;
    private bool setScoreBool = false; 

    private string url = "https://sid-restapi.onrender.com";

    private void Start()
    {
        clickCooldownTimer = 0f;

    }
    public void Reset()
    {
        clickCooldownTimer = 0f;
        TotalClicks = 0;
        ClickButton.interactable = true;
        // Deactivate the second button GameObject:
        ExitButton.SetActive(false); // Enable the second button
        LeaderboardButton.SetActive(false); // Enable the second button
        setScoreBool = false;
    }
    private void Update()
    {
        // Only update the countdown if the panel is active
        if (GamePanel.activeSelf)
        {
            clickCooldownTimer += Time.deltaTime;

            if (clickCooldownTimer >= clickCooldownDuration)
            {
                Username = Auth.Username;
                PlayerPrefs.SetInt("Score", TotalClicks);
                ClickButton.interactable = false; // Disable the button

                // Activate the second button GameObject:
                ExitButton.SetActive(true); // Enable the second button
                LeaderboardButton.SetActive(true); // Enable the second button

                if (setScoreBool == false) 
                {
                    setScoreBool = true;
                    StartCoroutine("GetProfile");
                }
            }

            float remainingTime = Mathf.Max(0f, clickCooldownDuration - clickCooldownTimer);
            CountdownText.text = $"Countdown: {remainingTime:F1} s";
        }
    }

    // ctrl+k,c comment & ctrl+k,u uncomment

    IEnumerator GetProfile()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/api/usuarios/" + Username);
        //Debug.Log("Sending Request GetProfile");
        request.SetRequestHeader("x-token", Auth.Token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthenticationData data = JsonUtility.FromJson<AuthenticationData>(request.downloadHandler.text);
                Debug.Log("El usuario " + data.usuario.username + " se encuentra autenticado y su puntaje es " + data.usuario.data.score);

                UsuarioJson usuario = new UsuarioJson();
                usuario.data = new DataUser();
                usuario.username = Username;
                int score = TotalClicks;
                if (score > usuario.data.score)
                {
                    usuario.data.score = TotalClicks;
                    Auth.StartCoroutine("Score", JsonUtility.ToJson(usuario));

                }

            }
            else
            {
                //Debug.Log(request.responseCode + "|" + request.error);
                Debug.Log("Usuario no autenticado");
            }
        }
    }
    public void AddClicks()
    {
        TotalClicks++;
        ClicksTotalText.text = TotalClicks.ToString("0");
        clickCooldownTimer = 0f;
    }
}
