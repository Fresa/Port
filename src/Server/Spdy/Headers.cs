using System.Collections.Generic;

namespace Port.Server.Spdy
{
    public class Headers : Control
    {
        public override short Type => 8;
        public bool IsLastFrame => Flags == 1;
        public Dictionary<string, string> Values { get; set; } =
            new Dictionary<string, string>();
    }
}