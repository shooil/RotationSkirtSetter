using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(SkirtEdit))]
public class SkirtEditUI:Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SkirtEdit input = target as SkirtEdit;
        if(GUILayout.Button("Setup")){this.SetupSkirt(input.RootBone);}
        if(GUILayout.Button("Setup new with Obj")){this.SetupSkirtNewObj(input.RootBone);}
    }

    public void SetupSkirt(GameObject rootBone){
        var skirt = new Skirt(rootBone);
        skirt.SetConstraint();
        skirt.SetPhysBone();
        Debug.Log("Completed Setting up Skirt");
    }

    public void SetupSkirtNewObj(GameObject rootBone){
        var skirt = new Skirt(rootBone);
        skirt.SetNewRoot();
        skirt.SetConstraint();
        skirt.SetPhysBone();
        Debug.Log("Completed Setting up Skirt with new root bone");
    }
}