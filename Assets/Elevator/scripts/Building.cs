using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;




public class Building : MonoBehaviour
{

    static ElevatorAcademy academy;
    public static Brain elevatorBrain;

    // Use this for initialization

    static GameObject resElevator;
    static GameObject resfloor;
    static float simulation_interval = 3f;

    List<Elevator> listElve = new List<Elevator>();
    List<Buildfloor> listFloor = new List<Buildfloor>();

    Elevator[,] callReqReserveCar;


    static int episodeTotalPassinger ;

    static GameObjPool s_GameObjPool;

    public ElevatorCtrlCenterAgent centerCtrlAgent;

    int currentPassinger;
    int restPassinger;
    int destPassinger;
    int addPassinger;

    public AnimationCurve simuPassinger;

    

    float simulattion_time = 0;

    float startTime = 0;

    int success = 0;
    int fail = 0;

    // Update is called once per frame
    public  void InitializeAgent()
    {

        if (s_GameObjPool == null)
            s_GameObjPool = new GameObjPool();

        if (resElevator == null)
            resElevator = (GameObject)Resources.Load("Elevator/elevator_unit");

        if (resfloor == null)
            resfloor = (GameObject)Resources.Load("Elevator/build_floor");


        if (academy == null)
            academy = FindObjectOfType<ElevatorAcademy>();
    }




    public  void CollectObservations(Agent a)
    {

        CollectObsFloor(a);

        for(int i=0; i< listElve.Count;++i)
        {
            listElve[i].CollectObsElevator(a);
        }


    }


    public void CollectObsFloor(Agent a)
    {
        for (int i = 0; i < ElevatorAcademy.floors; ++i)
        {
            var f = GetFloor(i);
            a.AddVectorObs(f.GetPassingerCount());
            a.AddVectorObs(f.IsCallRequest(MOVE_STATE.Down));
            a.AddVectorObs(f.IsCallRequest(MOVE_STATE.Up));
        }
    }


    // to be implemented by the developer
    public void AgentAction(float[] vectorAction, string textAction,out float reward)
    {

        reward = 0;

        int el = (int)vectorAction[0];

        if (el < 0 || el >= listElve.Count)
            return;


        float[] elAction = new float[vectorAction.Length - 1];
        System.Array.Copy(vectorAction, elAction, elAction.Length);
        listElve[el].AgentAction(elAction, textAction,out reward);
    }

    public void InitEnv()
    {
        if (s_GameObjPool == null)
            s_GameObjPool = new GameObjPool();

        if (resElevator == null)
            resElevator = (GameObject)Resources.Load("Elevator/elevator_unit");

        if(resfloor == null )
            resfloor = (GameObject)Resources.Load("Elevator/build_floor");

        if (academy ==null)
            academy = FindObjectOfType<ElevatorAcademy>();


        callReqReserveCar = new Elevator[ElevatorAcademy.floors, 2];

        ElevatorPassenger.InitPooler();


        centerCtrlAgent = gameObject.GetComponent<ElevatorCtrlCenterAgent>();



        if (centerCtrlAgent != null)
        {
            resElevator.GetComponent<Elevator>().enabled = false;
        }


        //if (centerCtrlAgent == null)
        {

            var brainObj = academy.gameObject.transform.Find("ElevatorctrlBrain");

            if(brainObj)
                elevatorBrain = brainObj.GetComponent<MLAgents.Brain>();
        }

       


        restPassinger = ElevatorAcademy.passinger;
        destPassinger = 0;
        simulattion_time = 0;
        addPassinger = 0;


        int dist = 4;
        int rest = ElevatorAcademy.elevatorCount% 2;
        int mok = ElevatorAcademy.elevatorCount / 2;

        Vector3 startPos = transform.position;
        if (rest<0.5f)
        {
            mok -= 1;
            startPos = transform.position - (Vector3.right * dist * mok)- (Vector3.right * (dist/2)); 
        }
        else
        {
            startPos = transform.position - (Vector3.right * dist * mok);
        }

        startPos += Vector3.back;


        for (int i = 0; i< ElevatorAcademy.elevatorCount; ++i)
        {

            if(i< listElve.Count)
            {
                listElve[i].Init();
                continue;
            }

            GameObject ele = (GameObject)Instantiate(resElevator, this.transform);

            ele.transform.position = startPos + (Vector3.right * dist * i);
            var el = ele.GetComponent<Elevator>();
            listElve.Add(el);
            el.InitFloor(i, ElevatorAcademy.floors);

            
            var agent = ele.GetComponent<Elevator>();
            agent.enabled = false;
            el.agent = centerCtrlAgent;
            

        }

        for (int i = 0; i < ElevatorAcademy.floors; ++i)
        {

            if (i < listFloor.Count)
            {
                listFloor[i].Init();
                continue;
            }


            GameObject fl = (GameObject)Instantiate(resfloor, this.transform);
            fl.transform.position = transform.position + (Vector3.up * ElevatorAcademy.height * i);
            fl.GetComponent<Buildfloor>().SetFloor(i,this);
            listFloor.Add(fl.GetComponent<Buildfloor>());

        }

        startTime = Time.fixedTime;


    }

  

    public void UpdateEnv()
    {

        SimulationFloorPassinger();
        SimulationEnterElevator();

        UpdatePos();

        if (IsDone())
        {
          
          

            success += 1;

            academy.Done();
            return;
        }


        if (academy.GetStepCount()+1== academy.maxSteps)
        {
            foreach (var el in listElve)
            {
                el.SetReward(-10f);
                fail += 1;
                academy.Done();
            }

        }



    }

    public void UpdatePos()
    {
        foreach (var e in listElve)
        {
            e.UpdateAction();
        }
    }


    public void SimulationFloorPassinger()
    {
        if (simulattion_time > Time.fixedTime)
            return;

        if (currentPassinger > episodeTotalPassinger * 0.3)
            return;

        int newPassinger = Random.Range(0, restPassinger+1);

        int[] floorPassinger = new int[listFloor.Count];



        floorPassinger[0] = Random.Range(0, (int)(newPassinger*0.8f));

        int rest = newPassinger - floorPassinger[0];


        while(rest>0)
        {
            int floor = Random.Range(1, listFloor.Count);
            int passinger = Random.Range(1, rest + 1);
            rest -= passinger;
            floorPassinger[floor] = passinger;
        }

       

        for (int i=0; i<listFloor.Count;++i)
        {
            if (floorPassinger[i] > 0)
            {
                listFloor[i].GetComponent<Buildfloor>().AddPassinger(floorPassinger[i]);
                addPassinger += floorPassinger[i];
                restPassinger -= floorPassinger[i];
            }

           
        }

        simulattion_time = Time.fixedTime + 5f;
    }


    public void SimulationEnterElevator()
    {

        for (int i = 0; i < listFloor.Count; ++i)
        {
            foreach(var el in listElve)
            {
                listFloor[i].EnterElevator(el);
            }
        }

    }


    public void CallRequest(int floor, MOVE_STATE dir)
    {
      
        switch(centerCtrlAgent.brain.brainType)
        {
            case BrainType.Player:
            case BrainType.Heuristic:
                SearchRuleBaseNearstElevator(floor, dir);
                break;

            case BrainType.External:
            case BrainType.Internal:
                break;


            default:
                break;
        }
        
    }

    public void ProcRuleBaseCallRequest()
    {
        foreach(var f in listFloor)
        {
            for(int i = (int)MOVE_STATE.Down;i< (int)MOVE_STATE.end;++i)
            {
                SearchRuleBaseNearstElevator(f.GetFloorNo(), (MOVE_STATE)i);
            }
        }

    }

    public int SearchRuleBaseNearstElevator(int floor,MOVE_STATE dir)
    {

        float min = 1000000f;
        float dist = 0;
        int buttonDir = 0;

        if(dir != MOVE_STATE.Down)
        {
            buttonDir = 1;
        }
       

        foreach(var e in listElve)
        {
            dist = e.GetFloorDist(floor, dir);

            if (dist < min)
            {
                callReqReserveCar[floor, buttonDir] = e;
                min = dist;
            }
        }

        if (callReqReserveCar[floor, buttonDir] != null)
        {
            var el = callReqReserveCar[floor, buttonDir];
            el.SetCallRequest(floor, dir);
            return el.GetNo();
        }

        return -1;
    }

    public Buildfloor GetFloor(int floor)
    {

        return listFloor[floor];

    }

    public MOVE_STATE GetAction(int floor,Elevator el)
    {
        return MOVE_STATE.Stop;
    }

    public void AddDestPassinger(int add =1)
    {
        destPassinger += add;
    }

    public bool IsNoCallRequest()
    {
        foreach (var f in listFloor)
        {
            if (!f.IsNoCall())
                return false;
        }

        return true;
    }

    public bool IsDone()
    {
        if (restPassinger > 0)
            return false;


        foreach (var el in listElve)
        {
            if (el.listPassinger.Count > 0)
                return false;
        }

        return IsNoCallRequest();

    }

   

    public int GetRestPassinger()
    {
        return ElevatorAcademy.passinger-destPassinger;
    }

    private void OnGUI()
    {
        GUI.TextArea(new Rect(10, 10, 250,25),
            string.Format("EP:{0}-Step:{1} Suc:{2} Fail:{3}", success+fail, academy.GetStepCount(), success,fail));

        GUI.TextArea(new Rect(10, 40, 250, 25),
          string.Format("Passinger:{0}/{1}", destPassinger, ElevatorAcademy.passinger));
    }



}
