using System;
using UnityEngine;

public class Handler
{
    public static void InitialHandler(InitialResponse res) {
        try 
        {
            GameManager.instance.userId = res.userId;
        } catch(Exception e)
        {
            Debug.LogError($"Error InitialHandelr: {e.Message}");
        }
    }

    public static void CreateGameHandler(CreateGameResponse res)
    {
        try
        {
            GameManager.instance.gameId = res.gameId;
            GameManager.instance.playerId = res.playerId;
            GameManager.instance.player.UpdatePositionFromServer(res.x, res.y);
            GameManager.instance.roomManager.AddRoom(res.gameId);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error CreateGameHandler: {e.Message}");
        }
    }

    public static void JoinGameHandler(JoinGameResponse res)
    {
        try
        {
            GameManager.instance.gameId = res.gameId;
            GameManager.instance.playerId = res.playerId;
            GameManager.instance.player.UpdatePositionFromServer(res.x, res.y);
            GameManager.instance.GameStart();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error CreateGameHandler: {e.Message}");
        }
    }

    public static void EndGameHandler(EndGameResponse res)
    {
        try
        {
            GameManager.instance.gameId = "";
            
            //TODO 방 상태 변경
        }
        catch (Exception e)
        {
            Debug.LogError($"Error EndGameHandler: {e.Message}");
        }
    }

    public static void GetGameSessionsHandler(GetGameSessionsResponse res)
    {
        try
        {
            GameManager.instance.roomManager.UpdateRooms(res.gameInfos);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error GetGameSessionsHandler: {e.Message}");
        }
    }
}