using Games.megrurimeguru1;
using Games.megrurimeguru1.CreateWorld;
using XorShiftAddSharp;

namespace TestProject2
{
    [TestClass]
    public class UnitTest1
    {
        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }


        [TestMethod]
        public void TestMethod2()
        {
            XorShiftAddSharp.XorShiftAdd xor = new(1331);
            Console.WriteLine(xor.NextDouble());
            Console.WriteLine(xor.NextDouble());
        }
        [TestMethod]
        public void CreateMapImgTest1()
        {
            XorShiftAddPool xorShiftAddPool = new(23);
            CreateMapImg grad = new(xorShiftAddPool);
            List<NoisePram> noisePrams = new();

            NoisePram noisePram1 = new NoisePram();
            noisePram1.Frequency = 2;
            noisePram1.Persistence = 1;
            noisePram1.Octaves = 3;
            noisePram1.Scale = 200;
            noisePram1.Mode = 0;
            
            NoisePram noisePram2 = new NoisePram();
            noisePram2.Frequency = 0.5;
            noisePram2.Persistence = 20;
            noisePram2.Octaves = 2;
            noisePram2.Scale = 200;
            noisePram2.Mode = 1;

            NoisePram noisePram3 = new NoisePram();
            noisePram3.Frequency = 2;
            noisePram3.Persistence = 1;
            noisePram3.Octaves = 1;
            noisePram3.Scale = 400;
            noisePram3.Mode = 1;
            noisePram3.OffsetX = 256;
            noisePram3.OffsetY = 256;

            NoisePram noisePram4 = new NoisePram();
            noisePram4.Frequency = 2;
            noisePram4.Persistence = 1;
            noisePram4.Octaves = 1;
            noisePram4.Scale = 2000;
            noisePram4.Mode = 0;
            noisePram4.OffsetX = 1000;
            noisePram4.OffsetY = 1000;

            noisePrams.Add(noisePram1);
            noisePrams.Add(noisePram2);
            noisePrams.Add(noisePram3);
            noisePrams.Add(noisePram4);

            grad.CreateImg(noisePrams,10000,10000);
        }
    }
}