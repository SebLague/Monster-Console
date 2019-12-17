using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour {
    public int keyIndex;
    public SpriteRenderer noteSprite;
    public Color selectedCol;
    Color defaultCol;

    void Awake () {
        defaultCol = noteSprite.color;
    }

    public void Select () {
        noteSprite.color = selectedCol;
    }

    public void Deselect () {
        noteSprite.color = defaultCol;
    }

    public float Duration {
        get {
            return transform.localScale.x;
        }
    }

    public float StartTime {
        get {
            return transform.position.x;
        }
    }

    public float EndTime {
        get {
            return StartTime + Duration;
        }
    }
}