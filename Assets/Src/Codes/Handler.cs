using System;
using UnityEngine;

public class Handler
{
    public static void InitialHandler(InitialResponse res) {
        try 
        {
            GameManager.instance.userId = res.userId;
            GameManager.instance.player.UpdatePositionFromServer(res.x, res.y);
        } catch(Exception e)
        {
            Debug.LogError($"Error InitialHandelr: {e.Message}");
        }
    }

    public static void CreateGameHandler(CreateGameResponse res)
    {
        try
        {
            GameManager.instance.roomManager.AddRoom(res.gameId);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error CreateGameHandler: {e.Message}");
        }
    }
}