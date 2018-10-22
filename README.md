
# Elevator-RL 설정

1. TensorflowSharp 설치
https://drive.google.com/drive/folders/1A4twqX0CY5uplxapQ4-c26VDxFzw6SC9
에서 다운받아서 
\Assets\ML-Agents\Plugins 로 복사

2. UnityML(UnityML v0.5) 설치
 https://github.com/Unity-Technologies/ml-agents
 

#유니티에서 환경 로드 하기
 Elevator 환경 Scene 파일: \Assets\Elevator\elevator_rl.unity

# Unity ML 기본 네트웍으로 학습 시키기
 1.ml-agents/config/trainer_config.yaml 열고 아래 내용추가

ElevatorBrain:
    use_curiosity: true
    curiosity_strength: 0.01
    curiosity_enc_size: 256
    batch_size: 32
    normalize: false
    num_layers: 1
    hidden_units: 256
    beta: 5.0e-3
    gamma: 0.9
    buffer_size: 256
    max_steps: 1.0e6
    summary_freq: 2000
    time_horizon: 32
    num_epoch: 3

 2.콘솔창 열고 ml-agents 폴더로 이동후 아래 내용 입력후 실행
    mlagents-learn config/trainer_config.yaml --train
 
 3.콘솔 실행후 Unity에디터에서 play버튼으로 실행

 자세한 내용은 참고 
 https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Readme.md

#Elevator 환경에 대한 간략 설명
 1.하나의 Agent가 여러 Elevator를 Control 하는 구조임이고 10층에 4개의 Elevator로 구성되어 있음
 2.Action은 ActionVector의 [0]은 Elevator 인덱스 , [1]은 Elevator 액션(멈춤,위,아래)
 3.Obervation 정보
   각층의 승객수와 버튼 누른 상태 (float:30개)
   각 Elevator의 상태,이동방향,탑승승객수,내부승객 도착층에 대한 버튼 정보(수만틈 반복)
   자세한건
   \Assets\Elevator\scripts\Building.cs 의 Building.CollectObservations 함수 참고
