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
        GameObject btn = Instantiate(roomBtnPrefab, lobbyViewContent.transform);

        if (!btn) return null;

        Room room = btn.GetComponent<Room>();

        if (!room) return null;

        room.SetId(gameId);

        room.AddPlayer(GameManager.instance.deviceId);

        rooms.Add(gameId, room);
        roomBtns.Add(gameId, btn);

        GameManager.instance.GameStart();

        return room;        
    }

    public void DeleteRoom(string gameId)
    {
        // Room 삭제
        rooms.Remove(gameId);
        GameObject btn = roomBtns[gameId];

        // Room 버튼 삭제
        roomBtns.Remove(gameId);

        // 오브젝트 삭제
        Destroy(btn);
    }    
}
