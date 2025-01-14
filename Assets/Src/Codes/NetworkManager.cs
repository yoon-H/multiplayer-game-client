using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public InputField ipInputField;
    public InputField portInputField;
    public InputField deviceIdInputField;
    public GameObject uiNotice;
    private TcpClient tcpClient;
    private NetworkStream stream;

    WaitForSecondsRealtime wait;

    private byte[] receiveBuffer = new byte[4096];
    private List<byte> incompleteData = new List<byte>();

    const string IP = "127.0.0.1";
    const string PORT = "5555";
    const string DEVICE_ID = "sparta";

    void Awake()
    {
        instance = this;
        wait = new WaitForSecondsRealtime(5);
    }
    public void OnStartButtonClicked()
    {
        string ip = IP;
        string port = PORT;

        if (IsValidPort(port))
        {
            int portNumber = int.Parse(port);

            if (deviceIdInputField.text != "")
            {
                GameManager.instance.deviceId = DEVICE_ID;
            }
            else
            {
                if (GameManager.instance.deviceId == "")
                {
                    GameManager.instance.deviceId = GenerateUniqueID();
                }
            }

            GameManager.instance.deviceId = DEVICE_ID;

            if (ConnectToServer(ip, portNumber))
            {
                StartGame();
                GameManager.instance.MoveToLobby();
            }
            else
            {
                AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
                StartCoroutine(NoticeRoutine(1));
            }

        }
        else
        {
            AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
            StartCoroutine(NoticeRoutine(0));
        }
    }

    bool IsValidIP(string ip)
    {
        // 간단한 IP 유효성 검사
        return System.Net.IPAddress.TryParse(ip, out _);
    }

    bool IsValidPort(string port)
    {
        // 간단한 포트 유효성 검사 (0 - 65535)
        if (int.TryParse(port, out int portNumber))
        {
            return portNumber > 0 && portNumber <= 65535;
        }
        return false;
    }

    bool ConnectToServer(string ip, int port)
    {
        try
        {
            tcpClient = new TcpClient(ip, port);
            stream = tcpClient.GetStream();
            Debug.Log($"Connected to {ip}:{port}");

            return true;
        }
        catch (SocketException e)
        {
            Debug.LogError($"SocketException: {e}");
            return false;
        }
    }

    string GenerateUniqueID()
    {
        return System.Guid.NewGuid().ToString();
    }

    public void CreateGame()
    {
        // 게임 세션 제작 패킷 보내기
        SendCreateGamePacket();
    }

    void StartGame()
    {
        // 게임 시작 코드 작성
        Debug.Log("Game Started");
        StartReceiving(); // Start receiving data
        SendInitialPacket();
    }

    IEnumerator NoticeRoutine(int index)
    {

        uiNotice.SetActive(true);
        uiNotice.transform.GetChild(index).gameObject.SetActive(true);

        yield return wait;

        uiNotice.SetActive(false);
        uiNotice.transform.GetChild(index).gameObject.SetActive(false);
    }

    public static byte[] ToBigEndian(byte[] bytes)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return bytes;
    }

    byte[] CreatePacketHeader(int dataLength, Packets.PacketType packetType)
    {
        int packetLength = 4 + 1 + dataLength; // 전체 패킷 길이 (헤더 포함)
        byte[] header = new byte[5]; // 4바이트 길이 + 1바이트 타입

        // 첫 4바이트: 패킷 전체 길이
        byte[] lengthBytes = BitConverter.GetBytes(packetLength);
        lengthBytes = ToBigEndian(lengthBytes);
        Array.Copy(lengthBytes, 0, header, 0, 4);

        // 다음 1바이트: 패킷 타입
        header[4] = (byte)packetType;

        return header;
    }

    // 공통 패킷 생성 함수
    async void SendPacket<T>(T payload, uint handlerId)
    {
        // ArrayBufferWriter<byte>를 사용하여 직렬화
        var payloadWriter = new ArrayBufferWriter<byte>();
        Packets.Serialize(payloadWriter, payload);
        byte[] payloadData = payloadWriter.WrittenSpan.ToArray();

        CommonPacket commonPacket = new CommonPacket
        {
            handlerId = handlerId,
            userId = GameManager.instance.userId,
            version = GameManager.instance.version,
            payload = payloadData,
        };

        // ArrayBufferWriter<byte>를 사용하여 직렬화
        var commonPacketWriter = new ArrayBufferWriter<byte>();
        Packets.Serialize(commonPacketWriter, commonPacket);
        byte[] data = commonPacketWriter.WrittenSpan.ToArray();

        // 헤더 생성
        byte[] header = CreatePacketHeader(data.Length, Packets.PacketType.Normal);

        // 패킷 생성
        byte[] packet = new byte[header.Length + data.Length];
        Array.Copy(header, 0, packet, 0, header.Length);
        Array.Copy(data, 0, packet, header.Length, data.Length);

        await Task.Delay(GameManager.instance.latency);

        // 패킷 전송
        stream.Write(packet, 0, packet.Length);
    }

    #region request 패킷 모음 

    void SendInitialPacket()
    {
        InitialPayload initialPayload = new InitialPayload
        {
            deviceId = GameManager.instance.deviceId,
            playerId = GameManager.instance.playerId,
            latency = GameManager.instance.latency,
        };

        // handlerId는 0으로 가정
        SendPacket(initialPayload, (uint)Packets.HandlerIds.Init);
    }

    void SendCreateGamePacket()
    {
        var now = DateTime.Now.ToLocalTime();
        var span = (now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
        int timestamp = (int)span.TotalSeconds;


        CreateGamePayload createGamePayload = new CreateGamePayload
        {
            timeStamp = timestamp,
        };

        SendPacket(createGamePayload, (uint)Packets.HandlerIds.CreateGame);
    }

    #endregion

    public void SendLocationUpdatePacket(float x, float y)
    {
        LocationUpdatePayload locationUpdatePayload = new LocationUpdatePayload
        {
            gameId = GameManager.instance.gameId,
            x = x,
            y = y,
        };

        SendPacket(locationUpdatePayload, (uint)Packets.HandlerIds.LocationUpdate);
    }


    void StartReceiving()
    {
        _ = ReceivePacketsAsync();
    }

    async System.Threading.Tasks.Task ReceivePacketsAsync()
    {
        while (tcpClient.Connected)
        {
            try
            {
                int bytesRead = await stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);
                if (bytesRead > 0)
                {
                    ProcessReceivedData(receiveBuffer, bytesRead);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Receive error: {e.Message}");
                break;
            }
        }
    }

    void ProcessReceivedData(byte[] data, int length)
    {
        incompleteData.AddRange(data.AsSpan(0, length).ToArray());

        while (incompleteData.Count >= 5)
        {
            // 패킷 길이와 타입 읽기
            byte[] lengthBytes = incompleteData.GetRange(0, 4).ToArray();
            int packetLength = BitConverter.ToInt32(ToBigEndian(lengthBytes), 0);
            Packets.PacketType packetType = (Packets.PacketType)incompleteData[4];

            //Debug.Log($"count : {incompleteData.Count}, packetLength : {packetLength}");

            if (incompleteData.Count < packetLength)
            {
                // 데이터가 충분하지 않으면 반환
                return;
            }

            // 패킷 데이터 추출
            byte[] packetData = incompleteData.GetRange(5, packetLength - 5).ToArray();
            incompleteData.RemoveRange(0, packetLength);

            //Debug.Log($"Received packet: Length = {packetLength}, Type = {packetType}");

            switch (packetType)
            {
                case Packets.PacketType.Ping:
                    HandlePingPacket(packetData);
                    break;
                case Packets.PacketType.Normal:
                    HandleNormalPacket(packetData);
                    break;
                case Packets.PacketType.Location:
                    HandleLocationPacket(packetData);
                    break;
            }
        }
    }

    void HandleNormalPacket(byte[] packetData)
    {
        // 패킷 데이터 처리
        var response = Packets.Deserialize<Response>(packetData);
        Debug.Log($"HandlerId: {response.handlerId}, responseCode: {response.responseCode}, timestamp: {response.timestamp}");

        if (response.responseCode != 0 && !uiNotice.activeSelf)
        {
            AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
            StartCoroutine(NoticeRoutine(2));
            return;
        }

        if (response.data != null && response.data.Length > 0)
        {

            switch ((Packets.HandlerIds)response.handlerId)
            {
                case Packets.HandlerIds.Init:
                    {
                        Handler.InitialHandler(Packets.ParsePayload<InitialResponse>(response.data));
                        break;
                    }
                case Packets.HandlerIds.CreateGame:
                    {
                        Handler.CreateGameHandler(Packets.ParsePayload<CreateGameResponse>(response.data));
                        break;
                    }
            }
            ProcessResponseData(response.data);
        }
    }

    void ProcessResponseData(byte[] data)
    {
        try
        {
            // var specificData = Packets.Deserialize<SpecificDataType>(data);
            string jsonString = Encoding.UTF8.GetString(data);
            Debug.Log($"Processed SpecificDataType: {jsonString}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing response data: {e.Message}");
        }
    }

    void HandleLocationPacket(byte[] data)
    {
        try
        {
            LocationUpdate response;

            if (data.Length > 0)
            {
                // 패킷 데이터 처리
                response = Packets.Deserialize<LocationUpdate>(data);
            }
            else
            {
                // data가 비어있을 경우 빈 배열을 전달
                response = new LocationUpdate { users = new List<LocationUpdate.UserLocation>() };
            }

            Spawner.instance.Spawn(response);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error HandleLocationPacket: {e.Message}");
        }
    }

    async void HandlePingPacket(byte[] data)
    {
        // 타임스탬프
        // 바로 돌려줄 예정

        // 헤더 생성
        byte[] header = CreatePacketHeader(data.Length, Packets.PacketType.Ping);

        // 패킷 생성
        byte[] packet = new byte[header.Length + data.Length];
        Array.Copy(header, 0, packet, 0, header.Length);
        Array.Copy(data, 0, packet, header.Length, data.Length);

        await Task.Delay(GameManager.instance.latency);

        // 패킷 전송
        stream.Write(packet, 0, packet.Length);
    }
}
