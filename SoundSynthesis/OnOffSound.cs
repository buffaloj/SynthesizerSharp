
namespace SoundSynthesis
{
    public delegate double GetSample(double timestep);

    public interface SoundState
    {
        void Play(StateContext ctx);
        void Stop(StateContext ctx);
        double? GetSample(double step, StateContext ctx);
        bool Available { get; }
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

        public bool Available => false;
    }

    public class Stopped : SoundState
    {
        public void Play(StateContext ctx) 
        {
            ctx.ResetSampleIndex();
            ctx.SetState(new Playing());
        }

        public void Stop(StateContext ctx) {}

        public double? GetSample(double step, StateContext ctx)
        {
            return null;
        }
        public bool Available => true;
    }

    public class FadingOut : SoundState
    {
        private double _fade = 1.0;

        public void Play(StateContext ctx) { }

        public void Stop(StateContext ctx) { }

        public double? GetSample(double step, StateContext ctx)
        {
            _fade -= ctx.FadeOutRate * step;
            if (_fade < 0.0)
            {
                ctx.ResetSampleIndex();
                return null;
            }

            var t = ctx.GetNextSampleIndex() * step;
            return ctx.GetSample(t) * _fade;
        }

        public bool Available => false;
    }

    public interface OnOff
    {
        void On();
        void Off();
    }

    public class OnOffSound : OnOff, StateContext
    {
        public GetSample GetSample { get; protected set; }

        public bool Available => _state.Available;

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

        public virtual void OnStopped()
        {
            _state = new Stopped();
            Console.WriteLine("Stopped");
            ResetSampleIndex();
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

    public class OnOffSoundJuggler : OnOff
    {
        public Action<OnOffSound> onAction { get; set; }

        private IList<OnOffSound> _sounds = new List<OnOffSound>();
        private OnOffSound? _current;

        private Func<OnOffSound> _createSound;

        public OnOffSoundJuggler(Func<OnOffSound> createSound)
        {
            _createSound = createSound;
        }

        public void On()
        {
            Console.WriteLine("On");

            var sound = GetSound();
            sound.onAction = () => onAction(sound);

            _current = sound;
            _current.On();
        }

        private OnOffSound GetSound()
        {
            var sound = _sounds.FirstOrDefault(s => s.Available);
            if (sound != null)
                return sound;

            sound = _createSound();
            _sounds.Add(sound);
            return sound;
        }

        public void Off()
        {
            Console.WriteLine("Off");

            _current?.Off();
            _current = null;
        }
    }
}
