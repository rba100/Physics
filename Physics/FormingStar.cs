
using Physics.Engine;

namespace Physics
{

    internal class FormingStar
    {
        public IParticle Particle;
        public int Frame = 0;

        public FormingStar(IParticle particle)
        {
            Particle = particle;
        }
    }
}
