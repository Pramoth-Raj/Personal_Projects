using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject inputManager;
    InputManager inputManagerref;
    public void QuitGame()
    {
        Application.Quit();
    }
    public void ResumeGame()
    {
        gameObject.SetActive(false);
        inputManagerref.EnableInputs();
    }
    private void Awake()
    {
        inputManagerref=inputManager.GetComponent<InputManager>();  
    }
}
