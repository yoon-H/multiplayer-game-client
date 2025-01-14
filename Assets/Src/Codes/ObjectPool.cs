using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public List<GameObject> prefabList = new(); // // 프리팹 리스트 등록 << 인스팩터 창에서 가능
    private Dictionary<string, Queue<GameObject>> prefabPoolDict = new(); // 딕셔너리 도서관 키값을 사용해서 원하는 데이터를 가져온다.

    public GameObject Spawn(string nameKey, Vector2 position = default, Quaternion rotation = default, Transform parent = null)
    {
        if (prefabPoolDict.ContainsKey(nameKey))
        {
            var origin = prefabPoolDict[nameKey].Dequeue();
            origin.transform.SetPositionAndRotation(position, rotation);
            origin.transform.SetParent(parent);
            return origin;
        }
        var prefab = prefabList.Find(x => x.name == nameKey);
        var copy = Instantiate<GameObject>(prefab, position, rotation, parent);
        copy.name = nameKey;
        return copy;
    }

    public void DeSpawn(GameObject go) // 돌려 보내기
    {
        var nameKey = go.name;
        if (prefabPoolDict.ContainsKey(nameKey))
        {
            prefabPoolDict[nameKey].Enqueue(go);
        }
        else
        {
            prefabPoolDict[nameKey] = new();
            prefabPoolDict[nameKey].Enqueue(go);
        }

    }
}
