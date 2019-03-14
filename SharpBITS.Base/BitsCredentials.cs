using JetBrains.Annotations;

namespace SharpBits.Base
{
    [PublicAPI]
    public class BitsCredentials
    {
        public AuthenticationScheme AuthenticationScheme { get; set; }

        public AuthenticationTarget AuthenticationTarget { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
