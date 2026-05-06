/*
 ISC License

 Copyright (c) 2025, Autonomous Vehicle Systems Lab, University of Colorado at Boulder

 Permission to use, copy, modify, and/or distribute this software for any
 purpose with or without fee is hereby granted, provided that the above
 copyright notice and this permission notice appear in all copies.

 THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

 */

using System.Linq;
using UnityEngine;

/// <summary>
/// Static class providing methods to build and update reaction wheel display panels
/// </summary>
public static class ReactionWheelUtilities
{
    public static bool HUDShowSpeed = true;
    public static double[] MaxTorque;
    public static double[] MaxSpeed;
    public static bool MaxSpeedChange;
    public static bool MaxTorqueChange;

    private const double RAD_PER_SEC_TO_RPM = 60.0 / (2.0 * Mathf.PI);

    public static void InitializeMaxTorqueAndSpeedArrays()
    {
        //This is some setup work for the max speed/torque arrays that has to be done at start to correctly size the arrays for the spacecraft count
        int scCount = MessageList.FirstMessage.Spacecraft.Count;
        MaxTorque = new double[scCount];
        MaxSpeed = new double[scCount];
        for (int i = 0; i < scCount; i++)
        {
            MaxTorque[i] = 0.001;
            MaxSpeed[i] = 0.001;
            //Check for user set maximum values in the messages
            int rwCount = MessageList.FirstMessage.Spacecraft[i].ReactionWheels.Count;
            if (rwCount > 0)
            {
                for (int j = 0; j < rwCount; j++)
                {
                    double wheelMaxTorque = MessageList.FirstMessage.Spacecraft[i].ReactionWheels[j].MaxTorque;
                    if (wheelMaxTorque > MaxTorque[i])
                    {
                        MaxTorque[i] = wheelMaxTorque;
                    }

                    double wheelMaxSpeed = RAD_PER_SEC_TO_RPM *
                                           MessageList.FirstMessage.Spacecraft[i].ReactionWheels[j].MaxSpeed;
                    if (wheelMaxSpeed > MaxSpeed[i])
                    {
                        MaxSpeed[i] = wheelMaxSpeed;
                    }
                }
            }
        }
    }

    public static double[] GetReactionWheelSpeeds(int spacecraftIndex)
    {
        int wheelCount = MessageList.CurrentMessage.Spacecraft[spacecraftIndex].ReactionWheels.Count;
        double[] wheelSpeedArray = new double[wheelCount];
        for (int i = 0; i < wheelCount; i++)
        {
            //Read wheel speed and convert to rpm
            wheelSpeedArray[i] = RAD_PER_SEC_TO_RPM *
                                 MessageList.CurrentMessage.Spacecraft[spacecraftIndex].ReactionWheels[i].WheelSpeed;
            //Check to see if max speed should be increased
            if (MaxSpeed[spacecraftIndex] < wheelSpeedArray[i])
            {
                MaxSpeed[spacecraftIndex] = wheelSpeedArray[i];
                MaxSpeedChange = true;
            }
        }

        return wheelSpeedArray;
    }

    public static double[] GetReactionWheelTorques(int spacecraftIndex)
    {
        int wheelCount = MessageList.CurrentMessage.Spacecraft[spacecraftIndex].ReactionWheels.Count;
        double[] wheelTorqueArray = new double[wheelCount];
        for (int i = 0; i < wheelCount; i++)
        {
            wheelTorqueArray[i] = MessageList.CurrentMessage.Spacecraft[spacecraftIndex].ReactionWheels[i].WheelTorque;
            //Check to see if max torque should be increased
            if (MaxTorque[spacecraftIndex] < wheelTorqueArray[i])
            {
                MaxTorque[spacecraftIndex] = wheelTorqueArray[i];
                MaxTorqueChange = true;
            }
        }

        return wheelTorqueArray;
    }

    //The following methods are for retrieving only one wheel's data at a time (used primarily for the HUD display)
    public static double GetReactionWheelTorque(int spacecraftIndex, int wheelIndex)
    {
        double currentTorque = MessageList.CurrentMessage.Spacecraft[spacecraftIndex].ReactionWheels[wheelIndex]
            .WheelTorque;
        if (MaxTorque[spacecraftIndex] < currentTorque)
        {
            MaxTorque[spacecraftIndex] = currentTorque;
            MaxTorqueChange = true;
        }

        return currentTorque;
    }

    public static double GetReactionWheelSpeed(int spacecraftIndex, int wheelIndex)
    {
        double currentSpeed = RAD_PER_SEC_TO_RPM *
                              MessageList.CurrentMessage.Spacecraft[spacecraftIndex].ReactionWheels[wheelIndex]
                                  .WheelSpeed;
        //Check to see if max speed should be increased
        if (MaxSpeed[spacecraftIndex] < currentSpeed)
        {
            MaxSpeed[spacecraftIndex] = currentSpeed;
            MaxSpeedChange = true;
        }

        return currentSpeed;
    }

    public static Vector3 GetReactionWheelPosition(int spacecraftIndex, int wheelIndex)
    {
        Vector3 wheelPosition = OrbitVectorMath.ReturnVector3(OrbitVectorMath.TransformFromBSKCStoUnity(MessageList
            .FirstMessage.Spacecraft[spacecraftIndex].ReactionWheels[wheelIndex].Position.ToArray()));
        return wheelPosition;
    }

    public static Vector3 GetReactionWheelSpinAxis(int spacecraftIndex, int wheelIndex)
    {
        Vector3 wheelPosition = OrbitVectorMath.ReturnVector3(OrbitVectorMath.TransformFromBSKCStoUnity(MessageList
            .FirstMessage.Spacecraft[spacecraftIndex].ReactionWheels[wheelIndex].SpinAxisVector.ToArray()));
        return wheelPosition;
    }

    public static void ResetReactionWheelUtilities()
    {
        HUDShowSpeed = true;
        MaxSpeedChange = false;
        MaxTorqueChange = false;
        MaxTorque = new double[] { };
        MaxSpeed = new double[] { };
    }
}