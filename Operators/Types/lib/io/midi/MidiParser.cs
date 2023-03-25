using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using T3.Core.DataTypes;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;
using T3.Operators.Types.Id_a3ceb788_4055_4556_961b_63b7221f93e7;

namespace T3.Operators.Types.Id_b6d6471a_a1fc_4292_8418_99a0a3e82c75
{
    public class MidiParser : Instance<MidiParser>
    {
        [Output(Guid = "ED6A2750-0F60-42F3-82CC-8B312B6EDC42", DirtyFlagTrigger = DirtyFlagTrigger.Animated)]
        public readonly Slot<List<float>> Notes = new(new List<float>(NumberNotesAndCc));
        
        [Output(Guid = "714BE192-0009-4434-A29F-46B6888C22A9", DirtyFlagTrigger = DirtyFlagTrigger.Animated)]
        public readonly Slot<List<float>> CC = new(new List<float>(NumberNotesAndCc));
        
        //[Output(Guid = "87C82DEC-A73A-4EEA-9D7A-AF87981064B4")]
        //public readonly Slot<List<Vector2>> NotesWithChannel = new();
        
        //[Output(Guid = "ACB1B5B0-C272-4D15-989E-756E4C6666EF")]
        //public readonly Slot<List<Vector2>> CCWithChannel = new();
        

        public MidiParser()
        {
            Notes.UpdateAction = Update;
            CC.UpdateAction = Update;

            Notes.Value = Enumerable.Repeat(0f, NumberNotesAndCc).ToList();
            CC.Value = Enumerable.Repeat(0f, NumberNotesAndCc).ToList();

            //NotesWithChannel.UpdateAction = Update;
            //CCWithChannel.UpdateAction = Update;
            //NotesWithChannel.Value = new List<Vector2>(capacity: 127);
            //CCWithChannel.Value = new List<Vector2>(capacity: 127);
        }

        private void Update(EvaluationContext context)
        {
            var dirty = MidiEvents.DirtyFlag.IsDirty || Channel.DirtyFlag.IsDirty || Track.DirtyFlag.IsDirty;
            if (!dirty)
                return;
            
            var midiEvents = MidiEvents.GetValue(context);
            var channel = Channel.GetValue(context);
            var track = Track.GetValue(context);

            if (_channel != channel || _track != track)
            {
                _channel = channel;
                _track = track;
                ClearOutput();
            }
            
            if (midiEvents is null) return;
            
            MidiEvents.DirtyFlag.Clear();
            Channel.DirtyFlag.Clear();
            Track.DirtyFlag.Clear();

            bool notesChanged = false;
            bool ccChanged = false;
            
            foreach (var midi in midiEvents)
            {
                if (string.IsNullOrWhiteSpace(midi.Key)) continue;
                var midiInfo = ParseMidiEvent(midi.Key);

                if (midiInfo.ChannelNumber != channel || midiInfo.TrackNumber != track)
                    continue;
                
                var index = midiInfo.NoteOrCC;

                var notes = Notes.GetValue(context);
                var cc = CC.GetValue(context);

                if (midiInfo.IsCc)
                {
                    cc[index] = midi.Value;
                    //CCWithChannel.Value[index] = new Vector2(index, midi.Value);
                    notesChanged = true;
                }
                else
                {
                    notes[index] = midi.Value;
                    //NotesWithChannel.Value[index] = new Vector2(index, midi.Value);
                    ccChanged = true;
                }
            }

            if (notesChanged)
            {
                Notes.DirtyFlag.Invalidate();
                //NotesWithChannel.DirtyFlag.Invalidate();
            }

            if (ccChanged)
            {
                CC.DirtyFlag.Invalidate();
                //CCWithChannel.DirtyFlag.Invalidate();
            }
        }

        private void ClearOutput()
        {
            var noteList = Notes.Value;
            var ccList = CC.Value;
            for (int i = 0; i < NumberNotesAndCc; i++)
            {
                noteList[i] = 0f;
                ccList[i] = 0f;
            }
        }

        /// <summary>
        /// Returns an int for the Midi note or CC channel, and outputs a bool for whether the event is CC or not
        /// </summary>
        private static MidiHeaderInfo ParseMidiEvent(string midiEventHeader)
        {
            ReadOnlySpan<char> input = new ReadOnlySpan<char>(midiEventHeader.ToCharArray());

            var track = ParseNextInt(input, out var trimmedSpan1);
            var channel = ParseNextInt(trimmedSpan1, out var trimmedSpan2);
            
            var isCc = trimmedSpan2.IndexOf(MidiClip.ControlMessagePrefix, StringComparison.OrdinalIgnoreCase) >= 0;
            var noteOrControlNumber = ParseNextInt(trimmedSpan2, out var _);

            return new MidiHeaderInfo
            {
                TrackNumber = track,
                ChannelNumber = channel,
                NoteOrCC = noteOrControlNumber,
                IsCc = isCc
            };
        }

        private static int ParseNextInt(ReadOnlySpan<char> span, out ReadOnlySpan<char> spanWithoutThisInt)
        {
            var startIndex = 0;

            // Seek the first digit of our integer
            while (!char.IsDigit(span[startIndex]))
                startIndex++;

            // Remove characters we don't care about
            var trimmed = span.Slice(startIndex);

            var numberOfDigits = 1;
            while (numberOfDigits < trimmed.Length && char.IsDigit(trimmed[numberOfDigits]))
                numberOfDigits++;
            
            var numberOnly = trimmed.Slice(0, numberOfDigits);

            spanWithoutThisInt = trimmed.Slice(numberOfDigits);
            
            // Parse and return the integer
            #if ALLOCATION_FREE_INTEGER_PARSING
                var result = 0;
                foreach (var digit in numberOnly)
                {
                    result = result * 10 + (digit - '0');
                }
                return result;
            #endif

            return int.Parse(numberOnly);
        }

        public struct MidiHeaderInfo
        {
            public int TrackNumber;
            public int ChannelNumber;
            public int NoteOrCC;
            public bool IsCc;
        }

        private int _channel = 1;
        private int _track = 0;
        private const int NumberNotesAndCc = 128;

        [Input(Guid = "8866CB69-18E8-4BC0-9D1F-8B05BD24A0FD")]
        public readonly InputSlot<Dict<string, float>> MidiEvents = new(new Dict<string, float>(0f));
        
        [Input(Guid = "149C67A6-EEBF-48DE-A1CC-788899B4F7BB")]
        public readonly InputSlot<int> Channel = new();
        
        [Input(Guid = "56706717-21AE-4D5D-AD3E-60B48A4C131C")]
        public readonly InputSlot<int> Track = new();
    }
}
