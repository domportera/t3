using System.Collections.Generic;
using T3.Core.Utils;

namespace T3.Core.Audio;

public static class MidiParser
{
    public readonly struct MidiNote
    {
        public readonly int TrackNumber;
        public readonly int ChannelNumber;
        public readonly int NoteNumber;
        public readonly NoteInfo NoteInfo;
        public readonly int Velocity; // Note number if Note, Cc channel if Cc message
        public readonly float VelocityNormalized = 0;
        public readonly bool IsNoteOff;

        private const int MinVelocity = 0;
        private const int MaxVelocity = 127;

        public MidiNote(int trackNumber, int channelNumber, int noteNumber, int velocity)
        {
            TrackNumber = trackNumber;
            ChannelNumber = channelNumber;
            NoteNumber = noteNumber;
            IsNoteOff = velocity == 0;
            Velocity = velocity;

            if (!IsNoteOff)
                VelocityNormalized = MathUtils.Remap(velocity, MinVelocity, MaxVelocity, 0f, 1f);

            GetNoteInfoFromMidiNoteNumber(noteNumber, out NoteInfo);
        }

        static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        static void GetNoteInfoFromMidiNoteNumber(int midiNoteNumber, out NoteInfo noteInfo)
        {
            int octave = midiNoteNumber / 12 - 1;
            int noteIndex = midiNoteNumber % 12;
            noteInfo = new NoteInfo(NoteNames[noteIndex], octave, noteIndex);
        }
    }

    public readonly struct NoteInfo
    {
        public readonly string Name;
        public readonly int Octave;
        public readonly int NoteNameInt;

        public NoteInfo(string name, int octave, int noteNameInt)
        {
            Name = name;
            Octave = octave;
            NoteNameInt = noteNameInt;
        }
    }

    public readonly struct ControlMessage
    {
        public readonly string Name;
        public readonly int TrackNumber;
        public readonly int ChannelNumber;
        public readonly int CcChannel;
        public readonly int Value;
        public readonly bool IsBinary;

        public ControlMessage(int trackNumber, int channelNumber, int ccChannel, int value)
        {
            TrackNumber = trackNumber;
            ChannelNumber = channelNumber;
            CcChannel = ccChannel;
            Value = value;
            Name = CcChannels[ccChannel];
            IsBinary = BinaryCcChannels.Contains(ccChannel);
        }

        private static readonly Dictionary<int, string> CcChannels = new()
                                                                         {
                                                                             { 0, "Bank Select (MSB)" },
                                                                             { 1, "Modulation Wheel (MSB)" },
                                                                             { 2, "Breath Controller (MSB)" },
                                                                             { 3, UndefinedName },
                                                                             { 4, "Foot Controller (MSB)" },
                                                                             { 5, "Portamento Time (MSB)" },
                                                                             { 6, "Data Entry (MSB)" },
                                                                             { 7, "Main Volume (MSB)" },
                                                                             { 8, "Balance (MSB)" },
                                                                             { 9, UndefinedName },
                                                                             { 10, "Pan (MSB)" },
                                                                             { 11, "Expression Controller (MSB)" },
                                                                             { 12, "Effect Control 1 (MSB)" },
                                                                             { 13, "Effect Control 2 (MSB)" },
                                                                             { 14, UndefinedName },
                                                                             { 15, UndefinedName },
                                                                             { 16, "General Purpose Controller 1 (MSB)" },
                                                                             { 17, "General Purpose Controller 2 (MSB)" },
                                                                             { 18, "General Purpose Controller 3 (MSB)" },
                                                                             { 19, "General Purpose Controller 4 (MSB)" },
                                                                             { 20, UndefinedName },
                                                                             { 21, UndefinedName },
                                                                             { 22, UndefinedName },
                                                                             { 23, UndefinedName },
                                                                             { 24, UndefinedName },
                                                                             { 25, UndefinedName },
                                                                             { 26, UndefinedName },
                                                                             { 27, UndefinedName },
                                                                             { 28, UndefinedName },
                                                                             { 29, UndefinedName },
                                                                             { 30, UndefinedName },
                                                                             { 31, UndefinedName },
                                                                             { 32, "Bank Select (LSB)" },
                                                                             { 33, "Modulation Wheel (LSB)" },
                                                                             { 34, "Breath Controller (LSB)" },
                                                                             { 35, UndefinedName },
                                                                             { 36, "Foot Controller (LSB)" },
                                                                             { 37, "Portamento Time (LSB)" },
                                                                             { 38, "Data Entry (LSB)" },
                                                                             { 39, "Main Volume (LSB)" },
                                                                             { 40, "Balance (LSB)" },
                                                                             { 41, UndefinedName },
                                                                             { 42, "Pan (LSB)" },
                                                                             { 43, "Expression Controller (LSB)" },
                                                                             { 44, "Effect Control 1 (LSB)" },
                                                                             { 45, "Effect Control 2 (LSB)" },
                                                                             { 46, UndefinedName },
                                                                             { 47, UndefinedName },
                                                                             { 48, "General Purpose Controller 1 (LSB)" },
                                                                             { 49, "General Purpose Controller 2 (LSB)" },
                                                                             { 50, "General Purpose Controller 3 (LSB)" },
                                                                             { 51, "General Purpose Controller 4 (LSB)" },
                                                                             { 52, UndefinedName },
                                                                             { 53, UndefinedName },
                                                                             { 54, UndefinedName },
                                                                             { 55, UndefinedName },
                                                                             { 56, UndefinedName },
                                                                             { 57, UndefinedName },
                                                                             { 58, UndefinedName },
                                                                             { 59, UndefinedName },
                                                                             { 60, UndefinedName },
                                                                             { 61, UndefinedName },
                                                                             { 62, UndefinedName },
                                                                             { 63, UndefinedName },
                                                                             { 64, "Damper Pedal (Sustain)" },
                                                                             { 64, "Damper Pedal (Sustain) (MSB)" },
                                                                             { 66, "Portamento (MSB)" },
                                                                             { 66, "Sostenuto Pedal (MSB)" },
                                                                             { 67, "Soft Pedal (MSB)" },
                                                                             { 68, "Legato Pedal (MSB)" },
                                                                             { 69, "Hold 2 Pedal (MSB)" },
                                                                             { 70, "Sound Variation (MSB)" },
                                                                             { 71, "Timbre/Harmonic Content (MSB)" },
                                                                             { 72, "Release Time (MSB)" },
                                                                             { 73, "Attack Time (MSB)" },
                                                                             { 74, "Brightness (MSB)" },
                                                                             { 75, "Sound Control 6 (MSB)" },
                                                                             { 76, "Sound Control 7 (MSB)" },
                                                                             { 77, "Sound Control 8 (MSB)" },
                                                                             { 78, "Sound Control 9 (MSB)" },
                                                                             { 79, "Sound Control 10 (MSB)" },
                                                                             { 80, UndefinedName },
                                                                             { 81, UndefinedName },
                                                                             { 82, UndefinedName },
                                                                             { 83, UndefinedName },
                                                                             { 84, UndefinedName },
                                                                             { 85, UndefinedName },
                                                                             { 86, UndefinedName },
                                                                             { 87, UndefinedName },
                                                                             { 88, UndefinedName },
                                                                             { 89, UndefinedName },
                                                                             { 90, UndefinedName },
                                                                             { 91, "Effects Level (MSB)" },
                                                                             { 92, "Tremolo Level (MSB)" },
                                                                             { 93, "Chorus Level (MSB)" },
                                                                             { 94, "Celeste Level (MSB)" },
                                                                             { 95, "Phaser Level (MSB)" },
                                                                             { 96, "Data Button Increment" },
                                                                             { 97, "Data Button Decrement" },
                                                                             { 98, "Non-Registered Parameter Number (LSB)" },
                                                                             { 99, "Non-Registered Parameter Number (MSB)" },
                                                                             { 100, "Registered Parameter Number (LSB)" },
                                                                             { 101, "Registered Parameter Number (MSB)" },
                                                                             { 102, "Undefined" },
                                                                             { 103, "Undefined" },
                                                                             { 104, "Undefined" },
                                                                             { 105, "Undefined" },
                                                                             { 106, "Undefined" },
                                                                             { 107, "Undefined" },
                                                                             { 108, "Undefined" },
                                                                             { 109, "Undefined" },
                                                                             { 110, "Undefined" },
                                                                             { 111, "Undefined" },
                                                                             { 112, "Undefined" },
                                                                             { 113, "Undefined" },
                                                                             { 114, "Undefined" },
                                                                             { 115, "Undefined" },
                                                                             { 116, "Undefined" },
                                                                             { 117, "Undefined" },
                                                                             { 118, "Undefined" },
                                                                             { 119, "All Sound Off" },
                                                                             { 120, "All Controllers Off" },
                                                                             { 121, "Local Keyboard" },
                                                                             { 122, "All Notes Off" },
                                                                             { 123, "Omni Mode Off" },
                                                                             { 124, "Omni Mode On" },
                                                                             { 125, "Poly Mode On/Off" },
                                                                             { 126, "Poly Mode On" },
                                                                             { 127, "Poly Mode Off" },
                                                                         };

        private static readonly HashSet<int> BinaryCcChannels = new()
                                                                    {
                                                                        64, // Hold Pedal (on/off)
                                                                        65, // Portamento (on/off)
                                                                        66, // Sustenuto Pedal (on/off)
                                                                        67, // Soft Pedal (on/off)
                                                                        68, // Legato Pedal (on/off)
                                                                        69, // Hold 2 Pedal (on/off)
                                                                        70, // Sound Controller 1 (on/off)
                                                                        71, // Sound Controller 2 (on/off)
                                                                        72, // Sound Controller 3 (on/off)
                                                                        73, // Sound Controller 4 (on/off)
                                                                        91, // Reverb (on/off)
                                                                        92, // Tremolo (on/off)
                                                                        93, // Chorus (on/off)
                                                                        94, // Celeste (on/off)
                                                                        95, // Phaser (on/off)
                                                                        120, // All Sound Off (on/off)
                                                                        121, // Reset All Controllers (on/off)
                                                                        122, // Local Control (on/off)
                                                                        123, // All Notes Off (on/off)
                                                                        124, // Omni Mode Off (on/off)
                                                                        125, // Omni Mode On (on/off)
                                                                        126, // Mono Mode On (on/off)
                                                                        127, // Poly Mode On (on/off)
                                                                    };

        private const string UndefinedName = "Undefined";
    }
}