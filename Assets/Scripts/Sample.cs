// MIT License
//
// Copyright(c) 2020 Pixel Precision LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using UnityEngine;

/// <summary>
/// A sample showing the basics of the Phonics library, including:
/// 
/// - Loading a file.
/// - Selecting an instrument.
/// - Playing and stopping an instrument note.
/// </summary>
public class Sample : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////////////////
    //
    //      CLASS : KeyInfo
    //
    //   The class represents a button in the IMGUI that can be pressed
    //  to play a note.
    //
    ////////////////////////////////////////////////////////////////////////////////
    public class KeyInfo
    { 
        public Vector2 pos;     // Button position
        public Vector2 sz;      // Button dimension
        public int id = -1;     // The handle for the current play id, or -1 if not playing.
        public PxPre.Phonics.WesternFreqUtils.Key k;        // The key of the button.
        public int octave;                                  // The octave of the button
        public string label;    // The text of the button.

        public KeyInfo(Vector2 pos, Vector2 sz, PxPre.Phonics.WesternFreqUtils.Key k, int octave, string label)
        { 
            this.pos    = pos;
            this.sz     = sz;
            this.k      = k;
            this.octave = octave;
            this.label  = label;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////
    //
    //      CLASS MEMBER VARIABLES
    //
    ////////////////////////////////////////////////////////////////////////////////

    // The Wiring document to load.
    // This sample uses the starting instruments from the Precision Keyboard App.
    public TextAsset document;
    
    // The master volume to control the volume of audio generated.
    public float master = 1.0f;

    // Phonics manager.
    //
    // In charge of managing procedural audio generation programs, as well
    // as the manager for recycling AudioSource components.
    PxPre.Phonics.GenManager genmgr = null;

    // The samples-per-second count.
    const int sampleFreq = 44100;

    // The collection of instruments loaded from file.
    WiringCollection wireCol = null;
    // The active intrument to synthesize audio when a note button is pressed.
    WiringDocument activeInstr = null;

    //      UI
    ////////////////////////////////////////////////////////////////////////////////

    // Button information for the 12 keys of octave 4 for this sample.
    KeyInfo [] keys = null;

    // The scroll position of the notes scrollview.
    Vector2 scroll = Vector2.zero;

    ////////////////////////////////////////////////////////////////////////////////
    //
    //      CLASS METHODS
    //
    ////////////////////////////////////////////////////////////////////////////////

    private void Awake()
    {

        //      INITIALIZE PHONICS
        //
        ////////////////////////////////////////////////////////////////////////////////
        
        // This is needed to precompute white noise - if this is not precomputed,
        // anything using a Noise node will crash.
        PxPre.Phonics.GenWhite.BakeNoise();

        // Initialize the phonics manager.
        this.genmgr = 
            new PxPre.Phonics.GenManager(this.gameObject);

        // Load the sample Wiring document to get access to our sample instruments.
        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
        doc.LoadXml(this.document.text);
        this.wireCol = new WiringCollection();
        this.wireCol.LoadDocument(doc, true);

        // Keep track of our own active instrument instead of going to the
        // wiring collection. This is done out of preference.
        this.activeInstr = this.wireCol.Active;

        //      INITIALIZE BUTTON DATA
        //
        ////////////////////////////////////////////////////////////////////////////////

        // Key button positioning constants.
        const float keyHeight = 50.0f;                  // The height of keys 
        const float keyYAcci = 100.0f;                  // Top of accidental keys
        const float keyYWhite = keyYAcci + keyHeight;   // Top of non-accidental keys
        const float keyWidth = 50.0f;                   // The width of keys

        // Define the position and note values for all the keys being
        // shown in the GUI.
        keys = 
            new KeyInfo[] 
            { 
                // Accidental keys
                new KeyInfo(new Vector2(0.5f * keyWidth, keyYAcci), new Vector2(keyWidth, keyHeight), PxPre.Phonics.WesternFreqUtils.Key.Cs, 4, "C#" ),
                new KeyInfo(new Vector2(1.5f * keyWidth, keyYAcci), new Vector2(keyWidth, keyHeight), PxPre.Phonics.WesternFreqUtils.Key.Ds, 4, "D#" ),
                new KeyInfo(new Vector2(3.5f * keyWidth, keyYAcci), new Vector2(keyWidth, keyHeight), PxPre.Phonics.WesternFreqUtils.Key.Fs, 4, "F#" ),
                new KeyInfo(new Vector2(4.5f * keyWidth, keyYAcci), new Vector2(keyWidth, keyHeight), PxPre.Phonics.WesternFreqUtils.Key.Gs, 4, "G#" ),
                new KeyInfo(new Vector2(5.5f * keyWidth, keyYAcci), new Vector2(keyWidth, keyHeight), PxPre.Phonics.WesternFreqUtils.Key.As, 4, "A#" ),

                // White Keys
                new KeyInfo(new Vector2(0.0f * keyWidth, keyYWhite), new Vector2(keyWidth, keyHeight), PxPre.Phonics.WesternFreqUtils.Key.C, 4, "C"),
                new KeyInfo(new Vector2(1.0f * keyWidth, keyYWhite), new Vector2(keyWidth, keyHeight), PxPre.Phonics.WesternFreqUtils.Key.D, 4, "D"),
                new KeyInfo(new Vector2(2.0f * keyWidth, keyYWhite), new Vector2(keyWidth, keyHeight), PxPre.Phonics.WesternFreqUtils.Key.E, 4, "E"),
                new KeyInfo(new Vector2(3.0f * keyWidth, keyYWhite), new Vector2(keyWidth, keyHeight), PxPre.Phonics.WesternFreqUtils.Key.F, 4, "F"),
                new KeyInfo(new Vector2(4.0f * keyWidth, keyYWhite), new Vector2(keyWidth, keyHeight), PxPre.Phonics.WesternFreqUtils.Key.G, 4, "G"),
                new KeyInfo(new Vector2(5.0f * keyWidth, keyYWhite), new Vector2(keyWidth, keyHeight), PxPre.Phonics.WesternFreqUtils.Key.A, 4, "A"),
                new KeyInfo(new Vector2(6.0f * keyWidth, keyYWhite), new Vector2(keyWidth, keyHeight), PxPre.Phonics.WesternFreqUtils.Key.B, 4, "B"),
            };

        
    }

    void Update() 
    {
        // Once per frame, do maintenence.
        this.genmgr.CheckFinishedRelease();
    }

    private void OnGUI()
    {
        //      THE GUI FOR THE EXTRA CONTROLS
        //
        ////////////////////////////////////////////////////////////////////////////////
        
        GUILayout.BeginHorizontal(GUILayout.Width( 200.0f));
            GUILayout.Label("Master", GUILayout.ExpandWidth(false));
            GUILayout.Space(10.0f);
            this.master = GUILayout.HorizontalSlider( this.master, 0.0f, 1.0f, GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();

        GUI.color = this.genmgr.AnyPlaying() ? Color.red : Color.white;

        GUILayout.Space(20.0f);
        if(GUILayout.Button("E-Stop", GUILayout.Width(200.0f)) == true)
            this.genmgr.StopAllNotes();

        GUI.color = Color.white;

        //      THE GUI FOR THE KEYBOARD
        //
        ////////////////////////////////////////////////////////////////////////////////
        
        foreach(KeyInfo ki in this.keys)
        { 
            bool down = GUI.RepeatButton(new Rect(ki.pos, ki.sz), ki.label);

            // IMGUI called OnGUI multiple times per-frame with varying return
            // values for GUI.RepeatButton depending on the pass type.
            //
            // Only handle the input on the IMGUI pass where down could be true from
            // input, or else we're going to thrash between true<->false.
            if(Event.current.type == EventType.Repaint)
            {
                if(down == false && ki.id != -1)
                { 
                    this.genmgr.StopNote(ki.id);
                    ki.id = -1;
                }
                else if(down == true && ki.id == -1)
                { 
                    // Create a version of the tree with hard-coded information on the 
                    // samplerate and note.
                    //
                    // This could be seen as quickly compiling a throw-away program.
                    //
                    // Get the actual frequency
                    float freq = PxPre.Phonics.WesternFreqUtils.GetFrequency(ki.k, ki.octave);
                    //
                    PxPre.Phonics.GenBase genProg = 
                        this.activeInstr.CreateGenerator(
                            freq,                   // Note frequency, calculated above with PxPre.Phonics utility function.
                            120.0f,                 // Not used in this sample, can be any sane BPM value (from 1 to 300).
                            this.master,
                            sampleFreq,             // Frequency count for quality/computation-cost trade-off.
                            this.activeInstr,       // A notification of what created it. As a 3rd party user, this will always be the invoker of CreateGenerator().
                            this.wireCol);          // A list of collection - some documents reference other documents and this is how it gets access to those other documents.

                    if(Input.GetKeyDown(KeyCode.A))
                        Debug.Log("Still going!");

                    ki.id = 
                        this.genmgr.StartGenerator(
                            genProg,                // Curried program and hard-coded parameters
                            1.0f,                   // An additional Gain - reserved for input velocity, such as if invoked from a MIDI input
                            sampleFreq,             // Buffer size, not used - but it is recommended to just use the samples/sec rate
                            sampleFreq);            // Samples/sec rate, this should match the value used from CreateGenerator().
                }
            }
        }

        //      THE GUI FOR THE INSTRUMENT SELECTION
        //
        ////////////////////////////////////////////////////////////////////////////////
        
        this.scroll = GUI.BeginScrollView( new Rect(0.0f, 220.0f, 200.0f, 400.0f), this.scroll, new Rect(0.0f, 0.0f, 200.0f, 700.0f));

            GUILayout.Label("Select the instrument to play:");
            GUILayout.Space(20.0f);

            int wireDocCt = this.wireCol.Count();
            for(int i = 0; i < wireDocCt; ++i)
            { 
                WiringDocument wd = this.wireCol.GetDocument(i);

                bool tog = GUILayout.Toggle( wd == this.activeInstr, wd.GetName() );

                if(tog == true)
                    this.activeInstr = wd;
            }

        GUI.EndScrollView();
    }
}
