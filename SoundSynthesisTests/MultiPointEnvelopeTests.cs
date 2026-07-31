using ControlGraph.AM;

namespace SoundSynthesisTests
{
    [TestClass]
    public class MultiPointEnvelopeTests
    {
        [TestMethod]
        public void MultiPointShape_WithSingleSlope_Test()
        {
            var sampleRate = 44100;
            var buffer = new short[sampleRate];
            for (int i = 0; i <  buffer.Length; i++) 
                buffer[i] = short.MaxValue;

            var amplitudes = new float[2];
            amplitudes[0] = 0.0f;
            amplitudes[1] = 1.0f;

            var result = MultiPointEnvelope.Shape(0, buffer, sampleRate, amplitudes, 1.0);

            var end = sampleRate - 1;
            var middle = sampleRate/2;
            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(short.MaxValue/2, buffer[middle]);
            Assert.AreEqual(short.MaxValue, buffer[end]);
        }

        [TestMethod]
        public void MultiPointShape_WithDoubleSlope_Test()
        {
            var sampleRate = 44100;
            var buffer = new short[sampleRate];
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = short.MaxValue;

            var amplitudes = new float[3];
            amplitudes[0] = 0.0f;
            amplitudes[1] = 1.0f;
            amplitudes[2] = 0.0f;

            var result = MultiPointEnvelope.Shape(0, buffer, sampleRate, amplitudes, 1.0);

            var end = sampleRate - 1;
            var middle = sampleRate / 2;
            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(short.MaxValue, buffer[middle]);
            Assert.AreEqual(0, buffer[end]);
        }

    }
}
