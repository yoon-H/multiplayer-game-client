using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class Room : MonoBehaviour
{
    private string gameId;
    private List<string> players = new List<string>();
    private RoomState state = RoomState.Waiting;
    private bool hasUpdated = false;

    public Button roomBtn;
    private Color waitColor = new Color32(20, 159, 19, 255);
    private Color progressColor = new Color32(183, 9, 20, 255);

    public void SetId(string id)
    {
        gameId = id;
    }

    public void AddPlayer(string id)
    {
        players.Add(id);
    }

    public void DeletePlayer(string id)
    {
        players.Remove(id);
    }

    public void JoinGame()
    {
        switch (state)
        {
            case RoomState.Waiting:
                GameManager.instance.gameId = gameId;
                GameManager.instance.GameStart();

                players.Add(GameManager.instance.deviceId);
                if(players.Count >=2)
                {
                    state = RoomState.InProgress;
                    ChangeRoomColor();
                }
                break;
            case RoomState.InProgress:
                // TODO 게임 참가 실패 코드
                break;
        }
        
    }

    public bool CheckHasUpdate()
    {
        return hasUpdated;
    }

    public void SetHasUpdate(bool flag)
    {
        hasUpdated = flag;
    }

    public void SetState(RoomState flag)
    {
        state = flag;

        ChangeRoomColor();
    }

    public void SetState(string flag)
    {
        switch (flag)
        {
            case "waiting":
                state = RoomState.Waiting;
                break;
            case "inProgress":
                state = RoomState.InProgress;
                break;
        }

        ChangeRoomColor();
    }

    public void ChangeRoomColor()
    {
        ColorBlock colorBlock = roomBtn.colors;

        switch(state)
        {
            case RoomState.Waiting:
                colorBlock.normalColor = waitColor;
                break;
            case RoomState.InProgress:
                colorBlock.normalColor = progressColor;
                break;
        }
        roomBtn.colors = colorBlock;
    }

    public void WriteGameId()
    {
        Text text = roomBtn.GetComponentInChildren<Text>();

        if (!text)
        {
            Debug.Log("There is not roomText");
            return;
        }

        text.text = gameId;
    }

}
