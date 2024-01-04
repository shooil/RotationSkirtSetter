using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Animations;
using VRC.SDK3.Dynamics.PhysBone.Components;
using System.IO.Ports;
using System.Numerics;

// Input class
public class SkirtRotateInput{
    public GameObject rootbone;
    public string structure;
    public float weightAll;
}

// All of the script will be hold by each bone of skirt bone under the root bone
// So skirt class responsible to execute method in skirt bone class
// skirt class also responsible to execute any action use multiple bones info
public class Skirt{
    public List<SkirtBone> bones;
    
    public Skirt(GameObject rootBone){
        this.bones = new List<SkirtBone>{};
        // Get All of bone under root bone
        for(var i=0;i<rootBone.transform.childCount;i++){
            var skirtBone = rootBone.transform.GetChild(i).gameObject;
            var skirt = new SkirtBone(skirtBone);
            this.bones.Add(skirt);
        }
        // Sort bone list by its angle from root bone
        // Needed to get information which bone is next to each other
        this.bones.Sort((a,b) => ( a.GetAngle(rootBone) - b.GetAngle(rootBone))>=0?1:-1);
    }

    // Set up rotation constraint script
    public void SetConstraint(){
        for(var i=0;i<this.bones.Count;i++){
            GameObject previous,next;
            // Find target's previous bone from sorted bone list
            // If ref bone is first element of bone list, select last element from bone list.
            if(i-1<0){
                previous = this.bones[this.bones.Count-1].GetSkirtBone();
            }else{
                previous = this.bones[i-1].GetSkirtBone();
            }
            
            // Find target's next bone from sorted bone list
            // If ref bone is last element of bone list, select first element from bone list.
            if(i+1>this.bones.Count-1){
                next = this.bones[0].GetSkirtBone();
            }else{
                next = this.bones[i+1].GetSkirtBone();
            }
            this.bones[i].AddConstraint(new List<GameObject>{previous,next});
        }
    }

    // Set up phsbone script
    public void SetPhysBone(){
        for(var i=0;i<this.bones.Count;i++){
            this.bones[i].AddPhysBone();
        }
    }

    public void SetNewRoot(){
        for(var i=0;i<this.bones.Count;i++){
            this.bones[i].AddSkirtRotateBase();
        }
    }
}

// SkirtBone class
public class SkirtBone {
    public GameObject bone;

    public SkirtBone(GameObject skirtbone){
        this.bone = skirtbone;
    }

    // Get Angle to root bone in xz plane
    public double GetAngle(GameObject root){
        var bonePos = this.bone.transform.localPosition;
        var refPos = root.transform.localPosition;
        var x = bonePos.x - refPos.x;
        var z = bonePos.z - refPos.z;
        // convert theta to degree easy to understand for debug
        // normal ArcTan func only work between -pi/2 ~ pi/2, only cover pi 
        // ArcTan2 function work for -pi ~ pi, cover 2pi
        var theta = Mathf.Atan2(z,x) * 180.0/Math.PI;
        return theta;
    }

    // Attach rotation constraint to the skirt bone
    // Set bones in list as constraint source
    public void AddConstraint(List<GameObject> bones){
        var rotate = this.bone.AddComponent<RotationConstraint>() as RotationConstraint;
        rotate.weight = 0.5f;
        foreach(var bone in bones){
            var src = new ConstraintSource();
            src.sourceTransform = bone.transform;
            src.weight = 0.5f;
            rotate.AddSource(src);
        }
        rotate.constraintActive = true;
    }

    // Get child bone to attach physbone script
    public GameObject GetSkirtBone(){
        if(this.bone.transform.childCount == 1){
            // If skirt bone have only one child bone return child
            return this.bone.transform.GetChild(0).gameObject;
        }else if(this.bone.transform.childCount == 0){
            // If skirt haven't child bone, this case means this skirt setup by parent constraint
            // Search which constrainted by and return that bone
            foreach(var script in this.bone.transform.parent.gameObject.GetComponentsInChildren<ParentConstraint>()){
                if(this.bone == script.GetSource(0).sourceTransform.gameObject){
                    return script.gameObject;
                }else{
                    continue;
                }
            }
        }
        // Default case
        return this.bone.transform.GetChild(0).gameObject;
    }

    public UnityEngine.Vector3 GetSkirtVector(){
        var vec = this.bone.transform.position;
        var vecChild = this.GetSkirtBone().transform.position;
        var norm = (vec - vecChild).normalized;
        return norm;
    }

    // Attach Physbone script
    public void AddPhysBone(){
        var bone = this.GetSkirtBone();
        var physbone = bone.AddComponent<VRCPhysBone>() as VRCPhysBone;
        physbone.pull = 0.1f;
        physbone.spring = 0.2f;
        physbone.immobile = 0.75f;
        physbone.limitType = VRC.Dynamics.VRCPhysBoneBase.LimitType.Hinge;
        physbone.maxAngleX = 90.0f;
        physbone.gravity = 0.5f;
        physbone.radius = 0.03f;
    }

    // Attach parent bone without weight
    // This method support to skirt structure that made for normal skirt setup
    public void AddSkirtRotateBase(){
        // Create new skirt bone object under root bone
        // position should be a little upper from old skirt bone
        var skirtroot = new GameObject();
        skirtroot.transform.parent = this.bone.transform.parent;
        skirtroot.transform.name = this.bone.transform.name + "_root";
        skirtroot.transform.localPosition = this.bone.transform.localPosition + this.GetSkirtVector()*0.01f;

        // Add parentconstraint to skirt bone follow new bone object
        var script = this.bone.AddComponent<ParentConstraint>();
        var src = new ConstraintSource();
        src.sourceTransform = skirtroot.transform;
        src.weight = 1.0f;
        script.AddSource(src);
        script.constraintActive = true;

        // Change skirt bone
        this.bone = skirtroot;
    }
}