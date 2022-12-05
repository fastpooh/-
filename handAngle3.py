import mediapipe as mp
import cv2
import numpy as np
import math

import socket

mp_drawing = mp.solutions.drawing_utils
mp_hands = mp.solutions.hands

class Finger:   #손가락에 대한 클래스
    fin_list = []
    vec1, vec2, vec3, vec4 = None, None, None, None  
    
    #손가락 마디의 좌표를 받아 둘을 빼서 벡터를 만듬
    def __init__(self, p1, p2, p3, p4, p5):  
        self.vec1 = np.array([p1.x - p2.x, p1.y - p2.y, p1.z - p2.z])
        self.vec2 = np.array([p2.x - p3.x, p2.y - p3.y, p2.z - p3.z])
        self.vec3 = np.array([p3.x - p4.x, p3.y - p4.y, p3.z - p4.z])
        self.vec4 = np.array([p4.x - p5.x, p4.y - p5.y, p4.z - p5.z])
        
    #fin벡터와 base벡터를 내적해서 손가락이 펴졌는지 판단
    def dot_calc(self):
        sol = np.dot(self.vec1, self.vec4)
        a,b = 1,0
        
        if sol > 0:
            return a
        else :
            return b
        
    def angle_calc(self):     
        #크기: 내적을 통해 구함
        size1 = math.sqrt(np.dot(self.vec2,self.vec2))
        size2 = math.sqrt(np.dot(self.vec3,self.vec3))
        size3 = math.sqrt(np.dot(self.vec4,self.vec4))
        
        #내적
        dot_result1 = np.dot(self.vec2,self.vec3)
        dot_result2 = np.dot(self.vec3,self.vec4)
        
        #각도 :내적의 결과에서 크기를 나누어서 cos값을 구함
        cos1 = dot_result1/size1/size2
        cos2 = dot_result2/size2/size3
        
        #코사인 역함수
        angle1 = math.acos(cos1)
        angle2 = math.acos(cos2)
        
        degree1 = round(math.degrees(angle1),4)
        degree2 = round(math.degrees(angle2),4)
        
        angle_list = [degree1, degree2]
        
        return angle_list


class Thumb(Finger): #Finger 클래스를 상속받음
    #이 부분은 추후에 수정할 것임!!!!!!!!!!!!!!!!!!!!!!!!!!!
    def angle_calc(self):   
        #크기
        size1 = math.sqrt(np.dot(self.vec1,self.vec1))
        size2 = math.sqrt(np.dot(self.vec2,self.vec2))
        size3 = math.sqrt(np.dot(self.vec3,self.vec3))
        
        #내적
        dot_result1 = np.dot(self.vec1,self.vec2)
        dot_result2 = np.dot(self.vec2,self.vec3)
        
        #각도
        cos1 = dot_result1/size1/size2
        cos2 = dot_result2/size2/size3
        
        angle1 = math.acos(cos1)
        angle2 = math.acos(cos2)
        
        degree1 = round(math.degrees(angle1),2)
        degree2 = round(math.degrees(angle2),2)
        
        angle_list = [degree1, degree2]
        
        return angle_list
    #fin벡터와 base벡터를 내적해서 손가락이 펴졌는지 판단
    def dot_calc(self):
        sol = np.dot(self.vec1, self.vec4)
        a,b = 1,0
        
        if sol > 0.00154:
            return a
        else :
            return b


def count_finger(hand): #손가락의 펴진 개수를 판단하는 함수
    
    tmp = 0
    angle_list = []
    
    #각 손가락에 대한 객체를 만든다.
    finger1 = Finger(hand.landmark[8],hand.landmark[7],hand.landmark[6], hand.landmark[5], hand.landmark[0])
    finger2 = Finger(hand.landmark[12],hand.landmark[11],hand.landmark[10], hand.landmark[9], hand.landmark[0])
    finger3 = Finger(hand.landmark[16],hand.landmark[15],hand.landmark[14], hand.landmark[13], hand.landmark[0])
    finger4 = Finger(hand.landmark[20],hand.landmark[19],hand.landmark[18], hand.landmark[17], hand.landmark[0])
    finger5 = Thumb(hand.landmark[4], hand.landmark[3],hand.landmark[2],hand.landmark[1], hand.landmark[0])
    
    #각도에 대한 값을 리스트에 추가
    angle_list.extend(finger5.angle_calc())
    angle_list.extend(finger1.angle_calc())
    angle_list.extend(finger2.angle_calc())
    angle_list.extend(finger3.angle_calc())
    angle_list.extend(finger4.angle_calc())
    
    tmp = finger1.dot_calc() + finger2.dot_calc() + finger3.dot_calc() + finger4.dot_calc() + finger5.dot_calc() 
    
    #손가락의 펴진 개수를 리스트에 추가
    angle_list.append(tmp)

    del finger1, finger2, finger3, finger4, finger5

    return angle_list


cap = cv2.VideoCapture(0)

r_angle_list = []
l_angle_list = []
r_cnt = 0
l_cnt = 0

##########데이터 전송할 때 쓰이는 String##########
string_r_angle_list = ""
string_r_angle_list_backup = ""
string_l_angle_list = ""
string_l_angle_list_backup = ""
string_list = ""
##################################################

#######################Socket 서버 관련#######################
host, port = "127.0.0.1", 25001
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect((host, port))
##############################################################

with mp_hands.Hands(min_detection_confidence=0.8, min_tracking_confidence=0.5) as hands:
    while cap.isOpened():
        ret, fram = cap.read()
        
        #BGR to RGB -> mediapipe는 RGB에서 작동하기 때문에 RGB로 바꿔줘야함
        image = cv2.cvtColor(fram, cv2.COLOR_BGR2RGB)
        
        #좌우 반전
        image = cv2.flip(image,1)
        
        # set flag
        image.flags.writeable = False
        
        # Detection, mediapipe로 손에 대한 정보를 구함
        results = hands.process(image)
        
        # Set flag to true
        image.flags.writeable = True
        
        # RGB to BGR
        image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
        
        #Detections
        if results.multi_hand_landmarks:   #손을 감지할 때       
            for hand, check in zip(results.multi_hand_landmarks, results.multi_handedness):               
                if check.classification[0].index == 1:  #오른손
                    #여기서 r_angle_list에 오른손 각도와 손가락 개수를 입력해준다
                    r_angle_list = count_finger(hand)
                    r_cnt = r_angle_list[10] #화면 출력을 위해 리스트에서 손가락의 개수를 받아옴
                    r_angle_list.append(round(hand.landmark[8].x,3))
                    r_angle_list.append(round(hand.landmark[8].y,3))
                    r_angle_list.append(round(hand.landmark[8].z,3))
                else :   #왼손
                    #여기서 l_list에 왼손 각도와 손가락 개수를 입력해준다.
                    for i in count_finger(hand): #각도가 -1을 곱해줌
                        l_angle_list.append(-1*i) 
                    l_angle_list[10] = l_angle_list[10] * (-1) #손가락의 개수는 다시 -1을 곱해줌
                    l_cnt = l_angle_list[10]
                    l_angle_list.append(round(hand.landmark[8].x,3)) #손가락 좌표를 l_point_list에 추가
                    l_angle_list.append(round(hand.landmark[8].y,3))
                    l_angle_list.append(round(hand.landmark[8].z,3))
                    
                    
        #####################여기서 r_list, l_list를 Unity로 보내야 됨#########################
        if string_r_angle_list_backup == "" and string_l_angle_list_backup == "":
            string_r_angle_list = ','.join(str(i) for i in [0 for i in range(12)]) + ',0.7,0'
            string_l_angle_list = ','.join(str(i) for i in [0 for i in range(12)]) + ',0.7,0'
        else:
            if r_angle_list == []:
                string_r_angle_list = string_r_angle_list_backup
            else:
                string_r_angle_list = ','.join(str(i) for i in r_angle_list)
            if l_angle_list == []:
                string_l_angle_list = string_l_angle_list_backup        
            else:
                string_l_angle_list = ','.join(str(i) for i in l_angle_list)
        
        string_r_angle_list_backup = string_r_angle_list
        string_l_angle_list_backup = string_l_angle_list
        string_list = ( string_r_angle_list + ',' + string_l_angle_list)
        print(string_list)
        sock.sendall(string_list.encode("utf-8"))   
        #######################################################################################
            
            
        #####화면에 출력하는 용도, 나중에 삭제해도됨##########
        cv2.putText(
            image, text='left =%d, right =%d' % (l_cnt,r_cnt), org=(10,30),
            fontFace=cv2.FONT_HERSHEY_SIMPLEX, fontScale=1,
            color=255, thickness=3
            )
            
        # 리스트 초기화 -> 리스트를 Unity로 보내고 다시 새로운 좌표를 받아야하기 때문
        r_angle_list.clear()
        l_angle_list.clear()
                
        cv2.imshow('Hand Tracking', image)
    
        if cv2.waitKey(10) & 0xFF == ord('q'):
            break

cap.release()
cv2.destroyAllWindows()
socket.close()