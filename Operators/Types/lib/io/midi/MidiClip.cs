using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using T3.Core.DataTypes;
using T3.Core.Logging;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;
using NAudio.Midi;


namespace T3.Operators.Types.Id_a3ceb788_4055_4556_961b_63b7221f93e7
{
    public class MidiClip : Instance<MidiClip>, IDisposable
    {
        [Output(Guid = "04BFDF5C-7D05-469A-89BE-525F27186F69", DirtyFlagTrigger = DirtyFlagTrigger.Animated)]
        public readonly TimeClipSlot<Dict<float>> Values = new();

        [Output(Guid = "C08C4B81-65B0-4FC3-AF46-F06E72838F9D")]
        public readonly Slot<List<string>> ChannelNames = new();

        [Output(Guid = "8592E9C6-C15D-4024-B13F-2206DD52ED72", DirtyFlagTrigger = DirtyFlagTrigger.Animated)]
        public readonly TimeClipSlot<Dict<float>> CurrentNoteEvents = new();
        
        [Output(Guid = "8BA700A8-464A-442F-ABEC-137DE04127C3", DirtyFlagTrigger = DirtyFlagTrigger.Animated)]
        public readonly TimeClipSlot<Dict<float>> NoteOnEvents = new();
        
        [Output(Guid = "B18D8417-FFC7-4F66-8F74-DF4F8BC8A3F0", DirtyFlagTrigger = DirtyFlagTrigger.Animated)]
        public readonly TimeClipSlot<Dict<float>> NoteOffEvents = new();
        
        [Output(Guid = "E443484C-2C3F-43B9-9D70-DCCA3A7E710D", DirtyFlagTrigger = DirtyFlagTrigger.Animated)]
        public readonly TimeClipSlot<Dict<float>> CCEvents = new();
        
        [Output(Guid = "AADD9189-0086-42D6-AC45-D694270C0252")]
        public readonly Slot<float> DeltaTicksPerQuarterNote = new();

        public MidiClip()
        {
            _initialized = false;
            Values.UpdateAction = Update;
            ChannelNames.UpdateAction = Update;
            DeltaTicksPerQuarterNote.UpdateAction = Update;
            CurrentNoteEvents.UpdateAction = Update;
            NoteOnEvents.UpdateAction = Update;
            NoteOffEvents.UpdateAction = Update;
            CCEvents.UpdateAction = Update;

            CurrentNoteEvents.Value = new(0f);
            CCEvents.Value = new Dict<float>(0f);
            NoteOnEvents.Value = new(0f);
            NoteOffEvents.Value = new(0f);
        }
        protected override void Dispose(bool isDisposing)
        {
            //if (!isDisposing)
            //    return;
        }

        private void Update(EvaluationContext context)
        {
            try
            {
                if (!_initialized || Filename.DirtyFlag.IsDirty)
                {
                    SetupMidiFile(context);
                    _channelNames = _midiEvents.Keys.ToList();
                    ChannelNames.Value = _channelNames;
                }

                if (!_initialized || UseNumbersAsNames.DirtyFlag.IsDirty)
                    ChangedUseNumbersAsNames(context);

                if (!_initialized || _midiEventCollection == null) 
                    return;

                _printLogMessages = PrintLogMessages.GetValue(context);
                

                // Get scaled time range of clip
                var timeRange = Values.TimeClip.TimeRange;
                var sourceRange = Values.TimeClip.SourceRange;

                // Get the time we should be at in the MIDI file according to the timeClip
                var bars = context.LocalTime - timeRange.Start;
                var nonZeroTimeRAnge = Math.Abs(timeRange.End - timeRange.Start) > 0.0001f;
                if (nonZeroTimeRAnge)
                {
                    var rate = (sourceRange.End - sourceRange.Start)
                               / (timeRange.End - timeRange.Start);
                    bars *= rate;
                }

                if (UseAbsoluteTime.DirtyFlag.IsDirty)
                {
                    _useAbsoluteTime = UseAbsoluteTime.GetValue(context);
                }
                
                if(_useAbsoluteTime)
                    bars += sourceRange.Start;

                // For now: brute-force rewind if we run backwards in time
                var rewound = bars < _lastTimeInBars;
                if (rewound)
                {
                    ClearTracks();
                }

                _lastTimeInBars = bars;

                // Include past events in our response
                var minRange = _useAbsoluteTime ? Math.Min(sourceRange.Start, sourceRange.End) : 0f;
                var someTrackChanged = false;
                var inRange = bars >= minRange && bars < minRange + Math.Abs(sourceRange.Duration);
                
                if (inRange)
                {
                    someTrackChanged = UpdateTracks(bars);
                }

                if (someTrackChanged)
                {
                    Log.Debug("Midi changed!!!!");
                    Values.Value = _midiEvents;
                    Values.DirtyFlag.SetUpdated();
                    Values.DirtyFlag.Invalidate();
                }
                

                string timeLog = $"Context LocalTime: {context.LocalTime}\n" +
                                 $"TimeRange: {timeRange.Start} -> {timeRange.End}\n" +
                                 $"SourceRange: {sourceRange.Start} -> {sourceRange.End}\n" +
                                 $"OG bars: {context.LocalTime - timeRange.Start}\n" +
                                 $"bars: {bars}\n" +
                                 $"rewound: {rewound}\n" +
                                 $"inrange: {inRange}\n" +
                                 $"someTrackChanged: {someTrackChanged}";
                
                //Log.Debug(timeLog);
            }
            
            catch (Exception e)
            {
                Log.Debug("Updating MidiClip failed:" + e, this);
            }
        }

        private List<string> _channelNames = new();

        private void SetupMidiFile(EvaluationContext context)
        {
            var filename = Filename.GetValue(context);
            if (string.IsNullOrEmpty(filename))
                return;
            
            // Initialize MIDI file reading, then read all parameters from file
            const bool noStrictMode = false;
            _midiFile = new MidiFile(filename, noStrictMode);
            _deltaTicksPerQuarterNote = _midiFile.DeltaTicksPerQuarterNote;
            _midiEventCollection = _midiFile.Events;
            ClearTracks();
            
            _timeSignature = _midiFile.Events[0].OfType<TimeSignatureEvent>().FirstOrDefault();

            // Update slots
            DeltaTicksPerQuarterNote.Value = (int)_deltaTicksPerQuarterNote;    // conversion to int is probably bad

            _initialized = true;
        }

        private void ClearTracks()
        {
            _lastTrackEventIndices = Enumerable.Repeat(-1, _midiFile.Tracks).ToList();
            
            CurrentNoteEvents.Value.Clear();

            foreach (var k in _midiEvents.Keys)
            {
                _midiEvents[k] = 0;
            }
        }

        private bool UpdateTracks(double bars)
        {
            var noteNameFormat = _useNumbersAsNames ? NoteNumberNamesFormat : StandardNameFormat;
            var ccNameFormat = _useNumbersAsNames ? CcNumberNamesFormat : StandardCcFormat;
            
            ClearSpecificEventLists();

            var someTrackChanged = false;
            for (var trackIndex = 0; trackIndex < _midiFile.Tracks; trackIndex++)
            {
                someTrackChanged |= UpdateTrack(trackIndex, bars, noteNameFormat, ccNameFormat);
            }

            return someTrackChanged;
        }
        
        private bool UpdateTrack(int trackIndex, double time, string noteNameFormat, string ccNameFormat)
        {
            if (trackIndex >= _lastTrackEventIndices.Count ||
                trackIndex >= _midiFile.Events[trackIndex].Count) 
                return false;

            var events = _midiFile.Events[trackIndex];
            
            var lastEventIndex = _lastTrackEventIndices[trackIndex];
            if (lastEventIndex + 1 >= events.Count) 
                return false;

            var valuesChanged = false;
            var timeInTicks = (long)(time * 4 * _deltaTicksPerQuarterNote);
            var nextEventIndex = lastEventIndex + 1;

            
            while (nextEventIndex < events.Count && timeInTicks >= events[nextEventIndex].AbsoluteTime)
            {
                var thisEvent = events[nextEventIndex];
                Log.Debug($"{timeInTicks} vs {thisEvent.AbsoluteTime}");
                if (_printLogMessages)
                {
                    Log.Debug(TimeToBarsBeatsTicks(thisEvent.AbsoluteTime, _deltaTicksPerQuarterNote, _timeSignature));
                }

                switch (thisEvent)
                {
                    case NoteOnEvent noteOnEvent:
                    {
                        var channel = noteOnEvent.Channel;
                        var name = _useNumbersAsNames ? noteOnEvent.NoteNumber.ToString() : noteOnEvent.NoteName;
                        var value = noteOnEvent.Velocity / 127f;
                        var key = string.Format(noteNameFormat, trackIndex.ToString(), channel.ToString(), name);
                        _midiEvents[key] = value;
                        valuesChanged = true;
                        NoteOnEvents.Value[key] = value;
                        CurrentNoteEvents.Value[key] = value;    

                        if (_printLogMessages)
                            Log.Debug(key + "=" + value);
                        break;
                    }
                    case NoteEvent noteEvent:
                    {
                        var channel = noteEvent.Channel;
                        var name = _useNumbersAsNames ? noteEvent.NoteNumber.ToString() : noteEvent.NoteName;
                        const float value = 0.0f;
                        var key = string.Format(noteNameFormat, trackIndex.ToString(), channel.ToString(), name);
                        _midiEvents[key] = value;
                        valuesChanged = true;
                        NoteOffEvents.Value[key] = value;
                        CurrentNoteEvents.Value.Remove(key);

                        if (_printLogMessages)
                            Log.Debug(key + "=" + value);
                        break;
                    }
                    case ControlChangeEvent controlChangeEvent:
                    {
                        var channel = controlChangeEvent.Channel;
                        var controller = (int)controlChangeEvent.Controller;
                        var value = controlChangeEvent.ControllerValue / 127f;
                        var key = string.Format(ccNameFormat, trackIndex.ToString(), channel.ToString(), controller.ToString());
                        _midiEvents[key] = value;
                        valuesChanged = true;
                        CCEvents.Value[key] = value;

                        if (_printLogMessages)
                            Log.Debug($"{key}={value}");
                        
                        break;
                    }
                }

                lastEventIndex = nextEventIndex;
                nextEventIndex = lastEventIndex + 1;
            }

            _lastTrackEventIndices[trackIndex] = lastEventIndex;
            return valuesChanged;
        }

        /**
         * From https://github.com/naudio/NAudio/blob/master/Docs/MidiFile.md
         */
        private static string TimeToBarsBeatsTicks(long eventTime, double ticksPerQuarterNote, TimeSignatureEvent timeSignature)
        {
            var beatsPerBar = timeSignature?.Numerator ?? 4;
            var ticksPerBar = timeSignature == null
                                  ? ticksPerQuarterNote * 4
                                  : (timeSignature.Numerator * ticksPerQuarterNote * 4) / (1 << timeSignature.Denominator);
            var ticksPerBeat = ticksPerBar / beatsPerBar;
            var bar = (eventTime / ticksPerBar);
            var beat = ((eventTime % ticksPerBar) / ticksPerBeat);
            var tick = eventTime % ticksPerBeat;
            return string.Format($"{bar}:{beat}:{tick}");
        }

        private void ChangedUseNumbersAsNames(EvaluationContext context)
        {
            var useNumbersAsNames = UseNumbersAsNames.GetValue(context);
            var useNumbersAsNamesChanged = _useNumbersAsNames == useNumbersAsNames;
            _useNumbersAsNames = useNumbersAsNames;

            if (!useNumbersAsNamesChanged)
                return;
            
            ClearTracks();

            _midiEvents.Clear();
            _channelNames.Clear();
            ClearSpecificEventLists();
        }

        private void ClearSpecificEventLists()
        {
            NoteOnEvents.Value.Clear();
            NoteOffEvents.Value.Clear();
            CCEvents.Value.Clear();
        }

        // The MIDI file input
        private bool _initialized = false;
        private MidiFile _midiFile = null;
        private MidiEventCollection _midiEventCollection = null;
        private double _deltaTicksPerQuarterNote = 500000.0 / 60;

        // Output data
        private readonly Dict<float> _midiEvents = new(0f);

        // Parsing the file
        private TimeSignatureEvent _timeSignature = null;
        private double _lastTimeInBars = 0f;
        private List<int> _lastTrackEventIndices = null;
        private bool _printLogMessages = false;
        private bool _useNumbersAsNames = false;
        private bool _useAbsoluteTime = true;
        
        public const string ControlMessagePrefix = "controller";
        const string StandardNameFormat = "/track{0}/channel{1}/{2}";
        const string NoteNumberNamesFormat = "{0}/{1}/{2}";
        const string StandardCcFormat = "/track{0}/channel{1}/" + ControlMessagePrefix + "{2}";
        const string CcNumberNamesFormat = "{0}/{1}/" + ControlMessagePrefix + "{2}";

        [Input(Guid = "31FE831F-C3BE-4AE3-884B-D2FC4F1754A4")]
        public readonly InputSlot<string> Filename = new();
        
        [Input(Guid = "01993AA9-CA73-4C74-AD03-340B37C4E47C")]
        public readonly InputSlot<bool> UseAbsoluteTime = new(true);

        [Input(Guid = "8B88C669-7351-4332-9294-9A06A46F45A1")]
        public readonly InputSlot<bool> PrintLogMessages = new();
        
        [Input(Guid = "C875E9B1-8F25-4F87-954B-05D2EC19DF57")]
        public readonly InputSlot<bool> UseNumbersAsNames = new(false);
    }
}