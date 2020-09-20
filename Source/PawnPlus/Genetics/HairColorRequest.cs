namespace FacialStuff.Genetics
{
    public class HairColorRequest
    {
        /*
        public HairColorRequest(float pheoMelanin, float euMelanin, float greyness, Baldness baldness)
        {
            this.EuMelanin = euMelanin;
            this.PheoMelanin = pheoMelanin;

            // this.Cuticula = cuticula;
            this.Greyness = greyness;
            this.Baldness = baldness;
        }
        */
        public HairColorRequest(float pheoMelanin, float euMelanin, float greyness)
        {
            this.EuMelanin = euMelanin;
            this.PheoMelanin = pheoMelanin;

            // this.Cuticula = cuticula;
            this.Greyness = greyness;
        }

        public float EuMelanin { get; set; }

        public float Greyness { get; set; }

        public float PheoMelanin { get; set; }

        // public float Cuticula { get; set; }

        // public Baldness Baldness;
    }
}