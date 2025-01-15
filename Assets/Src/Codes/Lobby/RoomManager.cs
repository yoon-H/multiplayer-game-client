using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class RoomManager : MonoBehaviour
{
    private Dictionary<string, Room> rooms = new Dictionary<string, Room>();
    private Dictionary<string, GameObject> roomBtns = new Dictionary<string, GameObject>();

    public GameObject lobbyViewContent;
    public GameObject roomBtnPrefab;
    private string roomprefabName = "Room";

    // 서버에서 얻어온 방 정보들 갱신
    public void UpdateRooms(List<GetGameSessionsResponse.GameInfo> gameList)
    {

        // 존재하는 방 갱신하고, 새로 생성된 방 스폰
        foreach(var game in gameList)
        {
            if(rooms.TryGetValue(game.gameId, out Room room))
            {
                room.SetState(game.state);
                room.SetHasUpdate(true);
            }
            else
            {
                SpawnRoom(game.gameId, game.state);
            }
        }

        List<string> idList = new List<string>();

        // 방이 삭제되었으면 id 저장
        foreach (var item in rooms)
        {
            var id = item.Key;
            var room = item.Value;

            if (room.CheckHasUpdate()) room.SetHasUpdate(false);
            else idList.Add(id);
        }

        // 삭제!
        foreach (var id in idList)
        {
            DeleteRoom(id);
        }

    }

    // 서버에서 방 정보 받아올 때 방 스폰하는 함수
    public void SpawnRoom(string gameId, string state)
    {
        GameObject btn = Instantiate(roomBtnPrefab, lobbyViewContent.transform);

        if (!btn) return;

        Room room = btn.GetComponent<Room>();

        if (!room) return;

        room.SetId(gameId);
        room.SetState(state);
        room.SetHasUpdate(true);

        rooms.Add(gameId, room);
        roomBtns.Add(gameId, btn);
    }


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
