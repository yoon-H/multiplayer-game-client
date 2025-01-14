using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public List<GameObject> prefabList = new(); // ������ ����Ʈ ��� << �ν����� â���� ����
    private Dictionary<string, Queue<GameObject>> prefabPoolDict = new(); // ��ųʸ� ������ Ű���� ����ؼ� ���ϴ� �����͸� �����´�.

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

    public void DeSpawn(GameObject go) // ���� ������
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
