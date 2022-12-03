using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
// TCP stuff
using System;
using System.Threading; //Thread Class를 포함
using System.Net;
using System.Net.Sockets; //TcpClient, TcpListener, NetworkStream를 포함
using System.Text;
using Unity.VisualScripting;


public class PlayerCtrl : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private CharacterController controller;
    private new Transform transform;
    private new Camera camera;
    private PhotonView pv;
    private CinemachineVirtualCamera virtualCamera;

    //TCP
    //전송받은 데이터를 할당하기 위한 변수들 선언//
    string[] Data_List = new string[28]; //전체 데이터에 대한 문자열 리스트

    float[] R_Angle_List = new float[10]; //오른손 각에 대한 float 리스트

    public int R_Finger_Number = 0; //오른손가락 개수

    float[] L_Angle_List = new float[10]; //왼손 각에 대한 float 리스트

    public int L_Finger_Number = 0; //왼손가락 개수

    Vector3 R_Position; //오른손 검지 8번 위치에 대한 Vector3
    Vector3 L_Position; //왼손 검지 8번 위치에 Vector3

    string string_text = "null null";//Python에서 건너온 Data를 String으로 형변환 후 할당하기 위해 선언한 변수


    // TCP 관련 변수
    Thread receiveThread; //스레드 생성을 위한 객체 선언
    TcpClient client; //클라이언트 어플리케이션 생성하기 위한 객체 선언
    TcpListener listener; //TCP 통신의 클라이언트에서 연결 수신하기 위한 객체 선언
    int port = 25001; //연결할 포트: 25001


    public Transform left_first_1;
    public Transform left_first_2;
    public Transform left_second_1;
    public Transform left_second_2;
    public Transform left_third_1;
    public Transform left_third_2;
    public Transform left_fourth_1;
    public Transform left_fourth_2;
    public Transform left_fifth_1;
    public Transform left_fifth_2;

    public Transform right_first_1;
    public Transform right_first_2;
    public Transform right_second_1;
    public Transform right_second_2;
    public Transform right_third_1;
    public Transform right_third_2;
    public Transform right_fourth_1;
    public Transform right_fourth_2;
    public Transform right_fifth_1;
    public Transform right_fifth_2;

    public Transform rightHand;
    public Transform leftHand;

    private Transform originalRightHand;
    private Transform originalLeftHand;

    void Start()
    {
        InitTCP();
        controller = GetComponent<CharacterController>();
        transform = GetComponent<Transform>();
        camera = Camera.main;

        pv = GetComponent<PhotonView>();

        virtualCamera = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
        if(pv.IsMine)
        {
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;
        }
        originalRightHand = rightHand;
        originalLeftHand = leftHand;
    }

//파이썬으로부터 구문을 받기 위해 TCP의 스레드 환경 구축
    private void InitTCP()
    {
        receiveThread = new Thread(new ThreadStart(ReceiveData)); //스레드 생성
        receiveThread.IsBackground = true; //receiveThread를 Background Thread로 설정
        receiveThread.Start(); //스레드 시작
    }

    //socket 서버를 통한 TCP 통신으로 데이터 수신하기
    private void ReceiveData()
    {
        try
        {
            print("Waiting");
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port); 
            listener.Start(); 
            Byte[] bytes = new Byte[1024]; //1024 bytes의 byte형 배열 선언

            while (true)
            {
                using (client = listener.AcceptTcpClient()) //클라이언트의 연결 요청 수락, Data Stream을 받기 위해 TcpClient 객체 반환해 client에 할당
                {
                    using (NetworkStream stream = client.GetStream()) //데이터 송수신에 사용되는 Network Stream을 가져와 stream에 할당
                    {
                        //Network Stream으로부터 Data를 읽음
                        int length;
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) 
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length); //incommingData에 btyes의 Data를 그대로 복사해서 붙여넣음.
                            string clientMessage = Encoding.UTF8.GetString(incommingData); //incommingData의 Byte[]를 String으로 형변환해서 clientMessage에 할당
                            ///////Ttext에 clientMessage를 할당////////////////////////
                            
                            string_text = clientMessage;

                            ///////////////////////////////////////////////////////////

                            //////////////////////Data를 유의미한 값으로 변환//////////////////////

                            //Data_List 생성
                            Data_List = Parse_String_List(string_text);

                            ///////////////////////////////////////////////////////////////////////


                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            //Exception: 예외 클래스, 예외 발생 시 해당 예외를 출력
            print(e.ToString());
        }
    }

 
    // 전송 받은 String을 배열 형태로 변환
    public static string[] Parse_String_List(string string_list)
    {
        // 괄호 제거
        string_list = string_list.Substring(1, string_list.Length - 2);

        //각 요소들을 그대로 배열에 저장
        string[] Data_Array = string_list.Split(',');

        return Data_Array;
    }

    //오른쪽 손의 각도 리스트 생성(float[] 형)
    public static float[] Make_R_Angle_Array(string[] string_array)
    {
        float[] R_Angle_List = new float[10];

        for (int i = 0; i < 10; i++)
        {
            R_Angle_List[i] = float.Parse(string_array[i]);
        }
        return R_Angle_List;
    }

    //오른쪽 손가락 개수 생성(int 형)
    public static int Make_R_Finger_Number(string[] string_array)
    {
        int R_Finger_Number = 0;
        R_Finger_Number = int.Parse(string_array[10]);
        return R_Finger_Number;
    }

    //왼쪽 손의 각도 리스트 생성(float[] 형)
    public static float[] Make_L_Angle_Array(string[] string_array)
    {
        float[] L_Angle_List = new float[10];
        for (int i = 14; i < 24; i++)
        {
            L_Angle_List[i-14] = float.Parse(string_array[i]);
        }
        return L_Angle_List;
    }

    //왼쪽 손가락 개수 생성(int 형)
    public static int Make_L_Finger_Number(string[] string_array)
    {
        int L_Finger_Number = 0;
        L_Finger_Number = int.Parse(string_array[24]);
        return L_Finger_Number;
    }

    //오른손 검지 8번의 X, Y, Z 데이터를 바탕으로 Vector3 생성
    public static Vector3 Make_R_Position(string[] Data_List)
    {
        Vector3 Position = new Vector3(
            float.Parse(Data_List[11]), 
            float.Parse(Data_List[12]), 
            float.Parse(Data_List[13]));

        return Position;
    }

    //왼손 검지 8번의 X, Y, Z 데이터를 바탕으로 Vector3 생성
    public static Vector3 Make_L_Position(string[] Data_List)
    {
        Vector3 Position = new Vector3(
            float.Parse(Data_List[25]),
            float.Parse(Data_List[26]),
            float.Parse(Data_List[27]));

        return Position;
    }

    void Update()
    {
        //각 변수(리스트 or int)에 값 할당
        if(pv.IsMine)
        {
            if (Data_List != null)
            {
                R_Angle_List = Make_R_Angle_Array(Data_List);

                R_Finger_Number = Make_R_Finger_Number(Data_List);

                L_Angle_List = Make_L_Angle_Array(Data_List);

                L_Finger_Number = Make_L_Finger_Number(Data_List);

                R_Position = Make_R_Position(Data_List);

                L_Position = Make_L_Position(Data_List);
            }

            if(transform.position.z < 0)
            {
                player1_HandControl();
                player1_AtkDef();
            }


            if(transform.position.z > 0)
            {
                player2_HandControl();
                player2_AtkDef();
            }
            
        }
    }

    void player1_HandControl()
    {
        ///////////// right hand
        Quaternion move_right_1_1 = Quaternion.Euler(R_Angle_List[0]*2f, 0, 0);
        right_first_1.rotation = move_right_1_1;
        Quaternion move_right_1_2 = Quaternion.Euler(R_Angle_List[1]*4f, 0, 0);
        right_first_2.rotation = move_right_1_2;
        Quaternion move_right_2_1 = Quaternion.Euler(R_Angle_List[2], 0, 0);
        right_second_1.rotation = move_right_2_1;
        Quaternion move_right_2_2 = Quaternion.Euler(R_Angle_List[3]*1.5f, 0, 0);
        right_second_2.rotation = move_right_2_2;
        Quaternion move_right_3_1 = Quaternion.Euler(R_Angle_List[4], 0, 0);
        right_third_1.rotation = move_right_3_1;
        Quaternion move_right_3_2 = Quaternion.Euler(R_Angle_List[5]*1.5f, 0, 0);
        right_third_2.rotation = move_right_3_2;
        Quaternion move_right_4_1 = Quaternion.Euler(R_Angle_List[6], 0, 0);
        right_fourth_1.rotation = move_right_4_1;
        Quaternion move_right_4_2 = Quaternion.Euler(R_Angle_List[7]*1.5f, 0, 0);
        right_fourth_2.rotation = move_right_4_2;
        Quaternion move_right_5_1 = Quaternion.Euler(R_Angle_List[8], 0, 0);
        right_fifth_1.rotation = move_right_5_1;
        Quaternion move_right_5_2 = Quaternion.Euler(R_Angle_List[9]*1.5f, 0, 0);
        right_fifth_2.rotation = move_right_5_2;
        ///////////// left hand
        Quaternion move_left_1_1 = Quaternion.Euler(-L_Angle_List[0]*2f, 0, 0);
        left_first_1.rotation = move_left_1_1;
        Quaternion move_left_1_2 = Quaternion.Euler(-L_Angle_List[1]*4f, 0, 0);
        left_first_2.rotation = move_left_1_2;
        Quaternion move_left_2_1 = Quaternion.Euler(-L_Angle_List[2], 0, 0);
        left_second_1.rotation = move_left_2_1;
        Quaternion move_left_2_2 = Quaternion.Euler(-L_Angle_List[3]*1.5f, 0, 0);
        left_second_2.rotation = move_left_2_2;
        Quaternion move_left_3_1 = Quaternion.Euler(-L_Angle_List[4], 0, 0);
        left_third_1.rotation = move_left_3_1;
        Quaternion move_left_3_2 = Quaternion.Euler(-L_Angle_List[5]*1.5f, 0, 0);
        left_third_2.rotation = move_left_3_2;
        Quaternion move_left_4_1 = Quaternion.Euler(-L_Angle_List[6], 0, 0);
        left_fourth_1.rotation = move_left_4_1;
        Quaternion move_left_4_2 = Quaternion.Euler(-L_Angle_List[7]*1.5f, 0, 0);
        left_fourth_2.rotation = move_left_4_2;
        Quaternion move_left_5_1 = Quaternion.Euler(-L_Angle_List[8], 0, 0);
        left_fifth_1.rotation = move_left_5_1;
        Quaternion move_left_5_2 = Quaternion.Euler(-L_Angle_List[9]*1.5f, 0, 0);
        left_fifth_2.rotation = move_left_5_2;            
    }

    void player2_HandControl()
    {
        ///////////// right hand
        Quaternion move_right_1_1 = Quaternion.Euler(-R_Angle_List[0]*2f, 0, 0);
        right_first_1.rotation = move_right_1_1;
        Quaternion move_right_1_2 = Quaternion.Euler(-R_Angle_List[1]*4f, 0, 0);
        right_first_2.rotation = move_right_1_2;
        Quaternion move_right_2_1 = Quaternion.Euler(-R_Angle_List[2], 0, 0);
        right_second_1.rotation = move_right_2_1;
        Quaternion move_right_2_2 = Quaternion.Euler(-R_Angle_List[3]*1.5f, 0, 0);
        right_second_2.rotation = move_right_2_2;
        Quaternion move_right_3_1 = Quaternion.Euler(-R_Angle_List[4], 0, 0);
        right_third_1.rotation = move_right_3_1;
        Quaternion move_right_3_2 = Quaternion.Euler(-R_Angle_List[5]*1.5f, 0, 0);
        right_third_2.rotation = move_right_3_2;
        Quaternion move_right_4_1 = Quaternion.Euler(-R_Angle_List[6], 0, 0);
        right_fourth_1.rotation = move_right_4_1;
        Quaternion move_right_4_2 = Quaternion.Euler(-R_Angle_List[7]*1.5f, 0, 0);
        right_fourth_2.rotation = move_right_4_2;
        Quaternion move_right_5_1 = Quaternion.Euler(-R_Angle_List[8], 0, 0);
        right_fifth_1.rotation = move_right_5_1;
        Quaternion move_right_5_2 = Quaternion.Euler(-R_Angle_List[9]*1.5f, 0, 0);
        right_fifth_2.rotation = move_right_5_2;
        ///////////// left hand
        Quaternion move_left_1_1 = Quaternion.Euler(L_Angle_List[0]*2f, 0, 0);
        left_first_1.rotation = move_left_1_1;
        Quaternion move_left_1_2 = Quaternion.Euler(L_Angle_List[1]*4f, 0, 0);
        left_first_2.rotation = move_left_1_2;
        Quaternion move_left_2_1 = Quaternion.Euler(L_Angle_List[2], 0, 0);
        left_second_1.rotation = move_left_2_1;
        Quaternion move_left_2_2 = Quaternion.Euler(L_Angle_List[3]*1.5f, 0, 0);
        left_second_2.rotation = move_left_2_2;
        Quaternion move_left_3_1 = Quaternion.Euler(L_Angle_List[4], 0, 0);
        left_third_1.rotation = move_left_3_1;
        Quaternion move_left_3_2 = Quaternion.Euler(L_Angle_List[5]*1.5f, 0, 0);
        left_third_2.rotation = move_left_3_2;
        Quaternion move_left_4_1 = Quaternion.Euler(L_Angle_List[6], 0, 0);
        left_fourth_1.rotation = move_left_4_1;
        Quaternion move_left_4_2 = Quaternion.Euler(L_Angle_List[7]*1.5f, 0, 0);
        left_fourth_2.rotation = move_left_4_2;
        Quaternion move_left_5_1 = Quaternion.Euler(L_Angle_List[8], 0, 0);
        left_fifth_1.rotation = move_left_5_1;
        Quaternion move_left_5_2 = Quaternion.Euler(L_Angle_List[9]*1.5f, 0, 0);
        left_fifth_2.rotation = move_left_5_2;            
    }
    void player1_AtkDef()
    {
        if(R_Position.x > 0.6 && R_Position.y < 0.2)
        {
            Vector3 moveRightHandDir = new Vector3(0, 0, 5);
            if(rightHand.position.z < 5.1)
            {
                rightHand.position += moveRightHandDir*Time.deltaTime*1.5f;
            }
        }
        else if(R_Position.x < 0.4 && R_Position.y < 0.2)
        {
            Vector3 moveRightHandDir = new Vector3(-2, 0, 5);
            if(rightHand.position.z < 5.1)
            {
                rightHand.position += moveRightHandDir*Time.deltaTime*1.5f;
            }
        }
        else if(L_Position.x > 0.6 && L_Position.y < 0.2)
        {
            Vector3 moveLeftHandDir = new Vector3(2, 0, 5);
            if(leftHand.position.z < 5.1)
            {
                leftHand.position += moveLeftHandDir*Time.deltaTime*1.5f;
            }
        }
        else if(L_Position.x < 0.4 && L_Position.y < 0.2)
        {
            Vector3 moveLeftHandDir = new Vector3(0, 0, 5);
            if(leftHand.position.z < 5.1)
            {
                leftHand.position += moveLeftHandDir*Time.deltaTime*1.5f;
            }
        }
        else
        {
            Vector3 moveRightToOriginal1 = new Vector3(0, 0, -5);
            Vector3 moveRightToOriginal2 = new Vector3(2, 0, -5);
            if(rightHand.position.z > -8.2)
            {
                if(rightHand.position.x < 3.61)
                {
                    rightHand.position += moveRightToOriginal2*Time.deltaTime*1.5f;
                }
                else
                {
                    rightHand.position += moveRightToOriginal1*Time.deltaTime*1.5f;
                }
            }
            Vector3 moveLeftToOriginal1 = new Vector3(0, 0, -5);
            Vector3 moveLeftToOriginal2 = new Vector3(-2, 0, -5);
            if(leftHand.position.z > -8.2)
            {
                if(leftHand.position.x > -3.6)
                {
                    leftHand.position += moveLeftToOriginal2*Time.deltaTime*1.5f;
                }
                else
                {
                    leftHand.position += moveLeftToOriginal1*Time.deltaTime*1.5f;
                }
            }
        }
    }

    void player2_AtkDef()                                          // 이제는 범위의 문제
    {
        if(R_Position.x > 0.6 && R_Position.y < 0.2)     
        {
            Vector3 moveRightHandDir = new Vector3(0, 0, -5);
            if(rightHand.position.z > -7)
            {
                rightHand.position += moveRightHandDir*Time.deltaTime*1.5f;
            }
        }
        else if(R_Position.x < 0.4 && R_Position.y < 0.2)
        {
            Vector3 moveRightHandDir = new Vector3(2, 0, -5);
            if(rightHand.position.z > -7)
            {
                rightHand.position += moveRightHandDir*Time.deltaTime*1.5f;
            }
        }
        else if(L_Position.x > 0.6 && L_Position.y < 0.2)
        {
            Vector3 moveLeftHandDir = new Vector3(-2, 0, -5);
            if(leftHand.position.z > -7)
            {
                leftHand.position += moveLeftHandDir*Time.deltaTime*1.5f;
            }
        }
        else if(L_Position.x < 0.4 && L_Position.y < 0.2)
        {
            Vector3 moveLeftHandDir = new Vector3(0, 0, -5);
            if(leftHand.position.z > -7)
            {
                leftHand.position += moveLeftHandDir*Time.deltaTime*1.5f;
            }
        }
        else
        {
            Vector3 moveRightToOriginal1 = new Vector3(0, 0, 5);
            Vector3 moveRightToOriginal2 = new Vector3(-2, 0, 5);
            if(rightHand.position.z < 8.2)
            {
                if(rightHand.position.x > -3.59)
                {
                    rightHand.position += moveRightToOriginal2*Time.deltaTime*1.5f;
                }
                else
                {
                    rightHand.position += moveRightToOriginal1*Time.deltaTime*1.5f;
                }
            }
            Vector3 moveLeftToOriginal1 = new Vector3(0, 0, 5);
            Vector3 moveLeftToOriginal2 = new Vector3(2, 0, 5);
            if(leftHand.position.z < 8.2)
            {
                if(leftHand.position.x < 3.59)
                {
                    leftHand.position += moveLeftToOriginal2*Time.deltaTime*1.5f;
                }
                else
                {
                    leftHand.position += moveLeftToOriginal1*Time.deltaTime*1.5f;
                }
            }
        }
    }


    void OnApplicationQuit()
    {
        //어플리케이션 종료 시 스레드 닫음
        receiveThread.Abort();
    }

/*
    void Update()
    {
        if(pv.IsMine)
        {
            GetInput();
            Move();
            Turn();
            //Attack();
        }
    }
*/
    
}
