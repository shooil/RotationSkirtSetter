using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Animations;
using VRC.SDK3.Dynamics.PhysBone.Components;

// Not used yet
public class PBTemplate{
    public static VRCPhysBone SkirtTemplate1(VRCPhysBone script){
        script.pull = 0.1f;
        script.spring = 0.2f;
        script.immobile = 0.75f;
        script.limitType = VRC.Dynamics.VRCPhysBoneBase.LimitType.Hinge;
        script.maxAngleX = 0.5f;
        script.gravity = 0.5f;
        return script;
    }

    public static VRCPhysBone SkirtTemplate2(VRCPhysBone script){
        return script;
    }
}