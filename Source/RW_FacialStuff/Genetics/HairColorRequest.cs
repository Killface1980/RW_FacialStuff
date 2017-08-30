namespace FacialStuff.Genetics
{
    public struct HairColorRequest
    {
        public HairColorRequest(float pheoMelanin, float euMelanin, float cuticula, float greyness)
        {
            this.EuMelanin = euMelanin;
            this.PheoMelanin = pheoMelanin;
            this.Cuticula = cuticula;
            this.Greyness = greyness;
        }

        public float Greyness { get; set; }

        public float EuMelanin { get; set; }

        public float PheoMelanin { get; set; }

        public float Cuticula { get; set; }
    }
}