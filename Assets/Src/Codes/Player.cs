using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public string deviceId;
    public RuntimeAnimatorController[] animCon;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;
    TextMeshPro myText;

    private Vector2 targetPosition; // 서버로부터 받은 목표 위치를 저장할 변수
    private bool isTargetPositionSet = false; // 서버에서 목표 위치를 받은 상태인지

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        myText = GetComponentInChildren<TextMeshPro>();
    }

    void OnEnable() {

        if (deviceId.Length > 5) {
            myText.text = deviceId[..5];
        } else {
            myText.text = deviceId;
        }
        myText.GetComponent<MeshRenderer>().sortingOrder = 6;
        
        anim.runtimeAnimatorController = animCon[GameManager.instance.playerId];
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.isLive) {
            return;
        }
        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");

        // 위치 이동 패킷 전송 -> 서버로
        NetworkManager.instance.SendLocationUpdatePacket(rigid.position.x, rigid.position.y);
    }


    void FixedUpdate() {
        if (!GameManager.instance.isLive) {
            return;
        }

        if (isTargetPositionSet)
        {
            // 서버로부터 받은 위치로 이동
            rigid.MovePosition(targetPosition);
            isTargetPositionSet = false; // 목표 위치로 이동 후 상태 초기화
        }
        else
        {
            // 입력에 따른 이동
            Vector2 nextVec = inputVec * speed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + nextVec);
        }
    }

    // Update가 끝난이후 적용
    void LateUpdate() {
        if (!GameManager.instance.isLive) {
            return;
        }

        anim.SetFloat("Speed", inputVec.magnitude);

        if (inputVec.x != 0) {
            spriter.flipX = inputVec.x < 0;
        }
    }

    void OnCollisionStay2D(Collision2D collision) {
        if (!GameManager.instance.isLive) {
            return;
        }
    }

    public void UpdatePositionFromServer(float x, float y)
    {
        targetPosition = new Vector2(x, y);
        isTargetPositionSet = true;
    }
}
