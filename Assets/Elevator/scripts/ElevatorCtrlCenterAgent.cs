using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
public class ElevatorCtrlCenterAgent : Agent
{

    Building building;

    MOVE_STATE recv_action;

    

    public override void InitializeAgent()
    {
        building = gameObject.GetComponent<Building>();
        building.InitializeAgent();
    }




 

    public override void CollectObservations()
    {


        building.CollectObservations(this);

    }



    // to be implemented by the developer
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        float reward = 0;
        building.AgentAction(vectorAction, textAction, out reward);
        AddReward(reward);

    }

}
