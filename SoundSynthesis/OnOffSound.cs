
using System;

namespace SoundSynthesis
{
    public delegate double GetSample(double timestep);

    public interface SoundState
    {
        void Play(StateContext ctx);
        void Stop(StateContext ctx);
        double? GetSample(double step, StateContext ctx);
    };

    public interface StateContext
    {
        void SetState(SoundState state);
        GetSample GetSample { get; }
        int GetNextSampleIndex();
        void ResetSampleIndex();
        double FadeOutRate { get; }
    }

    public class Playing : SoundState
    {
        public void Play(StateContext ctx) {}

        public void Stop(StateContext ctx) 
        {
            ctx.SetState(new FadingOut());
        }

        public double? GetSample(double step, StateContext ctx)
        {
            var t = ctx.GetNextSampleIndex() * step;
            return ctx.GetSample(t);
        }
    }

    public class Stopped : SoundState
    {
        public void Play(StateContext ctx) 
        {
            ctx.SetState(new Playing());
        }

        public void Stop(StateContext ctx) {}

        public double? GetSample(double step, StateContext ctx)
        {
            return null;
        }
    }

    public class FadingOut : SoundState
    {
        private bool _needsStart = false;
        private double _fade = 1.0;

        public void Play(StateContext ctx)
        {
            _needsStart = true;
        }

        public void Stop(StateContext ctx) { }

        public double? GetSample(double step, StateContext ctx)
        {
            _fade -= ctx.FadeOutRate * step;
            if (_fade < 0.0)
            {
                if (_needsStart)
                    ctx.SetState(new Playing());
                else
                    ctx.SetState(new Stopped());
                ctx.ResetSampleIndex();
                return 0.0;
            }

            var t = ctx.GetNextSampleIndex() * step;
            return ctx.GetSample(t) * _fade;
        }
    }

    public class OnOffSound : StateContext
    {
        public GetSample GetSample { get; protected set; }

        public Action onAction { get; set; }
        public double FadeOutRate => 10.0;

        private SoundState _state;
        private int _nextSampleIndex = 0;

        public OnOffSound(GetSample getSample)
        {
            GetSample = getSample;
            _state = new Stopped();
        }

        protected OnOffSound()
        {
            _state = new Stopped();
        }

        public void SetState(SoundState state)
        {
            Console.WriteLine($"{state.GetType().Name}");
            _state = state;
        }
        
        public void On()
        {
            _state.Play(this);
            onAction?.Invoke();
        }

        public void Off()
        {
            _state.Stop(this);
        }

        public void Reset()
        {
            Console.WriteLine("Reset");
        }

        public double? TryGetSample(double timeStep)
        {
            return _state.GetSample(timeStep, this);
        }

        public int GetNextSampleIndex()
        {
            return _nextSampleIndex++;
        }

        public void ResetSampleIndex()
        {
            _nextSampleIndex = 0;
        }
    }
}
