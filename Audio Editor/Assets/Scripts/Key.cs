using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour {

    int index;
    AudioInterface audioInterface;
    Material material;

    public void Init (int index, AudioInterface audioInterface) {
        this.index = index;
        this.audioInterface = audioInterface;
        material = GetComponent<SpriteRenderer> ().material;
    }

    public void OnPlay () {
        float v = .6f;
        material.color = new Color (v, v, v, 1);
    }

    public void OnEnd () {
        material.color = Color.white;
    }

    void OnMouseDown () {
        OnPlay ();
        audioInterface.PlayNote (index);
    }

    void OnMouseUp () {
        OnEnd ();
        audioInterface.EndNote ();
    }

}