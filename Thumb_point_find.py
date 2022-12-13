#!/usr/bin/env python
# coding: utf-8

# In[1]:


import mediapipe as mp
import cv2
import numpy as np
import math
from sklearn.datasets import load_iris
import pandas as pd
import numpy as np

# visualization packages
import matplotlib.pyplot as plt
import seaborn as sns

# Support Vector Machine
from sklearn import svm
from sklearn.metrics import accuracy_score
from sklearn import model_selection


# In[2]:


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
        
        return round(sol,6)
        
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
        
        degree1 = round(math.degrees(angle1),2)
        degree2 = round(math.degrees(angle2),2)
        
        angle_list = [degree1, degree2]
        
        return angle_list

class Thumb(Finger): #Finger 클래스를 상속받음
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

def count_finger(hand): #손가락의 펴진 개수를 판단하는 함수
    
    tmp = 0
    
    #엄지 손가락에 대한 객체 생성
    finger5 = Thumb(hand.landmark[4], hand.landmark[3],hand.landmark[2],hand.landmark[1], hand.landmark[0])
    #내적값을 받아옴
    tmp = finger5.dot_calc() 
    

    del finger5
    #내적값을 return
    return tmp


# In[36]:


cap = cv2.VideoCapture(0)
data_list1 = []
cnt = 0

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
                cnt = cnt + 1
                if check.classification[0].index == 1:  #오른손
                    data_list1.append([count_finger(hand)])
                else :   #왼손
                    data_list1.append([count_finger(hand)])
                
        cv2.imshow('Hand Tracking', image)
    
        if cv2.waitKey(10) & 0xFF == ord('q'):
            break
        if cnt > 1100:#약 1100개의 데이터가 모임
            break
        
cap.release()
cv2.destroyAllWindows()


# In[33]:


one_tmp = []
one_data = []
zero_tmp = []
zero_data = []
model_data = []
zero_all = []
result_data = []


# In[37]:


one_tmp.extend(data_list1)
#엄지의 핀 상태와 접은 상태에 대한 데이터 1000개씩 수집
one_data = one_tmp[:1000]


# In[35]:


zero_tmp.extend(data_list1)
zero_data = zero_tmp[:1000]


# In[38]:


zero_all = [[0]]*2000


# In[39]:


result_data = [[1]]*1000
result_data.extend([[0]]*1000)


# In[43]:


model_data.extend(one_data)
model_data.extend(zero_data)


# In[47]:


one_data1 = np.array(one_data, dtype=list)
zero_data1 = np.array(zero_data, dtype=list)
zero_all1 = np.array(zero_all, dtype=list)
model_data1 = np.array(model_data, dtype=list)
result_data1 = np.array(result_data, dtype=list)


# In[48]:


#여기서 내적값을 data로 column은 dot_result로 설정
df = pd.DataFrame(data=model_data1, columns=['dot'])
df['zero'] = zero_all1
df['result'] = result_data1
df['result'] = df['result'].map({0:"zero", 1:"one"})
df = df.sample(frac=1).reset_index(drop=True) # row 전체를 shuffle


# In[49]:


#x는 1,2번째 y는 마지막 
x_data = df.iloc[:, :-1]
y_data = df.iloc[:, [-1]]


# In[50]:


#data를 test와 train용도로 구분
x_train = x_data.iloc[:1600]
x_test = x_data.iloc[1600:]
y_train = y_data.iloc[:1600]
y_test = y_data.iloc[1600:]


# In[51]:


#딥러닝 모델 생성
C = 1.0
models = svm.LinearSVC(C=C, max_iter=10000)

models.fit(x_train, y_train.values.ravel()) #train
y_pred = models.predict(x_test) #test
print("Linear SVC with linear kernel", accuracy_score(y_test, y_pred)) #답과 예측 결과를 비교


# In[52]:


lr = 0
learning_rate = 0.00001
#판단기준을 설정
for i in range(0,3000):
    lr = lr + learning_rate
    if 'one' == models.predict([[lr,0]])[0] :
        print(lr)
        break





