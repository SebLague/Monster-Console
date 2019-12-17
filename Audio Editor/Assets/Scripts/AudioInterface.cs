using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioInterface : MonoBehaviour {
    // Notes:
    // Shift + R to toggle recording mode

    enum EditState { None, Moving, Scaling }

    [Header ("Behaviour")]
    public float playbackSpeed = 1;
    public int topNote;
    public float panSpeed = 3;
    public KeyCode[] keyboard;
    bool recording;

    [Header ("Appearance")]
    float keyWidth = 2;
    public int numKeys;
    public float spacing = .01f;

    [Header ("References")]
    public Song song;
    public Key keyPrefab;
    public Note notePrefab;
    public Transform playhead;

    bool remap = true;

    // Internal
    float recordingKeyStartTime;
    float recordingStartTime;
    Audio audioPlayer;
    Camera cam;

    Key[] keys;
    Note activeNote;
    EditState currentEditState;
    List<Note> notes;
    float dragOffset;

    Track activeTrack;

    void Start () {
        cam = Camera.main;
        audioPlayer = FindObjectOfType<Audio> ();

        float screenHeight = cam.orthographicSize * 2;
        float screenWidth = screenHeight * cam.aspect;
        float keyHeight = screenHeight / numKeys;

        keys = new Key[numKeys];
        for (int i = 0; i < numKeys; i++) {
            Key key = Instantiate (keyPrefab);
            key.Init (i, this);
            key.transform.localScale = new Vector3 (keyWidth, keyHeight - spacing, key.transform.localScale.z);
            key.transform.position = Vector3.left * (screenWidth - keyWidth) / 2 + Vector3.up * (screenHeight / 2 - keyHeight * (i + .5f));
            keys[i] = key;
            key.transform.parent = cam.transform;
        }

        notes = new List<Note> ();
        playhead.gameObject.SetActive (false);
    }

    void Update () {

        for (int i = 0; i < keyboard.Length; i++) {
            int keyI = keyboard.Length - 1 - i;
            KeyCode keycode = keyboard[i];
            if (Input.GetKeyDown (keycode)) {
                recordingKeyStartTime = Time.time;
                keys[keys.Length - 1 - i].OnPlay ();
                PlayNote (keyI);
            }
            if (Input.GetKeyUp (keycode)) {
                EndNote ();
                keys[keys.Length - 1 - i].OnEnd ();
                if (recording && !Input.GetKey (KeyCode.LeftShift)) {
                    float duration = Time.time - recordingKeyStartTime;
                    Note note = Instantiate (notePrefab);
                    note.keyIndex = keyI;
                    note.transform.position = new Vector3 (recordingKeyStartTime - recordingStartTime, keys[keyI].transform.position.y, 0);
                    note.transform.localScale = new Vector3 (duration, keys[keyI].transform.localScale.y, 1);
                    notes.Add (note);

                }
            }
        }

        Vector2 mousePos = cam.ScreenToWorldPoint (Input.mousePosition);
        float screenHeight = cam.orthographicSize * 2;
        float screenWidth = screenHeight * cam.aspect;

        if (Input.GetKeyUp (KeyCode.R) && Input.GetKey (KeyCode.LeftShift)) {
            recording = !recording;
            if (recording) {
                Debug.Log ("Start Recording");
                recordingStartTime = Time.time - ((notes.Count > 0) ? notes[notes.Count - 1].EndTime : 0);
            } else {
                Debug.Log ("End Recording");
                EndNote ();
            }
        }

        // Cam movement
        if (Input.GetMouseButton (2)) {
            float dx = -Input.GetAxisRaw ("Mouse X");
            cam.transform.position = new Vector3 (cam.transform.position.x + dx * panSpeed * Time.deltaTime, 0, -10);
        }

        // Handle track playing
        if (Input.GetKeyDown (KeyCode.Space)) {
            if (activeTrack != null && activeTrack.playing) {
                audioPlayer.frequency = 0;
                playhead.gameObject.SetActive (false);
                activeTrack.playing = false;
            } else if (notes.Count > 0) {
                Debug.Log ("Play");
                playhead.gameObject.SetActive (true);
                activeTrack = GenerateTrack ();
                activeTrack.playing = true;
            }
        }

        if (activeTrack != null && activeTrack.playing) {
            playhead.position = new Vector3 (activeTrack.currentPlayTime + notes[0].transform.position.x, 0, 0);

            float startTime = activeTrack.startTimes[activeTrack.playbackIndex];
            float endTime = activeTrack.startTimes[activeTrack.playbackIndex] + activeTrack.durations[activeTrack.playbackIndex];

            if (activeTrack.currentPlayTime >= startTime && activeTrack.currentPlayTime < endTime) {
                audioPlayer.frequency = activeTrack.frequencies[activeTrack.playbackIndex];
            } else if (activeTrack.currentPlayTime >= endTime) {
                audioPlayer.frequency = 0;
                activeTrack.playbackIndex++;
                if (activeTrack.playbackIndex >= activeTrack.numNotes) {
                    activeTrack.playing = false;
                    playhead.gameObject.SetActive (false);
                }
            }
            activeTrack.currentPlayTime += Time.deltaTime * playbackSpeed;
        }

        int keyIndex = (int) (Mathf.InverseLerp (screenHeight / 2, -screenHeight / 2, mousePos.y) * numKeys);
        keyIndex = Mathf.Clamp (keyIndex, 0, numKeys - 1);
        Key key = keys[keyIndex];

        bool leftMouseDown = Input.GetMouseButtonDown (0);
        bool leftMouseUp = Input.GetMouseButtonUp (0);
        // Handle LMB down:
        if (leftMouseDown && mousePos.x > cam.transform.position.x - screenWidth / 2 + keyWidth) {
            // Check if mouse over existing note
            bool mouseOverNote = false;
            bool mouseOverNoteX = false;
            for (int i = 0; i < notes.Count; i++) {
                mouseOverNote = notes[i].GetComponent<BoxCollider2D> ().bounds.Contains (mousePos);
                mouseOverNoteX |= notes[i].GetComponent<BoxCollider2D> ().bounds.Contains (new Vector2 (mousePos.x, notes[i].transform.position.y));

                if (mouseOverNote) {
                    activeNote = notes[i];
                    activeNote.Select ();
                    float noteDuration = notes[i].transform.localScale.x;
                    float mouseDstFromRightEdge = Mathf.Abs (mousePos.x - (notes[i].transform.position.x + noteDuration));

                    // If mouse close to right edge, enter scale mode; otherwise move mode
                    if (mouseDstFromRightEdge < Mathf.Min (0.2f, noteDuration / 2)) {
                        currentEditState = EditState.Scaling;
                    } else {
                        dragOffset = mousePos.x - activeNote.transform.position.x;
                        currentEditState = EditState.Moving;
                    }
                    break;
                }
            }
            // Spawn new note:
            if (!mouseOverNoteX) {
                activeNote = Instantiate (notePrefab);
                activeNote.Select ();
                activeNote.transform.position = new Vector3 (mousePos.x, key.transform.position.y);
                activeNote.transform.localScale = new Vector3 (1, key.transform.localScale.y - spacing, 1);
                activeNote.keyIndex = keyIndex;
                currentEditState = EditState.Scaling;
                notes.Add (activeNote);
                notes.Sort ((a, b) => (a.transform.position.x.CompareTo (b.transform.position.x)));
            }

        }

        // Handle scaling
        if (currentEditState == EditState.Scaling) {

            float duration = Mathf.Max (0, mousePos.x - activeNote.transform.position.x);
            if (Input.GetKey (KeyCode.LeftShift)) {
                float increment = .25f;
                float n = 1f / increment;
                duration = Mathf.Max (increment, ((int) (duration * n)) / n);
            }
            // Clamp duration to avoid intersecting with other notes
            foreach (Note note in notes) {
                if (note != activeNote) {
                    if (note.StartTime > activeNote.StartTime) {
                        duration = Mathf.Min (duration, note.StartTime - activeNote.StartTime);
                        break;
                    }
                }
            }
            activeNote.transform.localScale = new Vector3 (duration, activeNote.transform.localScale.y, 1);
            if (leftMouseUp) {
                const float minDuration = 0.01f;
                if (duration <= minDuration) {
                    notes.Remove (activeNote);
                    Destroy (activeNote.gameObject);
                }
            }
        }
        // Handle moving
        else if (currentEditState == EditState.Moving) {
            Vector2 newPos = new Vector2 (mousePos.x - dragOffset, key.transform.position.y);
            if (Input.GetKey (KeyCode.LeftShift)) {
                float increment = .5f;
                float n = 1f / increment;
                newPos.x = ((int) (newPos.x * n)) / n;
            }

            // Dont allow moving inside other notes
            foreach (Note note in notes) {
                if (note != activeNote) {
                    if (newPos.x >= note.StartTime && newPos.x <= note.EndTime) {
                        newPos.x = note.EndTime;
                        break;
                    } else if ((newPos.x + activeNote.Duration) >= note.StartTime && (newPos.x + activeNote.Duration) <= note.EndTime) {
                        newPos.x = note.StartTime - activeNote.Duration;
                        break;
                    }
                }
            }
            bool newPositionInvalid = false;
            foreach (Note note in notes) {
                if (note != activeNote) {
                    if (newPos.x > note.StartTime && newPos.x < note.EndTime) {
                        newPositionInvalid = true;
                        break;
                    } else if ((newPos.x + activeNote.Duration) > note.StartTime && (newPos.x + activeNote.Duration) < note.EndTime) {
                        newPositionInvalid = true;
                        break;
                    } else if (newPos.x <= note.StartTime && (newPos.x + activeNote.Duration) >= note.EndTime) {
                        newPositionInvalid = true;
                        break;
                    }
                }
            }
            if (!newPositionInvalid) {
                activeNote.transform.position = newPos;
                activeNote.keyIndex = keyIndex;
                notes.Sort ((a, b) => (a.transform.position.x.CompareTo (b.transform.position.x)));
            }
        }

        if (activeNote && Input.GetKeyDown (KeyCode.Backspace)) {
            notes.Remove (activeNote);
            Destroy (activeNote.gameObject);
            activeNote = null;
            currentEditState = EditState.None;
        }

        if (leftMouseUp) {
            if (activeNote) {
                activeNote.Deselect ();
                activeNote = null;
                currentEditState = EditState.None;
            }
        }
    }

    public void EndNote () {
        audioPlayer.frequency = 0;
    }

    public void PlayNote (int noteIndex) {
        int numHalfStepsAboveA = (remap) ? CalculateNumHalfSteps (noteIndex) : topNote - noteIndex;
        PlayNote (CalculateFrequency (numHalfStepsAboveA));

    }

    void PlayNote (float frequency) {
        audioPlayer.frequency = frequency;
    }

    int CalculateNumHalfSteps (int noteIndex) {
        int numStepsAboveA = topNote - noteIndex;
        int[] remapPos = { 0, 2, 3, 5, 7, 8, 10 };
        int[] remapNeg = { 0, -2, -4, -5, -7, -9, -10 };
        int numOctaves = numStepsAboveA / 7;

        int[] remap = (numStepsAboveA >= 0) ? remapPos : remapNeg;

        int numHalfStepsAboveA = 12 * numOctaves + remap[Mathf.Abs (numStepsAboveA) % 7];
        return numHalfStepsAboveA;
    }

    // https://pages.mtu.edu/~suits/NoteFreqCalcs.html
    float CalculateFrequency (int numHalfStepsAboveA) {
        // frequency of the A above middle C (440 Hz)
        const float a4 = 440;
        // 2^(1/12)
        const float r = 1.059463094359f;

        return a4 * Mathf.Pow (r, numHalfStepsAboveA);
    }

    public void Save () {
        if (song != null) {
            song.startTimes = new List<float> ();
            song.durations = new List<float> ();
            song.keyIndices = new List<int> ();

            for (int i = 0; i < notes.Count; i++) {
                song.startTimes.Add (notes[i].StartTime);
                song.durations.Add (notes[i].Duration);
                song.keyIndices.Add (notes[i].keyIndex);
            }
        }
    }

    public void Load () {
        if (song != null) {
            for (int i = 0; i < notes.Count; i++) {
                Destroy (notes[i].gameObject);
            }
            notes = new List<Note> ();
            for (int i = 0; i < song.keyIndices.Count; i++) {
                Note note = Instantiate (notePrefab);
                note.keyIndex = song.keyIndices[i];
                note.transform.localScale = new Vector3 (song.durations[i], keys[note.keyIndex].transform.localScale.y, 1);
                note.transform.position = new Vector3 (song.startTimes[i], keys[note.keyIndex].transform.position.y, 0);
                notes.Add (note);
            }
        }
    }

    Track GenerateTrack () {
        notes.Sort ((a, b) => (a.transform.position.x.CompareTo (b.transform.position.x)));
        float firstNoteStartTime = notes[0].transform.position.x;

        var track = new Track (notes.Count);

        for (int i = 0; i < notes.Count; i++) {
            var note = notes[i];
            float frequency = CalculateFrequency (CalculateNumHalfSteps (note.keyIndex));
            track.frequencies[i] = frequency;
            track.startTimes[i] = note.transform.position.x - firstNoteStartTime;
            track.durations[i] = note.transform.localScale.x;
        }

        return track;
    }

    public string Export () {
        var track = GenerateTrack ();
        string frequencyString = "int frequencies[] = {";
        string durationString = "int durations[] = {";
        string startTimeString = "int startTimes[] = {";
        for (int i = 0; i < track.numNotes; i++) {
            bool lastElement = i == track.numNotes - 1;
            frequencyString += Mathf.RoundToInt (track.frequencies[i]) + ((lastElement) ? "};" : ",");
            durationString += Mathf.RoundToInt (track.durations[i] / playbackSpeed * 1000) + ((lastElement) ? "};" : ",");
            startTimeString += Mathf.RoundToInt (track.startTimes[i] / playbackSpeed * 1000) + ((lastElement) ? "};" : ",");
        }

        return frequencyString + "\n" + startTimeString + "\n" + durationString;
    }

    public class Track {
        public float[] frequencies;
        public float[] startTimes;
        public float[] durations;
        public int numNotes;

        public bool playing;
        public int playbackIndex;
        public float currentPlayTime;

        public Track (int numNotes) {
            this.numNotes = numNotes;
            this.frequencies = new float[numNotes];
            this.startTimes = new float[numNotes];
            this.durations = new float[numNotes];
        }
    }
}