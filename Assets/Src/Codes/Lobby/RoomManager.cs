using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class RoomManager : MonoBehaviour
{
    private Dictionary<string, Room> rooms = new Dictionary<string, Room>();
    private Dictionary<string, GameObject> roomBtns = new Dictionary<string, GameObject>();

    public GameObject lobbyViewContent;
    public GameObject roomBtnPrefab;
    private string roomprefabName = "Room";


    public Room AddRoom(string gameId)
    {
        Debug.Log($"Add Room : gameId : {gameId}");
        GameObject btn = Instantiate(roomBtnPrefab, lobbyViewContent.transform);

        if (!btn) return null;
        Debug.Log($"Add Room : btn check");
        Room room = btn.GetComponent<Room>();

        if (!room) return null;
        Debug.Log($"Add Room : roomcheck");
        Debug.Log(room);
        room.SetId(gameId);
        Debug.Log($"Add Room : idcheck");
        room.AddPlayer(GameManager.instance.deviceId);

        Debug.Log($"Add Room : addplayer");

        rooms.Add(gameId, room);
        Debug.Log($"Add Room : addrooms");
        roomBtns.Add(gameId, btn);

        Debug.Log($"Add Room : addcheck");

        GameManager.instance.GameStart();

        return room;        
    }

    public void DeleteRoom(string gameId)
    {
        // Room 인스턴스 삭제
        rooms.Remove(gameId);
        GameObject btn = roomBtns[gameId];

        // Room 버튼 삭제
        roomBtns.Remove(gameId);

        // 오브젝트 삭제
        Destroy(btn);
    }    
}
