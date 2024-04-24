using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Linq;


public class AuthHandler : MonoBehaviour
{
    private string url = "https://sid-restapi.onrender.com";

    public GameObject LogInPanel;
    public GameObject GamePanel;
    public GameObject panelLeaderboard;
    public Manager _manager;
    public Transform[] userLBData = new Transform[5];
    // Start is called before the first frame update
    void Start()
    {
        Token = PlayerPrefs.GetString("token");
        if (string.IsNullOrEmpty(Token))
        {
            Debug.Log("No hay token");
        }
        else
        {
            Username = PlayerPrefs.GetString("username");
            StartCoroutine("GetProfile");
        }
    }

    public void sendRegister()
    {
        AuthenticationData data = new AuthenticationData();

        data.username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text;
        data.password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text;

        StartCoroutine("Register", JsonUtility.ToJson(data));
    }

    public void sendLogin()
    {
        AuthenticationData data = new AuthenticationData();

        data.username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text;
        data.password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text;

        StartCoroutine("Login", JsonUtility.ToJson(data));
    }

    IEnumerator Register(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(url + "/api/usuarios", json);
        request.method = "POST";
        request.SetRequestHeader("Content-Type", "application/json");
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
                Debug.Log("Registro Exitoso");
                StartCoroutine("Login", json);
            }
            else
            {
                Debug.Log(request.responseCode + "|" + request.error);
            }
        }
    }
    IEnumerator Login(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(url + "/api/auth/login", json);
        request.method = "POST";
        request.SetRequestHeader("Content-Type", "application/json");
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
                Token = data.token;
                Username = data.usuario.username;
                PlayerPrefs.SetString("username", Username);
                PlayerPrefs.SetString("token", Token);
                _manager.Reset();
                Debug.Log(data.token);

                GamePanel.SetActive(true);
                LogInPanel.SetActive(false);
            }
            else
            {
                Debug.Log(request.responseCode + "|" + request.error);
            }
        }
    }

    public string Username { get; set; }

    public string Token { get; set; }

    IEnumerator GetProfile()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/api/usuarios/" + Username);
        //Debug.Log("Sending Request GetProfile");
        request.SetRequestHeader("x-token", Token);
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
                GamePanel.SetActive(true);
                LogInPanel.SetActive(false);
                _manager.Reset();
            }
            else
            {
                //Debug.Log(request.responseCode + "|" + request.error);
                Debug.Log("Usuario no autenticado");
            }
        }
    }

    public void ShowLeaderBoard(UsuarioJson[] usuarios)
    {
        for (int i = 0; i < usuarios.Length; i++)
        {
            userLBData[i].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = usuarios[i].username + ":";
            userLBData[i].transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = usuarios[i].data.score + ":";
        }
    }
    public void OpenLeaderboard() 
    {
        StartCoroutine("ListScoreboard");
        panelLeaderboard.SetActive(true);
    }
    public void HideLeaderboard()
    {
        panelLeaderboard.SetActive(false);
    }
    IEnumerator ListScoreboard()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/api/usuarios");
        Debug.Log("Sending Request GetLeaderboard");
        request.SetRequestHeader("x-token", Token);
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
                ListaUsuarios users = JsonUtility.FromJson<ListaUsuarios>(request.downloadHandler.text);
                Debug.Log("El largo de el array es: " + users.usuarios.Length);

                var usuariosOrganizados = users.usuarios.OrderByDescending(u => u.data.score).Take(7).ToArray();
                ShowLeaderBoard(usuariosOrganizados);
            }
            else
            {
                //Debug.Log(request.responseCode + "|" + request.error);
                Debug.Log("Usuario no autenticado");
            }
        }
    }

        //public void SetScore(UsuarioJson usuario) 
        //{
        //    StartCoroutine("Score", usuario);
        //}
        IEnumerator Score(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(url + "/api/usuarios", json);
        request.method = "PATCH";
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-token", Token);
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
                Debug.Log("User " + data.usuario.username + " score uptated to: " + data.usuario.data.score);
            }
            else
            {
                Debug.Log(request.responseCode + "|" + request.error);
            }
        }
    }


    public void Logout()
    {
        GamePanel.SetActive(false);
        LogInPanel.SetActive(true);
        Token = null;
        PlayerPrefs.SetString("token", Token);
    }
}
[System.Serializable]

public class AuthenticationData
{
    public string username;
    public string password;
    public UsuarioJson usuario;
    public string token;
    public UsuarioJson[] usuarios;
}

public class ListaUsuarios
{
    public UsuarioJson[] usuarios;
}

[System.Serializable]

public class UsuarioJson
{
    public string _id;
    public string username;
    public DataUser data;
}

[System.Serializable]

public class DataUser
{
    public int score;
}
