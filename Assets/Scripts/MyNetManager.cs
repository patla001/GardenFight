using UnityEngine;
using System.Collections;
using Mirror;
using Mirror.Discovery;
using System;

public class MyNetManager : NetworkManager
{
	public SimpleNetworkDiscovery discovery;
    public GameObject HostButton;
    public GameObject JoinButton;
    public GameObject CancelButton;
    public GameObject EnterButton;
    public GameObject rematchButton;
    public GameObject exitButton;
    public GameObject WaitingText;
    public GameObject winText;
    public GameObject loseText;
    public Animator fadeAnimator;
    public string state;

    public override void OnStartHost()
	{
		base.OnStartHost();
		if (discovery != null)
		{
			discovery.AdvertiseServer();
		}
	}
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        if (discovery != null && NetworkServer.active)
        {
            discovery.AdvertiseServer();
        }
	}

    public override void OnClientDisconnect()
    {
        StopClient();
        print("Client is disconnected");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        HideUI();
	}

    public override void OnStopClient()
    {
		if (discovery != null)
		{
			discovery.StopDiscovery();
		}
        ResetUI();
	}

    public void StartGameHost()
    {
        fadeAnimator.SetTrigger("Fade In");
        StartCoroutine(StartingGameHost());
    }

    IEnumerator StartingGameHost()
    {
        yield return new WaitForSeconds(.75f);
        state = "Hosting";
        try
        {
            StartHost();
        } catch(Exception)
        {
            print("The port is occupied");
        }
    }

    public void StartJoinRequest()
    {
        state = "Joining";
        if (discovery != null)
        {
            discovery.StartDiscovery();
        }
    }

    public void HideUI()
    {
        state = "Playing";
        CancelButton.SetActive(true);
        exitButton.SetActive(false);
        HostButton.SetActive(false);
        JoinButton.SetActive(false);
        EnterButton.SetActive(false);
        WaitingText.SetActive(false);
        winText.SetActive(false);
        loseText.SetActive(false);
    }

    public void ResetUI()
    {
        HostButton.SetActive(true);
        JoinButton.SetActive(true);
        winText.SetActive(true);
        loseText.SetActive(true);
        exitButton.SetActive(true);
        Manager.isRefresed = false;
        CancelButton.SetActive(false);
        EnterButton.SetActive(false);
        WaitingText.SetActive(false);
        rematchButton.SetActive(false);
    }

    public void Cancel()
    {
        fadeAnimator.SetTrigger("Fade In");
        StartCoroutine(Cancelling());
    }

    IEnumerator Cancelling()
    {
        yield return new WaitForSeconds(.75f);
        if (state == "Joining")
        {
            if (discovery != null)
            {
                discovery.StopDiscovery();
            }
        }
        else
        {
            StopHost();
        }
        ResetUI();
    }

}
