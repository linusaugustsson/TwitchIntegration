using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{

    public GameObject startMenu;
    public GameObject noChannelName;

    public GameObject startInfo;
    public GameObject waitAuth;
    public GameObject success;


    public void CloseStartMenu()
    {
        startMenu.SetActive(false);
    }
    public void ShowStartMenu()
    {
        startMenu.SetActive(true);
    }

    public void AuthState(int _stage)
    {
        if(_stage == 0)
        {
            startInfo.SetActive(true);
            waitAuth.SetActive(false);
            success.SetActive(false);
            return;
        }


        if (_stage == 1)
        {
            startInfo.SetActive(false);
            waitAuth.SetActive(true);
            success.SetActive(false);
            return;
        }

        if (_stage == 2)
        {
            startInfo.SetActive(false);
            waitAuth.SetActive(false);
            success.SetActive(true);
            return;
        }
    }


}
